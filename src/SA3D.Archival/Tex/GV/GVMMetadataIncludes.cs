namespace SA3D.Archival.Tex.GV
{
	/// <summary>
	/// Contains parameters on what info should be written to the GVM header table.
	/// </summary>
	public struct GVMMetadataIncludes
	{
		/// <summary>
		/// Enables all params.
		/// </summary>
		public static readonly GVMMetadataIncludes IncludeAll = new()
		{
			IncludeNames = true,
			IncludeCategoryCodes = true,
			IncludeEntryInfo = true,
			IncludeGlobalIndices = true
		};

		/// <summary>
		/// Whether to store GVR names.
		/// </summary>
		public bool IncludeNames { get; set; }

		/// <summary>
		/// Whether to store GVR codec information.
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
