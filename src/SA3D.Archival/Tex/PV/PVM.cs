using SA3D.Archival.Tex.PV.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.Collections.Generic;
using System.IO;

namespace SA3D.Archival.Tex.PV
{
	/// <summary>
	/// Texture archive encoding used in Dreamcast/Gamecube games and their ports.
	/// </summary>
	public class PVM : TextureArchive
	{
		/// <summary>
		/// PVR textures in the archive.
		/// </summary>
		public List<PVR> PVRs { get; }

		/// <summary>
		/// PVP palettes in the archive.
		/// </summary>
		public List<PVP> PVPs { get; }

		/// <summary>
		/// Model names added to the archive.
		/// </summary>
		public List<string> ModelNames { get; }

		/// <summary>
		/// Name of the converter used to encode the image data.
		/// </summary>
		public string ConverterName { get; set; }

		/// <summary>
		/// Custom comment embedded into the archive.
		/// </summary>
		public string Comment { get; set; }

		/// <inheritdoc/>
		public override IReadOnlyList<TextureArchiveEntry> TextureEntries => PVRs;


		/// <summary>
		/// Creates a new empty PVM archive.
		/// </summary>
		public PVM() : base()
		{
			PVRs = [];
			PVPs = [];
			ModelNames = [];
			ConverterName = string.Empty;
			Comment = string.Empty;
		}


		/// <inheritdoc/>
		public override void WriteContentIndex(TextWriter writer)
		{
			foreach(PVR pvr in PVRs)
			{
				writer.WriteLine(pvr.Name);
			}
		}

		/// <inheritdoc/>
		public override TextureSet ToTextureSet()
		{
			List<Texture> textures = [];
			foreach(PVR pvr in PVRs)
			{
				Texture texture = pvr.ToTexture();
				if(texture is IndexTexture indexTex)
				{
					indexTex.Palette = PVPs.Find(x => x.Name == pvr.Name);
				}

				textures.Add(texture);
			}

			return new(textures.ToArray());
		}

		/// <summary>
		/// Converts a texture set to a PVM archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="pixelFormat">The pixel format to use for every texture.</param>
		/// <param name="dataFormat">The data format to use for every texture.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <returns>The converted PVM archive</returns>
		public static PVM FromTextureSet(TextureSet textureSet, PVRPixelFormat pixelFormat = PVRPixelFormat.ARGB4, PVRDataFormat dataFormat = PVRDataFormat.Rectangle, bool dither = true)
		{
			PVM result = new();

			foreach(Texture texture in textureSet.Textures)
			{
				PVR gvr = PVR.EncodeToPVR(texture, out TexturePalette? palette, pixelFormat, dataFormat, dither);
				result.PVRs.Add(gvr);
				if(palette != null)
				{
					result.PVPs.Add(PVP.FromTexturePalette(palette, pixelFormat));
				}
			}

			return result;
		}



		/// <summary>
		/// Checks whether data can be read as a PVM.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVM</returns>
		public static bool CheckIsPVM(EndianStackReader reader, uint address)
		{
			return PVMIO.IsPVM(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a PVM.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVM</returns>
		public static bool CheckIsPVM(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return CheckIsPVM(reader, address);
			}
		}

		/// <summary>
		/// Checks whether a file can be read as a PVM.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a PVM</returns>
		public static bool CheckIsPVMFile(string filepath)
		{
			return CheckIsPVM(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Read a PVM texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which the PVM starts.</param>
		/// <returns>The PVM texture that was read.</returns>
		public static PVM ReadPVM(EndianStackReader reader, uint address)
		{
			return PVMIO.ReadPVM(reader, address);
		}

		/// <summary>
		/// Read a PVM texture from byte data.
		/// </summary>
		/// <param name="data">Byte array with the PVM.</param>
		/// <param name="address">Address at which the PVM starts.</param>
		/// <returns>The PVM texture that was read.</returns>
		public static PVM ReadPVM(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return ReadPVM(reader, address);
			}
		}

		/// <summary>
		/// Reads a PVM texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The PVM texture that was read.</returns>
		public static PVM ReadPVMFromFile(string filepath)
		{
			return ReadPVM(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Writes a PVM archive to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WritePVM(EndianStackWriter writer, PVMMetadataIncludes includes)
		{
			PVMIO.WritePVM(this, writer, includes);
		}

		/// <summary>
		/// Writes a PVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WritePVM(EndianStackWriter writer)
		{
			WritePVM(writer, PVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a PVM archive to an endian stack writer.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WritePVM(Stream stream, PVMMetadataIncludes includes)
		{
			EndianStackWriter writer = new(stream);
			WritePVM(writer, includes);
		}

		/// <summary>
		/// Writes a PVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WritePVM(Stream stream)
		{
			WritePVM(stream, PVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a PVM archive to an endian stack writer.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public void WritePVMToFile(string filepath, PVMMetadataIncludes includes)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WritePVM(stream, includes);
			}
		}

		/// <summary>
		/// Writes a PVM archive to an endian stack writer. Includes all metadata in the archive header table.
		/// </summary>
		/// <param name="filepath">The path to the file to write to.</param>
		public void WritePVMToFile(string filepath)
		{
			WritePVMToFile(filepath, PVMMetadataIncludes.IncludeAll);
		}

		/// <summary>
		/// Writes a PVM archive to a byte array.
		/// </summary>
		/// <param name="includes">Specifies which metadata to include in the archives header table.</param>
		public byte[] WritePVMToBytes(PVMMetadataIncludes includes)
		{
			using(MemoryStream stream = new())
			{
				WritePVM(stream, includes);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes a PVM archive to a byte array. Includes all metadata in the archive header table.
		/// </summary>
		public byte[] WritePVMToBytes()
		{
			return WritePVMToBytes(PVMMetadataIncludes.IncludeAll);
		}

		/// <inheritdoc/>
		public override void WriteArchive(EndianStackWriter writer)
		{
			WritePVM(writer);
		}


		/// <summary>
		/// Exports all PVRs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		/// <param name="includeGlobalIndices">Whether to include the global texture indices in the PVR files.</param>
		public void ExportPVRsAsFiles(string folderPath, bool includeGlobalIndices)
		{
			for(int i = 0; i < PVRs.Count; i++)
			{
				PVR pvr = PVRs[i];
				string name = string.IsNullOrWhiteSpace(pvr.Name) ? i.ToString() : pvr.Name;
				string pvrPath = Path.Join(folderPath, name + ".pvr");
				pvr.WritePVRToFile(pvrPath, includeGlobalIndices);
			}
		}

		/// <summary>
		/// Exports all PVPs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		public void ExportPVPsAsFiles(string folderPath)
		{
			for(int i = 0; i < PVPs.Count; i++)
			{
				PVP pvr = PVPs[i];
				string name = string.IsNullOrWhiteSpace(pvr.Name) ? i.ToString() : pvr.Name;
				string pvpPath = Path.Join(folderPath, name + ".pvp");
				pvr.WritePVPToFile(pvpPath);
			}
		}
	}
}
