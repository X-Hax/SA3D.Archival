using SA3D.Archival.Tex.GV.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.IO;

namespace SA3D.Archival.Tex.GV
{
	/// <summary>
	/// Palette storage medium used in gamecube games.
	/// </summary>
	public class GVP : TexturePalette
	{
		/// <summary>
		/// Palette pixel format.
		/// </summary>
		public GVPaletteFormat PaletteFormat { get; set; }

		/// <summary>
		/// Palette entry offset.
		/// </summary>
		public ushort EntryOffset { get; }

		/// <summary>
		/// Palette bank offset.
		/// </summary>
		public ushort BankOffset { get; }


		internal GVP(string name, byte[] data, GVPaletteFormat format, ushort entryOffset, ushort bankOffset) : base(name, data)
		{
			EntryOffset = entryOffset;
			BankOffset = bankOffset;
			PaletteFormat = format;
		}


		/// <summary>
		/// Converts a texture palette to a GVP palette.
		/// </summary>
		/// <param name="palette">The palette to convert.</param>
		/// <param name="format">The format in which the palette should store the color values.</param>
		/// <param name="entryOffset">Palette entry offset</param>
		/// <param name="bankOffset">Palette bank offset</param>
		/// <returns>The converted palette</returns>
		public static GVP FromTexturePalette(TexturePalette palette, GVPaletteFormat format = GVPaletteFormat.Rgb5a3, ushort entryOffset = 0, ushort bankOffset = 0)
		{
			return new(palette.Name, palette.ColorData.ToArray(), format, entryOffset, bankOffset);
		}

		/// <summary>
		/// Creates a GVP palette from raw color values.
		/// </summary>
		/// <param name="colors">The RGBA8 colors to use.</param>
		/// <param name="format">The format in which the palette should store the color values.</param>
		/// <param name="entryOffset">Palette entry offset</param>
		/// <param name="bankOffset">Palette bank offset</param>
		/// <returns>The converted palette</returns>
		public static GVP FromColors(byte[] colors, GVPaletteFormat format = GVPaletteFormat.Rgb5a3, ushort entryOffset = 0, ushort bankOffset = 0)
		{
			return new(string.Empty, colors, format, entryOffset, bankOffset);
		}


		/// <summary>
		/// Checks whether data can be read as a GVP.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVP</returns>
		public static bool CheckIsGVP(EndianStackReader reader, uint address)
		{
			try
			{
				return GVHeader.GVPL.ValidateData(reader, address);
			}
			catch(ArgumentOutOfRangeException)
			{
				return false;
			}
		}

		/// <summary>
		/// Checks whether data can be read as a GVP.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVP</returns>
		public static bool CheckIsGVP(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsGVP(reader, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as a GVP.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a GVP</returns>
		public static bool CheckIsGVPFile(string filepath)
		{
			return CheckIsGVP(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads a GVP texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the GVP starts.</param>
		/// <param name="name">Name of the GVP texture.</param>
		/// <returns>The GVP that was read.</returns>
		public static GVP ReadGVP(EndianStackReader reader, uint address, string name)
		{
			GVP result = GVPIO.ReadGVP(reader, address);
			result.Name = name;
			return result;
		}

		/// <summary>
		/// Reads a GVP texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the GVP starts.</param>
		/// <returns>The GVP that was read.</returns>
		public static GVP ReadGVP(EndianStackReader reader, uint address)
		{
			return ReadGVP(reader, address, string.Empty);
		}



		/// <summary>
		/// Reads a GVP texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the GVP starts.</param>
		/// <param name="name">Name of the GVP texture.</param>
		/// <returns>The GVP that was read.</returns>
		public static GVP ReadGVP(byte[] data, uint address, string name)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadGVP(reader, address, name);
			}
		}

		/// <summary>
		/// Reads a GVP texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the GVP starts.</param>
		/// <returns>The GVP that was read.</returns>
		public static GVP ReadGVP(byte[] data, uint address)
		{
			return ReadGVP(data, address, string.Empty);
		}

		/// <summary>
		/// Reads a GVP texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The GVP that was read.</returns>
		public static GVP ReadGVPFromFile(string filepath)
		{
			return ReadGVP(File.ReadAllBytes(filepath), 0, Path.GetFileNameWithoutExtension(filepath));
		}


		/// <summary>
		/// Writes the GVP to an endian stack writer. Includes Global texture index.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WriteGVP(EndianStackWriter writer)
		{
			GVPIO.WriteGVP(this, writer);
		}

		/// <summary>
		/// Writes the GVP to a stream. Includes Global texture index.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteGVP(Stream stream)
		{
			WriteGVP(new EndianStackWriter(stream));
		}

		/// <summary>
		/// Writes the GVP to a byte array.
		/// </summary>
		/// <returns>The encoded GVP.</returns>
		public byte[] WriteGVPToBytes()
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				WriteGVP(writer);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes the GVP to a file
		/// </summary>
		/// <param name="filepath">Output filepath</param>
		public void WriteGVPToFile(string filepath)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteGVP(stream);
			}
		}

	}
}
