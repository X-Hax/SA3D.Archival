using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal class GlobalIndexVBlock : VBlock
	{
		public override string Header => "GBIX";

		public uint GlobalIndex { get; set; }

		public GlobalIndexVBlock() { }

		public GlobalIndexVBlock(uint globalIndex)
		{
			GlobalIndex = globalIndex;
		}

		protected override void ReadData(BinaryObjectReader reader, int size)
		{
			GlobalIndex = reader.ReadUInt32();
			reader.Skip(sizeof(uint));
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteUInt32(GlobalIndex);
			writer.Skip(sizeof(uint));
		}

		public override string ToString()
		{
			return $"{Header} : {GlobalIndex}";
		}
	}
}
