using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace SA3D.Archival.Textures.CommonV
{
	/// <summary>
	/// Base "V" palette
	/// </summary>
	public abstract class BaseVPalette : IFileSerializable, ITexturePalette
	{
		private byte[]? _colorData;

		/// <summary>
		/// Raw Palette data
		/// </summary>
		public byte[] Data
		{
			get;
			set
			{
				ClearColorData();
				field = value;
			}
		}

		/// <inheritdoc/>
		public string Name { get; set; }

		/// <inheritdoc/>
		public int Width
		{
			get;
			set
			{
				ClearColorData();
				field = value;
			}
		}

		/// <summary>
		/// Palette entry offset.
		/// </summary>
		public ushort EntryOffset { get; set; }

		/// <summary>
		/// Palette bank offset.
		/// </summary>
		public ushort BankOffset { get; set; }


		/// <summary>
		/// Base constructor
		/// </summary>
		/// <param name="width">Width of the palette</param>
		/// <param name="data">Palette data</param>
		protected BaseVPalette(byte[] data, int width)
		{
			Data = data;
			Width = width;
			Name = string.Empty;
		}


		/// <summary>
		/// Decodes palette colors
		/// </summary>
		/// <param name="destination">Output destination</param>
		protected abstract void DecodePalette(Span<byte> destination);


		/// <summary>
		/// Clears decoded color data
		/// </summary>
		protected void ClearColorData()
		{
			_colorData = null;
		}

		[MemberNotNull(nameof(_colorData))]
		private void LoadColorData()
		{
			if(_colorData != null)
			{
				return;
			}

			_colorData = new byte[4 * Width];
			DecodePalette(_colorData);
		}

		/// <inheritdoc/>
		public ReadOnlySpan<byte> GetColorData()
		{
			LoadColorData();
			return _colorData;
		}


		/// <inheritdoc/>
		public abstract bool Check(BinaryObjectReader reader);

		/// <inheritdoc/>
		public abstract void Read(BinaryObjectReader reader, FileContext context);

		/// <inheritdoc/>
		public abstract void Write(BinaryObjectWriter writer, FileContext context);


		internal abstract void FromBlock(BasePaletteVBlock block);

		internal abstract BasePaletteVBlock ToBlock();
	}
}
