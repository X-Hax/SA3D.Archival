using SA3D.Archival.Tex.GV.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.Collections.Generic;
using System.IO;

namespace SA3D.Archival.Tex.GV
{
	/// <summary>
	/// Texture archive encoding used in Gamecube games and their ports.
	/// </summary>
	public class GVM : TextureArchive
	{
		/// <summary>
		/// GVR textures in the archive
		/// </summary>
		public List<GVR> GVRs { get; }

		/// <summary>
		/// GVP palettes in the archive
		/// </summary>
		public List<GVP> GVPs { get; }

		/// <summary>
		/// Model names added to the archive
		/// </summary>
		public List<string> ModelNames { get; }

		/// <summary>
		/// Name of the converter used to encode the image data
		/// </summary>
		public string ConverterName { get; set; }

		/// <summary>
		/// Custom comment embedded into the archive
		/// </summary>
		public string Comment { get; set; }

		/// <inheritdoc/>
		public override IReadOnlyList<TextureArchiveEntry> TextureEntries => GVRs;


		/// <summary>
		/// Creates a new empty GVM archive.
		/// </summary>
		public GVM() : base()
		{
			GVRs = new();
			GVPs = new();
			ModelNames = new();
			ConverterName = string.Empty;
			Comment = string.Empty;
		}

		/// <inheritdoc/>
		public override void WriteContentIndex(TextWriter writer)
		{
			foreach(GVR gvr in GVRs)
			{
				writer.WriteLine(gvr.Name);
			}
		}

		/// <inheritdoc/>
		public override TextureSet ToTextureSet()
		{
			List<Texture> textures = new();
			foreach(GVR gvr in GVRs)
			{
				Texture texture = gvr.ToTexture();
				if(texture is IndexTexture indexTex)
				{
					indexTex.Palette = gvr.InternalPalette ?? (TexturePalette?)GVPs.Find(x => x.Name == gvr.Name);
				}

				textures.Add(texture);
			}

			return new(textures.ToArray());
		}

		/// <summary>
		/// Converts a texture set to a GVM archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="pixelFormat">The pixel format to encode the textures with.</param>
		/// <param name="mipmaps">Whether to create mipmaps for the textures.</param>
		/// <param name="paletteFormat">The pixel format to encode palettes with.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <returns>The converted GVM archive.</returns>
		public static GVM FromTextureSet(TextureSet textureSet, GVRPixelFormat? pixelFormat = null, bool mipmaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			GVM result = new();

			foreach(Texture texture in textureSet.Textures)
			{
				GVRPixelFormat format;
				if(pixelFormat != null)
				{
					format = pixelFormat.Value;
				}
				else
				{
					System.ReadOnlySpan<byte> colorPixel = texture.GetColorPixels();
					bool transparency = false;
					for(int i = 3; i < colorPixel.Length; i += 4)
					{
						byte val = colorPixel[i];
						if(val is > 0x1F and < 0xE0)
						{
							transparency = true;
							break;
						}
					}

					format = transparency ? GVRPixelFormat.RGB5A3 : GVRPixelFormat.DXT1;
				}

				GVR gvr = GVR.EncodeToGVR(texture, format, mipmaps, paletteFormat, dither);
				result.GVRs.Add(gvr);
			}

			return result;
		}


		/// <summary>
		/// Checks whether data can be read as a GVM.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVM</returns>
		public static bool CheckIsGVM(EndianStackReader reader, uint address)
		{
			return GVMIO.CheckIsGVM(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a GVM.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a GVM</returns>
		public static bool CheckIsGVM(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsGVM(reader, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as a GVM.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a GVM</returns>
		public static bool CheckIsGVMFile(string filepath)
		{
			return CheckIsGVM(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Read a GVM texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which the GVM starts.</param>
		/// <returns>The GVM texture that was read.</returns>
		public static GVM ReadGVM(EndianStackReader reader, uint address)
		{
			return GVMIO.ReadGVM(reader, address);
		}

		/// <summary>
		/// Read a GVM texture from byte data.
		/// </summary>
		/// <param name="data">Byte array with the GVM.</param>
		/// <param name="address">Address at which the GVM starts.</param>
		/// <returns>The GVM texture that was read.</returns>
		public static GVM ReadGVM(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadGVM(reader, address);
			}
		}

		/// <summary>
		/// Reads a GVM texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The GVM texture that was read.</returns>
		public static GVM ReadGVMFromFile(string filepath)
		{
			return ReadGVM(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Writes a GVM archive to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WriteGVM(EndianStackWriter writer, GVMMetadataIncludes includes)
		{
			GVMIO.WriteGVM(this, writer, includes);
		}

		/// <summary>
		/// Writes a GVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WriteGVM(EndianStackWriter writer)
		{
			WriteGVM(writer, GVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a GVM archive to an endian stack writer.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WriteGVM(Stream stream, GVMMetadataIncludes includes)
		{
			EndianStackWriter writer = new(stream);
			WriteGVM(writer, includes);
		}

		/// <summary>
		/// Writes a GVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WriteGVM(Stream stream)
		{
			WriteGVM(stream, GVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a GVM archive to an endian stack writer.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WriteGVMToFile(string filepath, GVMMetadataIncludes includes)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WriteGVM(stream, includes);
			}
		}

		/// <summary>
		/// Writes a GVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		public void WriteGVMToFile(string filepath)
		{
			WriteGVMToFile(filepath, GVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a GVM archive to a byte array.
		/// </summary>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public byte[] WriteGVMToBytes(GVMMetadataIncludes includes)
		{
			using(MemoryStream stream = new())
			{
				WriteGVM(stream, includes);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes a GVM archive to a byte array. Includes all metadata in the archive header table.
		/// </summary>
		public byte[] WriteGVMToByte()
		{
			return WriteGVMToBytes(GVMMetadataIncludes.IncludeAll);
		}

		/// <inheritdoc/>
		public override void WriteArchive(EndianStackWriter writer)
		{
			WriteGVM(writer);
		}


		/// <summary>
		/// Exports all GVRs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		/// <param name="includeGlobalIndices">Whether to include the global texture indices in the GVR files.</param>
		public void ExportGVRsAsFiles(string folderPath, bool includeGlobalIndices)
		{
			for(int i = 0; i < GVRs.Count; i++)
			{
				GVR pvr = GVRs[i];
				string name = string.IsNullOrWhiteSpace(pvr.Name) ? i.ToString() : pvr.Name;
				pvr.WriteGVRToFile($"{folderPath}\\{name}.gvr", includeGlobalIndices);
			}
		}

		/// <summary>
		/// Exports all GVPs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		public void ExportGVPsAsFiles(string folderPath)
		{
			for(int i = 0; i < GVPs.Count; i++)
			{
				GVP pvr = GVPs[i];
				string name = string.IsNullOrWhiteSpace(pvr.Name) ? i.ToString() : pvr.Name;
				pvr.WriteGVPToFile($"{folderPath}\\{name}.gvp");
			}
		}
	}
}
