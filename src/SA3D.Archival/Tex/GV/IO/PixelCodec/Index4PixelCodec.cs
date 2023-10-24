using System;

namespace SA3D.Archival.Tex.GV.IO.PixelCodec
{
	internal class Index4PixelCodec : UncompressedPixelCodec
	{
		public override int PaletteEntries => 16;
		protected override ByteType Type => ByteType.TwoPixels;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte index = src[0];
			dst[0] = (byte)((index & 0xF0) | (index >> 4));
			dst[1] = (byte)((index & 0x0F) | (index << 4));
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = (byte)((src[0] & 0xF0) | (src[1] >> 4));
		}
	}
}
