using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Archival.Textures.GV.IO
{
	internal static class GVRCodec
	{
		public static ReadOnlyMipMapSet Decode(this GVTextureCodec pixelCodec, ReadOnlySpan<byte> data, int width, int height, bool hasMipMaps)
		{
			return VCodec.Decode(
				pixelCodec.DecodeTexture,
				pixelCodec.CalculateTextureSize,
				data,
				width,
				height,
				hasMipMaps ? MipMapType.Descending : MipMapType.None,
				pixelCodec.OutputType
			);
		}


		public static byte[] Encode(this GVTextureCodec codec, IMipMapSet data)
		{
			if(codec.OutputType != data.TextureType)
			{
				throw new InvalidOperationException($"Encoder expected {codec.OutputType} texture data, but received {data.TextureType}");
			}

			if(data.TextureType == TextureType.RGBA32 && codec.TransparencyBits < 8)
			{
				data = MipMapSet.Copy(data);
				foreach(MipMapLevel mipmap in (MipMapSet)data)
				{
					VCodec.CorrectTransparency(mipmap.Data, codec.TransparencyBits);
				}
			}

			return VCodec.Encode(
				codec.EncodeTexture,
				codec.CalculateTextureSize,
				data,
				data.LevelCount == 1 ? MipMapType.None : MipMapType.Descending
			);
		}

	}
}
