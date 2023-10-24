using SA3D.Archival.Tex.GV;
using SA3D.Archival.Tex.PV;
using SA3D.Archival.Tex.PVX;
using SA3D.Common.IO;
using System.Collections.Generic;

namespace SA3D.Archival.Tex
{
	/// <summary>
	/// Archive designed to store a collection of textures.
	/// </summary>
	public abstract class TextureArchive : Archive
	{
		/// <summary>
		/// Invidiual textures stored in the texture archive.
		/// </summary>
		public abstract IReadOnlyList<TextureArchiveEntry> TextureEntries { get; }

		/// <inheritdoc/>
		public sealed override IReadOnlyList<ArchiveEntry> Entries => TextureEntries;


		/// <summary>
		/// Tries to read a texture archive.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read the archive.</param>
		/// <returns>The read texture archive.</returns>
		/// <exception cref="InvalidArchiveException"></exception>
		public static TextureArchive ReadTextureArchive(EndianStackReader reader, uint address)
		{
			if(PVM.CheckIsPVM(reader, address))
			{
				return PVM.ReadPVM(reader, address);
			}
			else if(GVM.CheckIsGVM(reader, address))
			{
				return GVM.ReadGVM(reader, address);
			}
			else if(PVMX.CheckIsPVMX(reader, address))
			{
				return PVMX.ReadPVMX(reader, address);
			}
			else
			{
				throw new InvalidArchiveException("Data is not any implemented texture archive!");
			}
		}

		/// <summary>
		/// Tries to read a texture archive from byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which to read the archive.</param>
		/// <returns>The read texture archive.</returns>
		/// <exception cref="InvalidArchiveException"></exception>
		public static TextureArchive ReadTextureArchive(byte[] data, uint address)
		{
			return ReadTextureArchive(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Tries to read a texture archive from a file.
		/// </summary>
		/// <param name="filepath">The path to the file to read.</param>
		/// <returns>The read texture archive.</returns>
		/// <exception cref="InvalidArchiveException"></exception>
		public static TextureArchive ReadTextureArchiveFromFile(string filepath)
		{
			return ReadTextureArchive(PRS.ReadPRSFile(filepath), 0);
		}

	}
}
