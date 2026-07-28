using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVGlobalIndexVBlock : GlobalIndexVBlock
	{
		public override string Header => "GCIX";

		public override Endianness BlockEndianness => Endianness.Big;

		public GVGlobalIndexVBlock() { }

		public GVGlobalIndexVBlock(uint globalIndex) : base(globalIndex) { }
	}
}
