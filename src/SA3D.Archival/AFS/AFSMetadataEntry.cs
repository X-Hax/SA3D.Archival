using Amicitia.IO.Binary;
using System;

namespace SA3D.Archival.AFS
{
	internal struct AFSMetadataEntry : IBinarySerializable
	{
		public const int StructSize = 48;

		public string? Filename { get; set; }
		public ushort Year { get; set; }
		public ushort Month { get; set; }
		public ushort Day { get; set; }
		public ushort Hour { get; set; }
		public ushort Minute { get; set; }
		public ushort Second { get; set; }
		public int Length { get; set; }

		public DateTime DateTime
		{
			readonly get => new(Year, Month, Day, Hour, Minute, Second);
			set
			{
				Year = (ushort)value.Year;
				Month = (ushort)value.Month;
				Day = (ushort)value.Day;
				Hour = (ushort)value.Hour;
				Minute = (ushort)value.Minute;
				Second = (ushort)value.Second;
			}
		}

		public void Read(BinaryObjectReader reader)
		{
			Filename = reader.ReadString(StringBinaryFormat.FixedLength, 32);
			Year = reader.ReadUInt16();
			Month = reader.ReadUInt16();
			Day = reader.ReadUInt16();
			Hour = reader.ReadUInt16();
			Minute = reader.ReadUInt16();
			Second = reader.ReadUInt16();
			Length = reader.ReadInt32();
		}

		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.FixedLength, Filename, 32);
			writer.WriteUInt16(Year);
			writer.WriteUInt16(Month);
			writer.WriteUInt16(Day);
			writer.WriteUInt16(Hour);
			writer.WriteUInt16(Minute);
			writer.WriteUInt16(Second);
			writer.WriteInt32(Length);
		}
	}
}
