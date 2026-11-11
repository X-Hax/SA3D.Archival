using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Archival.Textures.CommonV.IO.Blocks;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Archival.Textures.CommonV.IO
{
	internal abstract class VBlock : IBinarySerializable<VBlockAlignment>
	{
		public abstract string Header { get; }

		public virtual Endianness BlockEndianness => Endianness.Little;

		protected abstract void ReadData(BinaryObjectReader reader, int dataSize);
		protected abstract void WriteData(BinaryObjectWriter writer);

		public void Read(BinaryObjectReader reader, VBlockAlignment context)
		{
			// skipping the header
			reader.Skip(4);
			int size = reader.ReadInt32();

			using EndiannessToken endianness = reader.WithEndian(BlockEndianness);

			using(reader.At())
			{
				ReadData(reader, size);
			}

			reader.Skip(size);
		}

		public void Write(BinaryObjectWriter writer, VBlockAlignment context)
		{
			writer.WriteString(StringBinaryFormat.FixedLength, Header, 4);

			SeekToken dataSizeSeekToken = writer.At();
			writer.WriteUInt32(0);

			long dataStart = writer.Position;

			using(EndiannessToken endianness = writer.WithEndian(BlockEndianness))
			{
				WriteData(writer);
			}


			if(context.Size > 0)
			{
				writer.Align(context.Size, context.Start);
			}

			uint dataSize = (uint)(writer.Position - dataStart);
			using(writer.At())
			{
				dataSizeSeekToken.Dispose();
				writer.WriteUInt32(dataSize);
			}
		}

		public static VBlock[] ReadVBlocks(BinaryObjectReader reader, Func<VBlock>[] additionalBlockConstructors)
		{
			Func<VBlock>[] blockConstructors = [
				() => new CommentVBlock(),
				() => new CommentVBlock(),
				() => new ConverterNameVBlock(),
				() => new GlobalIndexVBlock(),
				() => new ImageDataVBlock(),
				() => new ModelNamesVBlock(),
				..additionalBlockConstructors,
			];

			Dictionary<string, Func<VBlock>> lut = [];
			foreach(Func<VBlock> blockConstructor in blockConstructors)
			{
				VBlock block = blockConstructor();
				lut[block.Header] = blockConstructor;
			}

			List<VBlock> result = [];

			do
			{
				string blockHeader;
				using(reader.At())
				{
					blockHeader = reader.ReadString(StringBinaryFormat.FixedLength, 4);

					int blockLength = reader.ReadInt32();
					if(blockLength > reader.Length - reader.Position)
					{
						break;
					}
				}

				if(!lut.TryGetValue(blockHeader, out Func<VBlock>? blockConstructor))
				{
					blockConstructor = () => new UnknownVBlock();
				}

				VBlock block = blockConstructor();
				block.Read(reader, default);
				result.Add(block);
			}
			while(reader.Position + 8 < reader.Length);

			return [.. result];
		}

		public static bool CheckBlockExists<T>(BinaryObjectReader reader) where T : VBlock, new()
		{
			string header = new T().Header;

			do
			{
				string blockHeader = reader.ReadString(StringBinaryFormat.FixedLength, 4);

				uint blockLength = reader.ReadUInt32();
				if(blockLength > reader.Length - reader.Position)
				{
					break;
				}

				if(blockHeader == header)
				{
					return true;
				}

				reader.Skip((int)blockLength);
			}
			while(reader.Position + 8 < reader.Length);

			return false;
		}

		public override string ToString()
		{
			return Header;
		}
	}
}
