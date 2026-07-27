namespace SA3D.Archival.Textures
{
	/// <summary>
	/// Texture archive entry binary serialization context
	/// </summary>
	public struct TextureIOContext
	{
		/// <summary>
		/// Whether to ignore file header information and only read/write data
		/// </summary>
		public bool DataOnly { get; set; }

		/// <summary>
		/// Whether to include Global index (if the codec makes it optional)
		/// </summary>
		public bool IncludeGlobalIndex { get; set; }
	}
}
