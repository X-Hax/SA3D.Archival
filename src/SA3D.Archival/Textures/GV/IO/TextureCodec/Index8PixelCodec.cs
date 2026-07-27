using SA3D.Texturing;
using System;

namespace SA3D.Archival.Textures.GV.IO.TextureCodec
{
	internal class Index8PixelCodec : UncompressedPixelCodec
	{
		public override TextureType OutputType => TextureType.Index8;

		protected override ByteType Type => ByteType.Pixel;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[0];
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[0];
		}
	}
}
