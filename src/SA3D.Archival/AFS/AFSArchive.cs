using SA3D.Archival.AFS.Structs;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.IO;

namespace SA3D.Archival.AFS
{
	/// <summary>
	/// Generic archive format used by various SEGA games.
	/// </summary>
	public class AFSArchive : Archive
	{
		/// <summary>
		/// AFS File header.
		/// </summary>
		private const uint _header = 0x534641;

		/// <summary>
		/// PAK files in the archive.
		/// </summary>
		public List<AFSEntry> AFSEntries { get; }

		/// <inheritdoc/>
		public override IReadOnlyList<ArchiveEntry> Entries => AFSEntries;

		/// <summary>
		/// Creates a new AFS archive.
		/// </summary>
		public AFSArchive() : base()
		{
			AFSEntries = new();
		}


		/// <inheritdoc/>
		public override void WriteContentIndex(TextWriter writer)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// Checks whether data can be read as an AFS archive.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as an AFS archive</returns>
		public static bool CheckIsAFSArchive(EndianStackReader reader, uint address)
		{
			return reader.ReadUInt(address) == _header;
		}

		/// <summary>
		/// Checks whether data can be read as an AFS archive.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as an AFS archive</returns>
		public static bool CheckIsAFSArchive(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsAFSArchive(data, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as an AFS archive.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as an AFS archive</returns>
		public static bool CheckIsAFSFileArchive(string filepath)
		{
			return CheckIsAFSArchive(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads an AFS archive texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the AFS starts.</param>
		/// <returns>The AFS that was read.</returns>
		public static AFSArchive ReadAFSArchive(EndianStackReader reader, uint address)
		{
			uint start = address;

			int entryCount = reader.ReadInt(address + 4);
			AFSFileEntry[] fileEntries = new AFSFileEntry[entryCount];

			uint entryAddress = address + 8;
			for(int i = 0; i < entryCount; i++, entryAddress += AFSFileEntry.StructSize)
			{
				fileEntries[i] = AFSFileEntry.Read(reader, entryAddress);
			}

			AFSFileEntry metadataFileEntry = AFSFileEntry.Read(reader, entryAddress);
			AFSMetadataEntry[]? metadata = null;
			if(metadataFileEntry.Offset > 0)
			{
				metadata = new AFSMetadataEntry[entryCount];
				uint metadataAddress = start + (uint)metadataFileEntry.Offset;
				for(int i = 0; i < entryCount; i++, metadataAddress += AFSMetadataEntry.StructSize)
				{
					metadata[i] = AFSMetadataEntry.Read(reader, metadataAddress);
				}
			}

			AFSArchive result = new();

			for(int i = 0; i < entryCount; i++)
			{
				AFSFileEntry fileEntry = fileEntries[i];
				uint fileAddress = start + (uint)fileEntry.Offset;
				byte[] fileData = reader.ReadBytes(fileAddress, fileEntry.Length);

				string filename = string.Empty;
				DateTime dateTime = default;

				if(metadata != null)
				{
					AFSMetadataEntry metadataEntry = metadata[i];
					filename = metadataEntry.Filename;
					dateTime = metadataEntry.GetAsDateTime();
				}

				result.AFSEntries.Add(new(fileData, filename, dateTime));
			}

			return result;
		}

		/// <summary>
		/// Reads an AFS archive texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the AFS starts.</param>
		/// <returns>The AFS that was read.</returns>
		public static AFSArchive ReadAFSArchive(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadAFSArchive(reader, address);
			}
		}

		/// <summary>
		/// Reads an AFS archive texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The AFS that was read.</returns>
		public static AFSArchive ReadAFSArchiveFromFile(string filepath)
		{
			return ReadAFSArchive(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Writes an AFS archive archive to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WriteAFSArchive(EndianStackWriter writer)
		{
			uint start = writer.Position;

			writer.WriteUInt(_header);
			writer.WriteUInt((uint)AFSEntries.Count);

			uint fileEntryPlaceholder = writer.Position;
			writer.WriteEmpty((uint)(AFSFileEntry.StructSize * (1 + AFSEntries.Count)));

			AFSFileEntry[] fileEntries = new AFSFileEntry[AFSEntries.Count];
			AFSMetadataEntry[] metadataEntries = new AFSMetadataEntry[AFSEntries.Count];

			for(int i = 0; i < AFSEntries.Count; i++)
			{
				AFSEntry entry = AFSEntries[i];
				fileEntries[i] = new((int)(writer.Position - start), entry.Data.Length);
				metadataEntries[i] = new(entry.Name, entry.DateTime, entry.Data.Length);

				writer.Write(entry.Data);
				writer.AlignFrom(4, start);
			}

			AFSFileEntry metadataFileEntry = new(
				(int)(writer.Position - start), 
				(int)(metadataEntries.Length * AFSMetadataEntry.StructSize));

			foreach(AFSMetadataEntry metadataEntry in metadataEntries)
			{
				metadataEntry.Write(writer);
			}

			uint end = writer.Position;
			writer.Seek(fileEntryPlaceholder, SeekOrigin.Begin);

			foreach(AFSFileEntry fileEntry in fileEntries)
			{
				fileEntry.Write(writer);
			}

			metadataFileEntry.Write(writer);

			writer.Seek(end, SeekOrigin.Begin);
		}

		/// <summary>
		/// Writes an AFS archive archive to an endian stack writer.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteAFSArchive(Stream stream)
		{
			EndianStackWriter writer = new(stream);
			WriteAFSArchive(writer);
		}

		/// <summary>
		/// Writes an AFS archive archive to an endian stack writer.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		public void WriteAFSArchiveToFile(string filepath)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteAFSArchive(stream);
			}
		}

		/// <summary>
		/// Writes an AFS archive archive to a byte array.
		/// </summary>
		public byte[] WriteAFSArchiveToBytes()
		{
			using(MemoryStream stream = new())
			{
				WriteAFSArchive(stream);
				return stream.ToArray();
			}
		}

		/// <inheritdoc/>
		public override void WriteArchive(EndianStackWriter writer)
		{
			WriteAFSArchive(writer);
		}
	}
}
