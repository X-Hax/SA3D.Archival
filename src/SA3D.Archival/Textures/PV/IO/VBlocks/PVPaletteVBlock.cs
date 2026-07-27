using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.PV.IO.VBlocks
{
	internal class PVPaletteVBlock : BasePaletteVBlock
	{
		public override string Header => "PVPL";

		public override ushort FormatData {
			get => (ushort)((int)PixelFormat << 8);
			set => PixelFormat = (PVPixelFormat)(value >> 8);
		}

		public PVPixelFormat PixelFormat { get; set; }

		public override string ToString()
		{
			return $"{Header} : {PixelFormat}, {EntryOffset}, {BankOffset} - {Width}, [{ Data.Length }]";
		}
	}
}
