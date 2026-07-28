using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal class ModelNamesVBlock : VBlock
	{
		public override string Header => "MDLN";

		public string[] Names { get; set; } = [];

		protected override void ReadData(BinaryObjectReader reader, int size)
		{
			ushort mdlnCount = reader.ReadUInt16();
			Names = [.. reader.ReadStringArray(StringBinaryFormat.NullTerminated, mdlnCount)];
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteUInt16((ushort)Names.Length);
			foreach(string modelName in Names)
			{
				writer.WriteString(StringBinaryFormat.NullTerminated, modelName);
			}
		}

		public override string ToString()
		{
			return $"{Header} : [{Names.Length}]";
		}
	}
}
