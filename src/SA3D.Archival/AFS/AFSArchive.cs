using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Archival.AFS
{
	/// <summary>
	/// Generic archive format used by various SEGA games.
	/// </summary>
	public class AFSArchive : IArchive
	{
		/// <summary>
		/// AFS File header.
		/// </summary>
		private const uint _header = 0x534641;

		/// <summary>
		/// PAK files in the archive.
		/// </summary>
		public List<AFSEntry> Entries { get; private set; }

		IReadOnlyList<IArchiveEntry> IArchive.Entries => Entries;


		/// <summary>
		/// Creates a new AFS archive.
		/// </summary>
		public AFSArchive()
		{
			Entries = [];
		}


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			return reader.ReadUInt32() == _header;
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext context)
		{
			using OffsetOriginToken offsetOriginToken = reader.WithOffsetOrigin();

			reader.Skip(sizeof(uint)); // skipping the header
			int entryCount = reader.ReadInt32();

			Entries = [];
			for(int i = 0; i < entryCount; i++)
			{
				long offset = reader.ReadOffsetValue();
				int length = reader.ReadInt32();

				byte[] fileData = reader.ReadArrayAtOffset<byte>(offset, length);
				Entries.Add(new(fileData, string.Empty, default));
			}

			long metadataOffset = reader.ReadOffsetValue();
			reader.Skip(sizeof(uint)); // skipping metadata size

			if(metadataOffset != reader.OffsetHandler.NullOffset)
			{
				AFSMetadataEntry[] metadata = reader.ReadObjectArrayAtOffset<AFSMetadataEntry>(metadataOffset, entryCount);
				for(int i = 0; i < metadata.Length; i++)
				{
					AFSEntry entry = Entries[i];
					AFSMetadataEntry entryMetadata = metadata[i];

					entry.Name = entryMetadata.Filename!;
					entry.DateTime = entryMetadata.DateTime;
				}
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext context)
		{
			using OffsetOriginToken offsetOriginToken = writer.WithOffsetOrigin();

			writer.WriteUInt32(_header);
			writer.WriteUInt32((uint)Entries.Count);

			foreach(AFSEntry entry in Entries)
			{
				writer.WriteOffset(() =>
				{
					writer.WriteArray(entry.Data);
					writer.Align(sizeof(uint));
				});

				writer.WriteInt32(entry.Data.Length);
			}

			AFSMetadataEntry[] metadata = [.. Entries.Select(x => new AFSMetadataEntry()
			{
				DateTime = x.DateTime,
				Filename = x.Name
			})];

			writer.WriteObjectArrayOffset(metadata);
			writer.WriteInt32(metadata.Length * AFSMetadataEntry.StructSize);
		}


		/// <inheritdoc/>
		public string WriteContentIndex()
		{
			throw new NotSupportedException();
		}
	}
}
