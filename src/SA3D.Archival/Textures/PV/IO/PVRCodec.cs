using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.PV.IO.DataCodec;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Archival.Textures.PV.IO
{
	internal static class PVRCodec
	{
		public static ReadOnlyMipMapSet Decode(this PVRDataCodec dataCodec, ReadOnlySpan<byte> data, int width, int height)
		{
			if(!dataCodec.CheckDimensionsValid(width, height))
			{
				throw new InvalidOperationException($"Invalid texture dimensions {width}x{height}!");
			}

			byte[]? palette = dataCodec.DecodeInternalPalette(data, width, out int textureDataOffset);

			return VCodec.Decode(
				(src, w, h, dst) => dataCodec.DecodeTexture(src, w, h, palette, dst),
				dataCodec.CalculateTextureSize,
				data[textureDataOffset..],
				width,
				height,
				dataCodec.HasMipmaps ? MipMapType.Ascending : MipMapType.None,
				dataCodec.OutputType
			);
		}

		private static byte[]? DecodeInternalPalette(this PVRDataCodec dataCodec, ReadOnlySpan<byte> data, int width, out int bytesRead)
		{
			byte[]? result = null;
			int paletteEntries = dataCodec.GetPaletteEntries(width);
			bytesRead = 0;

			if(paletteEntries > 0 && dataCodec.OutputType == TextureType.RGBA32)
			{
				PVPixelCodec pixelCodec = dataCodec.PixelCodec;

				int srcAddress = 0;
				result = new byte[paletteEntries * 4];
				Span<byte> destination = result;


				for(int i = 0; i < paletteEntries; i += pixelCodec.PixelPairSize)
				{
					pixelCodec.DecodePixel(data[srcAddress..], destination[(i * 4)..]);
					srcAddress += pixelCodec.BytesPerPixelPair;
				}

				bytesRead = srcAddress;
			}

			return result;
		}


		public static byte[] Encode(this PVRDataCodec dataCodec, IMipMapSet data)
		{
			if(dataCodec.OutputType != data.TextureType)
			{
				throw new InvalidOperationException($"Encoder expected {dataCodec.OutputType} texture data, but received {data.TextureType}");
			}
			else if(dataCodec.HasMipmaps && data.NoMipMaps)
			{
				throw new InvalidOperationException($"Encoder expect multiple mip map levels, but received only one");
			}
			else if(!dataCodec.HasMipmaps && data.LevelCount > 1)
			{
				throw new InvalidOperationException($"Encoder expected only one mip map level, but received multiple");
			}
			else if(!dataCodec.CheckDimensionsValid(data.BaseWidth, data.BaseHeight))
			{
				throw new InvalidOperationException($"Invalid texture dimensions ({data.BaseWidth}x{data.BaseHeight})!");
			}

			if(data.TextureType == TextureType.RGBA32 && dataCodec.PixelCodec.TransparencyBits < 8)
			{
				data = MipMapSet.Copy(data);
				foreach(MipMapLevel mipmap in (MipMapSet)data)
				{
					VCodec.CorrectTransparency(mipmap.Data, dataCodec.PixelCodec.TransparencyBits);
				}
			}

			if(dataCodec is VqDataCodec vqDataCodec)
			{
				return vqDataCodec.Encode(data);
			}
			else
			{
				return VCodec.Encode(
					dataCodec.EncodeTexture,
					dataCodec.CalculateTextureSize,
					data,
					dataCodec.HasMipmaps ? MipMapType.Ascending : MipMapType.None
				);
			}
		}
	}
}
