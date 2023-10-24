using SA3D.Texturing;
using System;

namespace SA3D.Archival.Tex.GV.IO.PaletteCodec
{
	internal class IntensityA8PaletteCodec : GVPaletteCodec
	{
		public override int BytesPerPixel => 2;

		protected override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[1];
			dst[1] = src[1];
			dst[2] = src[1];
			dst[3] = src[0];
		}

		protected override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[3];
			dst[1] = TextureUtilities.GetLuminance(src);
		}
	}
}
