﻿using SA3D.Texturing;
using System;

namespace SA3D.Archival.Tex.GV.IO.PixelCodec
{
	internal class Intensity4PixelCodec : UncompressedPixelCodec
	{
		protected override ByteType Type => ByteType.TwoPixels;


		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte index = src[0];
			byte first = (byte)((index & 0xF0) | (index >> 4));
			byte second = (byte)((index & 0x0F) | (index << 4));

			dst[0] = first;
			dst[1] = first;
			dst[2] = first;
			dst[3] = 0xFF;

			dst[4] = second;
			dst[5] = second;
			dst[6] = second;
			dst[7] = 0xFF;
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			byte first = TextureUtilities.GetLuminance(src);
			byte second = TextureUtilities.GetLuminance(src[4..]);

			dst[0] = (byte)((first & 0xF0) | (second >> 4));
		}
	}
}