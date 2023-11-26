using System;

namespace SA3D.Archival.AFS
{
	/// <summary>
	/// AFS Archive entry.
	/// </summary>
	public class AFSEntry : ArchiveEntry
	{
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
		public AFSEntry(byte[] data, string name, DateTime dateTime) : base(data, name)
		{
			DateTime = dateTime;
		}
	}
}
