using Amicitia.IO.Binary;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BaseRawDataVBlock : VBlock
	{
		public byte[] Data { get; set; } = [];

		protected override void ReadData(BinaryObjectReader reader, int size)
		{
			Data = reader.ReadArray<byte>(size);
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			writer.WriteArray(Data);
		}

		public override string ToString()
		{
			return $"{Header} : [{Data.Length}]";
		}
	}
}
