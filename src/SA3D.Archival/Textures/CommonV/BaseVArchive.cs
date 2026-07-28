using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Common.IO;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Archival.Textures.CommonV
{
	/// <summary>
	/// Base "V" archive
	/// </summary>
	public abstract class BaseVArchive<TTexture, TPalette> : ITextureArchive 
		where TTexture : BaseVTexture 
		where TPalette : BaseVPalette
	{
		/// <summary>
		/// Textures in the archive.
		/// </summary>
		public List<TTexture> Entries { get; set; }

		/// <summary>
		/// Palettes in the archive.
		/// </summary>
		public List<TPalette> Palettes { get; set; }

		/// <summary>
		/// Model names added to the archive.
		/// </summary>
		public List<string> ModelNames { get; set; }

		/// <summary>
		/// Name of the converter used to encode the image data.
		/// </summary>
		public string ConverterName { get; set; }

		/// <summary>
		/// Custom comment embedded into the archive.
		/// </summary>
		public string Comment { get; set; }

		IReadOnlyList<ITextureArchiveEntry> ITextureArchive.Entries => Entries;


		#region Include information

		/// <summary>
		/// Whether to store PVR names.
		/// </summary>
		public bool IncludeNames { get; set; } = true;

		/// <summary>
		/// Whether to store PVR codec information.
		/// </summary>
		public bool IncludeCategoryCodes { get; set; } = true;

		/// <summary>
		/// Whether to store texture metadata, such as dimensions.
		/// </summary>
		public bool IncludeEntryInfo { get; set; } = true;

		/// <summary>
		/// Whether to store global texture indices.
		/// </summary>
		public bool IncludeGlobalIndices { get; set; } = true;

		/// <summary>
		/// Whether to store archive info (input filenames and converter options)
		/// </summary>
		public bool IncludeArchiveInfo { get; set; }

		#endregion

		/// <summary>
		/// Base Constructor
		/// </summary>
		protected BaseVArchive()
		{
			Entries = [];
			Palettes = [];
			ModelNames = [];
			ConverterName = string.Empty;
			Comment = string.Empty;
		}


		/// <inheritdoc/>
		public abstract bool Check(BinaryObjectReader reader);

		/// <inheritdoc/>
		public abstract void Read(BinaryObjectReader reader, FileContext context);

		/// <inheritdoc/>
		public abstract void Write(BinaryObjectWriter writer, FileContext context);

		/// <inheritdoc/>
		public string WriteContentIndex()
		{
			return string.Join('\n', Entries.Select(x => x.Name));
		}


		internal abstract VBlock ToBlock(VArchiveIncludes includes);
	}
}
