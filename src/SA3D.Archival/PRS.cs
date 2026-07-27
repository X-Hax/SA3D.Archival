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
		/// Decompresses PRS data in a stream
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static long Decompress(Stream source, Stream destination)
		{
			uint bitpos = 9;
			int reuseCount;
			int reuseOffset;
			int offset;

			int currentByte = source.ReadByte();
			long start = destination.Position;

			byte[] reuseBuffer = new byte[256];

			while(true)
			{
				bitpos--;
				if(bitpos == 0)
				{
					currentByte = source.ReadByte();
					bitpos = 8;
				}

				bool flag = (currentByte & 1) == 1;
				currentByte >>= 1;
				if(flag)
				{
					destination.WriteByte((byte)source.ReadByte());
					continue;
				}

				bitpos--;
				if(bitpos == 0)
				{
					currentByte = source.ReadByte();
					bitpos = 8;
				}

				flag = (currentByte & 1) == 1;
				currentByte >>= 1;
				if(flag)
				{
					offset = source.ReadByte() | (source.ReadByte() << 8);
					if(offset == 0)
					{
						return destination.Position - start;
					}

					reuseCount = offset & 0x7;
					reuseOffset = unchecked((int)((uint)(offset >> 3) | 0xFFFFE000u));
					if(reuseCount == 0)
					{
						reuseCount = source.ReadByte() + 1;
					}
					else
					{
						reuseCount += 2;
					}
				}
				else
				{
					reuseCount = 0;
					for(int i = 0; i < 2; i++)
					{
						bitpos--;
						if(bitpos == 0)
						{
							currentByte = source.ReadByte();
							bitpos = 8;
						}

						flag = (currentByte & 1) == 1;
						currentByte >>= 1;
						reuseCount = (reuseCount << 1) | (flag ? 1 : 0);
					}

					reuseOffset = unchecked((int)(source.ReadByte() | 0xFFFFFF00u));
					reuseCount += 2;
				}

				int reuseBufferSize = Math.Min(-reuseOffset, reuseCount);

				long currentPosition = destination.Position;
				destination.Seek(reuseOffset, SeekOrigin.Current);
				_ = destination.Read(reuseBuffer, 0, reuseBufferSize);
				destination.Seek(currentPosition, SeekOrigin.Begin);

				int repeatCount = reuseCount / reuseBufferSize;
				for(int i = 0; i < repeatCount; i++)
				{
					destination.Write(reuseBuffer, 0, reuseBufferSize);
				}

				int repeatRemainder = reuseCount % reuseBufferSize;
				if(repeatRemainder > 0)
				{
					destination.Write(reuseBuffer, 0, repeatRemainder);
				}
			}
		}

		/// <summary>
		/// Decompresses PRS data from a stream
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static byte[] Decompress(Stream source)
		{
			using MemoryStream destination = new();
			Decompress(source, destination);
			return destination.ToArray();
		}

		/// <summary>
		/// Reads a PRS compressed file
		/// </summary>
		/// <param name="filepath">The path to the file to read.</param>
		/// <returns>The uncompressed file data.</returns>
		public static byte[] DecompressFile(string filepath)
		{
			using FileStream filestream = File.OpenRead(filepath);
			using MemoryStream destination = new();
			Decompress(filestream, destination);
			return destination.ToArray();
		}

		/// <summary>
		/// Decompresses PRS byte data
		/// </summary>
		/// <param name="data">The PRS data to decompress.</param>
		/// <returns></returns>
		public static byte[] DecompressBytes(byte[] data)
		{
			using MemoryStream source = new(data);
			return Decompress(source);
		}


		private class Compressor
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

			public Compressor(byte[] data)
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
		public static byte[] Compress(byte[] data)
		{
			return data.Length == 0 ? data : new Compressor(data).Compress();
		}
	}
}
