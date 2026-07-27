using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GlobalIndexBEVBlock : GlobalIndexVBlock
	{
		public override Endianness BlockEndianness => Endianness.Big;

		public GlobalIndexBEVBlock() { }

		public GlobalIndexBEVBlock(uint globalIndex) : base(globalIndex) { }
	}
}
