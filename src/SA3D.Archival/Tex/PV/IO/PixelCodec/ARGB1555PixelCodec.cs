using System;

namespace SA3D.Archival.Tex.PV.IO.PixelCodec
{
	internal class ARGB1555PixelCodec : PVPixelCodec
	{
		public override int BytesPerPixel => 2;

		public override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte r = (byte)((src[1] & 0x7C) << 1);
			dst[0] = (byte)(r | (r >> 5));

			byte g = (byte)((src[1] << 6) | ((src[0] & 0xE0) >> 2));
			dst[1] = (byte)(g | (g >> 5));

			byte b = (byte)(src[0] << 3);
			dst[2] = (byte)(b | (b >> 5));

			dst[3] = (byte)((src[1] & 0x80) == 0x80 ? 0xFF : 0);
		}

		public override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[1] = (byte)((src[3] & 0x80) | ((src[0] & 0xF8) >> 1) | (src[1] >> 6));
			dst[0] = (byte)(((src[1] & 0xF8) << 2) | ((src[2] & 0xF8) >> 3));
		}
	}
}
