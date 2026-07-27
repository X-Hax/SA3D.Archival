using System;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	[Flags]
	internal enum GVTextureAttributes : byte
	{
		Mipmaps = 0x1,
		ExternalPalette = 0x2,
		InternalPalette = 0x8,
		Palette = ExternalPalette | InternalPalette
	}
}
