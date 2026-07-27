using SA3D.Archival.Textures.GV.IO.TextureCodec;
using System;

namespace SA3D.Archival.Textures.GV.IO.PaletteCodec
{
	internal class RGB565PaletteCodec : GVPaletteCodec
	{
		public override GVPaletteFormat Format => GVPaletteFormat.Rgb565;

		public override int BytesPerPixel => 2;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			RGB565PixelCodec.RGB565ToRGBA8(src, dst);
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			RGB565PixelCodec.RGBA8ToRGB565(src, dst);
		}
	}
}
