using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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


		bool IFileSerializable.CheckCanReadFile(BinaryObjectReader reader, ref FileIOInfo fileInfo)
		{
			return CheckCanReadFile(reader, ref fileInfo);
		}

		/// <summary>
		/// Implementation for <see cref="IFileSerializable.CheckCanReadFile(BinaryObjectReader, ref FileIOInfo)"/>
		/// </summary>
		protected abstract bool CheckCanReadFile(BinaryObjectReader reader, ref FileIOInfo fileInfo);


		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			Read(reader);
		}

		/// <summary>
		/// Implementation for <see cref="IBinarySerializable.Read(BinaryObjectReader)"/>
		/// </summary>
		protected abstract void Read(BinaryObjectReader reader);

		void IFileSerializable.ReadFile(BinaryObjectReader reader, FileIOInfo info)
		{
			if(!string.IsNullOrEmpty(info.Filepath))
			{
				Name = Path.GetFileNameWithoutExtension(info.Filepath);
			}
		}


		void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			Write(writer);
		}

		/// <summary>
		/// Implementation for <see cref="IBinarySerializable.Write(BinaryObjectWriter)"/>
		/// </summary>
		protected abstract void Write(BinaryObjectWriter writer);


		internal abstract void FromBlock(BasePaletteVBlock block);

		internal abstract BasePaletteVBlock ToBlock();
	}
}
