using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.PV.IO.VBlocks;
using System;

namespace SA3D.Archival.Textures.PV.IO
{
	internal static class PVBlocks
	{
		private static readonly Type[] _pVBlockTypes = [
			typeof(PVArchiveVBlock),
			typeof(PVArchiveInfoVBlock),
			typeof(PVTextureVBlock),
			typeof(PVPaletteVBlock),
			typeof(PVPaletteNameVBlock)
		];

		public static VBlock[] ReadPVBlocks(BinaryObjectReader reader)
		{
			return VBlock.ReadVBlocks(reader, _pVBlockTypes);
		}
	}
}
