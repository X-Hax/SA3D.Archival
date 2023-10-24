using SA3D.Archival.Tex.GV.IO.PaletteCodec;
using SA3D.Common.IO;
using System;
using System.IO;

namespace SA3D.Archival.Tex.GV.IO
{
	internal class GVPIO
	{
		public static GVP ReadGVP(EndianStackReader data, uint address)
		{
			if(!GVHeader.GVPL.ValidateData(data, address))
			{
				throw new InvalidDataException("Data is not a PVR Palette!");
			}

			data.PushBigEndian(true);
			GVPaletteFormat pixelFormat = (GVPaletteFormat)data[address + 0x9];
			ushort bank = data.ReadUShort(address + 0xA);
			ushort entry = data.ReadUShort(address + 0xC);
			int width = data.ReadUShort(address + 0xE);
			data.PopEndian();

			GVPaletteCodec codec = GVPaletteCodec.GetPaletteCodec(pixelFormat);
			ReadOnlySpan<byte> source = data.Slice(0x10, width * codec.BytesPerPixel);

			byte[] result = codec.Decode(source);

			return new(string.Empty, result, pixelFormat, entry, bank);
		}

		public static void WriteGVP(GVP gvp, EndianStackWriter writer)
		{
			writer.PushBigEndian(false);
			GVHeader.GVPL.Write(writer);
			writer.WriteEmpty(4);

			uint dataStart = writer.Position;

			writer.PushBigEndian(true);
			writer.WriteByte(0);
			writer.WriteByte((byte)gvp.PaletteFormat);
			writer.WriteUShort(gvp.BankOffset);
			writer.WriteUShort(gvp.EntryOffset);
			writer.WriteUShort((ushort)gvp.Width);
			writer.PopEndian();

			GVPaletteCodec codec = GVPaletteCodec.GetPaletteCodec(gvp.PaletteFormat);
			writer.Write(codec.Encode(gvp.ColorData));

			writer.Align(4);

			uint dataLength = writer.Position - dataStart;
			writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
			writer.WriteUInt(dataLength);
			writer.Stream.Seek(0, SeekOrigin.End);
			writer.PopEndian();
		}
	}
}
