namespace SA3D.Archival.Tex.PV
{
	/// <summary>
	/// Contains parameters on what info should be written to the PVM header table.
	/// </summary>
	public struct PVMMetadataIncludes
	{
		/// <summary>
		/// Enables all params.
		/// </summary>
		public static readonly PVMMetadataIncludes IncludeAll = new()
		{
			IncludeNames = true,
			IncludeCategoryCodes = true,
			IncludeEntryInfo = true,
			IncludeGlobalIndices = true
		};

		/// <summary>
		/// Whether to store PVR names.
		/// </summary>
		public bool IncludeNames { get; set; }

		/// <summary>
		/// Whether to store PVR codec information.
		/// </summary>
		public bool IncludeCategoryCodes { get; set; }

		/// <summary>
		/// Whether to store texture metadata, such as dimensions.
		/// </summary>
		public bool IncludeEntryInfo { get; set; }

		/// <summary>
		/// Whether to store global texture indices.
		/// </summary>
		public bool IncludeGlobalIndices { get; set; }

	}
}
