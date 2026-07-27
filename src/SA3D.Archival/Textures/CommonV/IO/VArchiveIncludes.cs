using System;

namespace SA3D.Archival.Textures.CommonV.IO
{
	[Flags]
	internal enum VArchiveIncludes : ushort
	{
		GlobalIndices = 1 << 0,
		EntryInfo = 1 << 1,
		CategoryCode = 1 << 2,
		Filenames = 1 << 3,
		ModelNames = 1 << 4,
		ArchiveInfo = 1 << 5,
		ConverterName = 1 << 6,
		ImageData = 1 << 7,
		Textures = 1 << 8,
		Comment = 1 << 9,
		BankID = 1 << 10,
		Palettes = 1 << 11,
		paletteNames = 1 << 12
	}
}
