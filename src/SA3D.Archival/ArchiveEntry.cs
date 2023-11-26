using SA3D.Common.IO;
using SA3D.Texturing;
using System;

namespace SA3D.Archival
{
	/// <summary>
	/// Base class for archive entries.
	/// </summary>
	public abstract class ArchiveEntry
	{
		private readonly byte[] _data;

		/// <summary>
		/// Name of the Entry
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Creates a new archive entry.
		/// </summary>
		/// <param name="data">Data to use.</param>
		/// <param name="name">Name of the entry.</param>
		protected ArchiveEntry(byte[] data, string name)
		{
			Name = name;
			_data = data;
		}

		/// <summary>
		/// Readonly access to the data.
		/// </summary>
		public ReadOnlySpan<byte> Data => _data;

		/// <summary>
		/// Creates a new Endian stack reader for reading the archive data.
		/// </summary>
		public EndianStackReader CreateDataReader()
		{
			return new(_data);
		}

		/// <summary>
		/// Converts the archive data to a texture
		/// </summary>
		/// <returns></returns>
		public virtual Texture ToTexture()
		{
			return Texture.ReadTexture(_data, Name);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name} - [{_data.Length}]";
		}
	}
}
