using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.PV.IO.VBlocks
{
	internal class PVArchiveVBlock : BaseArchiveVBlock<PVTexture, PVArchiveTextureMetaData>
	{
		public override string Header => "PVMH";

		public override void CopyMetaDataToTexture(PVTexture texture, int index)
		{
			PVArchiveTextureMetaData metadata = MetaData[index];
			texture.Name = metadata.Filename;
			texture.GlobalIndex = metadata.GlobalIndex;
			texture.ArchiveMetaAttributes = metadata.EntryAttributes;
		}
	}
}
