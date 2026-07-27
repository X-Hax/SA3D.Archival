using Amicitia.IO.Binary;
using SA3D.Common.IO;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BaseArchiveVBlock<TTexture, TMeta> : VBlock
		where TTexture : BaseVTexture
		where TMeta : struct, IBinarySerializable<VArchiveIncludes>
	{
		public VArchiveIncludes Attributes { get; set; }
		public ushort TextureCount => (ushort)MetaData.Length;
		public TMeta[] MetaData { get; set; } = [];

		protected override void ReadData(BinaryObjectReader reader, int dataSize)
		{
			Attributes = (VArchiveIncludes)reader.ReadUInt16();
			ushort textureCount = reader.ReadUInt16();
			MetaData = reader.ReadObjectArray<TMeta, VArchiveIncludes>(textureCount, Attributes);
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteUInt16((ushort)Attributes);
			writer.WriteUInt16(TextureCount);
			writer.WriteObjectArray(MetaData, Attributes);
		}

		public abstract void CopyMetaDataToTexture(TTexture texture, int index);

		public override string ToString()
		{
			return $"{Header} : [{TextureCount}]";
		}
	}
}
