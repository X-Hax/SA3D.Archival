namespace SA3D.Archival.Textures.PVX
{
	internal enum PVXArchiveDictionaryField : byte
	{
		none,

		/// <summary>
		/// 32-bit integer global index
		/// </summary>
		global_index,

		/// <summary>
		/// Null-terminated file name
		/// </summary>
		name,

		/// <summary>
		/// Two 32-bit integers defining width and height
		/// </summary>
		dimensions,
	}
}
