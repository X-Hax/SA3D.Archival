using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVArchiveInfoVBlock : BaseArchiveInfoVBlock
	{
		public override string Header => "GVMI";

		public override Endianness BlockEndianness => Endianness.Big;
	}
}
