﻿using SA3D.Texturing;
using System;

namespace SA3D.Archival.Tex.GV.IO.PixelCodec
{
	internal class Intensity8PixelCodec : UncompressedPixelCodec
	{
		protected override ByteType Type => ByteType.Pixel;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte value = src[0];

			dst[0] = value;
			dst[1] = value;
			dst[2] = value;
			dst[3] = 0xFF;
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = TextureUtilities.GetLuminance(src);
		}
	}
}
