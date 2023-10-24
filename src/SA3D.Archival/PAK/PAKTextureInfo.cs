using SA3D.Archival.Tex.GV;
using SA3D.Common.IO;

namespace SA3D.Archival.PAK
{
	/// <summary>
	/// Texture index for a single texture file in a PAK archive.
	/// </summary>
	public struct PAKTextureInfo
	{
		/// <summary>
		/// Size of the structure.
		/// </summary>
		public const uint StructSize = 0x3C;

		/// <summary>
		/// Name of the texture.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Global texture index.
		/// </summary>
		public uint GlobalIndex { get; set; }

		/// <summary>
		/// The type of the texture (?).
		/// </summary>
		public GVRPixelFormat Type { get; set; }

		/// <summary>
		/// Texture bitdepth.
		/// </summary>
		public uint BitDepth { get; set; }

		/// <summary>
		/// GVR Pixel format of the texture.
		/// </summary>
		public GVRPixelFormat PixelFormat { get; set; }

		/// <summary>
		/// Texture width in pixels.
		/// </summary>
		public uint Width { get; set; }

		/// <summary>
		/// Texture height in pixels.
		/// </summary>
		public uint Height { get; set; }

		/// <summary>
		/// Texture data size in bytes.
		/// </summary>
		public uint DataSize { get; set; }

		/// <summary>
		/// Additional texture info attributes.
		/// </summary>
		public PAKTextureAttributes Attributes { get; set; }

		/// <summary>
		/// Creates a new PAK texture info entry.
		/// </summary>
		/// <param name="name">Name of the texture.</param>
		/// <param name="globalIndex">Global texture index.</param>
		/// <param name="type">The type of the texture (?).</param>
		/// <param name="bitDepth">Texture bitdepth.</param>
		/// <param name="pixelFormat">GVR Pixel format of the texture.</param>
		/// <param name="width">Texture width in pixels.</param>
		/// <param name="height">Texture height in pixels.</param>
		/// <param name="dataSize">Texture size in bytes.</param>
		/// <param name="attributes">Additional texture info attributes.</param>
		public PAKTextureInfo(string name, uint globalIndex, GVRPixelFormat type, uint bitDepth, GVRPixelFormat pixelFormat, uint width, uint height, uint dataSize, PAKTextureAttributes attributes)
		{
			Name = name;
			GlobalIndex = globalIndex;
			Type = type;
			BitDepth = bitDepth;
			PixelFormat = pixelFormat;
			Width = width;
			Height = height;
			DataSize = dataSize;
			Attributes = attributes;
		}

		/// <summary>
		/// Reads a texture info struct from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read the struct.</param>
		/// <returns>The read struct.</returns>
		public static PAKTextureInfo Read(EndianStackReader reader, uint address)
		{
			return new(
				reader.ReadStringLimited(address, 28, out _),
				reader.ReadUInt(address + 0x1C),
				(GVRPixelFormat)reader.ReadUInt(address + 0x20),
				reader.ReadUInt(address + 0x24),
				(GVRPixelFormat)reader.ReadUInt(address + 0x28),
				reader.ReadUInt(address + 0x2C),
				reader.ReadUInt(address + 0x30),
				reader.ReadUInt(address + 0x34),
				(PAKTextureAttributes)reader.ReadUInt(address + 0x38));
		}

		/// <summary>
		/// Writes the texture info as a struct to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteString(Name);
			if(Name.Length < 28)
			{
				writer.WriteEmpty((uint)(28 - Name.Length));
			}

			writer.WriteUInt(GlobalIndex);
			writer.WriteUInt((uint)Type);
			writer.WriteUInt(BitDepth);
			writer.WriteUInt((uint)PixelFormat);
			writer.WriteUInt(Width);
			writer.WriteUInt(Height);
			writer.WriteUInt(DataSize);
			writer.WriteUInt((uint)Attributes);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Name}, {GlobalIndex}, {Type}-{PixelFormat}-{BitDepth}, {Width}x{Height}, {Attributes:X8}";
		}
	}
}
