using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.CommonV.IO
{
	internal static class VArchiveIO<
		TArchive,
		TTexture,
		TPalette,
		TArchiveMetaData,
		TArchiveVBlock,
		TArchiveInfoVBlock,
		TTextureVBlock,
		TPaletteVBlock,
		TPaletteNameVBlock>
		where TTexture : BaseVTexture, new()
		where TPalette : BaseVPalette, new()
		where TArchive : BaseVArchive<TTexture, TPalette>
		where TArchiveMetaData : struct, IBinarySerializable<VArchiveIncludes>
		where TArchiveVBlock : BaseArchiveVBlock<TTexture, TArchiveMetaData>, new()
		where TArchiveInfoVBlock : BaseArchiveInfoVBlock, new()
		where TTextureVBlock : BaseTextureVBlock
		where TPaletteVBlock : BasePaletteVBlock
		where TPaletteNameVBlock : BaseStringVBlock, new()
	{
		public static bool Check(BinaryObjectReader reader)
		{
			return VBlock.CheckBlockExists<TArchiveVBlock>(reader);
		}

		/// <inheritdoc/>
		public static void Read(TArchive archive, VBlock[] blocks)
		{
			TArchiveVBlock headBlock = blocks.FirstOfType<TArchiveVBlock>()
				?? throw new InvalidDataException("No (valid) archive block found!");

			archive.IncludeNames = headBlock.Attributes.HasFlag(VArchiveIncludes.Filenames);
			archive.IncludeCategoryCodes = headBlock.Attributes.HasFlag(VArchiveIncludes.CategoryCode);
			archive.IncludeEntryInfo = headBlock.Attributes.HasFlag(VArchiveIncludes.EntryInfo);
			archive.IncludeGlobalIndices = headBlock.Attributes.HasFlag(VArchiveIncludes.GlobalIndices);
			archive.IncludeArchiveInfo = headBlock.Attributes.HasFlag(VArchiveIncludes.ArchiveInfo);

			archive.Entries = [];
			archive.Palettes = [];
			archive.ModelNames = blocks.FirstOfType<ModelNamesVBlock>()?.Names.ToList() ?? [];
			archive.ConverterName = blocks.FirstOfType<ConverterNameVBlock>()?.Value ?? string.Empty;
			archive.Comment = blocks.FirstOfType<CommentVBlock>()?.Value ?? string.Empty;

			TArchiveInfoVBlock? archiveInfo = blocks.FirstOfType<TArchiveInfoVBlock>();
			ImageDataVBlock[] imageData = [.. blocks.OfType<ImageDataVBlock>()];

			string currentPaletteName = string.Empty;

			foreach(VBlock block in blocks)
			{
				switch(block)
				{
					case TPaletteNameVBlock paletteNameBlock:
						currentPaletteName = paletteNameBlock.Value;
						break;

					case TPaletteVBlock paletteBlock:
						TPalette palette = new()
						{
							Name = currentPaletteName
						};
						palette.FromBlock(paletteBlock);
						archive.Palettes.Add(palette);
						currentPaletteName = string.Empty;
						break;

					case TTextureVBlock textureBlock:
						int index = archive.Entries.Count;

						TTexture texture = new();
						texture.FromBlock(textureBlock);
						headBlock.CopyMetaDataToTexture(texture, index);

						if(imageData.Length > index)
						{
							texture.OriginalImageData = imageData[index].Data;
						}

						if(archiveInfo?.Inputs.Length > index)
						{
							(texture.OriginalFilePath, texture.ConversionArguments) = archiveInfo.Inputs[index];
						}

						archive.Entries.Add(texture);
						break;
				}
			}
		}

		private static VArchiveIncludes GetIncludes(TArchive archive)
		{
			VArchiveIncludes flags = default;
			if(archive.IncludeNames)
			{
				flags |= VArchiveIncludes.Filenames;
			}

			if(archive.IncludeCategoryCodes)
			{
				flags |= VArchiveIncludes.CategoryCode;
			}

			if(archive.IncludeEntryInfo)
			{
				flags |= VArchiveIncludes.EntryInfo;
			}

			if(archive.IncludeGlobalIndices)
			{
				flags |= VArchiveIncludes.GlobalIndices;
			}

			if(archive.IncludeArchiveInfo)
			{
				flags |= VArchiveIncludes.ArchiveInfo;
			}

			if(!string.IsNullOrWhiteSpace(archive.Comment))
			{
				flags |= VArchiveIncludes.Comment;
			}

			if(!string.IsNullOrWhiteSpace(archive.ConverterName))
			{
				flags |= VArchiveIncludes.ConverterName;
			}

			if(archive.ModelNames.Count > 0)
			{
				flags |= VArchiveIncludes.ModelNames;
			}

			if(archive.Entries.Count > 0)
			{
				flags |= VArchiveIncludes.Textures;

				if(archive.Entries.Any(x => !string.IsNullOrWhiteSpace(x.OriginalFilePath) || !string.IsNullOrWhiteSpace(x.ConversionArguments)))
				{
					flags |= VArchiveIncludes.paletteNames;
				}

				if(archive.Entries.Any(x => x.OriginalImageData.Length > 0))
				{
					flags |= VArchiveIncludes.ImageData;
				}
			}

			if(archive.Palettes.Count > 0)
			{
				flags |= VArchiveIncludes.Palettes;

				if(archive.Palettes.Any(x => !string.IsNullOrWhiteSpace(x.Name)))
				{
					flags |= VArchiveIncludes.paletteNames;
				}
			}

			return flags;
		}

		/// <inheritdoc/>
		public static void Write(TArchive archive, BinaryObjectWriter writer)
		{
			VArchiveIncludes includes = GetIncludes(archive);

			List<VBlock> blocks = [
				archive.ToBlock(includes)
			];

			if(includes.HasFlag(VArchiveIncludes.Comment))
			{
				blocks.Add(new CommentVBlock() { Value = archive.Comment });
			}

			if(includes.HasFlag(VArchiveIncludes.ConverterName))
			{
				blocks.Add(new ConverterNameVBlock() { Value = archive.ConverterName });
			}

			if(includes.HasFlag(VArchiveIncludes.ModelNames))
			{
				blocks.Add(new ModelNamesVBlock() { Names = [.. archive.ModelNames] });
			}

			if(includes.HasFlag(VArchiveIncludes.ArchiveInfo))
			{
				blocks.Add(new TArchiveInfoVBlock() { Inputs = [.. archive.Entries.Select(x => (x.OriginalFilePath, x.ConversionArguments))] });
			}

			if(includes.HasFlag(VArchiveIncludes.ImageData))
			{
				blocks.AddRange(archive.Entries.Select(x => new ImageDataVBlock() { Data = x.OriginalImageData }));
			}

			if(includes.HasFlag(VArchiveIncludes.Palettes))
			{
				foreach(TPalette palette in archive.Palettes)
				{
					if(includes.HasFlag(VArchiveIncludes.paletteNames))
					{
						blocks.Add(new TPaletteNameVBlock() { Value = palette.Name });
					}

					blocks.Add(palette.ToBlock());
				}
			}

			long start = writer.Position;

			for(int i = 0; i < blocks.Count; i++)
			{
				writer.WriteObject(blocks[i], new VBlockAlignment(start, i == blocks.Count - 1 ? 32 : 4));
			}

			VBlockAlignment textureAlignment = new(start, 32);

			if(includes.HasFlag(VArchiveIncludes.Textures))
			{
				foreach(TTexture pvr in archive.Entries)
				{
					writer.WriteObject(pvr.ToBlock(), textureAlignment);
				}
			}
		}
	}
}
