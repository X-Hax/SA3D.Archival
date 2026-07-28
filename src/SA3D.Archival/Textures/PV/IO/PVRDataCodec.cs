using SA3D.Archival.Textures.PV.IO.DataCodec;
using SA3D.Common;
using SA3D.Texturing;
using System;

namespace SA3D.Archival.Textures.PV.IO
{
	internal abstract class PVRDataCodec
	{
		public PVPixelCodec PixelCodec { get; }

		/// <summary>
		/// Data format of the resulting texture
		/// </summary>
		public virtual TextureType OutputType => TextureType.RGBA32;

		/// <summary>
		/// Gets if this data format has mipmaps.
		/// </summary>
		public virtual bool HasMipmaps => false;


		protected PVRDataCodec(PVPixelCodec pixelCodec)
		{
			PixelCodec = pixelCodec;
		}

		public static PVRDataCodec Create(PVTextureDataFormat format, PVPixelCodec pixelCodec)
		{
			return format switch
			{
				PVTextureDataFormat.SquareTwiddled => new SquareTwiddledDataCodec(pixelCodec),
				PVTextureDataFormat.SquareTwiddledMipmaps => new SquareTwiddledMipmapsDataCodec(pixelCodec),
				PVTextureDataFormat.Vq => new VqDataCodec(pixelCodec),
				PVTextureDataFormat.VqMipmaps => new VqMipmapsDataCodec(pixelCodec),
				PVTextureDataFormat.Index4 => new Index4DataCodec(pixelCodec),
				PVTextureDataFormat.Index4Mipmaps => new Index4MipmapsDataCodec(pixelCodec),
				PVTextureDataFormat.Index8 => new Index8DataCodec(pixelCodec),
				PVTextureDataFormat.Index8Mipmaps => new Index8MipmapsDataCodec(pixelCodec),
				PVTextureDataFormat.Rectangle => new RectangleDataCodec(pixelCodec),
				PVTextureDataFormat.Stride => new StrideDataCodec(pixelCodec),
				PVTextureDataFormat.RectangleTwiddled => new RectangleTwiddledDataCodec(pixelCodec),
				PVTextureDataFormat.SmallVq => new SmallVqDataCodec(pixelCodec),
				PVTextureDataFormat.SmallVqMipmaps => new SmallVqMipmapsDataCodec(pixelCodec),
				PVTextureDataFormat.SquareTwiddledMipmapsDMA => new SquareTwiddledMipmapsDMADataCodec(pixelCodec),
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

		public abstract void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination);

		public abstract void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination);

	}
}
