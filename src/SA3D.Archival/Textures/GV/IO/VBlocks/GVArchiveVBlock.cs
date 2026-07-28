using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVArchiveVBlock : BaseArchiveVBlock<GVTexture, GVArchiveTextureMetaData>
	{
		public override string Header => "GVMH";

		public override Endianness BlockEndianness => Endianness.Big;

		public override void CopyMetaDataToTexture(GVTexture texture, int index)
		{
			GVArchiveTextureMetaData metadata = MetaData[index];
			texture.Name = metadata.Filename;
			texture.GlobalIndex = metadata.GlobalIndex;
			texture.ArchiveMetaAttributes = metadata.EntryAttributes;
		}
	}
}
