using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Tex.GV.IO
{
	internal class GVMIO
	{
		public static bool CheckIsGVM(EndianStackReader data, uint address)
		{
			try
			{
				return GVHeader.GVMH.ValidateData(data, address);
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		public static GVM ReadGVM(EndianStackReader data, uint address)
		{
			if(!GVHeader.GVMH.ValidateData(data, address))
			{
				throw new InvalidArchiveException("Data is not a GVM Archive!");
			}

			uint dataAddress = address + data.ReadUInt(address + 4) + 8;

			data.PushBigEndian(true);

			GVMFlags flags = (GVMFlags)data.ReadUShort(address + 8);
			ushort gvrCount = data.ReadUShort(address + 0xA);
			address += 0xC;

			GVMMeta[] metaData = new GVMMeta[gvrCount];
			for(int i = 0; i < metaData.Length; i++)
			{
				metaData[i] = GVMMeta.Read(data, ref address, flags);
			}

			data.PopEndian();

			GVM result = new();
			List<(string file, string args)> GVmiStrings = new();
			List<byte[]> imgcData = new();
			string paletteName = "";

			while(dataAddress < data.Length)
			{
				GVHeader dataHeader = (GVHeader)data.ReadUInt(dataAddress);
				int dataLength = data.ReadInt(dataAddress + 4);

				data.PushBigEndian(true);
				bool end = false;
				switch(dataHeader)
				{
					case GVHeader.GVRT:
						int GVrIndex = result.GVRs.Count;
						GVMMeta meta = metaData[GVrIndex];
						GVR GVr = GVRIO.ReadGVR(data, dataAddress);
						GVr.Name = meta.Filename;
						GVr.GlobalIndex = meta.GlobalIndex;

						if(GVmiStrings.Count > GVrIndex)
						{
							(GVr.OriginalFilePath, GVr.ConversionArguments) = GVmiStrings[GVrIndex];
						}

						if(imgcData.Count > GVrIndex)
						{
							GVr.OriginalImageData = imgcData[GVrIndex];
						}

						result.GVRs.Add(GVr);
						break;
					case GVHeader.MDLN:
						ushort mdlnCount = data.ReadUShort(dataAddress + 8);
						uint mdlnAddress = dataAddress + 10;
						while(result.ModelNames.Count < mdlnCount)
						{
							string modelName = data.ReadNullterminatedString(mdlnAddress, out uint mdlnLength);
							result.ModelNames.Add(modelName);
							mdlnAddress += mdlnLength + 1;
						}

						break;
					case GVHeader.CONV:
						result.ConverterName = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case GVHeader.COMM:
						result.Comment = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case GVHeader.GVMI:
						ushort GVmiFileLength = data.ReadUShort(dataAddress + 8);
						ushort GVmiOptionLength = data.ReadUShort(dataAddress + 0xA);
						uint GVmiAddress = dataAddress + 0xC;

						for(int GVmi = 0; GVmi < gvrCount; GVmi++)
						{
							string GVmiFile = data.ReadNullterminatedString(GVmiAddress);
							GVmiAddress += GVmiFileLength;

							string GVmiOptions = data.ReadNullterminatedString(GVmiAddress);
							GVmiAddress += GVmiOptionLength;

							GVmiStrings.Add((GVmiFile, GVmiOptions));
						}

						break;
					case GVHeader.IMGC:
						imgcData.Add(data.Slice(dataAddress + 8, dataLength).ToArray());
						break;
					case GVHeader.GVPL:
						GVP palette = GVP.ReadGVP(data, dataAddress);
						palette.Name = paletteName;
						paletteName = "";
						result.GVPs.Add(palette);
						break;
					case GVHeader.GVPN:
						paletteName = data.ReadNullterminatedString(dataAddress + 8);
						break;
					case GVHeader.GVMH:
						break;
					case GVHeader.GBIX:
						break;
					case GVHeader.GCIX:
						break;
					default:
						end = true;
						break;
				}

				data.PopEndian();

				if(end)
				{
					break;
				}

				dataAddress += (uint)dataLength + 8;
			}

			return result;
		}

		public static void WriteGVM(GVM gvm, EndianStackWriter writer, GVMMetadataIncludes includes)
		{
			GVMFlags flags = GetFlags(gvm, includes);

			uint start = writer.Position;
			uint lastDataChunk = 0;
			void WriteChunk(GVHeader header, uint pad, Action writeData)
			{
				header.Write(writer);
				writer.WriteEmpty(4);
				lastDataChunk = writer.Position;
				uint dataStart = writer.Position;

				writer.PushBigEndian(true);
				writeData();
				writer.PopEndian();

				writer.Align(pad, start);

				uint dataLength = writer.Position - dataStart;
				writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
				writer.WriteUInt(dataLength);
				writer.Stream.Seek(0, SeekOrigin.End);
			}

			WriteChunk(GVHeader.GVMH, 4, () =>
			{
				writer.WriteUShort((ushort)flags);
				writer.WriteUShort((ushort)gvm.GVRs.Count);

				for(ushort i = 0; i < gvm.GVRs.Count; i++)
				{
					GVR gvr = gvm.GVRs[i];

					GVRDataFlags dataFlags = default;
					if(gvr.HasMipMaps)
					{
						dataFlags |= GVRDataFlags.Mipmaps;
					}

					if(gvr.PixelFormat is GVRPixelFormat.Index4 or GVRPixelFormat.Index8)
					{
						if(gvr.InternalPalette == null)
						{
							dataFlags |= GVRDataFlags.ExternalPalette;
						}
						else
						{
							dataFlags |= GVRDataFlags.InternalPalette;
						}
					}

					new GVMMeta()
					{
						Index = i,
						Filename = gvr.Name,
						DataFlags = dataFlags,
						PixelFormat = gvr.PixelFormat,
						PaletteFormat = gvr.InternalPalette?.PaletteFormat ?? default,
						EntryAttributes = gvr.ArchiveMetaEntryAttributes,
						Width = (ushort)gvr.Width,
						Height = (ushort)gvr.Height,
						GlobalIndex = gvr.GlobalIndex,

					}.Write(writer, flags);
				}
			});

			if(flags.HasFlag(GVMFlags.Comment))
			{
				WriteChunk(GVHeader.COMM, 4, () => writer.WriteStringNullterminated(gvm.Comment));
			}

			if(flags.HasFlag(GVMFlags.MDLNChunk))
			{
				WriteChunk(GVHeader.MDLN, 4, () =>
				{
					writer.WriteUShort((ushort)gvm.ModelNames.Count);
					foreach(string modelName in gvm.ModelNames)
					{
						writer.WriteStringNullterminated(modelName);
					}
				});
			}

			if(flags.HasFlag(GVMFlags.GVMIChunk))
			{
				WriteChunk(GVHeader.GVMI, 4, () =>
				{
					int longestString = gvm.GVRs.Max(x => x.OriginalFilePath.Length) + 1;
					int longestArgs = gvm.GVRs.Max(x => x.ConversionArguments.Length) + 1;

					writer.WriteUShort((ushort)longestString);
					writer.WriteUShort((ushort)longestArgs);

					foreach(GVR GVr in gvm.GVRs)
					{
						writer.WriteString(GVr.OriginalFilePath, longestString);
						writer.WriteString(GVr.ConversionArguments, longestArgs);
					}
				});
			}

			if(flags.HasFlag(GVMFlags.GVMIChunk))
			{
				foreach(GVR gvp in gvm.GVRs)
				{
					WriteChunk(GVHeader.GVMI, 4, () => writer.Write(gvp.OriginalImageData));
				}
			}

			if(flags.HasFlag(GVMFlags.GVPLChunks))
			{
				foreach(GVP gvp in gvm.GVPs)
				{
					if(flags.HasFlag(GVMFlags.GVPNChunks))
					{
						WriteChunk(GVHeader.GVPN, 4, () => writer.WriteStringNullterminated(gvp.Name));
					}

					gvp.WriteGVP(writer);
					writer.Align(4);
				}
			}

			// correct the last chunks length
			uint correctedLength = writer.Position - lastDataChunk;
			writer.Stream.Seek(lastDataChunk - 4, SeekOrigin.Begin);
			writer.WriteUInt(correctedLength);
			writer.Stream.Seek(0, SeekOrigin.End);

			if(flags.HasFlag(GVMFlags.GVRTChunks))
			{
				for(int i = 0; i < gvm.GVRs.Count; i++)
				{
					GVRIO.WriteGVR(gvm.GVRs[i], writer, false, false);
				}
			}
		}

		private static GVMFlags GetFlags(GVM GVm, GVMMetadataIncludes includes)
		{
			GVMFlags flags = default;
			if(includes.IncludeNames)
			{
				flags |= GVMFlags.Filenames;
			}

			if(includes.IncludeCategoryCodes)
			{
				flags |= GVMFlags.CategoryCode;
			}

			if(includes.IncludeEntryInfo)
			{
				flags |= GVMFlags.EntryInfo;
			}

			if(includes.IncludeGlobalIndices)
			{
				flags |= GVMFlags.GlobalIndices;
			}

			if(!string.IsNullOrWhiteSpace(GVm.Comment))
			{
				flags |= GVMFlags.Comment;
			}

			if(!string.IsNullOrWhiteSpace(GVm.ConverterName))
			{
				flags |= GVMFlags.CONVChunk;
			}

			if(GVm.ModelNames.Count > 0)
			{
				flags |= GVMFlags.MDLNChunk;
			}

			if(GVm.GVRs.Count > 0)
			{
				flags |= GVMFlags.GVRTChunks;

				if(GVm.GVRs.Any(x => !string.IsNullOrWhiteSpace(x.OriginalFilePath) || !string.IsNullOrWhiteSpace(x.ConversionArguments)))
				{
					flags |= GVMFlags.GVPNChunks;
				}

				if(GVm.GVRs.Any(x => x.OriginalImageData.Length > 0))
				{
					flags |= GVMFlags.IMGCChunks;
				}
			}

			if(GVm.GVPs.Count > 0)
			{
				flags |= GVMFlags.GVPLChunks;

				if(GVm.GVPs.Any(x => !string.IsNullOrWhiteSpace(x.Name)))
				{
					flags |= GVMFlags.GVPNChunks;
				}
			}

			return flags;
		}
	}
}
