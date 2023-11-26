using SA3D.Common.IO;
using System.IO;
using System.Text;

namespace SA3D.Archival.Tex.PVX.IO
{
	internal static class PVRXIO
	{
		private const uint _pvrxHeader = 0x58525650;
		private const byte _version = 1;

		public static uint WriteMetaData(PVRX pvrx, EndianStackWriter writer)
		{
			writer.WriteByte((byte)PVMXDictionaryField.global_index);
			writer.WriteUInt(pvrx.GlobalIndex);

			writer.WriteByte((byte)PVMXDictionaryField.name);
			writer.WriteStringNullterminated(pvrx.Name, Encoding.UTF8);

			writer.WriteByte((byte)PVMXDictionaryField.dimensions);
			writer.WriteInt(pvrx.Width);
			writer.WriteInt(pvrx.Height);

			writer.WriteByte((byte)PVMXDictionaryField.none);

			uint result = writer.Position;
			writer.WriteEmpty(8);
			writer.WriteULong((ulong)pvrx.Data.Length);

			return result;
		}

		public static void WriteData(PVRX pvrx, EndianStackWriter writer, uint dataOffsetPointerAddr)
		{
			uint position = writer.PointerPosition;
			writer.Seek(dataOffsetPointerAddr, SeekOrigin.Begin);
			writer.WriteULong(position);
			writer.SeekEnd();

			writer.Write(pvrx.Data);
			writer.Align(4);
		}

		public static void WritePVRX(PVRX pvrx, EndianStackWriter writer)
		{
			writer.WriteUInt(_pvrxHeader);
			writer.WriteByte(_version);

			uint dataAddr = WriteMetaData(pvrx, writer);
			writer.Align(4);
			WriteData(pvrx, writer, dataAddr);
		}


		public static bool CheckIsPVRX(EndianStackReader reader, uint address)
		{
			return reader.Length > 6
				&& reader.ReadUInt(address) == _pvrxHeader
				&& (reader[address + 4] is > 0 and <= _version);
		}

		public static PVRX ReadData(EndianStackReader reader, ref uint address)
		{
			string name = string.Empty;
			uint gbix = 0;
			int width = 0;
			int height = 0;

			PVMXDictionaryField type = (PVMXDictionaryField)reader[address++];
			while(type != PVMXDictionaryField.none)
			{
				switch(type)
				{
					case PVMXDictionaryField.global_index:
						gbix = reader.ReadUInt(address);
						address += 4;
						break;

					case PVMXDictionaryField.name:
						name = reader.ReadNullterminatedString(address, Encoding.UTF8, out uint nameLength);
						address += nameLength + 1;
						break;

					case PVMXDictionaryField.dimensions:
						width = reader.ReadInt(address);
						height = reader.ReadInt(address + 4);
						address += 8;
						break;

					case PVMXDictionaryField.none:
					default:
						break;
				}

				type = (PVMXDictionaryField)reader[address++];
			}

			ulong offset = reader.ReadULong(address);
			ulong length = reader.ReadULong(address + 8);
			address += 16;

			byte[] texdata = reader.ReadBytes((uint)offset, (int)length);
			return new(texdata, gbix, width, height, name);
		}

		public static PVRX ReadPVRX(EndianStackReader reader, uint address)
		{
			if(reader.ReadUInt(address) != _pvrxHeader)
			{
				throw new InvalidDataException("Data is not a PVRX!");
			}

			if(reader[address + 4] is 0 or > _version)
			{
				throw new InvalidDataException("Version of the PVRX is not supported!");
			}

			address += 5;
			return ReadData(reader, ref address);
		}
	}
}
