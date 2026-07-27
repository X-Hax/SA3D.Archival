using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.IO;
using SA3D.Texturing.MipMapping;
using SA3D.Texturing.ReadOnly;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace SA3D.Archival.Textures.PVX
{
	/// <summary>
	/// Texture storage medium to store texture readable by DirectX. Based on PVR, designed by SF94.
	/// </summary>
	public class PVXTexture : ITextureArchiveEntry
	{
		private const uint _pvrxHeader = 0x58525650;
		private const byte _version = 1;

		#region Data Properties

		/// <summary>
		/// Decoded texture data
		/// </summary>
		[AllowNull]
		public ReadOnlyMipMapSet TextureData
		{
			get
			{
				field ??= ReadOnlyMipMapSet.Copy(TextureFileUtilities.ReadImageFromBytes(Data, Name).TextureData);
				return field;
			}

			private set;
		}

		/// <summary>
		/// Archive data
		/// </summary>
		public byte[] Data
		{
			get;
			set
			{
				TextureData = null;
				field = value;
			}

		}

		IMipMapSet IMipMapped.MipMaps => TextureData;

		ReadOnlySpan<byte> IArchiveEntry.Data => Data;

		#endregion

		#region Archive properties

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public TextureType TextureType => TextureType.RGBA32;

		/// <inheritdoc/>
		public bool HasMipMaps => TextureData.LevelCount > 0;

		#endregion

		#region Texture properties

		/// <inheritdoc/>
		public uint GlobalIndex { get; set; }

		/// <inheritdoc/>
		public int Width => TextureData.BaseWidth;

		/// <inheritdoc/>
		public int Height => TextureData.BaseHeight;

		/// <inheritdoc/>
		public int OverrideWidth { get; set; }

		/// <inheritdoc/>
		public int OverrideHeight { get; set; }

		#region Index texture propertes

		/// <inheritdoc/>
		public ITexturePalette? Palette
		{
			get => null;
			set { }
		}

		/// <inheritdoc/>
		public int PaletteRow
		{
			get => 0;
			set { }
		}

		#endregion

		#endregion


		private PVXTexture(byte[] data, ReadOnlyMipMapSet textureData, string name)
		{
			Data = data;
			TextureData = textureData;
			Name = name;
		}

		/// <summary>
		/// Creates a new PVRX instance
		/// </summary>
		/// <param name="data">Texture data.</param>
		/// <param name="name">Name of the entry.</param>
		public PVXTexture(byte[] data, string name)
		{
			Data = data;
			Name = name;
		}

		/// <summary>
		/// Creates a new, empty PVRX instance
		/// </summary>
		public PVXTexture() : this([], string.Empty) { }


		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetRGBA32Data(int mipMapLevel = 0)
		{
			return TextureData[mipMapLevel].Data;
		}

		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetIndexPixelData(int mipMapLevel = 0)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public bool CheckIsTransparent()
		{
			return TextureUtilities.CheckIsTextureTransparent(GetRGBA32Data());
		}


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			uint header = reader.ReadUInt32();
			byte version = reader.ReadByte();

			return reader.Length > 6
				&& header == _pvrxHeader
				&& (version is > 0 and <= _version);
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext<TextureIOContext> context)
		{
			if(!context.Context.DataOnly)
			{
				if(reader.ReadUInt32() != _pvrxHeader)
				{
					throw new InvalidDataException("Data is not a PVRX!");
				}

				byte version = reader.ReadByte();
				if(version is 0 or > _version)
				{
					throw new InvalidDataException($"PVRX has unsupported version {version}!");
				}
			}
			else if(!string.IsNullOrEmpty(context.Filepath))
			{
				Name = Path.GetFileNameWithoutExtension(context.Filepath);
			}

			for(PVXArchiveDictionaryField type = (PVXArchiveDictionaryField)reader.ReadByte();
				type != PVXArchiveDictionaryField.none;
				type = (PVXArchiveDictionaryField)reader.ReadByte())
			{
				switch(type)
				{
					case PVXArchiveDictionaryField.global_index:
						GlobalIndex = reader.ReadUInt32();
						break;

					case PVXArchiveDictionaryField.name:
						Name = reader.ReadString(Encoding.UTF8, StringBinaryFormat.NullTerminated);
						break;

					case PVXArchiveDictionaryField.dimensions:
						OverrideWidth = reader.ReadInt32();
						OverrideHeight = reader.ReadInt32();
						break;

					case PVXArchiveDictionaryField.none:
						throw new UnreachableException();
					default:
						break;
				}
			}

			using OffsetBinaryFormatToken format = reader.WithBinaryOffsetFormat(OffsetBinaryFormat.U64);
			long offset = reader.ReadOffsetValue();
			long length = reader.ReadOffsetValue();

			Data = reader.ReadArrayAtOffset<byte>(offset, (int)length);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext<TextureIOContext> context)
		{
			if(!context.Context.DataOnly)
			{
				writer.WriteUInt32(_pvrxHeader);
				writer.WriteByte(_version);
			}

			writer.WriteByte((byte)PVXArchiveDictionaryField.global_index);
			writer.WriteUInt32(GlobalIndex);

			writer.WriteByte((byte)PVXArchiveDictionaryField.name);
			writer.WriteString(Encoding.UTF8, StringBinaryFormat.NullTerminated, Name);

			writer.WriteByte((byte)PVXArchiveDictionaryField.dimensions);
			writer.WriteInt32(Width);
			writer.WriteInt32(Height);

			writer.WriteByte((byte)PVXArchiveDictionaryField.none);

			using OffsetBinaryFormatToken format = writer.WithBinaryOffsetFormat(OffsetBinaryFormat.U64);

			writer.WriteOffset(() =>
			{
				writer.WriteArray(Data);
				writer.Align(4);
			});

			writer.WriteOffsetValue(Data.LongLength);

			if(!context.Context.DataOnly)
			{
				writer.Align(4);
			}
		}


		/// <summary>
		/// Creates a PVRX from a texture
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="format">Image format to use</param>
		/// <returns>The encoded PVMX texture.</returns>
		public static PVXTexture CreateFromTexture(ITexture texture, ImageFormat format)
		{
			return new(texture.WriteImageToBytes(format), new ReadOnlyTexture(texture).TextureData, texture.Name)
			{
				GlobalIndex = texture.GlobalIndex,
				OverrideWidth = texture.OverrideWidth,
				OverrideHeight = texture.OverrideHeight
			};
		}

	}
}
