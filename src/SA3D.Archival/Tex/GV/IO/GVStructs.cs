using SA3D.Common.IO;
using System;

namespace SA3D.Archival.Tex.GV.IO
{
	internal enum GVHeader : uint
	{
		/// <summary>
		/// GVMH (Archive) header
		/// </summary>
		GVMH = 0x484d5647,

		/// <summary>
		/// GVRT (Texture) header
		/// </summary>
		GVRT = 0x54525647,

		/// <summary>
		/// GBIX (Texture global index) header
		/// </summary>
		GBIX = 0x58494347,

		/// <summary>
		/// GCIX (Texture global index) header
		/// </summary>
		GCIX = 0x58494247,

		/// <summary>
		/// GVP (Palette) header
		/// </summary>
		GVPL = 0x4C505647,

		/// <summary>
		/// GVPN (Palette name) header
		/// </summary>
		GVPN = 0x4E505647,

		/// <summary>
		/// MDLN (model name) header
		/// </summary>
		MDLN = 0x4E4C444D,

		/// <summary>
		/// GVMI (original file names) header
		/// </summary>
		GVMI = 0x494D5650,

		/// <summary>
		/// CONV (GVR encoding program name) header
		/// </summary>
		CONV = 0x564E4F43,

		/// <summary>
		/// IMGC (Original image data) header
		/// </summary>
		IMGC = 0x43474D49,

		/// <summary>
		/// COMM (Comment) header
		/// </summary>
		COMM = 0x4D4D4F43,
	}

	internal enum GVRFileFormat
	{
		None,
		GVRT,
		GBIX,
	}

	[Flags]
	internal enum GVMFlags : ushort
	{
		/// <summary>
		/// Specifies global indexes are provided.
		/// </summary>
		GlobalIndices = 1 << 0,

		/// <summary>
		/// Specifies texture dimensions are provided within the entry table.
		/// </summary>
		EntryInfo = 1 << 1,

		/// <summary>
		/// Specifies pixel and data formats are provided within the entry table.
		/// </summary>
		CategoryCode = 1 << 2,

		/// <summary>
		/// Specifies filenames are present within the entry table.
		/// </summary>
		Filenames = 1 << 3,

		/// <summary>
		/// Specifies an MDLN chunk is present, which contains the filenames of the models associated with this GVM.
		/// </summary>
		MDLNChunk = 1 << 4,

		/// <summary>
		/// Specifies a GVMI chunk is present, which contains the original filenames of the textures converted to GVR.
		/// </summary>
		GVMIChunk = 1 << 5,

		/// <summary>
		/// Specifies a CONV chunk is present, which contains the name of the converter used to convert textures to GVR.
		/// </summary>
		CONVChunk = 1 << 6,

		/// <summary>
		/// Specifies IMGC chunks are present, which contains the original data of the textures converted to GVR.
		/// </summary>
		IMGCChunks = 1 << 7,

		/// <summary>
		/// Specifies GVRT chunks are present.
		/// </summary>
		GVRTChunks = 1 << 8,

		Comment = 1 << 9,

		BankID = 1 << 10,

		GVPLChunks = 1 << 11,

		GVPNChunks = 1 << 12
	}

	[Flags]
	internal enum GVRDataFlags : byte
	{
		Mipmaps = 0x1,
		ExternalPalette = 0x2,
		InternalPalette = 0x8,
		Palette = ExternalPalette | InternalPalette
	}

	internal struct GVMMeta
	{
		public ushort Index { get; set; }
		public string Filename { get; set; }
		public GVRDataFlags DataFlags { get; set; }
		public GVPaletteFormat PaletteFormat { get; set; }
		public GVRPixelFormat PixelFormat { get; set; }
		public GVMMetaEntryAttributes EntryAttributes { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public uint GlobalIndex { get; set; }

		public static GVMMeta Read(EndianStackReader data, ref uint address, GVMFlags attributes)
		{
			GVMMeta result = default;
			result.Index = data.ReadUShort(address);
			address += 2;

			if(attributes.HasFlag(GVMFlags.Filenames))
			{
				result.Filename = data.ReadStringLimited(address, 28, out _);
				address += 28;
			}

			if(attributes.HasFlag(GVMFlags.CategoryCode))
			{
				byte paletteFormatAndFlags = data[address];
				result.DataFlags = (GVRDataFlags)(paletteFormatAndFlags & 0xF);
				result.PaletteFormat = (GVPaletteFormat)(paletteFormatAndFlags >> 4);
				result.PixelFormat = (GVRPixelFormat)data[address + 1];
				address += 2;
			}

			if(attributes.HasFlag(GVMFlags.EntryInfo))
			{
				result.EntryAttributes = (GVMMetaEntryAttributes)data[address];

				byte dimensions = data[address + 1];
				result.Width = (ushort)(4 << (dimensions & (0xF0 >> 4)));
				result.Height = (ushort)(4 << (dimensions & 0x0F));
				address += 2;
			}

			if(attributes.HasFlag(GVMFlags.GlobalIndices))
			{
				result.GlobalIndex = data.ReadUInt(address);
				address += 4;
			}

			return result;
		}

		public readonly void Write(EndianStackWriter writer, GVMFlags attributes)
		{
			writer.WriteUShort(Index);

			if(attributes.HasFlag(GVMFlags.Filenames))
			{
				writer.WriteString(Filename, 28);
			}

			if(attributes.HasFlag(GVMFlags.CategoryCode))
			{
				byte paletteFormatAndFlags = (byte)((byte)DataFlags | ((byte)PaletteFormat << 4));

				writer.WriteByte(paletteFormatAndFlags);
				writer.WriteByte((byte)PixelFormat);
			}

			if(attributes.HasFlag(GVMFlags.EntryInfo))
			{
				byte dimensions = (byte)((GetShift(Width) << 4) | GetShift(Height));
				static uint GetShift(ushort value)
				{
					uint result = 0;
					while(value > 7)
					{
						value >>= 1;
						result++;
					}

					return result;
				}

				writer.WriteByte((byte)EntryAttributes);
				writer.WriteByte(dimensions);
			}

			if(attributes.HasFlag(GVMFlags.GlobalIndices))
			{
				writer.WriteUInt(GlobalIndex);
			}
		}

		public override readonly string ToString()
		{
			return $"{Index}, \"{Filename}\", {PixelFormat}+{(DataFlags.HasFlag(GVRDataFlags.Palette) ? PaletteFormat.ToString() : "/")}, {Width}x{Height}, {GlobalIndex}";
		}
	}

	internal static class GVEnumExtensions
	{

		public static bool ValidateData(this GVHeader header, EndianStackReader data, uint address)
		{
			uint check = data.ReadUInt(address);
			uint length = data.ReadUInt(address + 4);

			if(check != (uint)header)
			{
				return false;
			}

			// validating the length
			if(length > data.Length - (address + 8))
			{
				throw new ArgumentOutOfRangeException($"{header} data at {address:X8} is not long enough!");
			}

			return true;
		}

		public static void Write(this GVHeader header, EndianStackWriter writer)
		{
			writer.WriteUInt((uint)header);
		}
	}
}
