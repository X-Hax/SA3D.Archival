using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Archival.Textures.CommonV.IO
{
	internal class VCodec
	{
		public static byte[] MergeByteArrays(byte[][] arrays)
		{
			byte[] result = new byte[arrays.Sum(x => x.Length)];

			int resultOffset = 0;
			foreach(byte[] resultData in arrays)
			{
				resultData.CopyTo(result, resultOffset);
				resultOffset += resultData.Length;
			}

			return result;
		}

		public static void CorrectTransparency(Span<byte> data, int transparencyBits)
		{
			if(transparencyBits == 0)
			{
				for(int i = 3; i < data.Length; i += 4)
				{
					data[i] = 0xFF;
				}
			}
			else
			{
				float roundFactor = transparencyBits / 255f;
				float reverse = 255f / transparencyBits;

				for(int i = 3; i < data.Length; i += 4)
				{
					data[i] = (byte)(float.Round(data[i] * roundFactor) * reverse);
				}
			}
		}


		public delegate void CodecCallback(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination);

		public delegate int CalculateTextureSize(int width, int height);

		public static ReadOnlyMipMapSet Decode(
			CodecCallback decodeTexture,
			CalculateTextureSize calculateTextureSize,
			ReadOnlySpan<byte> data,
			int width,
			int height,
			MipMapType mipMapType,
			TextureType outputType)
		{
			MipMapSet result = new(width, height, outputType, mipMapType == MipMapType.None);
			IEnumerable<MipMapLevel> mipMaps = mipMapType == MipMapType.Ascending ? result.Reverse<MipMapLevel>() : result;

			foreach(MipMapLevel mipMap in mipMaps)
			{
				decodeTexture(data, mipMap.Width, mipMap.Height, mipMap.Data);
				data = data[calculateTextureSize(mipMap.Width, mipMap.Height)..];
			}

			return ReadOnlyMipMapSet.Wrap(result);
		}


		public static byte[] Encode(
			CodecCallback encodeTexture,
			CalculateTextureSize calculateTextureSize,
			IMipMapSet data,
			MipMapType mipMapType)
		{
			byte[][] result = new byte[data.LevelCount][];

			for(int i = 0; i < data.LevelCount; i++)
			{
				IMipMapLevel mipmap = data[i];
				byte[] mipMapResult = new byte[calculateTextureSize(mipmap.Width, mipmap.Height)];
				encodeTexture(mipmap.Data, mipmap.Width, mipmap.Height, mipMapResult);
				result[i] = mipMapResult;
			}

			if(mipMapType == MipMapType.Ascending)
			{
				Array.Reverse(result);
			}

			return MergeByteArrays(result);
		}
	}
}
