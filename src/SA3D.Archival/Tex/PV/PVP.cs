using SA3D.Archival.Tex.PV.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.IO;

namespace SA3D.Archival.Tex.PV
{
	/// <summary>
	/// PVP data handler methods
	/// </summary>
	public class PVP : TexturePalette
	{
		/// <summary>
		/// Palette pixel format.
		/// </summary>
		public PVRPixelFormat PixelFormat { get; set; }

		/// <summary>
		/// Palette entry offset.
		/// </summary>
		public ushort EntryOffset { get; }

		/// <summary>
		/// Palette bank offset.
		/// </summary>
		public ushort BankOffset { get; }


		internal PVP(string name, byte[] data, PVRPixelFormat format, ushort entryOffset, ushort bankOffset) : base(name, data)
		{
			EntryOffset = entryOffset;
			BankOffset = bankOffset;
			PixelFormat = format;
		}


		/// <summary>
		/// Converts a texture palette to a PVP palette.
		/// </summary>
		/// <param name="palette">The palette to convert.</param>
		/// <param name="format">The format in which the palette should store the color values.</param>
		/// <param name="entryOffset">Palette entry offset</param>
		/// <param name="bankOffset">Palette bank offset</param>
		/// <returns>The converted palette</returns>
		public static PVP FromTexturePalette(TexturePalette palette, PVRPixelFormat format = PVRPixelFormat.ARGB8, ushort entryOffset = 0, ushort bankOffset = 0)
		{
			return new(palette.Name, palette.ColorData.ToArray(), format, entryOffset, bankOffset);
		}

		/// <summary>
		/// Creates a PVP palette from raw color values.
		/// </summary>
		/// <param name="colors">The RGBA8 colors to use.</param>
		/// <param name="format">The format in which the palette should store the color values.</param>
		/// <param name="entryOffset">Palette entry offset</param>
		/// <param name="bankOffset">Palette bank offset</param>
		/// <returns>The converted palette</returns>
		public static PVP FromColors(byte[] colors, PVRPixelFormat format = PVRPixelFormat.ARGB8, ushort entryOffset = 0, ushort bankOffset = 0)
		{
			return new(string.Empty, colors, format, entryOffset, bankOffset);
		}


		/// <summary>
		/// Checks whether data can be read as a PVP.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVP</returns>
		public static bool CheckIsPVP(EndianStackReader reader, uint address)
		{
			try
			{
				return PVHeader.PVPL.ValidateData(reader, address);
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks whether data can be read as a PVP.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVP</returns>
		public static bool CheckIsPVP(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsPVP(reader, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as a PVP.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a PVP</returns>
		public static bool CheckIsPVPFile(string filepath)
		{
			return CheckIsPVP(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads a PVP texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the PVP starts.</param>
		/// <param name="name">Name of the PVP texture.</param>
		/// <returns>The PVP that was read.</returns>
		public static PVP ReadPVP(EndianStackReader reader, uint address, string name)
		{
			PVP result = PVPIO.ReadPVP(reader, address);
			result.Name = name;
			return result;
		}

		/// <summary>
		/// Reads a PVP texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the PVP starts.</param>
		/// <returns>The PVP that was read.</returns>
		public static PVP ReadPVP(EndianStackReader reader, uint address)
		{
			return ReadPVP(reader, address, string.Empty);
		}

		/// <summary>
		/// Reads a PVP texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the PVP starts.</param>
		/// <param name="name">Name of the PVP texture.</param>
		/// <returns>The PVP that was read.</returns>
		public static PVP ReadPVP(byte[] data, uint address, string name)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadPVP(reader, address, name);
			}
		}

		/// <summary>
		/// Reads a PVP texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the PVP starts.</param>
		/// <returns>The PVP that was read.</returns>
		public static PVP ReadPVP(byte[] data, uint address)
		{
			return ReadPVP(data, address, string.Empty);
		}

		/// <summary>
		/// Reads a PVP texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The PVP that was read.</returns>
		public static PVP ReadPVPFromFile(string filepath)
		{
			return ReadPVP(File.ReadAllBytes(filepath), 0, Path.GetFileNameWithoutExtension(filepath));
		}


		/// <summary>
		/// Writes the PVP to an endian stack writer. Includes Global texture index.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WritePVP(EndianStackWriter writer)
		{
			PVPIO.WritePVP(this, writer);
		}

		/// <summary>
		/// Writes the PVP to a stream. Includes Global texture index.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WritePVP(Stream stream)
		{
			WritePVP(new EndianStackWriter(stream));
		}

		/// <summary>
		/// Writes the PVP to a byte array.
		/// </summary>
		/// <returns>The encoded PVP.</returns>
		public byte[] WritePVPToBytes()
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				WritePVP(writer);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes the PVP to a file
		/// </summary>
		/// <param name="filepath">Output filepath</param>
		public void WritePVPToFile(string filepath)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WritePVP(stream);
			}
		}
	}
}
