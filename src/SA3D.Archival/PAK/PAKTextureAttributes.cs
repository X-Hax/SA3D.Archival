
namespace SA3D.Archival.PAK
{
	/// <summary>
	/// PAK texture info attributes.
	/// </summary>
	public enum PAKTextureAttributes : uint
	{
		/// <summary>
		/// Indicates that the texture is palettized.
		/// </summary>
		Palettized = 0x00008000,

		/// <summary>
		/// Indicates that the texture is not storing pixels in twiddled order.
		/// </summary>
		NotTwiddled = 0x04000000,

		/// <summary>
		/// Indicates that the texture is storing pixels in stride.
		/// </summary>
		Stride = 0x02000000,

		/// <summary>
		/// Indicates that the texture uses VQ blocks.
		/// </summary>
		VQ = 0x40000000,

		/// <summary>
		/// Indicates that the texture stores mipmaps.
		/// </summary>
		Mipmapped = 0x80000000,
	}
}
