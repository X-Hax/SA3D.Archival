using SA3D.Archival.Tex.PV.IO.DataCodec;
using SA3D.Archival.Tex.PV.IO.PixelCodec;
using System;

namespace SA3D.Archival.Tex.PV
{
	/// <summary>
	/// PVR pixel encoding when applicable
	/// </summary>
	public enum PVRPixelFormat : byte
	{
		/// <summary>
		/// 16 bit color pixels; 1 Alpha bit and 5 for each color.
		/// </summary>
		ARGB1555 = 0x00,

		/// <summary>
		/// 16 bit RGB pixel.
		/// </summary>
		RGB565 = 0x01,

		/// <summary>
		/// 16 bit color pixels, where each channel takes 4 bits.
		/// </summary>
		ARGB4 = 0x02,

		/// <summary>
		/// Luma + double chroma color encoding.
		/// <br/> 4 bytes store 2 pixels, where where both pixels share the double chroma.
		/// </summary>
		YUV422 = 0x03,

		/// <summary>
		/// A format closely related to modern normal maps. It stores a direction using 2 angles, instead of storing 3 component vector directly.
		/// </summary>
		Bump = 0x04,

		/// <summary>
		/// 32 bit color pixels, where each channel takes a byte.
		/// </summary>
		ARGB8 = 0x06,
	}

	/// <summary>
	/// Determines how PVR texture arrange pixels.
	/// </summary>
	public enum PVRDataFormat : byte
{
		/// <summary>
		/// Encodes pixels with the Twiddle (Z-order) pattern.
		/// </summary>
		SquareTwiddled = 0x01,

		/// <summary>
		/// Encodes pixels with the Twiddle (Z-order) pattern. Includes mipmaps.
		/// </summary>
		SquareTwiddledMipmaps = 0x02,

		/// <summary>
		/// Encodes pixels into reused 2x2 blocks using vector quantization.
		/// </summary>
		Vq = 0x03,

		/// <summary>
		/// Encodes pixels into reused 2x2 blocks using vector quantization. Includes mipmaps.
		/// </summary>
		VqMipmaps = 0x04,

		/// <summary>
		/// Pixels store a palette index instead of the colors directly. Each pixel takes 4 bits.
		/// </summary>
		Index4 = 0x05,

		/// <summary>
		/// Pixels store a palette index instead of the colors directly. Each pixel takes 4 bits. Includes mipmaps.
		/// </summary>
		Index4Mipmaps = 0x06,

		/// <summary>
		/// Pixels store a palette index instead of the colors directly. Each pixel takes 8 bits.
		/// </summary>
		Index8 = 0x07,

		/// <summary>
		/// Pixels store a palette index instead of the colors directly. Each pixel takes 8 bits. Includes mipmaps.
		/// </summary>
		Index8Mipmaps = 0x08,

		/// <summary>
		/// Row by row arrangement of any size.
		/// </summary>
		Rectangle = 0x09,

		// 0xA Missing. 

		/// <summary>
		/// Same as Rectangle but with a more limited dimension range.
		/// </summary>
		Stride = 0x0B,

		// 0xC Missing

		/// <summary>
		/// Encodes pixels with the Twiddle (Z-order) pattern that is not limited to a square shape.
		/// </summary>
		RectangleTwiddled = 0x0D,

		// 0xE Missing
		// 0xF Missing

		/// <summary>
		/// Encodes pixels into reused 2x2 blocks using vector quantization. Smaller textures have less blocks available.
		/// </summary>
		SmallVq = 0x10,

		/// <summary>
		/// Encodes pixels into reused 2x2 blocks using vector quantization. Smaller textures have less blocks available. Includes mipmaps.
		/// </summary>
		SmallVqMipmaps = 0x11,

		/// <summary>
		/// Same as <see cref="SquareTwiddledMipmaps"/>, but the 1x1 mipmap takes the same space as 2x2 regardless of pixel codex.
		/// </summary>
		SquareTwiddledMipmapsDMA = 0x12,
	}


	/// <summary>
	/// PVR Metadata used in PVM archives.
	/// </summary>
	[Flags]
	public enum PVMMetaEntryAttributes : byte
	{
		/// <summary>
		/// [Tool Specific] Lock PVR position in the PVM archive.
		/// </summary>
		LockOrder = 0x01,

		/// <summary>
		/// [Tool Specific] Lock PVR contents.
		/// </summary>
		LockPVR = 0x02,

		/// <summary>
		/// Texture is utilizing dithering.
		/// </summary>
		Dither = 0x04,

		/// <summary>
		/// Texture alpha is utilizing dithering.
		/// </summary>
		AlphaDither = 0x08
	}

	/// <summary>
	/// Extension methods for PV related enums.
	/// </summary>
	public static class PVEnumExtensions
	{
		/// <summary>
		/// Determines whether a data format depends on a palette to work.
		/// </summary>
		/// <param name="dataFormat">The data format to check.</param>
		/// <returns></returns>
		public static bool CheckNeedsExternalPalette(this PVRDataFormat dataFormat)
		{
			return PVRDataCodec.Create(dataFormat, PVPixelCodec.GetPixelCodec(PVRPixelFormat.ARGB8)).NeedsExternalPalette;
		}
	}
}
