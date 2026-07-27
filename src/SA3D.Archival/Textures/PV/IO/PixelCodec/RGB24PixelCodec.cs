using System;

namespace SA3D.Archival.Textures.PV.IO.PixelCodec
{
	internal class RGB24PixelCodec : PVPixelCodec
	{
		public override PVPixelFormat Format => PVPixelFormat.RGB24;
		public override int BytesPerPixelPair => 3;
		public override int PixelPairSize => 1;

		public override void DecodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[0];
			dst[1] = src[1];
			dst[2] = src[2];
			dst[3] = 0xff;

		}

		public override void EncodePixel(ReadOnlySpan<byte> src, Span<byte> dst)
		{
			dst[0] = src[0];
			dst[1] = src[1];
			dst[2] = src[2];
		}
	}
}
