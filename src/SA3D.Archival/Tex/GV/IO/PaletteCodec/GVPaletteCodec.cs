using System;
using System.Collections.Generic;

namespace SA3D.Archival.Tex.GV.IO.PaletteCodec
{
	internal abstract class GVPaletteCodec
	{
		private static readonly Dictionary<GVPaletteFormat, GVPaletteCodec> _codecs = new()
		{
			{ GVPaletteFormat.IntensityA8, new IntensityA8PaletteCodec() },
			{ GVPaletteFormat.Rgb565, new RGB565PaletteCodec() },
			{ GVPaletteFormat.Rgb5a3, new RGB5A3PaletteCodec() },
		};

		/// <summary>
		/// How many bytes a pixel encodes to
		/// </summary>
		public abstract int BytesPerPixel { get; }

		public byte[] Decode(ReadOnlySpan<byte> source)
		{
			int count = source.Length / BytesPerPixel;
			byte[] result = new byte[count * 4];
			Span<byte> destination = result;

			for(int i = 0; i < count; i++)
			{
				DecodePixel(source.Slice(i * BytesPerPixel, BytesPerPixel), destination.Slice(i * 4, 4));
			}

			return result;
		}

		protected abstract void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst);

		public byte[] Encode(ReadOnlySpan<byte> source)
		{
			int count = source.Length / 4;
			byte[] result = new byte[count * BytesPerPixel];
			Span<byte> destination = result;

			for(int i = 0; i < count; i++)
			{
				EncodePixel(source.Slice(i * 4, 4), destination.Slice(i * BytesPerPixel, BytesPerPixel));
			}

			return result;
		}

		protected abstract void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst);


		public static GVPaletteCodec GetPaletteCodec(GVPaletteFormat paletteFormat)
		{
			if(_codecs.TryGetValue(paletteFormat, out GVPaletteCodec? result))
			{
				return result;
			}

			throw new NotImplementedException($"Palette format \"{paletteFormat}\" is not implemented");
		}
	}
}
