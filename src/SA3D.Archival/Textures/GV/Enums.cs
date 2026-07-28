using SA3D.Archival.Textures.GV.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;

namespace SA3D.Archival.Textures.GV
{
	/// <summary>
	/// The format in which palettes stores pixel colors.
	/// </summary>
	public enum GVPaletteFormat : byte
	{
		/// <summary>
		/// 8 bit grayscale color pixel.
		/// </summary>
		IntensityA8 = 0x00,

		/// <summary>
		/// 16 bit RGB color pixel
		/// </summary>
		Rgb565 = 0x01,

		/// <summary>
		/// A 16 bit pixel, which is either 15 bits RGB or 15 bits ARGB
		/// </summary>
		Rgb5a3 = 0x02,
	}

	/// <summary>
	/// The format in which a GVR texture stores pixel colors.
	/// </summary>
	public enum GVTextureFormat : byte
	{
		/// <summary>
		/// 4 bit grayscale pixel.
		/// </summary>
		Intensity4 = 0x00,

		/// <summary>
		/// 8 bit grayscale pixel.
		/// </summary>
		Intensity8 = 0x01,

		/// <summary>
		/// 8 bit grayscale + alpha pixel.
		/// </summary>
		IntensityA4 = 0x02,

		/// <summary>
		/// 16 bit grayscale + alpha pixel.
		/// </summary>
		IntensityA8 = 0x03,

		/// <summary>
		/// 16 bit RGB color pixel
		/// </summary>
		RGB565 = 0x04,

		/// <summary>
		/// A 16 bit pixel, which is either 15 bits RGB or 15 bits ARGB
		/// </summary>
		RGB5A3 = 0x05,

		/// <summary>
		/// A 32 bit ARGB pixel
		/// </summary>
		ARGB8 = 0x06,

		/// <summary>
		/// A 4 bit index pixel that is used in conjunction with a palette.
		/// </summary>
		Index4 = 0x08,

		/// <summary>
		/// An 8 bit index pixel that is used in conjunction with a palette.
		/// </summary>
		Index8 = 0x09,

		/// <summary>
		/// DXT1 block compression pixels. A 4x4 block of pixels is compressed into 16 bytes, where pixels are interpolated.
		/// </summary>
		DXT1 = 0x0E,
	}

	/// <summary>
	/// GVR Metadata used in GVM archives.
	/// </summary>
	[Flags]
	public enum GVArchiveEntryMetaAttributes : byte
	{
		/// <summary>
		/// [Tool Specific] Lock GVR position in the GVM archive.
		/// </summary>
		LockOrder = 0x01,

		/// <summary>
		/// [Tool Specific] Lock GVR contents.
		/// </summary>
		LockGVR = 0x02,

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
	/// GV Enum extension methods
	/// </summary>
	public static class GVEnumExtensions
	{
		internal static GVTextureCodec GetPixelCodec(this GVTextureFormat pixelFormat)
		{
			return GVTextureCodec.GetPixelCodec(pixelFormat);
		}

		internal static GVPaletteCodec GetPaletteCodec(this GVPaletteFormat paletteFormat)
		{
			return GVPaletteCodec.GetPaletteCodec(paletteFormat);
		}


		/// <summary>
		/// Returns the output format of the data format
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public static TextureType GetOutputFormat(this GVTextureFormat format)
		{
			return format.GetPixelCodec().OutputType;
		}

		/// <summary>
		/// Calculates the encoded texture size for a given data format and texture dimensions
		/// </summary>
		/// <param name="pixelFormat"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="withMipMaps"></param>
		/// <returns></returns>
		public static int CalculateTextureSize(this GVTextureFormat pixelFormat, int width, int height, bool withMipMaps)
		{
			GVTextureCodec pixelCodec = pixelFormat.GetPixelCodec();
			int result = pixelCodec.CalculateTextureSize(width, height);

			if(withMipMaps)
			{
				foreach((int mmWidth, int mmHeight) in MipMapUtils.GetMipMapSizes(width, height))
				{
					result += pixelCodec.CalculateTextureSize(mmWidth, mmHeight);
				}
			}

			return result;
		}
	}
}
