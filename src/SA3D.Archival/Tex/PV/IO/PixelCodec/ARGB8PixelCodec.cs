using System;

namespace SA3D.Archival.Tex.PV.IO.PixelCodec
{
	internal class ARGB8PixelCodec : PVPixelCodec
	{
		public override int BytesPerPixel => 4;

		public override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[2];
			dst[1] = src[1];
			dst[2] = src[0];
			dst[3] = src[3];
		}

		public override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[2];
			dst[1] = src[1];
			dst[2] = src[0];
			dst[3] = src[3];
		}
	}
}
