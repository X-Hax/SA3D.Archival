using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Archival.Textures.GV;
using SA3D.Archival.Textures.PV;
using SA3D.Archival.Textures.PVX;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SA3D.Archival.Textures
{
	/// <summary>
	/// Archive designed to store a collection of textures.
	/// </summary>
	public interface ITextureArchive : IArchive, ITextureSet
	{
		/// <summary>
		/// Invidiual textures stored in the texture archive.
		/// </summary>
		public new IReadOnlyList<ITextureArchiveEntry> Entries { get; }

		IReadOnlyList<IArchiveEntry> IArchive.Entries => Entries;

		IReadOnlyList<ITexture> ITextureSet.Textures => Entries;


		/// <summary>
		/// Attempts to read the data using all available implemented texture archive format:<br/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadTextureArchive(BinaryObjectReader reader, [NotNullWhen(true)] out ITextureArchive? result, FileIOInfo fileInfo = default)
		{
			result = (ITextureArchive?)ReadFile<PVArchive>(reader, fileInfo)
				?? (ITextureArchive?)ReadFile<GVArchive>(reader, fileInfo)
				?? ReadFile<PVXArchive>(reader, fileInfo);

			return result != null;
		}

		/// <summary>
		/// Attempt to read stream data using all available implemented texture archive format:<br/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">How to determine whether to PRS-decompress the data</param>
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns></returns>
		public static bool TryReadTextureArchiveFromStream(Stream stream, [NotNullWhen(true)] out ITextureArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, FileIOInfo fileInfo = default)
		{
			if(prsDetectionMode.Detect(fileInfo.Filepath))
			{
				using MemoryStream decompress = new();
				PRS.Decompress(stream, decompress);
				decompress.Seek(0, SeekOrigin.Begin);
				using BinaryObjectReader reader = new(decompress, StreamOwnership.Retain, Endianness.Little);
				return TryReadTextureArchive(reader, out result, fileInfo);

			}
			else
			{
				using BinaryObjectReader reader = new(stream, StreamOwnership.Retain, Endianness.Little);
				return TryReadTextureArchive(reader, out result, fileInfo);
			}
		}

		/// <summary>
		/// Attempt to read file data using all available implemented texture archive format:<br/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="data">Data to read</param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">Don't decompress the file data if the file extension is .prs</param>
		/// <param name="fileInfo">Info with which the file should be read</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadTextureArchiveFromBytes(byte[] data, [NotNullWhen(true)] out ITextureArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension, FileIOInfo fileInfo = default)
		{
			using MemoryStream stream = new(data);
			return TryReadTextureArchiveFromStream(stream, out result, prsDetectionMode, fileInfo);
		}

		/// <summary>
		/// Attempt to read file data using all available implemented texture archive format:<br/>
		/// - <see cref="PVArchive"/><br/>
		/// - <see cref="GVArchive"/><br/>
		/// - <see cref="PVXArchive"/>
		/// </summary>
		/// <param name="fileInfo">Info of the file to read</param>
		/// <param name="result">The resulting texture archive. If no archive format was able to read the data, null is returned</param>
		/// <param name="prsDetectionMode">Don't decompress the file data if the file extension is .prs</param>
		/// <returns>Whether an archive was successfully read</returns>
		public static bool TryReadTextureArchiveFromFile(FileIOInfo fileInfo, [NotNullWhen(true)] out ITextureArchive? result, PRSDetectionMode prsDetectionMode = PRSDetectionMode.FileExtension)
		{
			if(string.IsNullOrWhiteSpace(fileInfo.Filepath))
			{
				throw new ArgumentException("No filepath specified!");
			}

			using FileStream stream = File.OpenRead(fileInfo.Filepath);
			return TryReadTextureArchiveFromStream(stream, out result, prsDetectionMode, fileInfo);
		}
	}
}
