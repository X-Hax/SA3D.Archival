using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.PV.IO;
using SA3D.Archival.Textures.PV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.IO;
using System.Linq;

using PVArchiveIO = SA3D.Archival.Textures.CommonV.IO.VArchiveIO<
	SA3D.Archival.Textures.PV.PVArchive,
	SA3D.Archival.Textures.PV.PVTexture,
	SA3D.Archival.Textures.PV.PVPalette,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVArchiveTextureMetaData,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVArchiveVBlock,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVArchiveInfoVBlock,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVTextureVBlock,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVPaletteVBlock,
	SA3D.Archival.Textures.PV.IO.VBlocks.PVPaletteNameVBlock>;

namespace SA3D.Archival.Textures.PV
{
	/// <summary>
	/// PV Texture archive encoding used in Dreamcast/Gamecube games and their ports.
	/// </summary>
	public sealed class PVArchive : BaseVArchive<PVTexture, PVPalette>
	{
		/// <summary>
		/// Creates a new empty PVM archive.
		/// </summary>
		public PVArchive() : base()
		{
			Entries = [];
			Palettes = [];
			ModelNames = [];
			ConverterName = string.Empty;
			Comment = string.Empty;
		}


		/// <summary>
		/// Converts a texture set to a PVM archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="pixelFormat">The pixel format to use for every texture.</param>
		/// <param name="dataFormat">The data format to use for every texture.</param>
		/// <param name="dither">Whether to utilize dithering.</param>
		/// <returns>The converted PVM archive</returns>
		public static PVArchive FromTextureSet(ITextureSet textureSet, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8, PVTextureDataFormat dataFormat = PVTextureDataFormat.Rectangle, bool dither = true)
		{
			PVArchive result = new();

			foreach(ITexture texture in textureSet.Textures)
			{
				PVTexture pvr = PVTexture.CreateFromTexture(texture, pixelFormat, dataFormat, dither);
				result.Entries.Add(pvr);

				if(pvr.Palette != null)
				{
					if(pvr.Palette is not PVPalette palette)
					{
						palette = PVPalette.CreateFromPalette(pvr.Palette, pixelFormat);
					}

					result.Palettes.Add(palette);
				}
			}

			return result;
		}


		/// <summary>
		/// Exports all PVRs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		/// <param name="includeGlobalIndices">Whether to include the global texture indices in the PVR files.</param>
		public void ExportPVRsAsFiles(string folderPath, bool includeGlobalIndices)
		{
			TextureIOContext context = new()
			{
				IncludeGlobalIndex = includeGlobalIndices
			};

			for(int i = 0; i < Entries.Count; i++)
			{
				PVTexture pvr = Entries[i];
				string name = string.IsNullOrWhiteSpace(pvr.Name) ? i.ToString() : pvr.Name;
				string pvrPath = Path.Join(folderPath, name + ".pvr");
				FileUtil.WriteToFile(pvr, pvrPath, context);
			}
		}

		/// <summary>
		/// Exports all PVPs included in the archive as individual files.
		/// </summary>
		/// <param name="folderPath">The path to the folder to write the files to.</param>
		public void ExportPVPsAsFiles(string folderPath)
		{
			for(int i = 0; i < Palettes.Count; i++)
			{
				PVPalette palette = Palettes[i];
				string name = string.IsNullOrWhiteSpace(palette.Name) ? i.ToString() : palette.Name;
				string pvpPath = Path.Join(folderPath, name + ".pvp");
				FileUtil.WriteToFile(palette, pvpPath);
			}
		}


		internal override VBlock ToBlock(VArchiveIncludes includes)
		{
			PVArchiveTextureMetaData[] metaData = [.. Entries.Select((entry, i) => new PVArchiveTextureMetaData()
			{
				Index = (ushort)i,
				Filename = entry.Name,
				PixelFormat = entry.PixelFormat,
				DataFormat = entry.DataFormat,
				EntryAttributes = entry.ArchiveMetaAttributes,
				Width = (ushort)entry.Width,
				Height = (ushort)entry.Height,
				GlobalIndex = entry.GlobalIndex,
			})];


			return new PVArchiveVBlock()
			{
				Attributes = includes,
				MetaData = metaData
			};
		}

		/// <inheritdoc/>
		public override bool Check(BinaryObjectReader reader)
		{
			return PVArchiveIO.Check(reader);
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, FileContext context)
		{
			VBlock[] blocks = PVBlocks.ReadPVBlocks(reader);
			PVArchiveIO.Read(this, blocks);
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, FileContext context)
		{
			PVArchiveIO.Write(this, writer);
		}
	}
}
