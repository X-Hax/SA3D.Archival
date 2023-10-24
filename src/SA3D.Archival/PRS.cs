using System;
using System.IO;

namespace SA3D.Archival
{
	/// <summary>
	/// PRS de/compression algorithm.
	/// </summary>
	public static class PRS
	{
		/// <summary>
		/// Attempts to read a PRS file and decompress it. Only decompresses if the file ends with ".prs".
		/// </summary>
		/// <param name="filepath">The path to the file to read.</param>
		/// <returns>The uncompressed file data.</returns>
		public static byte[] ReadPRSFile(string filepath)
		{
			byte[] source = File.ReadAllBytes(filepath);
			if(Path.GetExtension(filepath).ToLowerInvariant() == ".prs")
			{
				source = DecompressPRS(source);
			}

			return source;
		}

		private static uint DecompressPRS(byte[] data, byte[]? result)
		{
			uint bitpos = 9;
			uint r3, r5;
			int offset;

			byte currentByte = data[0];
			uint sourceAddr = 1;
			uint resultAddr = 0;

			while(true)
			{
				bitpos--;
				if(bitpos == 0)
				{
					currentByte = data[sourceAddr];
					bitpos = 8;
					sourceAddr++;
				}

				bool flag = (currentByte & 1) == 1;
				currentByte >>= 1;
				if(flag)
				{
					result?.SetValue(data[sourceAddr], resultAddr);
					sourceAddr++;
					resultAddr++;
					continue;
				}

				bitpos--;
				if(bitpos == 0)
				{
					currentByte = data[sourceAddr];
					bitpos = 8;
					sourceAddr++;
				}

				flag = (currentByte & 1) == 1;
				currentByte >>= 1;
				if(flag)
				{
					r3 = data[sourceAddr] & 0xFFu;
					offset = (int)(((data[sourceAddr + 1] & 0xFFu) << 8) | r3);
					sourceAddr += 2;
					if(offset == 0)
					{
						return resultAddr;
					}

					r3 &= 0x7;
					r5 = (uint)(offset >> 3) | 0xFFFFE000;
					if(r3 == 0)
					{
						r3 = data[sourceAddr] & 0xFFu;
						sourceAddr++;
						r3++;
					}
					else
					{
						r3 += 2;
					}

					r5 += resultAddr;
				}
				else
				{
					r3 = 0;
					for(int i = 0; i < 2; i++)
					{
						bitpos--;
						if(bitpos == 0)
						{
							currentByte = data[sourceAddr];
							bitpos = 8;
							sourceAddr++;
						}

						flag = (currentByte & 1) == 1;
						currentByte >>= 1;
						offset = (int)r3 << 1;
						r3 = (uint)(offset | (flag ? 1 : 0));
					}

					offset = unchecked((int)(data[sourceAddr] | 0xFFFFFF00));
					r3 += 2;
					sourceAddr++;
					r5 = (uint)offset + resultAddr;
				}

				if(r3 == 0)
				{
					continue;
				}

				uint count = r3;
				for(int i = 0; i < count; i++)
				{
					result?.SetValue(result[r5], resultAddr);
					r5++;
					r3++;
					resultAddr++;
				}
			}
		}

		/// <summary>
		/// Decompresses PRS data.
		/// </summary>
		/// <param name="data">The PRS data to decompress.</param>
		/// <param name="outLength">Output length of the data. Pass 0 if unknown.</param>
		/// <returns></returns>
		public static byte[] DecompressPRS(byte[] data, uint outLength = 0)
		{
			if(outLength == 0)
			{
				outLength = DecompressPRS(data, null);
			}

			byte[] result = new byte[outLength];
			DecompressPRS(data, result);
			return result;
		}

		private class PrsCompressor
		{
			private byte _bitPos;
			private readonly byte[] _data;
			private byte[] _result;

			private uint _resultAddr;
			private uint _controlByteAddr;

