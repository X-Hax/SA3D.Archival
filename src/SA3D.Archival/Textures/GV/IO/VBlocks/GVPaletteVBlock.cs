using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVPaletteVBlock : BasePaletteVBlock
	{
		public override string Header => "GVPL";

		public override Endianness BlockEndianness => Endianness.Big;

		public override ushort FormatData
		{
			get => (ushort)((int)PaletteFormat << 8);
			set => PaletteFormat = (GVPaletteFormat)(value >> 8);
		}

		public GVPaletteFormat PaletteFormat { get; set; }

		public override string ToString()
		{
			return $"{Header} : {PaletteFormat}, {EntryOffset}, {BankOffset} - {Width}, [{ Data.Length }]";
		}
	}
}
