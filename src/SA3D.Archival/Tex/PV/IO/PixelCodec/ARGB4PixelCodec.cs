using System;

namespace SA3D.Archival.Tex.PV.IO.PixelCodec
{
	internal class ARGB4PixelCodec : PVPixelCodec
	{
		public override int BytesPerPixel => 2;

		public override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{

			byte r = (byte)(src[1] & 0x0F);
			dst[0] = (byte)(r | (r << 4));

			byte g = (byte)(src[0] & 0xF0);
			dst[1] = (byte)(g | (g >> 4));

			byte b = (byte)(src[0] & 0x0F);
			dst[2] = (byte)(b | (b << 4));

			byte a = (byte)(src[1] & 0xF0);
			dst[3] = (byte)(a | (a >> 4));
		}

		public override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[1] = (byte)((src[3] & 0xF0) | (src[0] >> 4));
			dst[0] = (byte)((src[1] & 0xF0) | (src[2] >> 4));
		}
	}
}
