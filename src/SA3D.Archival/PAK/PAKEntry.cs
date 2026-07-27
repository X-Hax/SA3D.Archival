using SA3D.Texturing;
using SA3D.Texturing.IO;
using System;

namespace SA3D.Archival.PAK
{
	/// <summary>
	/// A single PAK archive entry.
	/// </summary>
	public class PAKEntry : IArchiveEntry
	{
		/// <summary>
		/// The entries binary data
		/// </summary>
		public byte[] Data { get; set; }

		/// <summary>
		/// Full path to the entry (including filename). Enforces lowercase characters.
		/// </summary>
		public string LongPath
		{
			get;
			set => field = value.ToLowerInvariant();
		} = string.Empty;

		/// <inheritdoc/>
		public string Name { get; set; }

		ReadOnlySpan<byte> IArchiveEntry.Data => Data;


		/// <summary>
		/// Creates a new PAK archive entry.
		/// </summary>
		/// <param name="data">Data to use.</param>
		/// <param name="name">Name of the entry.</param>
		/// <param name="longPath">Full original path to the file that this entry represents.</param>
		public PAKEntry(byte[] data, string name, string longPath)
		{
			Name = name;
			LongPath = longPath;
			Data = data;
		}


		/// <summary>
		/// Converts the archive data to a texture
		/// </summary>
		/// <returns></returns>
		public Texture ReadTextureFromData()
		{
			return TextureFileUtilities.ReadImageFromBytes(Data, Name);
		}

		/// <summary>
		/// Converts the archive data to an index texture
		/// </summary>
		/// <returns></returns>
		public IndexTexture ReadndexTextureFromData()
		{
			return TextureFileUtilities.ReadIndexImageFromBytes(Data, Name);
		}
	}
}
