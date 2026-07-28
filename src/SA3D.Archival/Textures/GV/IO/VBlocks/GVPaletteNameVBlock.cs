using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVPaletteNameVBlock : BaseStringVBlock
	{
		public override string Header => "GVPN";

		public override Endianness BlockEndianness => Endianness.Big;
	}
}
