using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common.IO;
using System;
using System.IO;

namespace SA3D.Archival.Tex.PV.IO
{
	internal static class PVPIO
	{
		public static PVP ReadPVP(EndianStackReader data, uint address)
		{
			if(!PVHeader.PVPL.ValidateData(data, address))
			{
				throw new InvalidDataException("Data is not a PVR Palette!");
			}

			PVRPixelFormat pixelFormat = (PVRPixelFormat)data[address + 0x8];
			ushort bank = data.ReadUShort(address + 0xA);
			ushort entry = data.ReadUShort(address + 0xC);
			int width = data.ReadUShort(address + 0xE);

			byte[] result = new byte[4 * width];
			Span<byte> destination = result;
			ReadOnlySpan<byte> source = data.Slice(0x10);
			PVPixelCodec codec = PVPixelCodec.GetPixelCodec(pixelFormat);

			int size = width / codec.Pixels * codec.BytesPerPixel;
			int dstAddress = 0;
			for(int i = 0; i < size; i += codec.BytesPerPixel)
			{
				codec.DecodePixel(source.Slice(i, codec.BytesPerPixel), destination[dstAddress..]);
				dstAddress += 4 * codec.Pixels;
			}

			return new(string.Empty, result, pixelFormat, entry, bank);
		}

		public static void WritePVP(PVP pvp, EndianStackWriter writer)
		{
			PVHeader.PVPL.Write(writer);
			writer.WriteEmpty(4);

			uint dataStart = writer.Position;

			writer.WriteByte((byte)pvp.PixelFormat);
			writer.WriteByte(0);
			writer.WriteUShort(pvp.BankOffset);
			writer.WriteUShort(pvp.EntryOffset);
			writer.WriteUShort((ushort)pvp.Width);

			PVPixelCodec codec = PVPixelCodec.GetPixelCodec(pvp.PixelFormat);
			ReadOnlySpan<byte> colorData = pvp.ColorData;
			Span<byte> destination = new byte[codec.BytesPerPixel];

			for(int i = 0; i < colorData.Length; i += 4 * codec.Pixels)
			{
				codec.EncodePixel(colorData[i..], destination);
				writer.Write(destination);
			}

			writer.Align(4);

			uint dataLength = writer.Position - dataStart;
			writer.Stream.Seek(dataStart - 4, SeekOrigin.Begin);
			writer.WriteUInt(dataLength);
			writer.Stream.Seek(0, SeekOrigin.End);
		}
	}
}
