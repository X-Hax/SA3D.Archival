using System;

namespace SA3D.Archival.AFS
{
	/// <summary>
	/// AFS Archive entry.
	/// </summary>
	public class AFSEntry : IArchiveEntry
	{
		/// <summary>
		/// The entries binary data
		/// </summary>
		public byte[] Data { get; set; }

		ReadOnlySpan<byte> IArchiveEntry.Data => Data;

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <summary>
		/// Date and time info of the entry.
		/// </summary>
		public DateTime DateTime { get; set; }


		/// <summary>
		/// Creates a new AFS archive.
		/// </summary>
		/// <param name="data">Data to use.</param>
		/// <param name="name">Name of the entry.</param>
		/// <param name="dateTime">Date and time info of the entry.</param>
		public AFSEntry(byte[] data, string name, DateTime dateTime)
		{
			Name = name;
			DateTime = dateTime;
			Data = data;
		}
	}
}
