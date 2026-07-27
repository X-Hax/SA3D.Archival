using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.GV.IO.VBlocks;
using System;

namespace SA3D.Archival.Textures.GV.IO
{
	internal static class GVBlocks
	{
		private static readonly Type[] _gvBlockTypes = [
			typeof(GVArchiveVBlock),
			typeof(GVArchiveInfoVBlock),
			typeof(GVTextureVBlock),
			typeof(GVPaletteVBlock),
			typeof(GVPaletteNameVBlock),
			typeof(GVGlobalIndexVBlock),
			typeof(GlobalIndexBEVBlock),
		];

		public static VBlock[] ReadGVBlocks(BinaryObjectReader reader)
		{
			return VBlock.ReadVBlocks(reader, _gvBlockTypes);
		}
	}
}
