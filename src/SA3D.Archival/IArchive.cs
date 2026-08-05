using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Archival.AFS;
using SA3D.Archival.PAK;
using SA3D.Archival.Textures;
using SA3D.Archival.Textures.GV;
using SA3D.Archival.Textures.PV;
using SA3D.Archival.Textures.PVX;
using SA3D.Common.IO;
using System;
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


		internal static T? ReadFile<T>(BinaryObjectReader reader, FileIOInfo fileInfo) where T : class, IArchive, new()
		{
			if(!reader.CheckCanReadFile<T>(ref fileInfo))
			{
				return null;
			}

			T result = new();
			result.ReadFile(reader, fileInfo);
			return result;
		}

		/// <summary>
		/// Attempts to read the data using all available implemented archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchive(BinaryObjectReader reader, [NotNullWhen(true)] out IArchive? result, FileIOInfo fileInfo = default)
		{
			result = (IArchive?)ReadFile<PAKArchive>(reader, fileInfo)
				?? ReadFile<AFSArchive>(reader, fileInfo);

			if(result == null  && ITextureArchive.TryReadTextureArchive(reader, out ITextureArchive? textureArchive, fileInfo))
			{
				result = textureArchive;
			}

			return result != null;
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
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns></returns>
		public static bool TryReadArchiveFromStream(Stream stream, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, FileIOInfo fileInfo = default)
		{
			if(prsDetectionMode.Detect(fileInfo.Filepath))
			{
				using MemoryStream decompress = new();
				PRS.Decompress(stream, decompress);
				decompress.Seek(0, SeekOrigin.Begin);
				using BinaryObjectReader reader = new(decompress, StreamOwnership.Retain, Endianness.Little);
				return TryReadArchive(reader, out result, fileInfo);

			}
			else
			{
				using BinaryObjectReader reader = new(stream, StreamOwnership.Retain, Endianness.Little);
				return TryReadArchive(reader, out result, fileInfo);
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
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchiveFromBytes(byte[] data, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, FileIOInfo fileInfo = default)
		{
			using MemoryStream stream = new(data);
			return TryReadArchiveFromStream(stream, out result, prsDetectionMode, fileInfo);
		}

		/// <summary>
		/// Attempt to read file data using all available implemented archive formats:<br/>
		/// - <see cref="PAKArchive"/>
		/// - <see cref="AFSArchive"/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		///  <param name="fileInfo">Info to the file to be read</param>
		/// <param name="result">The resulting archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">Don't decompress the file data if the file extension is .prs</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadArchiveFromFile(FileIOInfo fileInfo, [NotNullWhen(true)] out IArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension)
		{
			if(string.IsNullOrWhiteSpace(fileInfo.Filepath))
			{
				throw new ArgumentException("No filepath specified!");
			}

			using FileStream stream = File.OpenRead(fileInfo.Filepath);
			return TryReadArchiveFromStream(stream, out result, prsDetectionMode, fileInfo);
		}
	}
}
