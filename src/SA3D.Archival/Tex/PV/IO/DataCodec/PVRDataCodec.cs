using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common;
using System;

namespace SA3D.Archival.Tex.PV.IO.DataCodec
{
	internal abstract class PVRDataCodec
	{
		public PVPixelCodec PixelCodec { get; }

		/// <summary>
		/// Gets if this data format requires an external palette file.
		/// </summary>
		public virtual bool NeedsExternalPalette => false;

		/// <summary>
		/// Gets if this data format has mipmaps.
		/// </summary>
		public virtual bool HasMipmaps => false;


		protected PVRDataCodec(PVPixelCodec pixelCodec)
		{
			PixelCodec = pixelCodec;
		}

		public static PVRDataCodec Create(PVRDataFormat format, PVPixelCodec pixelCodec)
		{
			return format switch
			{
				PVRDataFormat.SquareTwiddled => new SquareTwiddledDataCodec(pixelCodec),
				PVRDataFormat.SquareTwiddledMipmaps => new SquareTwiddledMipmapsDataCodec(pixelCodec),
				PVRDataFormat.Vq => new VqDataCodec(pixelCodec),
				PVRDataFormat.VqMipmaps => new VqMipmapsDataCodec(pixelCodec),
				PVRDataFormat.Index4 => new Index4DataCodec(pixelCodec),
				PVRDataFormat.Index4Mipmaps => new Index4MipmapsDataCodec(pixelCodec),
				PVRDataFormat.Index8 => new Index8DataCodec(pixelCodec),
				PVRDataFormat.Index8Mipmaps => new Index8MipmapsDataCodec(pixelCodec),
				PVRDataFormat.Rectangle => new RectangleDataCodec(pixelCodec),
				PVRDataFormat.Stride => new StrideDataCodec(pixelCodec),
				PVRDataFormat.RectangleTwiddled => new RectangleTwiddledDataCodec(pixelCodec),
				PVRDataFormat.SmallVq => new SmallVqDataCodec(pixelCodec),
				PVRDataFormat.SmallVqMipmaps => new SmallVqMipmapsDataCodec(pixelCodec),
				PVRDataFormat.SquareTwiddledMipmapsDMA => new SquareTwiddledMipmapsDMADataCodec(pixelCodec),
				_ => throw new ArgumentException($"No codec for format \"{format}\" implemented")
			};
		}

		public virtual bool CheckDimensionsValid(int width, int height)
		{
			return width == height
				&& width is >= 8 and <= 1024
				&& MathHelper.IsPow2(width);
		}

		/// <summary>
		/// Gets the maximum number of entries the palette allows for, or 0 if this pixel format doesn't use a palette.
		/// </summary>
		public virtual int GetPaletteEntries(int? width)
		{
			return 0;
		}

		public abstract int CalculateTextureSize(int width, int height);
		public byte[] Decode(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette)
		{
			byte[] result = new byte[width * height * (NeedsExternalPalette ? 1 : 4)];
			Span<byte> destination = result;

			InternalDecode(source, width, height, palette, destination);

			return result;
		}

		protected abstract void InternalDecode(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination);


		public byte[] Encode(ReadOnlySpan<byte> source, int width, int height)
		{
			byte[] result = new byte[CalculateTextureSize(width, height)];
			Span<byte> destination = result;

			InternalEncode(source, width, height, destination);

			return result;
		}

		protected abstract void InternalEncode(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination);

	}
}
