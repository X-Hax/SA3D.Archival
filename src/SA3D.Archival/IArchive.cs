using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Archival.AFS;
using SA3D.Archival.PAK;
using SA3D.Archival.Textures;
using SA3D.Archival.Textures.GV;
using SA3D.Archival.Textures.PV;
using SA3D.Archival.Textures.PVX;
using SA3D.Common.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SA3D.Archival
{
	/// <summary>
	/// Base Archive storage class
	/// </summary>
	public interface IArchive : IFileSerializable
	{
		/// <summary>
		/// Individual items stored in the archive.
		/// </summary>
		public abstract IReadOnlyList<IArchiveEntry> Entries { get; }

		/// <summary>
		/// Writes a content index for the archive to a writer.
		/// </summary>
		public abstract string WriteContentIndex();


		/// <summary>
		/// Attempts to read the data using all available implemented archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="fileContext">File context to read with</param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchive(BinaryObjectReader reader, FileContext fileContext, [NotNullWhen(true)] out IArchive? result)
		{
			if(reader.Check<PAKArchive>(fileContext))
			{
				result = reader.ReadObject<PAKArchive, FileContext>(fileContext);
			}
			else if(reader.Check<AFSArchive>(fileContext))
			{
				result = reader.ReadObject<AFSArchive, FileContext>(fileContext);
			}
			else if(ITextureArchive.TryReadTextureArchive(reader, fileContext, out ITextureArchive? textureArchive))
			{
				result = textureArchive;
			}
			else
			{
				result = null;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Attempt to read stream data using all available implemented archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="result">The resulting archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">How to determine whether to PRS-decompress the data</param>
		/// <param name="filepath">Path to the file being read, if available</param>
		/// <returns></returns>
		public static bool TryReadArchiveFromStream(Stream stream, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, string? filepath = null)
		{
			if(prsDetectionMode.Detect(filepath))
			{
				using MemoryStream decompress = new();
				PRS.Decompress(stream, decompress);
				decompress.Seek(0, SeekOrigin.Begin);
				using BinaryObjectReader reader = new(decompress, StreamOwnership.Retain, Endianness.Little);
				return TryReadArchive(reader, new(filepath), out result);

			}
			else
			{
				using BinaryObjectReader reader = new(stream, StreamOwnership.Retain, Endianness.Little);
				return TryReadArchive(reader, new(filepath), out result);
			}
		}

		/// <summary>
		/// Attempt to read file data using all available implemented  archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="data">Data to read</param>
		/// <param name="result">The resulting archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">Don't decompress the file data if the file extension is .prs</param>
		/// <param name="filepath">Path to the file being read, if available</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchiveFromBytes(byte[] data, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, string? filepath = null)
		{
			using MemoryStream stream = new(data);
			return TryReadArchiveFromStream(stream, out result, prsDetectionMode, filepath);
		}

		/// <summary>
		/// Attempt to read file data using all available implemented archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="filepath">Path to the file to read</param>
		/// <param name="result">The resulting archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">Don't decompress the file data if the file extension is .prs</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchiveFromFile(string filepath, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension)
		{
			using FileStream stream = File.OpenRead(filepath);
			return TryReadArchiveFromStream(stream, out result, prsDetectionMode, filepath);
		}
	}
}
