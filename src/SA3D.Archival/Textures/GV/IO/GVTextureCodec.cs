using SA3D.Archival.Textures.GV.IO.TextureCodec;
using SA3D.Texturing;
using System;
using System.Collections.Generic;

namespace SA3D.Archival.Textures.GV.IO
{
	internal abstract class GVTextureCodec
	{
		private static readonly Dictionary<GVTextureFormat, GVTextureCodec> _codecs = new()
		{
			{ GVTextureFormat.Intensity4, new Intensity4PixelCodec() },
			{ GVTextureFormat.Intensity8, new Intensity8PixelCodec() },
			{ GVTextureFormat.IntensityA4, new IntensityA4PixelCodec() },
			{ GVTextureFormat.IntensityA8, new IntensityA8PixelCodec() },
			{ GVTextureFormat.RGB565, new RGB565PixelCodec() },
			{ GVTextureFormat.RGB5A3, new RGB5A3PixelCodec() },
			{ GVTextureFormat.ARGB8, new ARGB8PixelCodec() },
			{ GVTextureFormat.Index4, new Index4PixelCodec() },
			{ GVTextureFormat.Index8, new Index8PixelCodec() },
			{ GVTextureFormat.DXT1, new DXT1PixelCodec() },
		};

		public static GVTextureCodec GetPixelCodec(GVTextureFormat pixelFormat)
		{
			if(_codecs.TryGetValue(pixelFormat, out GVTextureCodec? result))
			{
				return result;
			}

			throw new NotImplementedException($"Pixel format \"{pixelFormat}\" is not implemented");
		}


		public virtual TextureType OutputType => TextureType.RGBA32;

		public virtual int TransparencyBits => 0;


		public int CalculateTextureSize(int width, int height)
		{
			return Math.Max(32, InternalCalculateTextureSize(width, height));
		}

		protected abstract int InternalCalculateTextureSize(int width, int height);


		public abstract void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination);

		public abstract void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination);

	}
}
