using SA3D.Archival.Tex.PV.IO;
using SA3D.Archival.Tex.PV.IO.DataCodec;
using SA3D.Archival.Tex.PV.IO.PixelCodec;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.IO;

namespace SA3D.Archival.Tex.PV
{
	/// <summary>
	/// Texture storage medium used in dreamcast games.
	/// </summary>
	public class PVR : TextureArchiveEntry
	{
		/// <summary>
		/// Pixel format (if required by <see cref="DataFormat"/>)
		/// </summary>
		public PVRPixelFormat PixelFormat { get; }

		/// <summary>
		/// How the pixels are laid out in the data.
		/// </summary>
		public PVRDataFormat DataFormat { get; }

		/// <inheritdoc/>
		public override bool? Index4
		{
			get
			{
				if(DataFormat is < PVRDataFormat.Index4 or > PVRDataFormat.Index8Mipmaps)
				{
					return null;
				}

				return DataFormat is PVRDataFormat.Index4 or PVRDataFormat.Index4Mipmaps;
			}
		}

		/// <inheritdoc/>
		public override bool HasMipMaps
			=> PVRDataCodec.Create(DataFormat, PVPixelCodec.GetPixelCodec(PixelFormat)).HasMipmaps;

		/// <summary>
		/// Full filename before conversion.
		/// </summary>
		public string OriginalFilePath { get; set; }

		/// <summary>
		/// Arguments applied for converting the original image.
		/// </summary>
		public string ConversionArguments { get; set; }

		/// <summary>
		/// Original image data before conversion.
		/// </summary>
		public byte[] OriginalImageData { get; set; }

		/// <summary>
		/// Entry attributes for PVM files
		/// </summary>
		public PVMMetaEntryAttributes ArchiveMetaEntryAttributes { get; set; }


		internal PVR(byte[] data, uint globalIndex, ushort width, ushort height, PVRPixelFormat pixelFormat, PVRDataFormat dataFormat, string name)
			: base(data, globalIndex, width, height, name)
		{
			PixelFormat = pixelFormat;
			DataFormat = dataFormat;

			OriginalFilePath = string.Empty;
			ConversionArguments = string.Empty;
			OriginalImageData = Array.Empty<byte>();
		}


		/// <summary>
		/// Encodes a texture to a PVR texture.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="palette">The generated palette, if the dataformat requires one and the texture is not storing just indices.</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <param name="dataFormat">How the pixels are laid out in the data.</param>
		/// <param name="dither">Whether to use dithering (when applicable).</param>
		/// <returns>The encoded PVR texture.</returns>
		public static PVR EncodeToPVR(Texture texture, out TexturePalette? palette, PVRPixelFormat pixelFormat = PVRPixelFormat.ARGB8, PVRDataFormat dataFormat = PVRDataFormat.Rectangle, bool dither = true)
		{
			byte[] data = PVRCodec.Encode(texture, pixelFormat, dataFormat, out palette, dither);
			return new PVR(data, texture.GlobalIndex, (ushort)texture.Width, (ushort)texture.Height, pixelFormat, dataFormat, texture.Name);
		}

		/// <inheritdoc/>
		public override Texture ToTexture()
		{
			byte[] pixels = PVRCodec.Decode(this);
			if(RequiresPallet)
			{
				return new IndexTexture(Width, Height, pixels, Name, GlobalIndex)
				{
					IsIndex4 = Index4!.Value
				};
			}
			else
			{
				return new ColorTexture(Width, Height, pixels, Name, GlobalIndex);
			}
		}

		/// <inheritdoc/>
		protected override Texture InternalGetMipmap(int mipmapIndex)
		{
			byte[] pixels = PVRCodec.DecodeMipmap(this, mipmapIndex);
			int size = Width >> (mipmapIndex + 1);
			if(RequiresPallet)
			{
				return new IndexTexture(size, size, pixels, Name, GlobalIndex)
				{
					IsIndex4 = Index4!.Value
				};
			}
			else
			{
				return new ColorTexture(size, size, pixels, Name, GlobalIndex);
			}
		}


