using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.GV.IO;
using SA3D.Archival.Textures.GV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.IO;
using System.Linq;
using GVArchiveIO = SA3D.Archival.Textures.CommonV.IO.VArchiveIO<
	SA3D.Archival.Textures.GV.GVArchive,
	SA3D.Archival.Textures.GV.GVTexture,
	SA3D.Archival.Textures.GV.GVPalette,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVArchiveTextureMetaData,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVArchiveVBlock,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVArchiveInfoVBlock,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVTextureVBlock,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVPaletteVBlock,
	SA3D.Archival.Textures.GV.IO.VBlocks.GVPaletteNameVBlock>;

namespace SA3D.Archival.Textures.GV
{
	/// <summary>
	/// GV Texture archive encoding used in Dreamcast/Gamecube games and their ports.
	/// </summary>
	public sealed class GVArchive : BaseVArchive<GVTexture, GVPalette>
	{
		/// <summary>
		/// Creates a new empty GVM archive.
		/// </summary>
		public GVArchive() : base() { }


		/// <summary>
		/// Converts a texture set to a GVM archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="textureFormat">The texture format to use.</param>
		/// <param name="addMipMaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">The palette format to use for index texture formats.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <returns>The converted GVM archive</returns>
		public static GVArchive FromTextureSet(ITextureSet textureSet, GVTextureFormat textureFormat = GVTextureFormat.ARGB8, bool addMipMaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			GVArchive result = new();

			foreach(ITexture texture in textureSet.Textures)
			{
				GVTexture pvr = GVTexture.CreateFromTexture(texture, textureFormat, addMipMaps, paletteFormat, dither);
				result.Entries.Add(pvr);

				if(pvr.Palette != null)
				{
					if(pvr.Palette is not GVPalette palette)
					{
						palette = GVPalette.CreateFromPalette(pvr.Palette, paletteFormat);
					}

					result.Palettes.Add(palette);
				}
			}

			return result;
		}


		/// <summary>
		/// Exports all GVRs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		/// <param name="includeGlobalIndices">Whether to include the global texture indices in the GVR files.</param>
		public void ExportGVRsAsFiles(string folderPath, bool includeGlobalIndices)
		{
			TextureIOContext context = new()
			{
				IncludeGlobalIndex = includeGlobalIndices
			};

			for(int i = 0; i < Entries.Count; i++)
			{
				GVTexture gvr = Entries[i];
				string name = string.IsNullOrWhiteSpace(gvr.Name) ? i.ToString() : gvr.Name;
				string gvrPath = Path.Join(folderPath, name + ".gvr");
				gvr.WriteToFile(context, gvrPath);
			}
		}

		/// <summary>
		/// Exports all GVPs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		public void ExportGVPsAsFiles(string folderPath)
		{
			for(int i = 0; i < Palettes.Count; i++)
			{
				GVPalette palette = Palettes[i];
				string name = string.IsNullOrWhiteSpace(palette.Name) ? i.ToString() : palette.Name;
				string gvpPath = Path.Join(folderPath, name + ".gvp");
				palette.WriteToFile(gvpPath);
			}
		}


		internal override VBlock ToBlock(VArchiveIncludes includes)
		{
			GVTextureVBlock[] textureBlocks = Entries.Select(e => (GVTextureVBlock)e.ToBlock()).ToArray();

			GVArchiveTextureMetaData[] metaData = [.. Entries.Select((entry, i) => new GVArchiveTextureMetaData()
			{
				Index = (ushort)i,
				Filename = entry.Name,
				TextureAttributes = textureBlocks[i].TextureAttributes,
				PixelFormat = entry.TextureFormat,
				PaletteFormat = textureBlocks[i].PaletteFormat,
				EntryAttributes = entry.ArchiveMetaAttributes,
				Width = (ushort)entry.Width,
				Height = (ushort)entry.Height,
				GlobalIndex = entry.GlobalIndex,
			})];


			return new GVArchiveVBlock()
			{
				Attributes = includes,
				MetaData = metaData
			};
		}


		/// <inheritdoc/>
		protected override bool CheckCanReadFile(BinaryObjectReader reader, ref FileIOInfo info)
		{
			return GVArchiveIO.Check(reader);
		}

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader)
		{
			VBlock[] blocks = GVBlocks.ReadGVBlocks(reader);
			GVArchiveIO.Read(this, blocks);
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer)
		{
			GVArchiveIO.Write(this, writer);
		}
	}
}
