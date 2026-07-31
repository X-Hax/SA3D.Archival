using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.PVX
{
	/// <summary>
	/// PVM. Based on PVM, designed by SF94.
	/// </summary>
	public sealed class PVXArchive : ITextureArchive
	{
		private const uint _pvmxHeader = 0x584D5650;
		private const byte _version = 1;


		/// <summary>
		/// The PVMX archive entries.
		/// </summary>
		public List<PVXTexture> Entries { get; private set; }

		IReadOnlyList<ITextureArchiveEntry> ITextureArchive.Entries => Entries;


		/// <summary>
		/// Creates a new PVMX archive
		/// </summary>
		/// <param name="entries"></param>
		public PVXArchive(IEnumerable<PVXTexture> entries)
		{
			Entries = [.. entries];
		}

		/// <summary>
		/// Creates a new empty PVMX archive.
		/// </summary>
		public PVXArchive() : this([]) { }


		/// <summary>
		/// Converts a texture set to a PVMX archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="imageFormat">Image format to write images as</param>
		/// <returns>The converted PVMX archive.</returns>
		public static PVXArchive CreateFromTextureSet(ITextureSet textureSet, ImageFormat imageFormat)
		{
			return new(textureSet.Textures.Select(x => PVXTexture.CreateFromTexture(x, imageFormat)));
		}


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader, FileContext context)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			uint header = reader.ReadUInt32();
			byte version = reader.ReadByte();

			return reader.Length > 6
				&& header == _pvmxHeader
				&& (version is > 0 and <= _version);
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext context)
		{
			if(reader.ReadUInt32() != _pvmxHeader)
			{
				throw new InvalidDataException("Data is not a PVMX archive!");
			}

			byte version = reader.ReadByte();
			if(version is 0 or > _version)
			{
				throw new InvalidDataException($"PVMX has unsupported version {version}!");
			}

			bool HasData()
			{
				if(reader.Position >= reader.Length)
				{
					return false;
				}

				using SeekToken seekToken = reader.At();
				return reader.ReadByte() != 0;
			}

			FileContext<TextureIOContext> textureContext = new()
			{
				Context = new()
				{
					DataOnly = true
				}
			};

			Entries = [];
			while(HasData())
			{
				Entries.Add(reader.ReadObject<PVXTexture, FileContext<TextureIOContext>>(textureContext));
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext context)
		{
			FileContext<TextureIOContext> textureContext = new()
			{
				Context = new()
				{
					DataOnly = true
				}
			};

			writer.WriteUInt32(_pvmxHeader);
			writer.WriteByte(_version);
			writer.WriteObjectArray(Entries, textureContext);
		}

		/// <inheritdoc/>
		public string WriteContentIndex()
		{
			using StringWriter writer = new();

			foreach(PVXTexture pvrx in Entries)
			{
				writer.Write(pvrx.GlobalIndex);
				writer.Write(',');
				writer.Write(pvrx.Name);

				if(pvrx.OverrideHeight != 0 && pvrx.OverrideWidth != 0)
				{
					writer.Write(',');
					writer.Write(pvrx.Width);
					writer.Write('x');
					writer.Write(pvrx.Height);
				}

				writer.WriteLine();
			}

			return writer.ToString();
		}
	}
}
