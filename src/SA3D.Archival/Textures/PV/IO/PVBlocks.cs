using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.PV.IO.VBlocks;
using System;

namespace SA3D.Archival.Textures.PV.IO
{
	internal static class PVBlocks
	{
		private static readonly Func<VBlock>[] _pVBlockTypes = [
			() => new PVArchiveVBlock(),
			() => new PVArchiveInfoVBlock(),
			() => new PVTextureVBlock(),
			() => new PVPaletteVBlock(),
			() => new PVPaletteNameVBlock()
		];

		public static VBlock[] ReadPVBlocks(BinaryObjectReader reader)
		{
			return VBlock.ReadVBlocks(reader, _pVBlockTypes);
		}
	}
}
