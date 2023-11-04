using SA3D.Archival.Tex.PV.IO.DataCodec;
using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common.IO;
using System;
using System.IO;

namespace SA3D.Archival.Tex.PV.IO
{
	internal static class PVRIO
	{
		public static PVR ReadPVR(EndianStackReader data, uint address)
		{
			PVRFileFormat format;

			try
			{
				format = GetPVRFileFormat(data, address);
			}
			catch(Exception e)
			{
				throw new InvalidDataException("Data is not a PVR!", e);
			}

			if(format == PVRFileFormat.None)
			{
				throw new InvalidDataException("Data is not a PVR!");
			}

			uint globalIndex = 0;
			if(format == PVRFileFormat.GBIX)
			{
				globalIndex = data.ReadUInt(address + 8);
				address += data.ReadUInt(address + 4) + 8;
			}

			// ignoring datalength, as it may include padding that we do not want to include
			PVRPixelFormat pixelFormat = (PVRPixelFormat)data[address + 8];
			PVRDataFormat dataFormat = (PVRDataFormat)data[address + 9];
			ushort width = data.ReadUShort(address + 0xC);
			ushort height = data.ReadUShort(address + 0xE);

			PVPixelCodec pixelCodec = PVPixelCodec.GetPixelCodec(pixelFormat);
			PVRDataCodec dataCodec = PVRDataCodec.Create(dataFormat, pixelCodec);

			address += 0x10;
			int dataSize = dataCodec.CalculateTextureSize(width, height);

			int paletteEntries = dataCodec.GetPaletteEntries(width);
			if(paletteEntries > 0 && !dataCodec.NeedsExternalPalette)
			{
				dataSize += paletteEntries / pixelCodec.Pixels * pixelCodec.BytesPerPixel;
			}

			if(dataCodec.HasMipmaps)
			{
				for(int size = 1; size < width; size <<= 1)
				{
					dataSize += dataCodec.CalculateTextureSize(size, size);
				}
			}

			byte[] pvrData = data.Slice(address, dataSize).ToArray();
			return new(pvrData, globalIndex, width, height, pixelFormat, dataFormat, string.Empty);
		}

		public static void WritePVR(PVR pvr, EndianStackWriter writer, bool includeGlobalIndex, bool pad)
		{
			EndianStackReader data = pvr.CreateDataReader();
			uint start = writer.Position;

			if(includeGlobalIndex)
			{
				PVHeader.GBIX.Write(writer);
				writer.WriteUInt(8);
				writer.WriteUInt(pvr.GlobalIndex);
				writer.WriteEmpty(4);
			}

			PVHeader.PVRT.Write(writer);
			writer.WriteEmpty(4);

			uint dataStart = writer.Position;

			writer.WriteByte((byte)pvr.PixelFormat);
			writer.WriteByte((byte)pvr.DataFormat);
			writer.WriteEmpty(2);
			writer.WriteUShort((ushort)pvr.Width);
			writer.WriteUShort((ushort)pvr.Height);
			writer.Write(data.Source);

			if(pad)
			{
				writer.AlignFrom(32, start);
			}

			uint dataLength = writer.Position - dataStart;

			writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
			writer.WriteUInt(dataLength);
			writer.Stream.Seek(0, SeekOrigin.End);
		}


		public static bool IsPVR(EndianStackReader data, uint address)
		{
			try
			{
				return GetPVRFileFormat(data, address) != PVRFileFormat.None;
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		public static PVRFileFormat GetPVRFileFormat(EndianStackReader data, uint address)
		{
			if(CheckPVRT(data, address))
			{
				return PVRFileFormat.PVRT;
			}

			if(CheckGBIX(data, address))
			{
				return PVRFileFormat.GBIX;
			}

			return PVRFileFormat.None;
		}

		private static bool CheckPVRT(EndianStackReader data, uint address, uint? expectedLength = null)
		{
			try
			{
				if(!PVHeader.PVRT.ValidateData(data, address))
				{
					return false;
				}
			}
			catch(ArgumentOutOfRangeException)
			{
				if(expectedLength == null || data.ReadUInt(address + 4) != expectedLength)
				{
					throw;
				}
			}

			return true;
		}

		private static bool CheckGBIX(EndianStackReader data, uint address, uint? expectedLength = null)
		{
			if(!PVHeader.GBIX.ValidateData(data, address))
			{
				return false;
			}

			uint length = data.ReadUInt(address + 4) + 8;
			if(expectedLength != null)
			{
				expectedLength -= length;
			}

			return CheckPVRT(data, address + length, expectedLength);
		}
	}
}
