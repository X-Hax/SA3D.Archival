using SA3D.Archival.Tex.GV.IO.PixelCodec;
using SA3D.Common.IO;
using SA3D.Texturing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System;
using System.IO;

namespace SA3D.Archival.Tex.GV.IO
{
	internal static class GVRCodec
	{
		public static byte[] Decode(GVR gvr)
		{
			return GVRPixelCodec.GetPixelCodec(gvr.PixelFormat).Decode(gvr.Data, gvr.Width, gvr.Height);
		}

		public static byte[] DecodeMipmap(GVR gvr, int mipmapIndex)
		{
			GVRPixelCodec pixelCodec = GVRPixelCodec.GetPixelCodec(gvr.PixelFormat);

			int offset = pixelCodec.CalculateTextureSize(gvr.Width, gvr.Width);
			int size = gvr.Width >> 1;
			for(int i = 0; i < mipmapIndex; i++, size >>= 1)
			{
				offset += pixelCodec.CalculateTextureSize(size, size);
			}

			ReadOnlySpan<byte> data = gvr.Data[offset..];

			return pixelCodec.Decode(data, size, size);
		}

		public static byte[] Encode(Texture texture, GVRPixelFormat pixelFormat, bool mipmaps, out GVP? palette, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			if(mipmaps && texture.Width != texture.Height)
			{
				throw new InvalidOperationException("Texture is not square, cannot encode with mipmaps");
			}

			using MemoryStream stream = new();
			EndianStackWriter writer = new(stream);

			GVRPixelCodec pixelCodec = GVRPixelCodec.GetPixelCodec(pixelFormat);
			palette = null;

			if(pixelCodec.PaletteEntries > 0)
			{
				bool index4 = pixelFormat == GVRPixelFormat.Index4;

				if(texture is not IndexTexture indexTex)
				{
					GVRPixelFormat correspondingPixelFormat = paletteFormat switch
					{
						GVPaletteFormat.IntensityA8 => GVRPixelFormat.IntensityA8,
						GVPaletteFormat.Rgb565 => GVRPixelFormat.RGB565,
						GVPaletteFormat.Rgb5a3 => throw new NotImplementedException(),
						_ => GVRPixelFormat.RGB5A3,
					};

					indexTex = CalculateLossy(texture, correspondingPixelFormat).Palettize(index4, dither);
				}

				palette = indexTex.Palette == null ? null : GVP.FromTexturePalette(indexTex.Palette, paletteFormat);
				writer.Write(pixelCodec.Encode(indexTex.Data, indexTex.Width, indexTex.Height));

				if(mipmaps)
				{
					TexturePalette quantizerPalette = palette ?? TexturePalette.GetDefaultPalette(index4);
					PaletteQuantizer quantizer = quantizerPalette.CreatePaletteQuantizer(pixelCodec.PaletteEntries, pixelCodec.PaletteEntries * indexTex.PaletteRow, dither);
					EncodeMipMaps(indexTex.ToImageSharp(), quantizer.CreatePixelSpecificQuantizer<Rgba32>(Configuration.Default), pixelCodec, writer);
				}
			}
			else
			{
				writer.Write(pixelCodec.Encode(texture.GetColorPixels(), texture.Width, texture.Height));

				if(mipmaps)
				{
					EncodeMipMaps(texture.ToImageSharp(), null, pixelCodec, writer);
				}
			}

			return stream.ToArray();
		}

		private static ColorTexture CalculateLossy(Texture texture, GVRPixelFormat format)
		{
			GVRPixelCodec compressCodec = GVRPixelCodec.GetPixelCodec(format);
			byte[] encoded = compressCodec.Encode(texture.GetColorPixels(), texture.Width, texture.Height);
			byte[] decoded = compressCodec.Decode(encoded, texture.Width, texture.Height);
			return new ColorTexture(texture.Width, texture.Height, decoded);
		}

		private static void EncodeMipMaps(Image<Rgba32> image, IQuantizer<Rgba32>? quantizer, GVRPixelCodec pixelCodec, EndianStackWriter writer)
		{
			for(int size = image.Width >> 1; size > 0; size >>= 1)
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
				}

				writer.Write(pixelCodec.Encode(mipMapPixels, size, size));
			}
		}
	}
}
