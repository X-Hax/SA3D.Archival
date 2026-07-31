using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SA3D.Archival.Textures.CommonV
{
	/// <summary>
	/// Base "V" texture container
	/// </summary>
	public abstract class BaseVTexture : ITextureArchiveEntry
	{
		#region Data properties

		/// <summary>
		/// Decoded texture data
		/// </summary>
		[AllowNull]
		public ReadOnlyMipMapSet TextureData
		{
			get
			{
				field ??= DecodeTextureData();
				return field;
			}
			private set;
		}

		/// <summary>
		/// Encoded texture data
		/// </summary>
		public byte[] Data
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
			}
		}

		IMipMapSet IMipMapped.MipMaps => TextureData;

		ReadOnlySpan<byte> IArchiveEntry.Data => Data;

		#endregion

		#region Texture properties

		/// <inheritdoc/>
		public abstract TextureType TextureType { get; }

		/// <inheritdoc/>
		public virtual bool HasMipMaps
		{
			get;
			set
			{
				field = value;
				ClearTextureData();
			}
		}

		/// <inheritdoc/>
		public uint GlobalIndex { get; set; }

		/// <inheritdoc/>
		public int Width
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
			}
		}

		/// <inheritdoc/>
		public int Height
		{
			get;
			set
			{
				ClearTextureData();
				field = value;
			}
		}

		/// <inheritdoc/>
		public int OverrideWidth => 0;

		/// <inheritdoc/>
		public int OverrideHeight => 0;

		#region Index texture propertes

		/// <inheritdoc/>
		public ITexturePalette? Palette { get; set; }

		/// <inheritdoc/>
		public int PaletteRow { get; set; }

		#endregion

		#endregion

		#region Archive Data

		/// <inheritdoc/>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Full filename before conversion.
		/// </summary>
		public string OriginalFilePath { get; set; } = string.Empty;

		/// <summary>
		/// Arguments applied for converting the original image.
		/// </summary>
		public string ConversionArguments { get; set; } = string.Empty;

		/// <summary>
		/// Original image data before conversion.
		/// </summary>
		public byte[] OriginalImageData { get; set; } = [];

		/// <summary>
		/// Whether the file uses a GV-based global index (GCIX instead of GBIX)
		/// </summary>
		public bool UsesGVGlobalIndex { get; set; }

		#endregion

		/// <summary>
		/// Base constructor
		/// </summary>
		/// <param name="data">Encoded texture data</param>
		/// <param name="width">Texture Width</param>
		/// <param name="height">Texture Height</param>
		protected BaseVTexture(byte[] data, int width, int height)
		{
			Data = data;
			Width = width;
			Height = height;
			Name = string.Empty;
		}


		/// <summary>
		/// Decodes the archives data
		/// </summary>
		protected abstract ReadOnlyMipMapSet DecodeTextureData();

		/// <summary>
		/// Clears decoded texture data
		/// </summary>
		protected void ClearTextureData()
		{
			TextureData = null;
		}


		/// <inheritdoc/>
		public bool CheckIsTransparent()
		{
			if(TextureType == TextureType.RGBA32)
			{
				return TextureUtilities.CheckIsTextureTransparent(TextureData[0].Data);
			}
			else
			{
				return TextureUtilities.CheckIndexTextureUsesTransparency(this.GetUsedPaletteColors(), TextureData[0].Data, ((IIndexTexture)this).IsIndex4);
			}
		}

		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetRGBA32Data(int mipMapLevel = 0)
		{
			if(TextureType == TextureType.RGBA32)
			{
				return TextureData[mipMapLevel].Data;
			}
			else
			{
				return TextureUtilities.ApplyPaletteToIndexTexture(this.GetUsedPaletteColors(), TextureData[mipMapLevel].Data, ((IIndexTexture)this).IsIndex4);
			}
		}

		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetIndexPixelData(int mipMapLevel = 0)
		{
			if(TextureType == TextureType.RGBA32)
			{
				throw new InvalidOperationException("Texture does not have index data");
			}

			return TextureData[mipMapLevel].Data;
		}


		/// <inheritdoc/>
		public abstract bool Check(BinaryObjectReader reader, FileContext<TextureIOContext> context);

		/// <inheritdoc/>
		public abstract void Read(BinaryObjectReader reader, FileContext<TextureIOContext> context);

		/// <inheritdoc/>
		public abstract void Write(BinaryObjectWriter writer, FileContext<TextureIOContext> context);



		internal abstract void FromBlock(VBlock block);

		internal abstract BaseTextureVBlock ToBlock();
	}
}
