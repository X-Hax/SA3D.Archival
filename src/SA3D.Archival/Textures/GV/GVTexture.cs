using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Archival.Textures.GV.IO;
using SA3D.Archival.Textures.GV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.GV
{
	/// <summary>
	/// Texture storage medium used in gamecube games.
	/// </summary>
	public sealed class GVTexture : BaseVTexture
	{
		private GVTextureCodec _codec;

		/// <inheritdoc/>
		public override TextureType TextureType => TextureFormat switch
		{
			GVTextureFormat.Index4 => TextureType.Index4,
			GVTextureFormat.Index8 => TextureType.Index8,
			_ => TextureType.RGBA32
		};

		/// <inheritdoc/>
		public override bool HasMipMaps
		{
			get;
			set
			{
				field = value;
				ClearTextureData();
			}
		}

		/// <summary>
		/// Whether to store <see cref="BaseVTexture.Palette"/> internally
		/// </summary>
		public bool StorePalette { get; set; }

		/// <summary>
		/// Texture format
		/// </summary>
		[MemberNotNull(nameof(_codec))]
		public GVTextureFormat TextureFormat
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
				_codec = TextureFormat.GetPixelCodec();
			}
		}

		/// <summary>
		/// Entry attributes for GVArchive files
		/// </summary>
		public GVArchiveEntryMetaAttributes ArchiveMetaAttributes { get; set; }


		/// <summary>
		/// Creates a new GVTexture instance
		/// </summary>
		/// <param name="pixelFormat">Pixel format</param>
		/// <param name="data">Texture data.</param>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels</param>
		public GVTexture(GVTextureFormat pixelFormat, byte[] data, ushort width, ushort height) : base(data, width, height)
		{
			TextureFormat = pixelFormat;
		}

		/// <summary>
		/// Creates a new, empty GVTexture instance
		/// </summary>
		public GVTexture() : this(GVTextureFormat.DXT1, [], 0, 0) { }


		/// <summary>
		/// Encodes a texture to a GVTexture.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="textureFormat">The pixel format to encode to.</param>
		/// <param name="addMipMaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">Palette format to use when encoding a color texture to an indexed format</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded PVR texture.</returns>
		public static GVTexture CreateFromTexture(ITexture texture, GVTextureFormat textureFormat = GVTextureFormat.ARGB8, bool addMipMaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			GVTexture result = new()
			{
				TextureFormat = textureFormat
			};

			result.EncodeTexture(texture, addMipMaps, paletteFormat, dither);
			result.Name = texture.Name;
			result.GlobalIndex = texture.GlobalIndex;

			if(texture is IIndexTexture indexTexture && result.Palette == null)
			{
				result.Palette = indexTexture.Palette;
				result.PaletteRow = indexTexture.PaletteRow;
			}

			return result;
		}

		/// <summary>
		/// Encodes a texture to a GVTexture.
		/// </summary>
		/// <param name="data">The texture data to encode</param>
		/// <param name="width">Width of the texture data</param>
		/// <param name="height">Height of the texture data</param>
		/// <param name="inputType">Format of the input texture data</param>
		/// <param name="textureFormat">The texture format to encode to.</param>
		/// <param name="addMipMaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">Palette format to use when encoding a color texture to an indexed format</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded PVR texture.</returns>
		public static GVTexture CreateFromTextureData(ReadOnlySpan<byte> data, int width, int height, TextureType inputType, GVTextureFormat textureFormat = GVTextureFormat.ARGB8, bool addMipMaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			GVTexture result = new()
			{
				TextureFormat = textureFormat
			};

			result.EncodeTextureData(data, width, height, inputType, addMipMaps, paletteFormat, dither);

			return result;
		}

		/// <summary>
		/// Encodes texture data from a mip map set
		/// </summary>
		/// <param name="set">The mip map set to encode</param>
		/// <param name="textureFormat">The texture format to encode to.</param>
		/// <returns></returns>
		public static GVTexture CreateFromMipMapSet(IMipMapSet set, GVTextureFormat textureFormat = GVTextureFormat.ARGB8)
		{
			GVTexture result = new()
			{
				TextureFormat = textureFormat
			};

			result.EncodeMipMapSet(set);
			return result;
		}

		/// <summary>
		/// Encodes a textures data
		/// </summary>
		/// <param name="texture">The texture data to encode</param>
		/// <param name="addMipMaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">Palette format to use when encoding a color texture to an indexed format</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		public void EncodeTexture(ITexture texture, bool addMipMaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			ReadOnlySpan<byte> data;
			TextureType inputFormat;

			if(texture is IIndexTexture indexTexture)
			{
				data = indexTexture.GetIndexPixelData();
				inputFormat = indexTexture.IsIndex4 ? TextureType.Index4 : TextureType.Index8;
			}
			else
			{
				data = texture.GetRGBA32Data();
				inputFormat = TextureType.RGBA32;
			}

			EncodeTextureData(data, texture.Width, texture.Height, inputFormat, addMipMaps, paletteFormat, dither);
		}

		/// <summary>
		/// Encodes raw texture data
		/// </summary>
		/// <param name="data">The texture data to encode</param>
		/// <param name="width">Width of the texture data</param>
		/// <param name="height">Height of the texture data</param>
		/// <param name="inputType">Format of the input texture data</param>
		/// <param name="addMipMaps">Whether to generate mipmaps</param>
		/// <param name="paletteFormat">Palette format to use when encoding a color texture to an indexed format</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		public void EncodeTextureData(ReadOnlySpan<byte> data, int width, int height, TextureType inputType, bool addMipMaps = true, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3, bool dither = true)
		{
			MipMapSet mipMapSet = MipMapSet.GenerateMipMaps(data, width, height, inputType, _codec.OutputType, out byte[]? paletteColors, addMipMaps, dither);
			EncodeMipMapSet(mipMapSet);
			Palette = paletteColors == null ? null : GVPalette.CreateFromColors(paletteColors, paletteFormat);
			PaletteRow = 0;
		}

		/// <summary>
		/// Encodes texture data from a mip map set
		/// </summary>
		/// <param name="set">Already set-up texture data</param>
		public void EncodeMipMapSet(IMipMapSet set)
		{
			Data = _codec.Encode(set);
			Width = set.BaseWidth;
			Height = set.BaseHeight;
			HasMipMaps = set.LevelCount > 1;
		}

		/// <inheritdoc/>
		protected override ReadOnlyMipMapSet DecodeTextureData()
		{
			return _codec.Decode(Data, Width, Height, HasMipMaps);
		}


		/// <inheritdoc/>
		protected override bool CheckCanReadFile(BinaryObjectReader reader, TextureIOContext context, ref FileIOInfo info)
		{
			return VBlock.CheckBlockExists<GVTextureVBlock>(reader);
		}

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader, TextureIOContext context)
		{
			VBlock[] blocks = GVBlocks.ReadGVBlocks(reader);

			GVTextureVBlock block = blocks.OfType<GVTextureVBlock>().FirstOrDefault()
				?? throw new InvalidDataException("No (valid) texture block found!");

			TextureFormat = block.TextureFormat;
			Width = block.Width;
			Height = block.Height;
			HasMipMaps = block.TextureAttributes.HasFlag(GVTextureAttributes.Mipmaps);
			Data = block.Data;

			if(_codec.OutputType != TextureType.RGBA32 && block.TextureAttributes.HasFlag(GVTextureAttributes.InternalPalette))
			{
				int paletteWidth = _codec.OutputType.GetPaletteWidth();
				int paletteSize = block.PaletteFormat.GetPaletteCodec().BytesPerPixel * paletteWidth;

				Palette = new GVPalette(block.PaletteFormat, Data[..paletteSize], paletteWidth);
				StorePalette = true;

				Data = Data[paletteSize..];
			}

			if(blocks.OfType<GlobalIndexBEVBlock>().FirstOrDefault() is GlobalIndexBEVBlock globalIndexBlock)
			{
				GlobalIndex = globalIndexBlock.GlobalIndex;
			}
			else if(blocks.OfType<GVGlobalIndexVBlock>().FirstOrDefault() is GVGlobalIndexVBlock gvglobalIndexBlock)
			{
				GlobalIndex = gvglobalIndexBlock.GlobalIndex;
			}
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer, TextureIOContext context)
		{
			VBlockAlignment alignment = new(writer.Position, 32);

			if(context.IncludeGlobalIndex)
			{
				if(UsesGVGlobalIndex)
				{
					writer.WriteObject(new GVGlobalIndexVBlock(GlobalIndex));
				}
				else
				{
					writer.WriteObject(new GlobalIndexBEVBlock(GlobalIndex));
				}
			}

			writer.WriteObject(ToBlock(), alignment);
		}


		internal override void FromBlock(VBlock block)
		{
			GVTextureVBlock pvBlock = (GVTextureVBlock)block;

			TextureFormat = pvBlock.TextureFormat;
			Width = pvBlock.Width;
			Height = pvBlock.Height;
			Data = pvBlock.Data;
		}

		internal override BaseTextureVBlock ToBlock()
		{
			GVTextureAttributes attributes = default;
			if(HasMipMaps)
			{
				attributes |= GVTextureAttributes.Mipmaps;
			}

			GVPaletteFormat paletteFormat = default;
			byte[] data = Data;

			if(TextureType != TextureType.RGBA32)
			{
				if(Palette != null && StorePalette)
				{
					if(Palette is not GVPalette gvp)
					{
						gvp = GVPalette.CreateFromPalette(Palette, 
							TextureUtilities.CheckIsTextureTransparent(Palette.GetColorData())
								? GVPaletteFormat.Rgb5a3
								: GVPaletteFormat.Rgb565
						);
					}

					data = [.. gvp.Data, .. Data];
					paletteFormat = gvp.PaletteFormat;
					attributes |= GVTextureAttributes.InternalPalette;
				}
				else
				{
					attributes |= GVTextureAttributes.ExternalPalette;
				}
			}

			return new GVTextureVBlock()
			{
				TextureAttributes = attributes,
				PaletteFormat = paletteFormat,
				TextureFormat = TextureFormat,
				Width = (ushort)Width,
				Height = (ushort)Height,
				Data = data
			};
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name} - {TextureFormat}";
		}
	}
}
