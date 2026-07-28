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

		public static VBlock[] ReadVBlocks(BinaryObjectReader reader, Type[] additionalBlockTypes)
		{
			Type[] blockTypes = [
				typeof(CommentVBlock),
				typeof(ConverterNameVBlock),
				typeof(GlobalIndexVBlock),
				typeof(ImageDataVBlock),
				typeof(ModelNamesVBlock),
				..additionalBlockTypes,
			];

			Dictionary<string, Type> lut = [];
			foreach(Type blockType in blockTypes)
			{
				if(Activator.CreateInstance(blockType) is not VBlock block)
				{
					throw new ArgumentException($"Type \"{blockType}\" does not inherit from VBlock!");
				}

				lut[block.Header] = blockType;
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

				if(!lut.TryGetValue(blockHeader, out Type? blockType))
				{
					blockType = typeof(UnknownVBlock);
				}

				VBlock block = (VBlock)Activator.CreateInstance(blockType)!;
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
