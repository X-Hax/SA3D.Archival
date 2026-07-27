namespace SA3D.Archival
{
	/// <summary>
	/// When to de/compress PRS data
	/// </summary>
	public enum PRSDetectionMode
	{
		/// <summary>
		/// Only de/compress data if the file extension is .prs
		/// </summary>
		FileExtension,

		/// <summary>
		/// Never de/compress data
		/// </summary>
		Never,

		/// <summary>
		/// Always de/compress data
		/// </summary>
		Always
	}

	/// <summary>
	/// Extension methods for <see cref="PRSDetectionMode"/>
	/// </summary>
	public static class PRSDetectionModeExtensions
	{
		/// <summary>
		/// Detect whether data is to be de/compressed
		/// </summary>
		/// <param name="mode">Detection mode</param>
		/// <param name="filepath">The filepath to check, if available</param>
		/// <returns></returns>
		public static bool Detect(this PRSDetectionMode mode, string? filepath)
		{
			if(mode == PRSDetectionMode.Always)
			{
				return true;
			}
			else if(mode == PRSDetectionMode.Never)
			{
				return false;
			}
			else if(!string.IsNullOrWhiteSpace(filepath))
			{
				return filepath.ToLowerInvariant().EndsWith(".prs");
			}

			return false;
		}
	}
}
