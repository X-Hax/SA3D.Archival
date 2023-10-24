namespace SA3D.Archival.PAK
{
	/// <summary>
	/// Ini file container for pak files
	/// </summary>
	internal class PAKIniItem
	{
		/// <summary>
		/// Full file path
		/// </summary>
		public string? LongPath { get; set; }

		public PAKIniItem(string? longPath)
		{
			LongPath = longPath;
		}

		public PAKIniItem() { }
	}
}
