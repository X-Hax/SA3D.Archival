using Amicitia.IO.Binary;
using SA3D.Archival.Textures.CommonV.IO.Blocks;

namespace SA3D.Archival.Textures.GV.IO.VBlocks
{
	internal class GVTextureVBlock : BaseTextureVBlock
	{
		public override string Header => "GVRT";
		public override Endianness BlockEndianness => Endianness.Big;

		public override uint Format
		{
			get => ((uint)TextureFormat) | ((uint)TextureAttributes << 8) | ((uint)PaletteFormat << 12);
			set
			{
				TextureFormat = (GVTextureFormat)(value & 0xFF);
				TextureAttributes = (GVTextureAttributes)((value >> 8) & 0x0F);
				PaletteFormat = (GVPaletteFormat)((value >> 12) & 0x0F);
			}
		}

		public GVTextureFormat TextureFormat { get; set; }
		public GVPaletteFormat PaletteFormat { get; set; }
		public GVTextureAttributes TextureAttributes { get; set; }

		public override string ToString()
		{
			return $"{Header} : {TextureAttributes}, {PaletteFormat}, {TextureFormat} - {Width}x{Height}, [{Data.Length}]";
		}
	}
}
