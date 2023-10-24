using System;
using System.Collections.Generic;

namespace SA3D.Archival.Tex.PV.IO.PixelCodec
{
	internal abstract class PVPixelCodec
	{
		private static readonly Dictionary<PVRPixelFormat, PVPixelCodec> _codecs = new()
		{
			{ PVRPixelFormat.ARGB1555, new ARGB1555PixelCodec() },
			{ PVRPixelFormat.RGB565, new RGB565PixelCodec() },
			{ PVRPixelFormat.ARGB4, new ARGB4PixelCodec() },
			{ PVRPixelFormat.YUV422, new YUV422PixelCodec() },
			{ PVRPixelFormat.Bump, new BumpPixelCodec() },
			{ PVRPixelFormat.ARGB8, new ARGB8PixelCodec() },
		};

		public abstract int BytesPerPixel { get; }

		public virtual int Pixels => 1;

		public abstract void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst);

		public abstract void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst);

		public static PVPixelCodec GetPixelCodec(PVRPixelFormat pixelFormat)
		{
			if(_codecs.TryGetValue(pixelFormat, out PVPixelCodec? result))
			{
				return result;
			}

			throw new NotImplementedException($"Pixel format \"{pixelFormat}\" is not implemented");
		}
	}
}
