using SA3D.Archival.Tex.PVX.IO;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.Collections.Generic;
using System.IO;

namespace SA3D.Archival.Tex.PVX
{
	/// <summary>
	/// PVM. Based on PVM, designed by SF94.
	/// </summary>
	public class PVMX : TextureArchive
	{
		/// <summary>
		/// The PVMX archive entries.
		/// </summary>
		public List<PVRX> PVRXs { get; }

		/// <inheritdoc/>
		public override IReadOnlyList<TextureArchiveEntry> TextureEntries => PVRXs;

		/// <summary>
		/// Creates a new empty PVMX archive.
		/// </summary>
		public PVMX()
		{
			PVRXs = new();
		}

		/// <inheritdoc/>
		public override void WriteContentIndex(TextWriter writer)
		{
			for(int u = 0; u < PVRXs.Count; u++)
			{
				PVRX pvmxentry = (PVRX)Entries[u];
				writer.Write(pvmxentry.GlobalIndex);
				writer.Write(',');
				writer.Write(pvmxentry.Name);

				if(pvmxentry.HasDimensions)
				{
					writer.Write(',');
					writer.Write(pvmxentry.Width);
					writer.Write('x');
					writer.Write(pvmxentry.Height);
				}

				writer.WriteLine();
			}
		}

		/// <inheritdoc/>
		public override void WriteArchive(EndianStackWriter writer)
		{
			PVMXIO.WritePVMX(this, writer);
		}

		/// <summary>
		/// Converts a Texture set to a PVMX archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="indexInAlpha">Whether the index should be stored in the alpha channel, instead of outputing a grayscale image</param>
		/// <param name="useDDS">Whether to use DDS images, instead of PNG.</param>
		/// <returns>The converted PVMX archive.</returns>
		public static PVMX PVMXFromTextureSet(TextureSet textureSet, bool indexInAlpha = false, bool useDDS = false)
		{
			PVMX result = new();

			foreach(Texture texture in textureSet.Textures)
			{
				byte[] data = texture is IndexTexture indexTex
					? useDDS
						? indexTex.WriteIndexedAsDDSToBytes()
						: indexTex.WriteIndexedAsPNGToBytes(indexInAlpha)
					: useDDS
						? texture.WriteColoredAsDDSToBytes()
						: texture.WriteColoredAsPNGToBytes();

				result.PVRXs.Add(new(data, texture.GlobalIndex, texture.OverrideWidth, texture.OverrideHeight, texture.Name));
			}

			return result;
		}


		/// <summary>
		/// Checks whether data can be read as a PVMX.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVMX</returns>
		public static bool CheckIsPVMX(EndianStackReader reader, uint address)
		{
			return PVMXIO.CheckIsPVMX(reader, address);
		}

		/// <summary>
		/// Checks whether data can be read as a PVMX.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to check the data.</param>
		/// <returns>Whether the data can be read as a PVMX</returns>
		public static bool CheckIsPVMX(byte[] data, uint address)
		{
			return CheckIsPVMX(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether a file can be read as a PVMX.
		/// </summary>
		/// <param name="filepath">The file to check.</param>
		/// <returns>Whether the file can be read as a PVMX</returns>
		public static bool CheckIsPVMXFile(string filepath)
		{
			return CheckIsPVMX(File.ReadAllBytes(filepath), 0);
		}


		/// <summary>
		/// Reads a PVMX texture from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the PVMX starts.</param>
		/// <returns>The PVMX that was read.</returns>
		public static PVMX ReadPVMX(EndianStackReader reader, uint address)
		{
			return PVMXIO.ReadPVMX(reader, address);
		}

		/// <summary>
		/// Reads a PVMX texture from byte data.
		/// </summary>
		/// <param name="data">Byte data to read.</param>
		/// <param name="address">Address at which the PVMX starts.</param>
		/// <returns>The PVMX that was read.</returns>
		public static PVMX ReadPVMX(byte[] data, uint address)
		{
			return ReadPVMX(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Reads a PVMX texture from a file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The PVMX that was read.</returns>
		public static PVMX ReadPVMXFromFile(string filepath)
		{
			return ReadPVMX(File.ReadAllBytes(filepath), 0);
		}

	}
}
