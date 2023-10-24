using SA3D.Archival.Tex.PVX.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.IO;

namespace SA3D.Archival.Tex.PVX
{
	/// <summary>
	/// Texture storage medium to store texture readable by DirectX. Based on PVR, designed by SF94.
	/// </summary>
	public class PVRX : TextureArchiveEntry
	{
		/// <summary>
		/// Whether the PVMX has valid texture dimensions (in pixels).
		/// </summary>
		public bool HasDimensions
			=> Width * Height != 0;

		internal PVRX(byte[] data, uint globalIndex, int width, int height, string name)
			: base(data, globalIndex, width, height, name) { }

		/// <inheritdoc/>
		public override Texture ToTexture()
		{
			Texture texture = base.ToTexture();
			texture.GlobalIndex = GlobalIndex;
			texture.OverrideHeight = Height;
			texture.OverrideWidth = Width;
			return texture;
		}

		/// <inheritdoc/>
		protected override Texture InternalGetMipmap(int mipmapIndex)
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// Encodes a colored texture to e PVMX texture using either PNG or DDS.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="useDDS">Whether to use DDS instead of PNG.</param>
		/// <returns>The encoded PVMX texture.</returns>
		public static PVRX EncodeColoredToPVRX(Texture texture, bool useDDS = false)
		{
			byte[] data = useDDS
				? texture.WriteColoredAsDDSToBytes()
				: texture.WriteColoredAsPNGToBytes();

			return new(data, texture.GlobalIndex, texture.OverrideWidth, texture.OverrideWidth, texture.Name);
		}

		/// <summary>
		/// Encodes an indexed texture to e PVMX texture using either PNG or DDS.
		/// </summary>
		/// <param name="texture">The texture to encode.</param>
		/// <param name="indexInAlpha">Whether to store the index in the images alpha component.</param>
		/// <param name="useDDS">Whether to use DDS instead of PNG.</param>
		/// <returns>The encoded PVMX texture.</returns>
		public static PVRX EncodeIndexedToPVRX(IndexTexture texture, bool indexInAlpha = false, bool useDDS = false)
		{
			byte[] data = useDDS
				? texture.WriteIndexedAsDDSToBytes()
				: texture.WriteIndexedAsPNGToBytes(indexInAlpha);

			return new(data, texture.GlobalIndex, texture.OverrideWidth, texture.OverrideWidth, texture.Name);
		}


		/// <summary>
		/// Checks whether data can be read as a PVRX.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVRX</returns>
		public static bool CheckIsPVRX(EndianStackReader reader, uint address)
		{
			return PVRXIO.CheckIsPVRX(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a PVRX.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVRX</returns>
		public static bool CheckIsPVRX(byte[] data, uint address)
		{
			return CheckIsPVRX(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether a file can be read as a PVRX.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a PVRX</returns>
		public static bool CheckIsPVRXFile(string filepath)
		{
			return CheckIsPVRX(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads a PVRX texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the PVRX starts.</param>
		/// <returns>The PVRX that was read.</returns>
		public static PVRX ReadPVRX(EndianStackReader reader, uint address)
		{
			return PVRXIO.ReadPVRX(reader, address);
		}

		/// <summary>
		/// Reads a PVRX texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the PVRX starts.</param>
		/// <returns>The PVRX that was read.</returns>
		public static PVRX ReadPVRX(byte[] data, uint address)
		{
			return ReadPVRX(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Reads a PVRX texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The PVRX that was read.</returns>
		public static PVRX ReadPVRXFromFile(string filepath)
		{
			return ReadPVRX(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Writes the PVRX to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void WritePVRX(EndianStackWriter writer)
		{
			PVRXIO.WritePVRX(this, writer);
		}

		/// <summary>
		/// Writes the PVRX to a stream.
		/// </summary>
		/// <param name="stream">The stream to write to.</param>
		public void WritePVRX(Stream stream)
		{
			WritePVRX(new EndianStackWriter(stream));
		}

		/// <summary>
		/// Writes the PVRX to a byte array.
		/// </summary>
		/// <returns>The encoded PVRX.</returns>
		public byte[] WritePVRXToBytes()
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				WritePVRX(writer);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes the PVRX to a file
		/// </summary>
		/// <param name="filepath">Output filepath</param>
		public void WritePVRXToFile(string filepath)
		{
			using(FileStream stream = File.Create(filepath))
			{
				WritePVRX(stream);
			}
		}

	}
}
