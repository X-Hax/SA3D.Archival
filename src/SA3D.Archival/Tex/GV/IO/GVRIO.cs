using SA3D.Archival.Tex.GV.IO.PaletteCodec;
using SA3D.Archival.Tex.GV.IO.PixelCodec;
using SA3D.Common.IO;
using System;
using System.IO;

namespace SA3D.Archival.Tex.GV.IO
{
	internal static class GVRIO
	{
		public static GVR ReadGVR(EndianStackReader data, uint address)
		{

			GVRFileFormat format;

			try
			{
				format = GetGVRFileFormat(data, address);
			}
			catch(Exception e)
			{
				throw new InvalidDataException("Data is not a GVR!", e);
			}

			if(format == GVRFileFormat.None)
			{
				throw new InvalidDataException("Data is not a GVR!");
			}

			uint globalIndex = 0;
			if(format == GVRFileFormat.GBIX)
			{
				data.PushBigEndian(true);
				globalIndex = data.ReadUInt(address + 8);
				data.PopEndian();

				address += data.ReadUInt(address + 4) + 8;
			}

			data.PushBigEndian(true);
			// ignoring datalength, as it may include padding that we do not want to include
			byte paletteFormatAndFlags = data[address + 0xA];
			GVRDataFlags flags = (GVRDataFlags)(paletteFormatAndFlags & 0xF);
			GVPaletteFormat paletteFormat = (GVPaletteFormat)(paletteFormatAndFlags >> 4);
			GVRPixelFormat pixelFormat = (GVRPixelFormat)data[address + 0xB];

			ushort width = data.ReadUShort(address + 0xC);
			ushort height = data.ReadUShort(address + 0xE);

			data.PopEndian();

			GVRPixelCodec pixelCodec = GVRPixelCodec.GetPixelCodec(pixelFormat);

			address += 0x10;
			int dataSize = pixelCodec.CalculateTextureSize(width, height);

			GVP? internalPalette = null;
			if(pixelCodec.PaletteEntries != 0 && flags.HasFlag(GVRDataFlags.InternalPalette))
			{
				GVPaletteCodec paletteCodec = GVPaletteCodec.GetPaletteCodec(paletteFormat);
				int paletteSize = paletteCodec.BytesPerPixel * pixelCodec.PaletteEntries;
				internalPalette = new(string.Empty, paletteCodec.Decode(data.Slice(address, paletteSize)), paletteFormat, 0, 0);
				address += (uint)paletteSize;
			}

			bool hasMipMaps = flags.HasFlag(GVRDataFlags.Mipmaps);
			if(hasMipMaps)
			{
				for(int size = width >> 1; size > 0; size >>= 1)
				{
					dataSize += pixelCodec.CalculateTextureSize(size, size);
				}
			}

			byte[] gvrData = data.Slice(address, dataSize).ToArray();
			return new GVR(gvrData, globalIndex, width, height, pixelFormat, internalPalette, hasMipMaps, string.Empty);
		}

		public static void WriteGVR(GVR gvr, EndianStackWriter writer, bool includeGlobalIndex, bool pad)
		{
			writer.PushBigEndian(false);

			EndianStackReader data = gvr.CreateDataReader();
			uint start = writer.Position;

			if(includeGlobalIndex)
			{
				GVHeader.GBIX.Write(writer);
				writer.WriteUInt(8);

				writer.PushBigEndian(true);

				writer.WriteUInt(gvr.GlobalIndex);
				writer.WriteEmpty(4);

				writer.PopEndian();
			}

			GVHeader.GVRT.Write(writer);
			writer.WriteEmpty(4);

			uint dataStart = writer.Position;

			writer.PushBigEndian(true);

			writer.WriteEmpty(2);
			writer.WriteByte(AssembleDataFlagsAndPaletteFormat(
				gvr.MipMapCount > 0,
				gvr.InternalPalette?.PaletteFormat,
				gvr.RequiresPallet));
			writer.WriteByte((byte)gvr.PixelFormat);
			writer.WriteUShort((ushort)gvr.Width);
			writer.WriteUShort((ushort)gvr.Height);

			writer.PopEndian();

			if(gvr.InternalPalette != null)
			{
				GVPaletteCodec paletteCodec = GVPaletteCodec.GetPaletteCodec(gvr.InternalPalette.PaletteFormat);
				writer.Write(paletteCodec.Encode(gvr.InternalPalette.ColorData));
			}

			writer.Write(data.Source);

			if(pad)
			{
				writer.AlignFrom(32, start);
			}

			uint dataLength = writer.Position - dataStart;

			writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
			writer.WriteUInt(dataLength);
			writer.Stream.Seek(0, SeekOrigin.End);

			writer.PopEndian();
		}

		public static byte AssembleDataFlagsAndPaletteFormat(bool mipmaps, GVPaletteFormat? paletteFormat, bool externalPalette)
		{
			GVRDataFlags result = default;
			if(mipmaps)
			{
				result |= GVRDataFlags.Mipmaps;
			}

			if(paletteFormat != null)
			{
				result |= GVRDataFlags.InternalPalette;
			}
			else if(externalPalette)
			{
				result |= GVRDataFlags.ExternalPalette;
			}

			byte paletteFormatAndFlags = (byte)result;
			if(paletteFormat != null)
			{
				paletteFormatAndFlags |= (byte)(((byte)paletteFormat) << 4);
			}

			return paletteFormatAndFlags;
		}

		public static bool IsGVR(EndianStackReader data, uint address)
		{
			try
			{
				return GetGVRFileFormat(data, address) != GVRFileFormat.None;
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		public static GVRFileFormat GetGVRFileFormat(EndianStackReader data, uint address)
		{
			GVRFileFormat result = GVRFileFormat.None;
			data.PushBigEndian(false);
			try
			{
				if(CheckGVRT(data, address))
				{
					result = GVRFileFormat.GVRT;
				}
				else if(CheckGBIX(data, address))
				{
					result = GVRFileFormat.GBIX;
				}
			}
			finally
			{
				data.PopEndian();
			}

			return result;
		}

		private static bool CheckGVRT(EndianStackReader data, uint address, uint? expectedLength = null)
		{
			try
			{
				if(!GVHeader.GVRT.ValidateData(data, address))
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
			if(!GVHeader.GBIX.ValidateData(data, address) && !GVHeader.GCIX.ValidateData(data, address))
			{
				return false;
			}

			uint length = data.ReadUInt(address + 4) + 8;
			if(expectedLength != null)
			{
				expectedLength -= length;
			}

			return CheckGVRT(data, address + length, expectedLength);
		}
	}
}
