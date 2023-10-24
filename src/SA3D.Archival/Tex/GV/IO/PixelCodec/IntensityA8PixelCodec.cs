﻿using SA3D.Texturing;
using System;

namespace SA3D.Archival.Tex.GV.IO.PixelCodec
{
	internal class IntensityA8PixelCodec : UncompressedPixelCodec
	{
		protected override ByteType Type => ByteType.HalfPixel;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte intensity = src[1];
			byte alpha = src[0];

			dst[0] = intensity;
			dst[1] = intensity;
			dst[2] = intensity;
			dst[3] = alpha;
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[3];
			dst[1] = TextureUtilities.GetLuminance(src);
		}
	}
}