		/// <summary>
		/// Checks whether data can be read as a PVR.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVR</returns>
		public static bool CheckIsPVR(EndianStackReader reader, uint address)
		{
			return PVRIO.IsPVR(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a PVR.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVR</returns>
		public static bool CheckIsPVR(byte[] data, uint address)
		{
			return CheckIsPVR(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether a file can be read as a PVR.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a PVR</returns>
		public static bool CheckIsPVRFile(string filepath)
		{
			return CheckIsPVR(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Read a PVR texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which the PVR starts.</param>
		/// <param name="name">Name of the PVR texture.</param>
		/// <returns>The PVR texture that was read.</returns>
		public static PVR ReadPVR(EndianStackReader reader, uint address, string name)
		{
			PVR result = PVRIO.ReadPVR(reader, address);
			result.Name = name;
			return result;
		}

		/// <summary>
		/// Read a PVR texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which the PVR starts.</param>
		/// <returns>The PVR texture that was read.</returns>
		public static PVR ReadPVR(EndianStackReader reader, uint address)
		{
			return ReadPVR(reader, address, string.Empty);
		}

		/// <summary>
		/// Read a PVR texture from byte data.
		/// </summary>
		/// <param name="data">Byte array with the PVR.</param>
		/// <param name="address">Address at which the PVR starts.</param>
		/// <param name="name">Name of the PVR texture.</param>
		/// <returns>The PVR texture that was read.</returns>
		public static PVR ReadPVR(byte[] data, uint address, string name)
		{
			return ReadPVR(new EndianStackReader(data), address, name);
		}

		/// <summary>
		/// Read a PVR texture from byte data.
		/// </summary>
		/// <param name="data">Byte array with the PVR.</param>
		/// <param name="address">Address at which the PVR starts.</param>
		/// <returns>The PVR texture that was read.</returns>
		public static PVR ReadPVR(byte[] data, uint address)
		{
			return ReadPVR(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Reads a PVR texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The PVR texture that was read.</returns>
		public static PVR ReadPVRFromFile(string filepath)
		{
			return ReadPVR(File.ReadAllBytes(filepath), 0, Path.GetFileNameWithoutExtension(filepath));
		}



		/// <summary>
		/// Writes the PVR to an endian stack writer.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		/// <param name="includeGlobalIndex">Whether to include the global texture index in the data.</param>
		public void WritePVR(EndianStackWriter writer, bool includeGlobalIndex)
		{
			PVRIO.WritePVR(this, writer, includeGlobalIndex, true);
		}

		/// <summary>
		/// Writes the PVR to an endian stack writer. Includes the global texture index.
		/// </summary>
		/// <param name="writer">Writer to write to.</param>
		public void WritePVR(EndianStackWriter writer)
		{
			WritePVR(writer, true);
		}

		/// <summary>
		/// Writes the PVR to a stream.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		/// <param name="includeGlobalIndex">Whether to include the global texture index in the data.</param>
		public void WritePVR(Stream stream, bool includeGlobalIndex)
		{
			WritePVR(new EndianStackWriter(stream), includeGlobalIndex);
		}

		/// <summary>
		/// Writes the PVR to a stream. Includes the global texture index.
		/// </summary>
		/// <param name="stream">Stream to write to.</param>
		public void WritePVR(Stream stream)
		{
			WritePVR(stream, true);
		}

		/// <summary>
		/// Writes the PVR out as a byte array
		/// </summary>
		/// <param name="includeGlobalIndex">Include the Global Texture index in the file</param>
		public byte[] WritePVRToByteData(bool includeGlobalIndex = true)
		{
			using(MemoryStream stream = new())
			{
				WritePVR(stream, includeGlobalIndex);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes the PVR to a file.
		/// </summary>
		/// <param name="filepath">Path to write the file to.</param>
		/// <param name="includeGlobalIndex">Include the Global Texture index in the file.</param>
		public void WritePVRToFile(string filepath, bool includeGlobalIndex = true)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WritePVR(stream, includeGlobalIndex);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return base.ToString() + $" - {PixelFormat}, {DataFormat}, #{MipMapCount}";
		}
	}
}
