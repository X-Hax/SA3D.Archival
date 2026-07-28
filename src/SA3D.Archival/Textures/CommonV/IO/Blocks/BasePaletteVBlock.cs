using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BasePaletteVBlock : VBlock
	{
		public abstract ushort FormatData { get; set; }

		public ushort EntryOffset { get; set; }

		public ushort BankOffset { get; set; }

		public ushort Width { get; set; }

		public byte[] Data { get; set; } = [];


		protected override void ReadData(BinaryObjectReader reader, int dataSize)
		{
			FormatData = reader.ReadUInt16();
			BankOffset = reader.ReadUInt16();
			EntryOffset = reader.ReadUInt16();
			Width = reader.ReadUInt16();
			Data = reader.ReadArray<byte>(dataSize - 8);
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteUInt16(FormatData);
			writer.WriteUInt16(BankOffset);
			writer.WriteUInt16(EntryOffset);
			writer.WriteUInt16(Width);
			writer.WriteArray(Data);
		}
	}
}
