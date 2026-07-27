using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal class UnknownVBlock : BaseRawDataVBlock
	{
		public override string Header => UnknownHeader;

		public string UnknownHeader { get; set; } = string.Empty;

		protected override void ReadData(BinaryObjectReader reader, int size)
		{
			using(SeekToken seekToken = reader.At())
			{
				reader.Seek(-2 * sizeof(uint), System.IO.SeekOrigin.Current);
				UnknownHeader = reader.ReadString(StringBinaryFormat.FixedLength, 4);
			}

			base.ReadData(reader, size);
		}
	}
}
