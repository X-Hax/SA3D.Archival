namespace SA3D.Archival.PAK
{
	/// <summary>
	/// A single PAK archive entry.
	/// </summary>
	public class PAKEntry : ArchiveEntry
	{
		private string _longPath = string.Empty;

		/// <summary>
		/// Full path to the entry (including filename). Enforces lowercase characters.
		/// </summary>
		public string LongPath
		{
			get => _longPath;
			set => _longPath = value.ToLowerInvariant();
		}

		/// <summary>
		/// Creates a new PAK archive entry.
		/// </summary>
		/// <param name="data">Data to use.</param>
		/// <param name="name">Name of the entry.</param>
		/// <param name="longPath">Full original path to the file that this entry represents.</param>
		public PAKEntry(byte[] data, string name, string longPath) : base(data, name)
		{
			LongPath = longPath;
		}

	}
}
