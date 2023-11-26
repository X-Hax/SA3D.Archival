﻿using SA3D.Archival.Tex.PV.IO.DataCodec;
using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common.IO;
using SA3D.Texturing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Tex.PV.IO
{
	internal static class PVRCodec
	{
		public static byte[] Decode(PVR pvr)
		{
			PVPixelCodec pixelCodec = PVPixelCodec.GetPixelCodec(pvr.PixelFormat);
			PVRDataCodec dataCodec = PVRDataCodec.Create(pvr.DataFormat, pixelCodec);
			ReadOnlySpan<byte> palette = DecodeInternalPalette(pvr, dataCodec, out _);

			int textureAddress = pvr.Data.Length - dataCodec.CalculateTextureSize(pvr.Width, pvr.Height);
			ReadOnlySpan<byte> textureData = pvr.Data[textureAddress..];
			return dataCodec.Decode(textureData, pvr.Width, pvr.Height, palette);
		}

		public static byte[] DecodeMipmap(PVR pvr, int mipmapIndex)
		{
			PVPixelCodec pixelCodec = PVPixelCodec.GetPixelCodec(pvr.PixelFormat);
			PVRDataCodec dataCodec = PVRDataCodec.Create(pvr.DataFormat, pixelCodec);
			ReadOnlySpan<byte> palette = DecodeInternalPalette(pvr, dataCodec, out int paletteSize);

			ReadOnlySpan<byte> data = pvr.Data[paletteSize..];
			int offset = paletteSize + dataCodec.CalculateTextureSize(pvr.Width, pvr.Width);
			int size = pvr.Width >> 1;
			for(int i = 0; i < mipmapIndex; i++, size >>= 1)
			{
				offset += dataCodec.CalculateTextureSize(size, size);
			}

			return dataCodec.Decode(data, size, size, palette);
		}

		private static ReadOnlySpan<byte> DecodeInternalPalette(PVR pvr, PVRDataCodec dataCodec, out int bytesRead)
		{
			Span<byte> result = null;
			int paletteEntries = dataCodec.GetPaletteEntries(pvr.Width);
			bytesRead = 0;

			if(paletteEntries > 0 && !dataCodec.NeedsExternalPalette)
			{
				PVPixelCodec pixelCodec = dataCodec.PixelCodec;

				int srcAddress = 0;
				result = new byte[paletteEntries * 4];

				for(int i = 0; i < paletteEntries; i += pixelCodec.Pixels)
				{
					pixelCodec.DecodePixel(pvr.Data[srcAddress..], result[(i * 4)..]);
					srcAddress += pixelCodec.BytesPerPixel;
				}

				bytesRead = srcAddress;
			}

			return result;
		}



		public static byte[] Encode(Texture texture, PVRPixelFormat pixelFormat, PVRDataFormat dataFormat, out TexturePalette? palette, bool ditherIndex = true)
		{
			using MemoryStream stream = new();
			EndianStackWriter writer = new(stream);

			PVPixelCodec pixelCodec = PVPixelCodec.GetPixelCodec(pixelFormat);
			PVRDataCodec dataCodec = PVRDataCodec.Create(dataFormat, pixelCodec);

			if(!dataCodec.CheckDimensionsValid(texture.Width, texture.Height))
			{
				throw new InvalidOperationException($"The dimensions ({texture.Width}x{texture.Height}) of the specified image are not valid for data format {dataFormat}.");
			}

			palette = null;

			if(dataCodec is VqDataCodec)
			{
				EncodeVQ(texture, dataCodec, writer);
			}
			else if(dataCodec.GetPaletteEntries(texture.Width) == 0)
			{
				EncodeColored(texture, dataCodec, writer);
			}
			else
			{
				EncodeIndexed(texture, dataCodec, ditherIndex, writer, out palette);
			}

			return stream.ToArray();
		}

		private static void EncodeIndexed(Texture texture, PVRDataCodec dataCodec, bool dither, EndianStackWriter writer, out TexturePalette palette)
		{
			bool index4 = dataCodec is Index4DataCodec or Index4MipmapsDataCodec;

			int indexRange = dataCodec.GetPaletteEntries(texture.Width);
			if(texture is not IndexTexture indexTexture)
			{
				indexTexture = CalculateLossy(texture, dataCodec.PixelCodec).Palettize(index4, dither);
			}

			palette = indexTexture.Palette ?? TexturePalette.GetDefaultPalette(index4);

			if(!dataCodec.NeedsExternalPalette)
			{
				EncodePalette(palette, indexRange, dataCodec.PixelCodec, writer);
			}

			if(dataCodec.HasMipmaps)
			{
				PaletteQuantizer quantizer = palette.CreatePaletteQuantizer(indexRange, indexRange * indexTexture.PaletteRow, dither);
				EncodeMipMaps(indexTexture.ToImageSharp(), quantizer.CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default), dataCodec, writer);
			}

			writer.Write(dataCodec.Encode(indexTexture.Data, texture.Width, texture.Height));
		}

		private static void EncodeColored(Texture texture, PVRDataCodec dataCodec, EndianStackWriter writer)
		{
			if(dataCodec.HasMipmaps)
			{
				Image<Rgba32> image = texture.ToImageSharp();
				EncodeMipMaps(image, null, dataCodec, writer);
			}

			byte[] mainImageData = dataCodec.Encode(texture.Data, texture.Width, texture.Height);
			writer.Write(mainImageData);
		}


		private static ColorTexture CalculateLossy(Texture texture, PVPixelCodec pixelCodec)
		{
			PVRDataCodec dataCodec = PVRDataCodec.Create(PVRDataFormat.Rectangle, pixelCodec);
			byte[] encoded = dataCodec.Encode(texture.GetColorPixels(), texture.Width, texture.Height);
			byte[] decoded = dataCodec.Decode(encoded, texture.Width, texture.Height, null);
			return new(texture.Width, texture.Height, decoded);
		}

		private static void EncodePalette(TexturePalette palette, int paletteEntries, PVPixelCodec pixelCodec, EndianStackWriter writer)
		{
			ReadOnlySpan<byte> paletteColors = palette.ColorData;

			Span<byte> destination = new byte[pixelCodec.BytesPerPixel * paletteEntries / pixelCodec.Pixels];
			for(int i = 0; i < paletteEntries; i += pixelCodec.Pixels)
			{
				pixelCodec.EncodePixel(paletteColors[(i * 4)..], destination[(i * pixelCodec.BytesPerPixel)..]);
			}

			writer.Write(destination);
		}

		private static void EncodeMipMaps(Image<Rgba32> image, IQuantizer<Rgba32>? quantizer, PVRDataCodec dataCodec, EndianStackWriter writer)
		{
			for(int size = 1; size < image.Width; size <<= 1)
			{
				Image<Rgba32> mipMapImage = image.Clone();
				mipMapImage.Mutate(x => x.Resize(size, size));

				byte[] mipMapPixels;
				if(quantizer != null)
				{
					IndexedImageFrame<Rgba32> mipMapFrame = quantizer.QuantizeFrame(mipMapImage.Frames[0], new(0, 0, size, size));

					mipMapPixels = new byte[size * size];
					Span<byte> pixelData = mipMapPixels;

					for(int y = 0; y < size; y++)
					{
						mipMapFrame.DangerousGetRowSpan(y).CopyTo(pixelData[(y * size)..]);
					}
				}
				else
				{
					mipMapPixels = new byte[size * size * 4];
					mipMapImage.CopyPixelDataTo(mipMapPixels);
				}

				writer.Write(dataCodec.Encode(mipMapPixels, size, size));
			}
		}

		private static void EncodeVQ(Texture texture, PVRDataCodec dataCodec, EndianStackWriter writer)
		{
			if(!dataCodec.CheckDimensionsValid(texture.Width, texture.Height))
			{
				throw new InvalidDataException($"Resolution ({texture.Width}x{texture.Height}) of texture {texture.Name} not valid!");
			}

			PVPixelCodec pixelCodec = dataCodec.PixelCodec;
			if(pixelCodec is not ARGB8PixelCodec)
			{
				texture = CalculateLossy(texture, pixelCodec);
			}

			int evalDataLength = texture.Width * texture.Height * 4;

			int textureCount = 1;
			if(dataCodec.HasMipmaps)
			{
				textureCount = (int)Math.Log(texture.Width, 2);

				int curLength = evalDataLength >> 2;
				for(int i = textureCount - 1; i > 0; i--, curLength >>= 2)
				{
					evalDataLength += curLength;
				}
			}

			Span<byte> evalData = new byte[evalDataLength];
			Image<Rgba32> image = texture.ToImageSharp();
			int destinationAddress = 0;

			for(int i = 0; i < textureCount; i++)
			{
				Span<byte> imageBuffer = new byte[image.Width * image.Height * 4];
				image.CopyPixelDataTo(imageBuffer);

				// next we remap it so that a VQ superpixel is a 16 byte row
				int rowSize = image.Width * 4;
				for(int y = 0; y < image.Height; y += 2)
				{
					for(int x = 0; x < image.Width; x += 2)
					{
						int addr = (x * 4) + (y * rowSize);
						imageBuffer.Slice(addr, 4).CopyTo(evalData[destinationAddress..]);
						imageBuffer.Slice(addr + rowSize, 4).CopyTo(evalData[(destinationAddress + 4)..]);
						imageBuffer.Slice(addr + 4, 4).CopyTo(evalData[(destinationAddress + 8)..]);
						imageBuffer.Slice(addr + 4 + rowSize, 4).CopyTo(evalData[(destinationAddress + 12)..]);
						destinationAddress += 16;
					}
				}

				image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2));
			}

			// now quantize the data
			int superPixelCount = dataCodec.GetPaletteEntries(texture.Width) / 4;
			(int[] indices, byte[] clusters) = VectorQuantization.QuantizeByteData(evalData, 16, superPixelCount);

			// the palette
			uint paletteSize = (uint)(dataCodec.GetPaletteEntries(texture.Width) / pixelCodec.Pixels * pixelCodec.BytesPerPixel);

			Span<byte> palette = new byte[paletteSize];
			int paletteIndex = 0;
			for(int i = 0; i < clusters.Length; i += 4 * pixelCodec.Pixels)
			{
				pixelCodec.EncodePixel(clusters[i..], palette[paletteIndex..]);
				paletteIndex += pixelCodec.BytesPerPixel;
			}

			writer.Write(palette);

			// The indices are basically our texture, so just write those.
			// But if we have mipmaps, then we need to write it in "reverse"

			Span<byte> textures = indices.Select(x => (byte)x).ToArray();

			if(textureCount > 1)
			{
				int mipmapAddr = textures.Length - 1;

				// the 1x1 mipmap is essentially just a 2x2 mipmap, which is at the last pixel
				writer.Write(dataCodec.Encode(textures[mipmapAddr..], 1, 1));

				int size = 2;
				for(; mipmapAddr > 0; mipmapAddr -= size * size, size <<= 1)
				{
					writer.Write(dataCodec.Encode(textures[mipmapAddr..], size, size));
				}
			}

			writer.Write(dataCodec.Encode(textures, texture.Width, texture.Height));
		}
	}
}
