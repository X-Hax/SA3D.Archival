using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BaseTextureVBlock : VBlock
	{
		public abstract uint Format { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public byte[] Data { get; set; } = [];

		protected override void ReadData(BinaryObjectReader reader, int dataSize)
		{
			Format = reader.ReadUInt32();
			Width = reader.ReadUInt16();
			Height = reader.ReadUInt16();

			Data = reader.ReadArray<byte>(dataSize - 8);
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteUInt32(Format);
			writer.WriteUInt16(Width);
			writer.WriteUInt16(Height);
			writer.WriteArray(Data);
		}
	}
}
