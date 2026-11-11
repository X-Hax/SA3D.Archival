using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.GV.IO.VBlocks;
using System;

namespace SA3D.Archival.Textures.GV.IO
{
	internal static class GVBlocks
	{
		private static readonly Func<VBlock>[] _gvBlockTypes = [
			() => new GVArchiveVBlock(),
			() => new GVArchiveInfoVBlock(),
			() => new GVTextureVBlock(),
			() => new GVPaletteVBlock(),
			() => new GVPaletteNameVBlock(),
			() => new GVGlobalIndexVBlock(),
			() => new GlobalIndexBEVBlock(),
		];

		public static VBlock[] ReadGVBlocks(BinaryObjectReader reader)
		{
			return VBlock.ReadVBlocks(reader, _gvBlockTypes);
		}
	}
}
