using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Archival.Textures.PV.IO;
using SA3D.Archival.Textures.PV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.PV
{
	/// <summary>
	/// "PVP" Palette storage medium used in dreamcast games
	/// </summary>
	public sealed class PVPalette : BaseVPalette
	{
		private PVPixelCodec _codec;

		/// <summary>
		/// Pixel format
		/// </summary>
		[MemberNotNull(nameof(_codec))]
		public PVPixelFormat PixelFormat
		{
			get;
			set
			{
				ClearColorData();
				field = value;
				_codec = value.GetPixelCodec();
			}
		}



		/// <summary>
		/// Creates a new PV Palette instance
		/// </summary>
		/// <param name="format">Pixel format of the palette</param>
		/// <param name="data">Palette data</param>
		/// <param name="width">Width of the palette</param>
		public PVPalette(PVPixelFormat format, byte[] data, int width) : base(data, width)
		{
			PixelFormat = format;
		}

		/// <summary>
		/// Creates a new, empty PV Palette instance
		/// </summary>
		public PVPalette() : this(PVPixelFormat.ARGB8, [], 0) { }


		/// <summary>
		/// Encodes a palette to a PVPalette.
		/// </summary>
		/// <param name="palette">The palette to encode.</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <returns>The encoded PVPalette.</returns>
		public static PVPalette CreateFromPalette(ITexturePalette palette, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8)
		{
			return CreateFromColors(palette.GetColorData(), pixelFormat);
		}

		/// <summary>
		/// Encodes colors to a PVPalette.
		/// </summary>
		/// <param name="data">The RGBA32 color data to encode.</param>
		/// <param name="pixelFormat">The pixel format to encode to.</param>
		/// <returns>The encoded PVP palette.</returns>
		public static PVPalette CreateFromColors(ReadOnlySpan<byte> data, PVPixelFormat pixelFormat = PVPixelFormat.ARGB8)
		{
			PVPalette result = new()
			{
				PixelFormat = pixelFormat,
				Width = data.Length / 4
			};

			result.EncodePaletteColors(data);
			return result;
		}

		/// <summary>
		/// Encodes a palettes color data
		/// </summary>
		/// <param name="palette">The palette data to encode</param>
		public void EncodePalette(ITexturePalette palette)
		{
			EncodePaletteColors(palette.GetColorData());
		}

		/// <summary>
		/// Encodes raw palette color data
		/// </summary>
		/// <param name="data">Palette colors to encode</param>
		public void EncodePaletteColors(ReadOnlySpan<byte> data)
		{
			int width = data.Length / 4;
			Data = new byte[width / _codec.PixelPairSize * _codec.BytesPerPixelPair];
			Span<byte> destination = Data;

			for(int i = 0; i < width; i += _codec.PixelPairSize)
			{
				_codec.EncodePixel(data[(i * 4)..], destination[(i / _codec.PixelPairSize * _codec.BytesPerPixelPair)..]);
			}
		}

		/// <inheritdoc/>
		protected override void DecodePalette(Span<byte> destination)
		{
			int size = Width / _codec.PixelPairSize * _codec.BytesPerPixelPair;
			ReadOnlySpan<byte> source = Data;

			int dstAddress = 0;
			for(int i = 0; i < size; i += _codec.BytesPerPixelPair)
			{
				_codec.DecodePixel(source.Slice(i, _codec.BytesPerPixelPair), destination[dstAddress..]);
				dstAddress += 4 * _codec.PixelPairSize;
			}
		}


		/// <inheritdoc/>
		protected override bool CheckCanReadFile(BinaryObjectReader reader, ref FileIOInfo info)
		{
			return VBlock.CheckBlockExists<PVPaletteVBlock>(reader);
		}

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader)
		{
			VBlock[] blocks = PVBlocks.ReadPVBlocks(reader);

			PVPaletteVBlock block = blocks.OfType<PVPaletteVBlock>().FirstOrDefault()
				?? throw new InvalidDataException("No (valid) palette block found!");

			PixelFormat = block.PixelFormat;
			BankOffset = block.BankOffset;
			EntryOffset = block.EntryOffset;
			Width = block.Width;
			Data = block.Data;
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer)
		{
			writer.WriteObject(ToBlock(), new VBlockAlignment(writer.Position, 4));
		}


		internal override void FromBlock(BasePaletteVBlock block)
		{
			PVPaletteVBlock pvBlock = (PVPaletteVBlock)block;
			PixelFormat = pvBlock.PixelFormat;
			Width = pvBlock.Width;
			BankOffset = pvBlock.BankOffset;
			EntryOffset = pvBlock.EntryOffset;
			Data = pvBlock.Data;
		}

		internal override BasePaletteVBlock ToBlock()
		{
			return new PVPaletteVBlock()
			{
				PixelFormat = PixelFormat,
				BankOffset = BankOffset,
				EntryOffset = EntryOffset,
				Width = (ushort)Width,
				Data = Data
			};
		}
	}
}
