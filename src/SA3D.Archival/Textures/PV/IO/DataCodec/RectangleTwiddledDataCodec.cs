using System;

namespace SA3D.Archival.Textures.PV.IO.DataCodec
{
	internal class RectangleTwiddledDataCodec : RectangleDataCodec
	{
		public override int CalculateTextureSize(int width, int height)
		{
			return width / PixelCodec.PixelPairSize * height * PixelCodec.BytesPerPixelPair;
		}

		public RectangleTwiddledDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination)
		{
			int size = Math.Min(width, height);
			TwiddleMap twiddleMap = new(width);
			int sourceBlockIndex = 0;

			if(PixelCodec.PixelPairSize > 1)
			{
				Span<byte> pixelBuffer = new byte[PixelCodec.BytesPerPixelPair];
				int pixelPart = PixelCodec.BytesPerPixelPair / PixelCodec.PixelPairSize;

				for(int yStart = 0; yStart < height; yStart += size)
				{
					for(int xStart = 0; xStart < width; xStart += size)
					{
						for(int y = 0; y < size; y++)
						{
							for(int x = 0; x < size; x += PixelCodec.PixelPairSize)
							{
								for(int px = 0; px < PixelCodec.PixelPairSize; px++)
								{
									int srcAddress = sourceBlockIndex + (twiddleMap[x + px, y] * pixelPart);
									source.Slice(srcAddress, pixelPart).CopyTo(pixelBuffer[(px * pixelPart)..]);
								}

								int destinationIndex = (((yStart + y) * width) + xStart + x) * 4;

								PixelCodec.DecodePixel(pixelBuffer, destination[destinationIndex..]);
							}
						}

						sourceBlockIndex += size * size * PixelCodec.BytesPerPixelPair;
					}
				}
			}
			else
			{
				for(int yStart = 0; yStart < height; yStart += size)
				{
					for(int xStart = 0; xStart < width; xStart += size)
					{
						for(int y = 0; y < size; y++)
						{
							for(int x = 0; x < size; x++)
							{
								int sourceIndex = sourceBlockIndex + (twiddleMap[x, y] * PixelCodec.BytesPerPixelPair);
								int destinationIndex = (((yStart + y) * width) + xStart + x) * 4;

								PixelCodec.DecodePixel(source[sourceIndex..], destination[destinationIndex..]);
							}
						}

						sourceBlockIndex += size * size * PixelCodec.BytesPerPixelPair;
					}
				}
			}
		}

		public override void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination)
		{
			int size = Math.Min(width, height);
			TwiddleMap twiddleMap = new(width);
			int destinationBlockIndex = 0;

			if(PixelCodec.PixelPairSize > 1)
			{
				Span<byte> pixelBuffer = new byte[PixelCodec.BytesPerPixelPair];
				int pixelPart = PixelCodec.BytesPerPixelPair / PixelCodec.PixelPairSize;

				for(int yStart = 0; yStart < height; yStart += size)
				{
					for(int xStart = 0; xStart < width; xStart += size)
					{
						for(int y = 0; y < size; y++)
						{
							for(int x = 0; x < size; x += PixelCodec.PixelPairSize)
							{
								int sourceIndex = (((y + yStart) * width) + xStart + x) * 4;
								PixelCodec.EncodePixel(source[sourceIndex..], pixelBuffer);

								for(int px = 0; px < PixelCodec.PixelPairSize; px++)
								{
									int dstAddress = destinationBlockIndex + (twiddleMap[x + px, y] * pixelPart);
									pixelBuffer.Slice(px * pixelPart, pixelPart).CopyTo(destination[dstAddress..]);
								}
							}
						}

						destinationBlockIndex += size * size * PixelCodec.BytesPerPixelPair;
					}
				}
			}
			else
			{
				for(int yStart = 0; yStart < height; yStart += size)
				{
					for(int xStart = 0; xStart < width; xStart += size)
					{
						for(int y = 0; y < size; y++)
						{
							for(int x = 0; x < size; x += PixelCodec.PixelPairSize)
							{
								int sourceIndex = (((y + xStart) * width) + xStart + x) * 4;
								int destinationIndex = destinationBlockIndex + (twiddleMap[x / PixelCodec.PixelPairSize, y] * PixelCodec.BytesPerPixelPair);

								PixelCodec.EncodePixel(source[sourceIndex..], destination[destinationIndex..]);
							}
						}

						destinationBlockIndex += size * PixelCodec.BytesPerPixelPair;
					}
				}
			}
		}
	}
}
