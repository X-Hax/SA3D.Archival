using SA3D.Common.IO;
using System;

namespace SA3D.Archival.Tex.PV.IO
{
	internal enum PVHeader : uint
	{
		/// <summary>
		/// PVMH (Archive) header
		/// </summary>
		PVMH = 0x484d5650,

		/// <summary>
		/// PVRT (Texture) header
		/// </summary>
		PVRT = 0x54525650,

		/// <summary>
		/// GBIX (Texture global index) header
		/// </summary>
		GBIX = 0x58494247,

		/// <summary>
		/// PVP (Palette) header
		/// </summary>
		PVPL = 0x4C505650,

		/// <summary>
		/// PVPN (Palette name) header
		/// </summary>
		PVPN = 0x4E505650,

		/// <summary>
		/// MDLN (model name) header
		/// </summary>
		MDLN = 0x4E4C444D,

		/// <summary>
		/// PVMI (original file names) header
		/// </summary>
		PVMI = 0x494D5650,

		/// <summary>
		/// CONV (PVR encoding program name) header
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

	internal enum PVRFileFormat
	{
		None,
		PVRT,
		GBIX,
	}

	[Flags]
	internal enum PVMFlags : ushort
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
		/// Specifies an MDLN chunk is present, which contains the filenames of the models associated with this PVM.
		/// </summary>
		MDLNChunk = 1 << 4,

		/// <summary>
		/// Specifies a PVMI chunk is present, which contains the original filenames of the textures converted to PVR.
		/// </summary>
		PVMIChunk = 1 << 5,

		/// <summary>
		/// Specifies a CONV chunk is present, which contains the name of the converter used to convert textures to PVR.
		/// </summary>
		CONVChunk = 1 << 6,

		/// <summary>
		/// Specifies IMGC chunks are present, which contains the original data of the textures converted to PVR.
		/// </summary>
		IMGCChunks = 1 << 7,

		/// <summary>
		/// Specifies PVRT chunks are present.
		/// </summary>
		PVRTChunks = 1 << 8,

		Comment = 1 << 9,

		BankID = 1 << 10,

		PVPLChunks = 1 << 11,

		PVPNChunks = 1 << 12
	}

	internal struct PVMMeta
	{
		public ushort Index { get; set; }
		public string Filename { get; set; }
		public PVRPixelFormat PixelFormat { get; set; }
		public PVRDataFormat DataFormat { get; set; }
		public PVMMetaEntryAttributes EntryAttributes { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public uint GlobalIndex { get; set; }

		public static PVMMeta Read(EndianStackReader data, ref uint address, PVMFlags attributes)
		{
			PVMMeta result = default;
			result.Index = data.ReadUShort(address);
			address += 2;

			if(attributes.HasFlag(PVMFlags.Filenames))
			{
				result.Filename = data.ReadStringLimited(address, 28, out _);
				address += 28;
			}

			if(attributes.HasFlag(PVMFlags.CategoryCode))
			{
				result.PixelFormat = (PVRPixelFormat)data[address];
				result.DataFormat = (PVRDataFormat)data[address + 1];
				address += 2;
			}

			if(attributes.HasFlag(PVMFlags.EntryInfo))
			{
				byte dimensions = data[address];
				result.Width = (ushort)(4 << (dimensions & (0xF0 >> 4)));
				result.Height = (ushort)(4 << (dimensions & 0x0F));
				result.EntryAttributes = (PVMMetaEntryAttributes)data[address + 1];
				address += 2;
			}

			if(attributes.HasFlag(PVMFlags.GlobalIndices))
			{
				result.GlobalIndex = data.ReadUInt(address);
				address += 4;
			}

			return result;
		}

		public readonly void Write(EndianStackWriter writer, PVMFlags attributes)
		{
			writer.WriteUShort(Index);

			if(attributes.HasFlag(PVMFlags.Filenames))
			{
				writer.WriteString(Filename, 28);
			}

			if(attributes.HasFlag(PVMFlags.CategoryCode))
			{
				writer.WriteByte((byte)PixelFormat);
				writer.WriteByte((byte)DataFormat);
			}

			if(attributes.HasFlag(PVMFlags.EntryInfo))
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

				writer.WriteByte(dimensions);
				writer.WriteByte((byte)EntryAttributes);
			}

			if(attributes.HasFlag(PVMFlags.GlobalIndices))
			{
				writer.WriteUInt(GlobalIndex);
			}
		}

		public override readonly string ToString()
		{
			return $"{Index}, \"{Filename}\", {PixelFormat}-{DataFormat}, {Width}x{Height}, {GlobalIndex}";
		}
	}

	internal static class PVEnumExtensions
	{
		public static bool ValidateData(this PVHeader header, EndianStackReader data, uint address)
		{
			uint check = data.ReadUInt(address);
			if(check != (uint)header)
			{
				return false;
			}

			// validating the length
			uint length = data.ReadUInt(address + 4);
			if(length > data.Length - (address + 8))
			{
				throw new ArgumentOutOfRangeException($"{header} data at {address:X8} is not long enough!");
			}

			return true;
		}

		public static void Write(this PVHeader header, EndianStackWriter writer)
		{
			writer.WriteUInt((uint)header);
		}
	}
}
