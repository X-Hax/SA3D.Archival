using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV;
using SA3D.Archival.Textures.CommonV.IO;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Archival.Textures.GV.IO;
using SA3D.Archival.Textures.GV.IO.VBlocks;
using SA3D.Common.IO;
using SA3D.Texturing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace SA3D.Archival.Textures.GV
{
	/// <summary>
	/// Palette storage medium used in gamecube games.
	/// </summary>
	public class GVPalette : BaseVPalette
	{
		private GVPaletteCodec _codec;

		/// <summary>
		/// Palette format
		/// </summary>
		[MemberNotNull(nameof(_codec))]
		public GVPaletteFormat PaletteFormat
		{
			get;
			set
			{
				ClearColorData();
				field = value;
				_codec = value.GetPaletteCodec();
			}
		}

		/// <summary>
		/// Creates a new GV Palette instance
		/// </summary>
		/// <param name="format">Palette data format</param>
		/// <param name="data">Palette data</param>
		/// <param name="width">Palette width</param>
		public GVPalette(GVPaletteFormat format, byte[] data, int width) : base(data, width)
		{
			PaletteFormat = format;
		}

		/// <summary>
		/// Creates a new, empty GV Palette instance
		/// </summary>
		public GVPalette() : this(GVPaletteFormat.Rgb5a3, [], 0) { }

		internal GVPalette(GVPaletteVBlock block) : this(block.PaletteFormat, block.Data, block.Width)
		{
			EntryOffset = block.EntryOffset;
			BankOffset = block.BankOffset;
		}


		/// <summary>
		/// Encodes a palette to a GVPalette.
		/// </summary>
		/// <param name="palette">The palette to encode.</param>
		/// <param name="paletteFormat">The pixel format to encode to.</param>
		/// <returns>The encoded PVPalette.</returns>
		public static GVPalette CreateFromPalette(ITexturePalette palette, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3)
		{
			return CreateFromColors(palette.GetColorData(), paletteFormat);
		}

		/// <summary>
		/// Encodes colors to a GVPalette.
		/// </summary>
		/// <param name="data">The RGBA32 color data to encode.</param>
		/// <param name="paletteFormat">The pixel format to encode to.</param>
		/// <returns>The encoded PVP palette.</returns>
		public static GVPalette CreateFromColors(ReadOnlySpan<byte> data, GVPaletteFormat paletteFormat = GVPaletteFormat.Rgb5a3)
		{
			GVPalette result = new()
			{
				PaletteFormat = paletteFormat,
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
			Data = _codec.Encode(data);
		}

		/// <inheritdoc/>
		protected override void DecodePalette(Span<byte> destination)
		{
			_codec.Decode(Data).CopyTo(destination);
		}


		/// <inheritdoc/>
		public override bool Check(BinaryObjectReader reader)
		{
			return VBlock.CheckBlockExists<GVPaletteVBlock>(reader);
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, FileContext context)
		{
			if(!string.IsNullOrEmpty(context.Filepath))
			{
				Name = Path.GetFileNameWithoutExtension(context.Filepath);
			}

			VBlock[] blocks = GVBlocks.ReadGVBlocks(reader);

			GVPaletteVBlock block = blocks.OfType<GVPaletteVBlock>().FirstOrDefault()
				?? throw new InvalidDataException("No (valid) palette block found!");

			PaletteFormat = block.PaletteFormat;
			BankOffset = block.BankOffset;
			EntryOffset = block.EntryOffset;
			Width = block.Width;
			Data = block.Data;
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, FileContext context)
		{
			writer.WriteObject(ToBlock(), new VBlockAlignment(writer.Position, 4));
		}


		internal override void FromBlock(BasePaletteVBlock block)
		{
			GVPaletteVBlock pvBlock = (GVPaletteVBlock)block;
			PaletteFormat = pvBlock.PaletteFormat;
			Width = pvBlock.Width;
			BankOffset = pvBlock.BankOffset;
			EntryOffset = pvBlock.EntryOffset;
			Data = pvBlock.Data;
		}

		internal override BasePaletteVBlock ToBlock()
		{
			return new GVPaletteVBlock()
			{
				PaletteFormat = PaletteFormat,
				BankOffset = BankOffset,
				EntryOffset = EntryOffset,
				Width = (ushort)Width,
				Data = Data
			};
		}
	}
}
