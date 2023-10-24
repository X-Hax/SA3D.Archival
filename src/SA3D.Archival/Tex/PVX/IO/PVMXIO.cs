using SA3D.Common.IO;
using System.IO;

namespace SA3D.Archival.Tex.PVX.IO
{
	internal static class PVMXIO
	{
		private const uint _pvmxHeader = 0x584D5650;
		private const byte _version = 1;

		public static void WritePVMX(PVMX pvmx, EndianStackWriter writer)
		{
			writer.WriteUInt(_pvmxHeader);
			writer.WriteByte(_version);

			uint[] offsetPositions = new uint[pvmx.PVRXs.Count];
			for(int i = 0; i < pvmx.PVRXs.Count; i++)
			{
				offsetPositions[i] = PVRXIO.WriteMetaData(pvmx.PVRXs[i], writer);
			}

			writer.WriteByte((byte)PVMXDictionaryField.none);
			writer.Align(4);

			for(int i = 0; i < pvmx.PVRXs.Count; i++)
			{
				PVRXIO.WriteData(pvmx.PVRXs[i], writer, offsetPositions[i]);
			}
		}


		public static bool CheckIsPVMX(EndianStackReader reader, uint address)
		{
			return reader.Length > 6
				&& reader.ReadUInt(address) == _pvmxHeader
				&& (reader[address + 4] is > 0 and <= _version);
		}

		public static PVMX ReadPVMX(EndianStackReader reader, uint address)
		{
			if(reader.ReadUInt(address) != _pvmxHeader)
			{
				throw new InvalidDataException("Data is not a PVMX!");
			}

			if(reader[address + 4] is 0 or > _version)
			{
				throw new InvalidDataException("Version of the PVMX is not supported!");
			}

			PVMX result = new();

			address += 5;
			while(reader[address] != 0)
			{
				result.PVRXs.Add(PVRXIO.ReadData(reader, ref address));
			}

			return result;
		}
	}
}
