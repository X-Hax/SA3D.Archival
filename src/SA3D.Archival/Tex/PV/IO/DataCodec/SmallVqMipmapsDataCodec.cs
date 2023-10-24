using SA3D.Archival.Tex.PV.IO.PixelCodec;

namespace SA3D.Archival.Tex.PV.IO.DataCodec
{
	internal class SmallVqMipmapsDataCodec : VqMipmapsDataCodec
	{
		public SmallVqMipmapsDataCodec(PVPixelCodec pixelCodec) : base(pixelCodec) { }

		public override int GetPaletteEntries(int? width)
		{
			if(width == null)
			{
				return 1024;
			}
			else if(width <= 16)
			{
				return 64;
			}
			else if(width <= 32)
			{
				return 256;
			}
			else
			{
				return 1024;
			}
		}
	}
}
