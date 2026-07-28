using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.PV.IO.VBlocks
{
	internal class PVTextureVBlock : BaseTextureVBlock
	{
		public override string Header => "PVRT";

		public override uint Format
		{
			get => ((uint)PixelFormat) | ((uint)DataFormat << 8);
			set
			{
				PixelFormat = (PVPixelFormat)(value & 0xFF);
				DataFormat = (PVTextureDataFormat)((value >> 8) & 0xFF);
			}
		}

		public PVPixelFormat PixelFormat { get; set; }

		public PVTextureDataFormat DataFormat { get; set; }

		public override string ToString()
		{
			return $"{Header} : {PixelFormat}, {DataFormat} - {Width}x{Height}, [{Data.Length}]";
		}
	}
}
