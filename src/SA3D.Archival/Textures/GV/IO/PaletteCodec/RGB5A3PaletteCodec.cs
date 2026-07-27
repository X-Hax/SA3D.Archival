using SA3D.Archival.Textures.GV.IO.TextureCodec;
using System;

namespace SA3D.Archival.Textures.GV.IO.PaletteCodec
{
	internal class RGB5A3PaletteCodec : GVPaletteCodec
	{
		public override GVPaletteFormat Format => GVPaletteFormat.Rgb5a3;

		public override int BytesPerPixel => 2;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			RGB5A3PixelCodec.RGB5A3ToRGBA8(src, dst);
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			RGB5A3PixelCodec.RGBA8ToRGB5A3(src, dst);
		}
	}
}
