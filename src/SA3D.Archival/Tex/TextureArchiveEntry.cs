using SA3D.Texturing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SA3D.Archival.Tex
{
	/// <summary>
	/// Base class for texture archive entries.
	/// </summary>
	public abstract class TextureArchiveEntry : ArchiveEntry
	{
		/// <summary>
		/// Global texture index.
		/// </summary>
		public uint GlobalIndex { get; set; }

		/// <summary>
		/// Width of the texture in pixels.
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Height of the texture in pixels.
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Whether the texture requires a pallet.
		/// </summary>
		[MemberNotNullWhen(true, nameof(Index4))]
		public bool RequiresPallet => Index4 != null;

		/// <summary>
		/// Type of the indexed texture, indicating the size of the needed external palette.
		/// </summary>
		public virtual bool? Index4 => null;

		/// <summary>
		/// Whether the texture has mipmaps.
		/// </summary>
		public virtual bool HasMipMaps => false;

		/// <summary>
		/// Number of mipmaps stored with the texture.
		/// </summary>
		public int MipMapCount => HasMipMaps ? (int)Math.Log(Width, 2) : 0;


		/// <summary>
		/// Base constructor for texture archive entries.
		/// </summary>
		/// <param name="data">Texture data.</param>
		/// <param name="globalIndex">Global texture index.</param>
		/// <param name="width">Width of the texture in pixels.</param>
		/// <param name="height">Height of the texture in pixels</param>
		/// <param name="name">Name of the entry.</param>
		protected TextureArchiveEntry(byte[] data, uint globalIndex, int width, int height, string name) : base(data, name)
		{
			GlobalIndex = globalIndex;
			Width = width;
			Height = height;
		}


		/// <summary>
		/// Internal method for getting the mipmap. Only gets called when the mipmap index is in range.
		/// </summary>
		/// <param name="mipmapIndex">The verified mipmap index.</param>
		/// <returns>The mipmap texture.</returns>
		protected abstract Texture InternalGetMipmap(int mipmapIndex);


		/// <summary>
		/// Returns a specific mipmap of the texture.
		/// </summary>
		/// <param name="mipmapIndex">The index of the mipmap to get.</param>
		/// <returns>The mipmap texture.</returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Texture GetMipmap(int mipmapIndex)
		{
			if(mipmapIndex < 0 || Width >> (mipmapIndex + 1) == 0)
			{
				throw new IndexOutOfRangeException("Mipmap index out of range");
			}

			return InternalGetMipmap(mipmapIndex);
		}


		/// <summary>
		/// Returns all mipmaps stored in the texture.
		/// </summary>
		/// <returns>The mipmap textures.</returns>
		public Texture[] GetMipmaps()
		{
			Texture[] result = new Texture[MipMapCount];

			for(int i = 0; i < result.Length; i++)
			{
				result[i] = InternalGetMipmap(i);
			}

			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Name} - {GlobalIndex} - {Width}x{Height}";
		}
	}
}
