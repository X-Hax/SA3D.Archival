using System;

namespace SA3D.Archival.Textures.PV.IO.DataCodec
{
	internal class SquareTwiddledMipmapsDataCodec : SquareTwiddledDataCodec
	{
		public override bool HasMipmaps => true;

		public SquareTwiddledMipmapsDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override int CalculateTextureSize(int width, int height)
		{
			// A 1x1 mipmap takes up as much space as a 2x1 texture (old twiddle method, idk).
			// Probably because YUV encodes for pixel pairs
			if(width == 1)
			{
				width = 2;
			}

			return width / PixelCodec.PixelPairSize * height * PixelCodec.BytesPerPixelPair;
		}

		public override void DecodeTexture(ReadOnlySpan<byte> source, int width, int height, ReadOnlySpan<byte> palette, Span<byte> destination)
		{
			if(width == 1)
			{
				byte[] tmpSrc = new byte[8];
				source[..4].CopyTo(tmpSrc);
				byte[] tmpDst = new byte[16];
				base.DecodeTexture(source, 2, 2, palette, tmpDst);
				((ReadOnlySpan<byte>)tmpDst)[4..8].CopyTo(destination);
			}
			else
			{
				base.DecodeTexture(source, width, height, palette, destination);
			}
		}

		public override void EncodeTexture(ReadOnlySpan<byte> source, int width, int height, Span<byte> destination)
		{
			if(width == 1)
			{
				PixelCodec.EncodePixel(
					source[0..],
					destination.Slice(destination.Length - PixelCodec.BytesPerPixelPair, PixelCodec.BytesPerPixelPair));
			}
			else
			{
				base.EncodeTexture(source, width, height, destination);
			}
		}
	}
}
