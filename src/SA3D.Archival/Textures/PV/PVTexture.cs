using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Archival.Textures.PV.IO;
using SA3D.Archival.Textures.PV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.PV
{
	/// <summary>
	/// "PVR" Texture storage medium c
	/// </summary>
	public sealed class PVTexture : BaseVTexture
	{
		private PVRDataCodec _codec;

		/// <inheritdoc/>
		public override TextureType TextureType => DataFormat switch
		{
			PVTextureDataFormat.Index4 or PVTextureDataFormat.Index4Mipmaps => TextureType.Index4,
			PVTextureDataFormat.Index8 or PVTextureDataFormat.Index8Mipmaps => TextureType.Index8,
			_ => TextureType.RGBA32,
		};

		/// <inheritdoc/>
		public override bool HasMipMaps
		{
			get => _codec.HasMipmaps;
			set
			{
				if(value == _codec.HasMipmaps)
				{
					return;
				}

				DataFormat = DataFormat switch
				{
					PVTextureDataFormat.SquareTwiddled => PVTextureDataFormat.SquareTwiddledMipmaps,
					PVTextureDataFormat.SquareTwiddledMipmaps => PVTextureDataFormat.SquareTwiddled,
					PVTextureDataFormat.Vq => PVTextureDataFormat.VqMipmaps,
					PVTextureDataFormat.VqMipmaps => PVTextureDataFormat.Vq,
					PVTextureDataFormat.Index4 => PVTextureDataFormat.Index4Mipmaps,
					PVTextureDataFormat.Index4Mipmaps => PVTextureDataFormat.Index4,
					PVTextureDataFormat.Index8 => PVTextureDataFormat.Index8Mipmaps,
					PVTextureDataFormat.Index8Mipmaps => PVTextureDataFormat.Index8,
					PVTextureDataFormat.SmallVq => PVTextureDataFormat.SmallVqMipmaps,
					PVTextureDataFormat.SmallVqMipmaps => PVTextureDataFormat.SmallVq,
					PVTextureDataFormat.SquareTwiddledMipmapsDMA => PVTextureDataFormat.SquareTwiddled,
					_ => throw new InvalidOperationException($"Data format {DataFormat} cannot have mipmaps"),
				};
			}
		}


		/// <summary>
		/// Pixel format (if required by <see cref="DataFormat"/>)
		/// </summary>
		[MemberNotNull(nameof(_codec))]
		public PVPixelFormat PixelFormat
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
				_codec = DataFormat.CreateDataCodec(PixelFormat);
			}
		}

		/// <summary>
		/// How the pixels are laid out in the data.
		/// </summary>
		[MemberNotNull(nameof(_codec))]
		public PVTextureDataFormat DataFormat
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
				_codec = DataFormat.CreateDataCodec(PixelFormat);
			}
		}

		/// <summary>
		/// Entry attributes for PVM files
		/// </summary>
		public PVArchiveEntryMetaAttributes ArchiveMetaAttributes { get; set; }


		/// <summary>
		/// Creates a new PVR instance
		/// </summary>
		/// <param name="data">Texture data.</param>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels</param>
		/// <param name="dataFormat">Data format</param>
		/// <param name="pixelFormat">Pixel format (if required by dataFormat)</param>
		public PVTexture(PVTextureDataFormat dataFormat, PVPixelFormat pixelFormat, byte[] data, ushort width, ushort height) : base(data, width, height)
		{
			DataFormat = dataFormat;
			PixelFormat = pixelFormat;
		}

		/// <summary>
		/// Creates a new, empty PVR instance
		/// </summary>
		public PVTexture() : this(PVTextureDataFormat.Rectangle, PVPixelFormat.ARGB8, [], 0, 0) { }


		/// <summary>
		/// Encodes a texture to a PVR texture.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <param name="dataFormat">How the pixels are laid out in the data.</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded PVR texture.</returns>
		public static PVTexture CreateFromTexture(ITexture texture, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8, PVTextureDataFormat dataFormat = PVTextureDataFormat.Rectangle, bool dither = true)
		{
			PVTexture result = new()
			{
				DataFormat = dataFormat,
				PixelFormat = pixelFormat
			};

			result.EncodeTexture(texture, dither);
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
		/// Encodes raw texture data to a new PVTexture
		/// </summary>
		/// <param name="data">The texture data to encode</param>
		/// <param name="width">Width of the texture data</param>
		/// <param name="height">Height of the texture data</param>
		/// <param name="inputType">Format of the input texture data</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <param name="dataFormat">How the pixels are laid out in the data.</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded PVR texture.</returns>
		public static PVTexture CreateFromTextureData(ReadOnlySpan<byte> data, int width, int height, TextureType inputType, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8, PVTextureDataFormat dataFormat = PVTextureDataFormat.Rectangle, bool dither = true)
		{
			PVTexture result = new()
			{
				DataFormat = dataFormat,
				PixelFormat = pixelFormat
			};

			result.EncodeTextureData(data, width, height, inputType, dither);

			return result;
		}

		/// <summary>
		/// Encodes texture data from a mip map set to a new PVTexture
		/// </summary>
		/// <param name="set">Mip map set to encode</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <param name="dataFormat">How the pixels are laid out in the data.</param>
		/// <returns></returns>
		public static PVTexture CreateFromMipMapSet(IMipMapSet set, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8, PVTextureDataFormat dataFormat = PVTextureDataFormat.Rectangle)
		{
			PVTexture result = new()
			{
				DataFormat = dataFormat,
				PixelFormat = pixelFormat
			};

			result.EncodeMipMapSet(set);

			return result;
		}

		/// <summary>
		/// Encodes a textures data
		/// </summary>
		/// <param name="texture">The texture data to encode</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		public void EncodeTexture(ITexture texture, bool dither = true)
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

			EncodeTextureData(data, texture.Width, texture.Height, inputFormat, dither);
		}

		/// <summary>
		/// Encodes raw texture data
		/// </summary>
		/// <param name="data">The texture data to encode</param>
		/// <param name="width">Width of the texture data</param>
		/// <param name="height">Height of the texture data</param>
		/// <param name="inputType">Format of the input texture data</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		public void EncodeTextureData(ReadOnlySpan<byte> data, int width, int height, TextureType inputType, bool dither = true)
		{
			MipMapSet mipMapSet = MipMapSet.GenerateMipMaps(data, width, height, inputType, _codec.OutputType, out byte[]? paletteColors, !_codec.HasMipmaps, dither);
			EncodeMipMapSet(mipMapSet);
			Palette = paletteColors == null ? null : PVPalette.CreateFromColors(paletteColors, PixelFormat);
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
		}

		/// <inheritdoc/>
		protected override ReadOnlyMipMapSet DecodeTextureData()
		{
			return _codec.Decode(Data, Width, Height);
		}


		/// <inheritdoc/>
		public override bool Check(BinaryObjectReader reader, FileContext<TextureIOContext> context)
		{
			return VBlock.CheckBlockExists<PVTextureVBlock>(reader);
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, FileContext<TextureIOContext> context)
		{
			if(!context.Context.DataOnly && !string.IsNullOrEmpty(context.Filepath))
			{
				Name = Path.GetFileNameWithoutExtension(context.Filepath);
			}

			VBlock[] blocks = PVBlocks.ReadPVBlocks(reader);

			PVTextureVBlock block = blocks.OfType<PVTextureVBlock>().FirstOrDefault()
				?? throw new InvalidDataException("No (valid) texture block found!");

			PixelFormat = block.PixelFormat;
			DataFormat = block.DataFormat;
			Width = block.Width;
			Height = block.Height;
			Data = block.Data;

			if(blocks.OfType<GlobalIndexVBlock>().FirstOrDefault() is GlobalIndexVBlock globalIndexBlock)
			{
				GlobalIndex = globalIndexBlock.GlobalIndex;
			}
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, FileContext<TextureIOContext> context)
		{
			VBlockAlignment alignment = new(writer.Position, 32);

			if(context.Context.IncludeGlobalIndex)
			{
				writer.WriteObject(new GlobalIndexVBlock(GlobalIndex));
			}

			writer.WriteObject(ToBlock(), alignment);
		}

		internal override void FromBlock(VBlock block)
		{
			PVTextureVBlock pvBlock = (PVTextureVBlock)block;

			PixelFormat = pvBlock.PixelFormat;
			DataFormat = pvBlock.DataFormat;
			Width = pvBlock.Width;
			Height = pvBlock.Height;
			Data = pvBlock.Data;
		}

		internal override BaseTextureVBlock ToBlock()
		{
			return new PVTextureVBlock()
			{
				PixelFormat = PixelFormat,
				DataFormat = DataFormat,
				Width = (ushort)Width,
				Height = (ushort)Height,
				Data = Data
			};
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name} - {PixelFormat}, {DataFormat}";
		}
	}
}
