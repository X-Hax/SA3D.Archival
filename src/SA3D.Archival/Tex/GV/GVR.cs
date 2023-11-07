using SA3D.Archival.Tex.GV.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.IO;

namespace SA3D.Archival.Tex.GV
{
	/// <summary>
	/// Texture storage medium used in gamecube games.
	/// </summary>
	public class GVR : TextureArchiveEntry
	{
		/// <summary>
		/// Pixel format (if required by the data format
		/// </summary>
		public GVRPixelFormat PixelFormat { get; }

		/// <summary>
		/// Internal GRV palette. If palette entries are > 0 and no internal palette is given, an external one will be required
		/// </summary>
		public GVP? InternalPalette { get; set; }

		/// <inheritdoc/>
		public override bool? Index4
		{
			get
			{
				if(PixelFormat is GVRPixelFormat.Index4 or GVRPixelFormat.Index8)
				{
					return PixelFormat == GVRPixelFormat.Index4;
				}

				return null;
			}
		}

		/// <inheritdoc/>
		public override bool HasMipMaps { get; }

		/// <summary>
		/// Full filepath before conversion.
		/// </summary>
		public string OriginalFilePath { get; set; }
		
		/// <summary>
		/// Arguments applied for converting the original image.
		/// </summary>
		public string ConversionArguments { get; set; }

		/// <summary>
		/// Original image data before conversion.
		/// </summary>
		public byte[] OriginalImageData { get; set; }

		/// <summary>
		/// Entry attributes for GVM files
		/// </summary>
		public GVMMetaEntryAttributes ArchiveMetaEntryAttributes { get; set; }


		internal GVR(byte[] data, uint globalIndex, ushort width, ushort height, GVRPixelFormat pixelFormat, GVP? internalPalette, bool hasMipMaps, string name)
			: base(data, globalIndex, width, height, name)
		{
			PixelFormat = pixelFormat;
			InternalPalette = internalPalette;
			HasMipMaps = hasMipMaps;

			OriginalFilePath = string.Empty;
			ConversionArguments = string.Empty;
			OriginalImageData = Array.Empty<byte>();
		}


		/// <summary>
		/// Encodes a texture to a GVR texture.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <param name="mipmaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">Palette pixel format to use if the texture has a palette to convert.</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded GVR texture.</returns>
		public static GVR EncodeToGVR(Texture texture, GVRPixelFormat pixelFormat = GVRPixelFormat.DXT1, bool mipmaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			byte[] data = GVRCodec.Encode(texture, pixelFormat, mipmaps, out GVP? palette, paletteFormat, dither);
			return new GVR(data, texture.GlobalIndex, (ushort)texture.Width, (ushort)texture.Height, pixelFormat, palette, mipmaps, texture.Name);
		}

		/// <inheritdoc/>
		public override Texture ToTexture()
		{
			byte[] pixels = GVRCodec.Decode(this);
			if(RequiresPallet)
			{
				return new IndexTexture(Width, Height, pixels, Name, GlobalIndex)
				{
					IsIndex4 = Index4!.Value,
					Palette = InternalPalette
				};
			}
			else
			{
				return new ColorTexture(Width, Height, pixels, Name, GlobalIndex);
			}
		}

		/// <inheritdoc/>
		protected override Texture InternalGetMipmap(int mipmapIndex)
		{
			byte[] pixels = GVRCodec.DecodeMipmap(this, mipmapIndex);
			int size = Width >> (mipmapIndex + 1);
			if(RequiresPallet)
			{
				return new IndexTexture(size, size, pixels, Name, GlobalIndex)
				{
					IsIndex4 = Index4!.Value,
					Palette = InternalPalette
				};
			}
			else
			{
				return new ColorTexture(size, size, pixels, Name, GlobalIndex);
			}
		}


		/// <summary>
		/// Checks whether data can be read as a GVR.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVR</returns>
		public static bool CheckIsGVR(EndianStackReader reader, uint address)
		{
			return GVRIO.IsGVR(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a GVR.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVR</returns>
		public static bool CheckIsGVR(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsGVR(reader, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as a GVR.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a GVR</returns>
		public static bool CheckIsGVRFile(string filepath)
		{
			return CheckIsGVR(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads a GVR texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the GVR starts.</param>
		/// <param name="name">Name of the GVR texture.</param>
		/// <returns>The GVR that was read.</returns>
		public static GVR ReadGVR(EndianStackReader reader, uint address, string name)
		{
			GVR result = GVRIO.ReadGVR(reader, address);
			result.Name = name;
			return result;
		}

		/// <summary>
		/// Reads a GVR texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the GVR starts.</param>
		/// <returns>The GVR that was read.</returns>
		public static GVR ReadGVR(EndianStackReader reader, uint address)
		{
			return ReadGVR(reader, address, string.Empty);
		}

		/// <summary>
		/// Reads a GVR texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the GVR starts.</param>
		/// <param name="name">Name of the GVR texture.</param>
		/// <returns>The GVR that was read.</returns>
		public static GVR ReadGVR(byte[] data, uint address, string name)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadGVR(reader, address, name);
			}
		}

		/// <summary>
		/// Reads a GVR texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the GVR starts.</param>
		/// <returns>The GVR that was read.</returns>
		public static GVR ReadGVR(byte[] data, uint address)
		{
			return ReadGVR(data, address, string.Empty);
		}

		/// <summary>
		/// Reads a GVR texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The GVR that was read.</returns>
		public static GVR ReadGVRFromFile(string filepath)
		{
			return ReadGVR(File.ReadAllBytes(filepath), 0, Path.GetFileNameWithoutExtension(filepath));
		}


		/// <summary>
		/// Writes the GVR to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="includeGlobalIndex">Include the Global Texture index.</param>
		public void WriteGVR(EndianStackWriter writer, bool includeGlobalIndex)
		{
			GVRIO.WriteGVR(this, writer, includeGlobalIndex, true);
		}

		/// <summary>
		/// Writes the GVR to an endian stack writer. Includes Global texture index.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WriteGVR(EndianStackWriter writer)
		{
			WriteGVR(writer, false);
		}

		/// <summary>
		/// Writes the GVR to a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="includeGlobalIndex">Include the Global Texture index.</param>
		public void WriteGVR(Stream stream, bool includeGlobalIndex)
		{
			WriteGVR(new EndianStackWriter(stream), includeGlobalIndex);
		}

		/// <summary>
		/// Writes the GVR to a stream. Includes Global texture index.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteGVR(Stream stream)
		{
			WriteGVR(new EndianStackWriter(stream));
		}

		/// <summary>
		/// Writes the GVR to a byte array.
		/// </summary>
		/// <param name="includeGlobalIndex">Include the Global Texture index in the file</param>
		/// <returns>The encoded GVR.</returns>
		public byte[] WriteGVRToBytes(bool includeGlobalIndex = true)
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				WriteGVR(writer, includeGlobalIndex);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes the GVR to a file
		/// </summary>
		/// <param name="filepath">Output filepath</param>
		/// <param name="includeGlobalIndex">Include the Global Texture index in the file</param>
		public void WriteGVRToFile(string filepath, bool includeGlobalIndex = true)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteGVR(stream, includeGlobalIndex);
			}
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return base.ToString() + $" - {PixelFormat}, {InternalPalette?.ToString() ?? "-"}, #{MipMapCount}";
		}
	}
}
