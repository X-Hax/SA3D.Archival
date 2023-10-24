using System;
using System.Collections.Generic;

namespace SA3D.Archival.Tex.GV.IO.PixelCodec
{
	internal abstract class GVRPixelCodec
	{
		private static readonly Dictionary<GVRPixelFormat, GVRPixelCodec> _codecs = new()
		{
			{ GVRPixelFormat.Intensity4, new Intensity4PixelCodec() },
			{ GVRPixelFormat.Intensity8, new Intensity8PixelCodec() },
			{ GVRPixelFormat.IntensityA4, new IntensityA4PixelCodec() },
			{ GVRPixelFormat.IntensityA8, new IntensityA8PixelCodec() },
			{ GVRPixelFormat.RGB565, new RGB565PixelCodec() },
			{ GVRPixelFormat.RGB5A3, new RGB5A3PixelCodec() },
			{ GVRPixelFormat.ARGB8, new ARGB8PixelCodec() },
			{ GVRPixelFormat.Index4, new Index4PixelCodec() },
			{ GVRPixelFormat.Index8, new Index8PixelCodec() },
			{ GVRPixelFormat.DXT1, new DXT1PixelCodec() },
		};

		/// <summary>
		/// Gets the maximum number of entries the palette allows for, or 0 if this pixel format doesn't use a palette.
		/// </summary>
		public virtual int PaletteEntries => 0;

		public int CalculateTextureSize(int width, int height)
		{
			return Math.Max(32, InternalCalculateTextureSize(width, height));
		}

		protected abstract int InternalCalculateTextureSize(int width, int height);

		public byte[] Decode(ReadOnlySpan<byte> src, int width, int height)
		{
			byte[] result = new byte[width * height * (PaletteEntries > 0 ? 1 : 4)];
			InternalDecode(src, width, height, result);
			return result;
		}

		protected abstract void InternalDecode(ReadOnlySpan<byte> src, int width, int height, Span<byte> dst);

		public byte[] Encode(ReadOnlySpan<byte> src, int width, int height)
		{
			byte[] result = new byte[CalculateTextureSize(width, height)];
			InternalEncode(src, width, height, result);
			return result;
		}

		protected abstract void InternalEncode(ReadOnlySpan<byte> src, int width, int height, Span<byte> dst);

		public static GVRPixelCodec GetPixelCodec(GVRPixelFormat pixelFormat)
		{
			if(_codecs.TryGetValue(pixelFormat, out GVRPixelCodec? result))
			{
				return result;
			}

			throw new NotImplementedException($"Pixel format \"{pixelFormat}\" is not implemented");
		}

	}
}
