using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.PV.IO.VBlocks
{
	internal class PVArchiveInfoVBlock : BaseArchiveInfoVBlock
	{
		public override string Header => "PVMI";

		public override string ToString()
		{
			return $"{Header} : [{Inputs.Length}]";
		}
	}
}
