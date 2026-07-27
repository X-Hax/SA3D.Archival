using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal struct GVArchiveTextureMetaData : IBinarySerializable<VArchiveIncludes>
	{
		public ushort Index { get; set; }
		public string Filename { get; set; }
		public GVTextureAttributes TextureAttributes { get; set; }
		public GVPaletteFormat PaletteFormat { get; set; }
		public GVTextureFormat PixelFormat { get; set; }
		public GVArchiveEntryMetaAttributes EntryAttributes { get; set; }
		public ushort Width { get; set; }
		public ushort Height { get; set; }
		public uint GlobalIndex { get; set; }

		public void Read(BinaryObjectReader reader, VArchiveIncludes attributes)
		{
			Index = reader.ReadUInt16();

			if(attributes.HasFlag(VArchiveIncludes.Filenames))
			{
				Filename = reader.ReadString(StringBinaryFormat.FixedLength, 28);
			}

			if(attributes.HasFlag(VArchiveIncludes.CategoryCode))
			{
				byte paletteFormatAndFlags = reader.ReadByte();
				TextureAttributes = (GVTextureAttributes)(paletteFormatAndFlags & 0xF);
				PaletteFormat = (GVPaletteFormat)(paletteFormatAndFlags >> 4);
				PixelFormat = (GVTextureFormat)reader.ReadByte();
			}

			if(attributes.HasFlag(VArchiveIncludes.EntryInfo))
			{
				byte dimensions = reader.ReadByte();
				Width = (ushort)(4 << (dimensions & (0xF0 >> 4)));
				Height = (ushort)(4 << (dimensions & 0x0F));
				EntryAttributes = (GVArchiveEntryMetaAttributes)reader.ReadByte();
			}

			if(attributes.HasFlag(VArchiveIncludes.GlobalIndices))
			{
				GlobalIndex = reader.ReadUInt32();
			}
		}

		public readonly void Write(BinaryObjectWriter writer, VArchiveIncludes attributes)
		{
			writer.WriteUInt16(Index);

			if(attributes.HasFlag(VArchiveIncludes.Filenames))
			{
				writer.WriteString(StringBinaryFormat.FixedLength, Filename, 28);
			}

			if(attributes.HasFlag(VArchiveIncludes.CategoryCode))
			{
				byte paletteFormatAndFlags = (byte)((byte)TextureAttributes | ((byte)PaletteFormat << 4));

				writer.WriteByte(paletteFormatAndFlags);
				writer.WriteByte((byte)PixelFormat);
			}

			if(attributes.HasFlag(VArchiveIncludes.EntryInfo))
			{
				byte dimensions = (byte)((GetShift(Width) << 4) | GetShift(Height));
				static uint GetShift(ushort value)
				{
					uint result = 0;
					while(value > 7)
					{
						value >>= 1;
						result++;
					}

					return result;
				}

				writer.WriteByte(dimensions);
				writer.WriteByte((byte)EntryAttributes);
			}

			if(attributes.HasFlag(VArchiveIncludes.GlobalIndices))
			{
				writer.WriteUInt32(GlobalIndex);
			}
		}

		public override readonly string ToString()
		{
			return $"{Index}, \"{Filename}\", {PixelFormat}-{PaletteFormat}, {Width}x{Height}, {GlobalIndex}";
		}
	}
}
