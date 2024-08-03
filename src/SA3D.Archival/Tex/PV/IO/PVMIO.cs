using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Tex.PV.IO
{
	internal static class PVMIO
	{
		public static bool IsPVM(EndianStackReader data, uint address)
		{
			try
			{
				return PVHeader.PVMH.ValidateData(data, address);
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		public static PVM ReadPVM(EndianStackReader data, uint address)
		{
			if(!PVHeader.PVMH.ValidateData(data, address))
			{
				throw new InvalidArchiveException("Data is not a PVM Archive!");
			}

			uint dataAddress = address + data.ReadUInt(address + 4) + 8;
			PVMFlags flags = (PVMFlags)data.ReadUShort(address + 8);
			ushort pvrCount = data.ReadUShort(address + 0xA);
			address += 0xC;

			PVMMeta[] metaData = new PVMMeta[pvrCount];
			for(int i = 0; i < metaData.Length; i++)
			{
				metaData[i] = PVMMeta.Read(data, ref address, flags);
			}

			PVM result = new();
			List<(string file, string args)> pvmiStrings = [];
			List<byte[]> imgcData = [];
			string paletteName = "";

			while(dataAddress < data.Length)
			{
				PVHeader dataHeader = (PVHeader)data.ReadUInt(dataAddress);
				int dataLength = data.ReadInt(dataAddress + 4);

				bool end = false;
				switch(dataHeader)
				{
					case PVHeader.PVRT:
						int pvrIndex = result.PVRs.Count;
						PVMMeta meta = metaData[pvrIndex];
						PVR pvr = PVRIO.ReadPVR(data, dataAddress);
						pvr.Name = meta.Filename;
						pvr.GlobalIndex = meta.GlobalIndex;

						if(pvmiStrings.Count > pvrIndex)
						{
							(pvr.OriginalFilePath, pvr.ConversionArguments) = pvmiStrings[pvrIndex];
						}

						if(imgcData.Count > pvrIndex)
						{
							pvr.OriginalImageData = imgcData[pvrIndex];
						}

						result.PVRs.Add(pvr);
						break;
					case PVHeader.MDLN:
						ushort mdlnCount = data.ReadUShort(dataAddress + 8);
						uint mdlnAddress = dataAddress + 10;
						while(result.ModelNames.Count < mdlnCount)
						{
							string modelName = data.ReadNullterminatedString(mdlnAddress, out uint mdlnLength);
							result.ModelNames.Add(modelName);
							mdlnAddress += mdlnLength + 1;
						}

						break;
					case PVHeader.CONV:
						result.ConverterName = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case PVHeader.COMM:
						result.Comment = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case PVHeader.PVMI:
						ushort pvmiFileLength = data.ReadUShort(dataAddress + 8);
						ushort pvmiOptionLength = data.ReadUShort(dataAddress + 0xA);
						uint pvmiAddress = dataAddress + 0xC;

						for(int pvmi = 0; pvmi < pvrCount; pvmi++)
						{
							string pvmiFile = data.ReadNullterminatedString(pvmiAddress);
							pvmiAddress += pvmiFileLength;

							string pvmiOptions = data.ReadNullterminatedString(pvmiAddress);
							pvmiAddress += pvmiOptionLength;

							pvmiStrings.Add((pvmiFile, pvmiOptions));
						}

						break;
					case PVHeader.IMGC:
						imgcData.Add(data.Slice(dataAddress + 8, dataLength).ToArray());
						break;
					case PVHeader.PVPL:
						PVP palette = PVP.ReadPVP(data, dataAddress);
						palette.Name = paletteName;
						paletteName = "";
						result.PVPs.Add(palette);
						break;
					case PVHeader.PVPN:
						paletteName = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case PVHeader.PVMH:
						break;
					case PVHeader.GBIX:
						break;
					default:
						end = true;
						break;
				}

				if(end)
				{
					break;
				}

				dataAddress += (uint)dataLength + 8;
			}

			return result;
		}

		public static void WritePVM(PVM pvm, EndianStackWriter writer, PVMMetadataIncludes includes)
		{
			PVMFlags flags = GetFlags(pvm, includes);

			uint start = writer.Position;
			uint lastDataChunk = 0;
			void WriteChunk(PVHeader header, uint pad, Action writeData)
			{
				header.Write(writer);
				writer.WriteEmpty(4);
				lastDataChunk = writer.Position;
				uint dataStart = writer.Position;

				writeData();

				writer.AlignFrom(pad, start);

				uint dataLength = writer.Position - dataStart;
				writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
				writer.WriteUInt(dataLength);
				writer.Stream.Seek(0, SeekOrigin.End);
			}

			WriteChunk(PVHeader.PVMH, 4, () =>
			{
				writer.WriteUShort((ushort)flags);
				writer.WriteUShort((ushort)pvm.PVRs.Count);

				for(ushort i = 0; i < pvm.PVRs.Count; i++)
				{
					PVR pvr = pvm.PVRs[i];
					new PVMMeta()
					{
						Index = i,
						Filename = pvr.Name,
						PixelFormat = pvr.PixelFormat,
						DataFormat = pvr.DataFormat,
						EntryAttributes = pvr.ArchiveMetaEntryAttributes,
						Width = (ushort)pvr.Width,
						Height = (ushort)pvr.Height,
						GlobalIndex = pvr.GlobalIndex,

					}.Write(writer, flags);
				}
			});

			if(flags.HasFlag(PVMFlags.Comment))
			{
				WriteChunk(PVHeader.COMM, 4, () => writer.WriteStringNullterminated(pvm.Comment));
			}

			if(flags.HasFlag(PVMFlags.MDLNChunk))
			{
				WriteChunk(PVHeader.MDLN, 4, () =>
				{
					writer.WriteUShort((ushort)pvm.ModelNames.Count);
					foreach(string modelName in pvm.ModelNames)
					{
						writer.WriteStringNullterminated(modelName);
					}
				});
			}

			if(flags.HasFlag(PVMFlags.PVMIChunk))
			{
				WriteChunk(PVHeader.PVMI, 4, () =>
				{
					int longestString = pvm.PVRs.Max(x => x.OriginalFilePath.Length) + 1;
					int longestArgs = pvm.PVRs.Max(x => x.ConversionArguments.Length) + 1;

					writer.WriteUShort((ushort)longestString);
					writer.WriteUShort((ushort)longestArgs);

					foreach(PVR pvr in pvm.PVRs)
					{
						writer.WriteString(pvr.OriginalFilePath, longestString);
						writer.WriteString(pvr.ConversionArguments, longestArgs);
					}
				});
			}

			if(flags.HasFlag(PVMFlags.PVMIChunk))
			{
				foreach(PVR pvr in pvm.PVRs)
				{
					WriteChunk(PVHeader.PVMI, 4, () => writer.Write(pvr.OriginalImageData));
				}
			}

			if(flags.HasFlag(PVMFlags.PVPLChunks))
			{
				foreach(PVP pvp in pvm.PVPs)
				{
					if(flags.HasFlag(PVMFlags.PVPNChunks))
					{
						WriteChunk(PVHeader.PVPN, 4, () => writer.WriteStringNullterminated(pvp.Name));
					}

					pvp.WritePVP(writer);
					writer.Align(4);
				}
			}

			writer.AlignFrom(32, start + 0x10);

			// correct the last chunks length
			uint correctedLength = writer.Position - lastDataChunk;
			writer.Stream.Seek(lastDataChunk - 4, SeekOrigin.Begin);
			writer.WriteUInt(correctedLength);
			writer.Stream.Seek(0, SeekOrigin.End);

			if(flags.HasFlag(PVMFlags.PVRTChunks))
			{
				for(int i = 0; i < pvm.PVRs.Count; i++)
				{
					PVRIO.WritePVR(pvm.PVRs[i], writer, false, i < pvm.PVRs.Count - 1);
				}
			}
		}

		private static PVMFlags GetFlags(PVM pvm, PVMMetadataIncludes includes)
		{
			PVMFlags flags = default;
			if(includes.IncludeNames)
			{
				flags |= PVMFlags.Filenames;
			}

			if(includes.IncludeCategoryCodes)
			{
				flags |= PVMFlags.CategoryCode;
			}

			if(includes.IncludeEntryInfo)
			{
				flags |= PVMFlags.EntryInfo;
			}

			if(includes.IncludeGlobalIndices)
			{
				flags |= PVMFlags.GlobalIndices;
			}

			if(!string.IsNullOrWhiteSpace(pvm.Comment))
			{
				flags |= PVMFlags.Comment;
			}

			if(!string.IsNullOrWhiteSpace(pvm.ConverterName))
			{
				flags |= PVMFlags.CONVChunk;
			}

			if(pvm.ModelNames.Count > 0)
			{
				flags |= PVMFlags.MDLNChunk;
			}

			if(pvm.PVRs.Count > 0)
			{
				flags |= PVMFlags.PVRTChunks;

				if(pvm.PVRs.Any(x => !string.IsNullOrWhiteSpace(x.OriginalFilePath) || !string.IsNullOrWhiteSpace(x.ConversionArguments)))
				{
					flags |= PVMFlags.PVPNChunks;
				}

				if(pvm.PVRs.Any(x => x.OriginalImageData.Length > 0))
				{
					flags |= PVMFlags.IMGCChunks;
				}
			}

			if(pvm.PVPs.Count > 0)
			{
				flags |= PVMFlags.PVPLChunks;

				if(pvm.PVPs.Any(x => !string.IsNullOrWhiteSpace(x.Name)))
				{
					flags |= PVMFlags.PVPNChunks;
				}
			}

			return flags;
		}
	}
}
