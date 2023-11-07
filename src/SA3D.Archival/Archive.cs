using SA3D.Archival.PAK;
using SA3D.Archival.Tex;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.Collections.Generic;
using System.IO;

namespace SA3D.Archival
{
	/// <summary>
	/// Base Archive storage class
	/// </summary>
	public abstract class Archive
	{
		/// <summary>
		/// Individual items stored in the archive.
		/// </summary>
		public abstract IReadOnlyList<ArchiveEntry> Entries { get; }


		/// <summary>
		/// Encodes the archive to file data and writes it to an endian writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public abstract void WriteArchive(EndianStackWriter writer);

		/// <summary>
		/// Encodes the archive to a file data stream.
		/// </summary>
		/// <param name="stream">The stream to write to</param>
		public void WriteArchive(Stream stream)
		{
			EndianStackWriter writer = new(stream);
			WriteArchive(writer);
		}

		/// <summary>
		/// Writes the archive to a file.
		/// </summary>
		/// <param name="outputFile">Filepapth to write to</param>
		public void WriteArchiveToFile(string outputFile)
		{
			using(FileStream stream = File.Create(outputFile))
			{
				WriteArchive(stream);
			}
		}

		/// <summary>
		/// Encodes the archive to file data.
		/// </summary>
		/// <returns>The encoded archive.</returns>
		public byte[] WriteArchiveToBytes()
		{
			using(MemoryStream stream = new())
			{
				WriteArchive(stream);
				return stream.ToArray();
			}
		}


		/// <summary>
		/// Writes a content index for the archive to a writer.
		/// </summary>
		public abstract void WriteContentIndex(TextWriter writer);

		/// <summary>
		/// Writes a content index for the archive to a file.
		/// </summary>
		/// <param name="filepath">The path to write the file to.</param>
		public void WriteContentIndexToFile(string filepath)
		{
			using(StreamWriter stream = File.CreateText(filepath))
			{
				WriteContentIndex(stream);
			}
		}

		/// <summary>
		/// Generates a content index for the archive.
		/// </summary>
		/// <returns>The content index.</returns>
		public string WireContentIndexToLines()
		{
			using(StringWriter writer = new())
			{
				WriteContentIndex(writer);
				return writer.ToString();
			}
		}


		/// <summary>
		/// Converts the archive to a texture set. Entries that cannot be converted to textures are ignored.
		/// </summary>
		/// <returns></returns>
		public virtual TextureSet ToTextureSet()
		{
			List<Texture> textures = new();

			foreach(ArchiveEntry item in Entries)
			{
				try
				{
					textures.Add(item.ToTexture());
				}
				catch
				{
					continue;
				}
			}

			return new(textures.ToArray());
		}


		/// <summary>
		/// Tries to read data from a reader as one of the implemented archive types.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address from which to read the archive.</param>
		/// <param name="filename">The filename of the archive. Needed for PAK archives.</param>
		/// <returns>The archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"></exception>
		public static Archive ReadArchive(EndianStackReader reader, uint address, string filename)
		{
			if(PAKArchive.CheckIsPAKArchive(reader, address))
			{
				return PAKArchive.ReadPAKArchive(reader, address, filename);
			}
			else
			{
				try
				{
					return TextureArchive.ReadTextureArchive(reader, address);
				}
				catch(InvalidArchiveException)
				{
					throw new InvalidArchiveException($"Data does not contain any implemented archive format");
				}
			}
		}

		/// <summary>
		/// Tries to read data from a reader as one of the implemented archive types.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address from which to read the archive.</param>
		/// <returns>The archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"></exception>
		public static Archive ReadArchive(EndianStackReader reader, uint address)
		{
			return ReadArchive(reader, address, string.Empty);
		}

		/// <summary>
		/// Tries to read file data as one of the implemented archive types.
		/// </summary>
		/// <param name="data">Data to read.</param>
		/// <param name="address">The address from which to read the archive.</param>
		/// <param name="filename">The filepath to use. Needed for PAK archives.</param>
		/// <returns>The archive that was read.</returns>
		public static Archive ReadArchive(byte[] data, uint address, string filename)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadArchive(reader, address, filename);
			}
		}

		/// <summary>
		/// Tries to read file data as one of the implemented archive types.
		/// </summary>
		/// <param name="data">Data to read.</param>
		/// <param name="address">The address from which to read the archive.</param>
		/// <returns>The archive that was read.</returns>
		public static Archive ReadArchive(byte[] data, uint address)
		{
			return ReadArchive(data, address, string.Empty);
		}

		/// <summary>
		/// Tries to read data from a file as one of the implemented archive types.
		/// <br/> If the filename ends with .prs, the data will be decompressed beforehand.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The archive that was read.</returns>
		public static Archive ReadArchiveFromFile(string filepath)
		{
			return ReadArchive(PRS.ReadPRSFile(filepath), 0, Path.GetFileNameWithoutExtension(filepath));
		}

	}
}
