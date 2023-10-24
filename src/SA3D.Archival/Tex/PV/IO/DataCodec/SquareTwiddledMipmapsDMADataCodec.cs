using SA3D.Archival.Tex.PV.IO.PixelCodec;

namespace SA3D.Archival.Tex.PV.IO.DataCodec
{
	internal class SquareTwiddledMipmapsDMADataCodec : SquareTwiddledMipmapsDataCodec
	{
		public SquareTwiddledMipmapsDMADataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override int CalculateTextureSize(int width, int height)
		{
			// A 1x1 mipmap takes up as much space as a 2x2 mipmap.
			// Probably because YUV encodes for pixel pairs
			if(width == 1)
			{
				width = 2;
			}

			return width / PixelCodec.Pixels * width * PixelCodec.BytesPerPixel;
		}
	}
}
