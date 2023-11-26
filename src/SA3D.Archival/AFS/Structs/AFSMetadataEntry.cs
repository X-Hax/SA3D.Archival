using SA3D.Common.IO;
using System;

namespace SA3D.Archival.AFS.Structs
{
	internal struct AFSMetadataEntry
	{
		public const uint StructSize = 48;

		public string Filename { get; set; }
		public ushort Year { get; set; }
		public ushort Month { get; set; }
		public ushort Day { get; set; }
		public ushort Hour { get; set; }
		public ushort Minute { get; set; }
		public ushort Second { get; set; }
		public int Length { get; set; }

		public AFSMetadataEntry()
		{
			Filename = string.Empty;
		}

		public AFSMetadataEntry(string filename, ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second, int length)
		{
			Filename = filename;
			Year = year;
			Month = month;
			Day = day;
			Hour = hour;
			Minute = minute;
			Second = second;
			Length = length;
		}

		public AFSMetadataEntry(string filename, DateTime datetime, int length)
		{
			Filename = filename;
			Year = (ushort)datetime.Year;
			Month = (ushort)datetime.Month;
			Day = (ushort)datetime.Day;
			Hour = (ushort)datetime.Hour;
			Minute = (ushort)datetime.Minute;
			Second = (ushort)datetime.Second;
			Length = length;
		}

		public readonly DateTime GetAsDateTime()
		{
			return new(Year, Month, Day, Hour, Minute, Second);
		}

		public static AFSMetadataEntry Read(EndianStackReader reader, uint address)
		{
			return new AFSMetadataEntry(
				reader.ReadStringLimited(address, 32, out _),
				reader.ReadUShort(address + 32),
				reader.ReadUShort(address + 34),
				reader.ReadUShort(address + 36),
				reader.ReadUShort(address + 38),
				reader.ReadUShort(address + 40),
				reader.ReadUShort(address + 42),
				reader.ReadInt(address + 44)
			);
		}

		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteString(Filename, 32);
			writer.WriteUShort(Year);
			writer.WriteUShort(Month);
			writer.WriteUShort(Day);
			writer.WriteUShort(Hour);
			writer.WriteUShort(Minute);
			writer.WriteUShort(Second);
			writer.WriteInt(Length);
		}
	}
}
