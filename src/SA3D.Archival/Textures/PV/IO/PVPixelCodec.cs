using SA3D.Archival.Textures.PV.IO.PixelCodec;
using System;
using System.Collections.Generic;

namespace SA3D.Archival.Textures.PV.IO
{
	internal abstract class PVPixelCodec
	{
		private static readonly Dictionary<PVPixelFormat, PVPixelCodec> _codecs = new()
		{
			{ PVPixelFormat.ARGB1555, new ARGB1555PixelCodec() },
			{ PVPixelFormat.RGB565, new RGB565PixelCodec() },
			{ PVPixelFormat.ARGB4, new ARGB4PixelCodec() },
			{ PVPixelFormat.YUV422, new YUV422PixelCodec() },
			{ PVPixelFormat.Bump, new BumpPixelCodec() },
			{ PVPixelFormat.ARGB8, new ARGB8PixelCodec() },
			{ PVPixelFormat.RGB24, new RGB24PixelCodec() },
		};

		public abstract PVPixelFormat Format { get; }

		public abstract int BytesPerPixelPair { get; }

		public virtual int PixelPairSize => 1;

		public virtual int TransparencyBits => 0;


		public abstract void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst);

		public abstract void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst);

		public static PVPixelCodec GetPixelCodec(PVPixelFormat pixelFormat)
		{
			if(_codecs.TryGetValue(pixelFormat, out PVPixelCodec? result))
			{
				return result;
			}

			throw new NotImplementedException($"Pixel format \"{pixelFormat}\" is not implemented");
		}
	}
}
