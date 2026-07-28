using SA3D.Texturing;
using System;

namespace SA3D.Archival.Textures.GV.IO.TextureCodec
{
	internal class IntensityA4PixelCodec : UncompressedPixelCodec
	{
		protected override ByteType Type => ByteType.Pixel;

		public override int TransparencyBits => 8;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte value = src[0];

			byte intensity = (byte)((value & 0x0F) | (value << 4));
			byte alpha = (byte)((value & 0xF0) | (value >> 4));

			dst[0] = intensity;
			dst[1] = intensity;
			dst[2] = intensity;
			dst[3] = alpha;
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte intensity = TextureUtilities.GetLuminance(src);
			byte alpha = src[3];

			dst[0] = (byte)((intensity >> 4) | (alpha & 0xF0));
		}
	}
}
