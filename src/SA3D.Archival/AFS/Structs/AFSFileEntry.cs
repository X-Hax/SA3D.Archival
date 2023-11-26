using SA3D.Common.IO;

namespace SA3D.Archival.AFS.Structs
{
	internal struct AFSFileEntry
	{
		public const uint StructSize = 8;

		public int Offset { get; set; }
		public int Length { get; set; }

		public AFSFileEntry(int offset, int length)
		{
			Offset = offset;
			Length = length;
		}

		public static AFSFileEntry Read(EndianStackReader reader, uint address)
		{
			return new(reader.ReadInt(address), reader.ReadInt(address + 4));
		}

		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteInt(Offset);
			writer.WriteInt(Length);
		}
	}
}
