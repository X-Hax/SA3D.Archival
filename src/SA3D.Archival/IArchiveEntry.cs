using System;

namespace SA3D.Archival
{
	/// <summary>
	/// Base class for archive entries.
	/// </summary>
	public interface IArchiveEntry
	{
		/// <summary>
		/// Name of the Entry
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Archive data
		/// </summary>
		public ReadOnlySpan<byte> Data { get; }
	}
}
