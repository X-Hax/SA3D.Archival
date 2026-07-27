using SA3D.Common;
using System;

namespace SA3D.Archival.Textures.PV.IO.DataCodec
{
	internal class RectangleDataCodec : PVRDataCodec
	{
		public override int CalculateTextureSize(int width, int height)
		{
			return width / PixelCodec.PixelPairSize * height * PixelCodec.BytesPerPixelPair;
		}

		public override bool CheckDimensionsValid(int width, int height)
		{
			return width is >= 8 and <= 1024
				&& height is >= 8 and <= 1024
				&& MathHelper.IsPow2(width)
				&& MathHelper.IsPow2(height);
		}

		public RectangleDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination)
		{
			int srcAddress = 0;
			int dstAddress = 0;

			for(int y = 0; y < height; y++)
			{
				for(int x = 0; x < width; x += PixelCodec.PixelPairSize)
				{
					PixelCodec.DecodePixel(
						source[srcAddress..],
						destination[dstAddress..]);

					srcAddress += PixelCodec.BytesPerPixelPair;
					dstAddress += 4 * PixelCodec.PixelPairSize;
				}
			}
		}

		public override void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination)
		{
			int srcAddress = 0;
			int dstAddress = 0;

			for(int y = 0; y < height; y++)
			{
				for(int x = 0; x < width; x += PixelCodec.PixelPairSize)
				{
					PixelCodec.EncodePixel(
						source[srcAddress..],
						destination[dstAddress..]);

					srcAddress += 4 * PixelCodec.PixelPairSize;
					dstAddress += PixelCodec.BytesPerPixelPair;
				}
			}
		}
	}
}
