namespace SA3D.Archival.Textures.CommonV.IO
{
	internal struct VBlockAlignment
	{
		public long Start { get; set; }
		public int Size { get; set; }

		public VBlockAlignment(long start, int size)
		{
			Start = start;
			Size = size;
		}
	}
}