			// Has to be public, otherwise compiler recognizes it as unused private property
			public byte this[uint index]
			{
				set
				{
					try
					{
						_result[index] = value;
					}
					catch(IndexOutOfRangeException)
					{
						IncreaseResultSize(index);
						_result[index] = value;
					}
				}
				get
				{
					try
					{
						return _result[index];
					}
					catch
					{
						IncreaseResultSize(index);
						return _result[index];
					}
				}
			}

			public PrsCompressor(byte[] data)
			{
				_data = data;
				_result = new byte[data.Length];
				_resultAddr = 1;
			}

			private void IncreaseResultSize(uint minIndex)
			{
				int newLength = _result.Length + _data.Length;
				while(_result.Length < minIndex)
				{
					newLength += _data.Length;
				}

				Array.Resize(ref _result, newLength);
			}

			private void PutControlBitNoSave(bool put)
			{
				this[_controlByteAddr] >>= 1;
				if(put)
				{
					this[_controlByteAddr] |= 0x80;
				}

				_bitPos++;
			}

			private void PutControlSave()
			{
				if(_bitPos >= 8)
				{
					_bitPos = 0;
					_controlByteAddr = _resultAddr;
					_resultAddr++;
				}
			}

			private void PutControlBit(bool put)
			{
				PutControlBitNoSave(put);
				PutControlSave();
			}

			private void Finish()
			{
				PutControlBit(false);
				PutControlBit(true);

				if(_bitPos != 0)
				{
					this[_controlByteAddr] >>= 8 - _bitPos;
				}

				this[_resultAddr++] = 0;
				this[_resultAddr++] = 0;
				Array.Resize(ref _result, (int)_resultAddr);
			}


			private void Copy(int offset, byte size)
			{
				PutControlBit(false);

				if(offset >= -0x100 && size <= 5)
				{
					PutControlBit(false);
					PutControlBit(((size - 2) & 2) == 2);
					PutControlBitNoSave(((size - 2) & 1) == 1);
					this[_resultAddr++] = (byte)(offset & 0xFF);
				}
				else if(size <= 9)
				{
					PutControlBitNoSave(true);
					this[_resultAddr++] = (byte)(((offset << 3) & 0xF8) | ((size - 2) & 0x07));
					this[_resultAddr++] = (byte)((offset >> 5) & 0xFF);
				}
				else
				{
					PutControlBitNoSave(true);
					this[_resultAddr++] = (byte)((offset << 3) & 0xF8);
					this[_resultAddr++] = (byte)((offset >> 5) & 0xFF);
					this[_resultAddr++] = (byte)(size - 1);
				}

				PutControlSave();
			}

			public byte[] Compress()
			{
				for(int i = 0; i < _data.Length;)
				{
					int blockOffset = 0, blockSize = 0;

					for(int j = i - 1; j > 0 && j >= i - 0x1FF0 && blockSize < 256; j--)
					{
						uint checkSize = 1;
						if(_data[i] == _data[j])
						{
							do
							{
								checkSize++;
							}
							while(checkSize < 256
									&& i + checkSize <= _data.Length
									&& _data[i + checkSize - 1] == _data[j + checkSize - 1]);

							checkSize--;
							if(((checkSize >= 2 && j - i >= -0x100) || checkSize >= 3) && checkSize > blockSize)
							{
								blockOffset = j - i;
								blockSize = (int)checkSize;
							}
						}
					}

					if(blockSize == 0)
					{
						PutControlBitNoSave(true);
						this[_resultAddr++] = _data[i++];
						PutControlSave();
					}
					else
					{
						Copy(blockOffset, (byte)blockSize);
						i += blockSize;
					}
				}

				Finish();

				return _result;
			}
		}

		/// <summary>
		/// Compresses a byte array using the PRS compression algorithm.
		/// </summary>
		/// <param name="data">Data to compress.</param>
		/// <returns>The compressed data.</returns>
		public static byte[] CompressPRS(byte[] data)
		{
			return data.Length == 0 ? data : new PrsCompressor(data).Compress();
		}
	}
}
