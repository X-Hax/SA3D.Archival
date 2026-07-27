using SA3D.Texturing.MipMapping;
using System;
using System.Linq;

namespace SA3D.Archival.Textures.PV.IO.DataCodec
{
	internal class VqDataCodec : PVRDataCodec
	{
		public VqDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override int GetPaletteEntries(int? width)
		{
			return 1024; // 256 blocks
		}

		public override int CalculateTextureSize(int width, int height)
		{
			return Math.Max(width * height / 4, 1);
		}

		public override void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination)
		{
			TwiddleMap twiddleMap = new(width);

			for(int y = 0; y < height; y += 2)
			{
				for(int x = 0; x < width; x += 2)
				{
					int sourceIndex = twiddleMap[x / 2, y / 2];
					int paletteIndex = source[sourceIndex] * 4;

					for(int x2 = 0; x2 < 2; x2++)
					{
						for(int y2 = 0; y2 < 2; y2++)
						{
							int destinationIndex = (((y + y2) * width) + x + x2) * 4;
							palette.Slice(paletteIndex * 4, 4).CopyTo(destination[destinationIndex..]);
							paletteIndex++;
						}
					}
				}
			}
		}

		public override void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination)
		{
			TwiddleMap twiddleMap = new(width);

			width /= 2;
			height /= 2;

			int sourceIndex = 0;
			for(int y = 0; y < height; y++)
			{
				for(int x = 0; x < width; x++)
				{
					int destinationIndex = twiddleMap[x, y];
					destination[destinationIndex] = source[sourceIndex];
					sourceIndex++;
				}
			}

		}


		public byte[] Encode(IMipMapSet data)
		{
			Span<byte> evalData = new byte[data.Sum(x => Math.Max(4, x.Width * x.Height)) * 4];
			int destinationAddress = 0;

			foreach(IMipMapLevel mipMapLevel in data.Reverse())
			{
				ReadOnlySpan<byte> imageBuffer = mipMapLevel.Data;

				if(mipMapLevel.Width == 1 && mipMapLevel.Height == 1)
				{
					// i know this is not the best way to calculate the color of the 1x1 mipmap...
					// but i will not write a proper "find the superpixel with the most matching
					// bottom right color" algorithm until somebody personally approaches me and
					// complains - Justin113D

					Span<byte> lastLevel = new byte[2 * 2 * 4];

					imageBuffer.CopyTo(lastLevel);
					imageBuffer.CopyTo(lastLevel[4..]);
					imageBuffer.CopyTo(lastLevel[8..]);
					imageBuffer.CopyTo(lastLevel[12..]);

					imageBuffer = lastLevel;
				}

				// next we remap it so that a VQ superpixel is a 16 byte row
				int rowSize = mipMapLevel.Width * 4;
				for(int y = 0; y < mipMapLevel.Height; y += 2)
				{
					for(int x = 0; x < mipMapLevel.Width; x += 2)
					{
						int addr = (x * 4) + (y * rowSize);
						imageBuffer.Slice(addr, 4).CopyTo(evalData[destinationAddress..]);
						imageBuffer.Slice(addr + rowSize, 4).CopyTo(evalData[(destinationAddress + 4)..]);
						imageBuffer.Slice(addr + 4, 4).CopyTo(evalData[(destinationAddress + 8)..]);
						imageBuffer.Slice(addr + 4 + rowSize, 4).CopyTo(evalData[(destinationAddress + 12)..]);
						destinationAddress += 16;
					}
				}
			}


			// now quantize the data
			int superPixelLimit = GetPaletteEntries(data.BaseWidth) / 4;
			(byte[] indices, byte[] clusters) = VectorQuantization.QuantizeByteData<byte>(evalData, 16, superPixelLimit);

			// create the palette
			int pixelPairSize = PixelCodec.PixelPairSize;
			int bytesPerPixelPair = PixelCodec.BytesPerPixelPair;

			uint paletteSize = (uint)(GetPaletteEntries(data.BaseWidth) / pixelPairSize * bytesPerPixelPair);
			byte[] palette = new byte[paletteSize];

			Span<byte> paletteSpan = palette;
			Span<byte> clusterSpan = clusters;
			int paletteIndex = 0;
			for(int i = 0; i < clusters.Length; i += 4 * pixelPairSize)
			{
				PixelCodec.EncodePixel(clusterSpan[i..], paletteSpan[paletteIndex..]);
				paletteIndex += bytesPerPixelPair;
			}

			byte[] result = new byte[palette.Length + data.Sum(x => CalculateTextureSize(x.Width, x.Height))];
			palette.CopyTo(result);

			// The indices are basically our texture, so just encode those
			Span<byte> indicesSpan = indices;
			Span<byte> resultSpan = result.AsSpan()[palette.Length..];

			foreach(IMipMapLevel mipMapLevel in data.Reverse())
			{
				EncodeTexture(indicesSpan, mipMapLevel.Width, mipMapLevel.Height, resultSpan);

				int indicesSize = Math.Max(4, mipMapLevel.Width * mipMapLevel.Height) / 4;
				int resultSize = CalculateTextureSize(mipMapLevel.Width, mipMapLevel.Height);

				indicesSpan = indicesSpan[indicesSize..];
				resultSpan = resultSpan[resultSize..];
			}

			return result;
		}
	}
}
