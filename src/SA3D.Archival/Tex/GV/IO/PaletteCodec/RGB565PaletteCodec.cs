using SA3D.Archival.Tex.GV.IO.PixelCodec;
using System;

namespace SA3D.Archival.Tex.GV.IO.PaletteCodec
{
	internal class RGB565PaletteCodec : GVPaletteCodec
	{
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
