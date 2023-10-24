using System;

namespace SA3D.Archival.Tex.PV.IO.PixelCodec
{
	internal class RGB565PixelCodec : PVPixelCodec
	{
		public override int BytesPerPixel => 2;

		public override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte r = (byte)(src[1] & 0xF8);
			dst[0] = (byte)(r | (r >> 5));

			byte g = (byte)((src[1] << 5) | ((src[0] & 0xE0) >> 3));
			dst[1] = (byte)(g | (g >> 6));

			byte b = (byte)(src[0] << 3);
			dst[2] = (byte)(b | (b >> 5));

			dst[3] = 0xFF;
		}

		public override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = (byte)(((src[1] & 0x1C) << 3) | ((src[2] & 0xF8) >> 3));
			dst[1] = (byte)((src[0] & 0xF8) | ((src[1] & 0xE0) >> 5));
		}
	}
}
