using SA3D.Archival.Tex.GV.IO.PixelCodec;
using System;

namespace SA3D.Archival.Tex.GV.IO.PaletteCodec
{
	internal class RGB5A3PaletteCodec : GVPaletteCodec
	{
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
