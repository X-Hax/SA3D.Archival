using Amicitia.IO.Binary;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Archival.Textures.CommonV.IO.Blocks
{
	internal abstract class BaseArchiveInfoVBlock : VBlock
	{
		public (string filename, string options)[] Inputs { get; set; } = [];

		protected override void ReadData(BinaryObjectReader reader, int dataSize)
		{
			ushort fileLength = reader.ReadUInt16();
			ushort optionLength = reader.ReadUInt16();
			int entryLength = fileLength + optionLength;

			int offset = sizeof(ushort) * 2;

			List<(string, string)> result = [];

			while(offset <= (dataSize - entryLength))
			{
				string filename = reader.ReadString(StringBinaryFormat.FixedLength, fileLength).TrimEnd('\0');
				string options = reader.ReadString(StringBinaryFormat.FixedLength, optionLength).TrimEnd('\0');

				result.Add((filename, options));
				offset += entryLength;
			}

			Inputs = [.. result];
		}

		protected override void WriteData(BinaryObjectWriter writer)
		{
			int longestFilename = Inputs.Max(x => x.filename.Length) + 1;
			int longestOptions = Inputs.Max(x => x.options.Length) + 1;

			writer.WriteUInt16((ushort)longestFilename);
			writer.WriteUInt16((ushort)longestOptions);

			foreach((string filename, string options) in Inputs)
			{
				writer.WriteString(StringBinaryFormat.FixedLength, filename, longestFilename);
				writer.WriteString(StringBinaryFormat.FixedLength, options, longestOptions);
			}
		}

		public override string ToString()
		{
			return $"{Header} : [{Inputs.Length}]";
		}
	}
}
