using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BaseStringVBlock : VBlock
	{
		public string Value { get; set; } = string.Empty;

		protected override void ReadData(BinaryObjectReader reader, int size)
		{
			Value = reader.ReadString(StringBinaryFormat.NullTerminated);
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.NullTerminated, Value);
		}

		public override string ToString()
		{
			return $"{Header} : \"{Value}\"";
		}
	}
}
