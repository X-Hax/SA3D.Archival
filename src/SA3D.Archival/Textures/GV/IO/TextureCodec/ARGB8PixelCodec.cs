using System;

namespace SA3D.Archival.Textures.GV.IO.TextureCodec
{
	internal class ARGB8PixelCodec : UncompressedPixelCodec
	{
		protected override ByteType Type => ByteType.QuarterPixel;

		public override int TransparencyBits => 8;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[1];
			dst[1] = src[32];
			dst[2] = src[33];
			dst[3] = src[0];
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[3];
			dst[1] = src[0];
			dst[32] = src[1];
			dst[33] = src[2];
		}
	}
}
