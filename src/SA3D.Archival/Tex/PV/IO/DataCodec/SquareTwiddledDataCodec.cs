using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common;

namespace SA3D.Archival.Tex.PV.IO.DataCodec
{
	internal class SquareTwiddledDataCodec : RectangleTwiddledDataCodec
	{
		public SquareTwiddledDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override bool CheckDimensionsValid(int width, int height)
		{
			// "Underiving" it
			return width == height
				&& width is >= 8 and <= 1024
				&& MathHelper.IsPow2(width);
		}
	}
}
