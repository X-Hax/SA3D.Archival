using Amicitia.IO.Binary;
using SA3D.Archival.Textures.GV;

namespace SA3D.Archival.PAK
{
	/// <summary>
	/// Texture index for a single texture file in a PAK archive.
	/// </summary>
	public struct PAKTextureInfo : IBinarySerializable
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
		public GVTextureFormat Type { get; set; }

		/// <summary>
		/// Texture bitdepth.
		/// </summary>
		public uint BitDepth { get; set; }

		/// <summary>
		/// GVR Pixel format of the texture.
		/// </summary>
		public GVTextureFormat PixelFormat { get; set; }

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


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Name = reader.ReadString(StringBinaryFormat.FixedLength, 28);
			GlobalIndex = reader.ReadUInt32();
			Type = (GVTextureFormat)reader.ReadUInt32();
			BitDepth = reader.ReadUInt32();
			PixelFormat = (GVTextureFormat)reader.ReadUInt32();
			Width = reader.ReadUInt32();
			Height = reader.ReadUInt32();
			DataSize = reader.ReadUInt32();
			Attributes = (PAKTextureAttributes)reader.ReadUInt32();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.FixedLength, Name, 28);
			writer.WriteUInt32(GlobalIndex);
			writer.WriteUInt32((uint)Type);
			writer.WriteUInt32(BitDepth);
			writer.WriteUInt32((uint)PixelFormat);
			writer.WriteUInt32(Width);
			writer.WriteUInt32(Height);
			writer.WriteUInt32(DataSize);
			writer.WriteUInt32((uint)Attributes);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Name}, {GlobalIndex}, {Type}-{PixelFormat}-{BitDepth}, {Width}x{Height}, {Attributes:X8}";
		}
	}
}
