using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Archival.Textures.GV;
using SA3D.Common.Ini;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SA3D.Archival.PAK
{
	/// <summary>
	/// Generic archive format used in Sonic Adventure 2 PC.
	/// </summary>
	public sealed class PAKArchive : IArchive
	{
		/// <summary>
		/// PAK File header (START PAK)
		/// </summary>
		private const uint _header = 0x6B617001;

		/// <summary>
		/// The name of the folder that the PAK archive compresses.
		/// </summary>
		public string FolderName
		{
			get;
			set => field = value.ToLowerInvariant();
		} = string.Empty;

		/// <summary>
		/// PAK files in the archive.
		/// </summary>
		public List<PAKEntry> Entries { get; private set; }

		/// <inheritdoc/>
		IReadOnlyList<IArchiveEntry> IArchive.Entries => Entries;


		/// <summary>
		/// Creates a new PAK archive.
		/// </summary>
		/// <param name="folderName">The name of the folder that the PAK archive compresses</param>
		public PAKArchive(string folderName) : base()
		{
			FolderName = folderName;
			Entries = [];
		}

		/// <summary>
		/// Creates a new PAK archive with no folder name.
		/// </summary>
		public PAKArchive() : this(string.Empty) { }


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			return reader.ReadUInt32() == _header;
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext context)
		{
			if(context.Filepath != null)
			{
				FolderName = Path.GetFileNameWithoutExtension(context.Filepath);
			}

			Entries = [];

			reader.Skip(0x39);

			int numfiles = reader.ReadInt32();
			(string longpath, string name, int length)[] fileInfo
				= new (string longpath, string name, int length)[numfiles];


			for(int i = 0; i < numfiles; i++)
			{
				int stringLength = reader.ReadInt32();
				string longPath = reader.ReadString(Encoding.ASCII, StringBinaryFormat.FixedLength, stringLength);

				stringLength = reader.ReadInt32();
				string name = reader.ReadString(Encoding.ASCII, StringBinaryFormat.FixedLength, stringLength);

				int length = reader.ReadInt32();
				reader.Skip(sizeof(int));

				fileInfo[i] = (longPath, name, length);
			}

			for(int i = 0; i < numfiles; i++)
			{
				(string longpath, string name, int length) = fileInfo[i];
				byte[] entryData = reader.ReadArray<byte>(length);
				Entries.Add(new(entryData, Path.GetFileName(name), longpath));
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext context)
		{
			int totalLength = Entries.Sum((a) => a.Data.Length);

			writer.WriteUInt32(_header);
			writer.Skip(33);
			writer.WriteInt32(Entries.Count);
			writer.WriteInt32(totalLength);
			writer.WriteInt32(totalLength);
			writer.Skip(8);
			writer.WriteInt32(Entries.Count);

			foreach(PAKEntry item in Entries)
			{
				writer.WriteInt32(item.LongPath.Length);
				writer.WriteString(Encoding.ASCII, StringBinaryFormat.FixedLength, item.LongPath.ToLower(), item.LongPath.Length);

				string fullname = $"{FolderName}\\{item.Name}".ToLower();
				writer.WriteInt32(fullname.Length);
				writer.WriteString(Encoding.ASCII, StringBinaryFormat.FixedLength, fullname, fullname.Length);

				writer.WriteInt32(item.Data.Length);
				writer.WriteInt32(item.Data.Length);
			}

			foreach(PAKEntry item in Entries)
			{
				writer.WriteArray(item.Data);
			}
		}

		/// <inheritdoc/>
		public string WriteContentIndex()
		{
			Dictionary<string, PAKIniItem> list = new(Entries.Count);
			foreach(PAKEntry item in Entries)
			{
				list.Add($"{FolderName}\\{item.Name}", new(item.LongPath));
			}

			using StringWriter writer = new();
			IniSerializer.Serialize(list).Write(writer);
			return writer.ToString();
		}


		/// <summary>
		/// Converts the archive to a texture set. Entries that cannot be converted to textures are ignored.
		/// </summary>
		/// <returns></returns>
		public TextureSet ToTextureSet()
		{
			List<Texture> textures = [];

			PAKEntry? infFile = Entries.FirstOrDefault(x => x.Name.EndsWith(".inf"));
			if(infFile == null)
			{
				foreach(PAKEntry item in Entries)
				{
					try
					{
						textures.Add(item.ReadTextureFromData());
					}
					catch
					{
						continue;
					}
				}

				return new(textures);
			}

			int infoCount = (int)(infFile.Data.Length / PAKTextureInfo.StructSize);
			PAKTextureInfo[] info;

			using(MemoryStream stream = new(infFile.Data))
			{
				using(BinaryObjectReader infReader = new(stream, StreamOwnership.Retain, Endianness.Little))
				{
					info = infReader.ReadObjectArray<PAKTextureInfo>(infoCount);
				}
			}

			for(int i = 0; i < info.Length; i++)
			{
				PAKTextureInfo texInfo = info[i];

				PAKEntry entry = Entries.FirstOrDefault(x => x.Name == texInfo.Name)
					?? Entries.First(x => x.Name.StartsWith(texInfo.Name));

				Texture tex = entry.ReadTextureFromData();

				tex.GlobalIndex = texInfo.GlobalIndex;
				tex.Name = Path.GetFileNameWithoutExtension(texInfo.Name);
				tex.OverrideWidth = (int)texInfo.Width;
				tex.OverrideHeight = (int)texInfo.Height;
				textures.Add(tex);
			}

			return new(textures);
		}

		/// <summary>
		/// Converts a texture set to a PAK archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="folderName">The name of the folder that the archive compresses.</param>
		/// <param name="itemBasePath">The item base path / The long path up until <paramref name="folderName"/> (exclusive). </param>
		/// <param name="format">Image format to convert to</param>
		/// <param name="storeIndexInAlpha">Whether the index for indexed textures should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		/// <returns>The converted PAK archive.</returns>
		public static PAKArchive FromTextureSet(TextureSet textureSet, string folderName, string itemBasePath, ImageFormat format, bool storeIndexInAlpha = false)
		{
			PAKTextureInfo[] textureInfo = new PAKTextureInfo[textureSet.Textures.Count];
			PAKArchive result = new(folderName);

			for(int i = 0; i < textureSet.Textures.Count; i++)
			{
				ITexture texture = textureSet.Textures[i];
				PAKTextureInfo texInfo = new()
				{
					Name = texture.Name.ToLower(),
					GlobalIndex = texture.GlobalIndex,
					Type = GVTextureFormat.DXT1,
					BitDepth = 0,
					PixelFormat = GVTextureFormat.DXT1,
					Width = (ushort)texture.Width,
					Height = (ushort)texture.Height,
					DataSize = 0,
					Attributes = default
				};

				byte[] texData;
				using(MemoryStream texStream = new())
				{
					if(texture is IndexTexture indexTex && indexTex.Palette == null)
					{
						indexTex.WriteIndexImage(texStream, format, storeIndexInAlpha);
						texInfo.PixelFormat = texInfo.Type = indexTex.IsIndex4 ? GVTextureFormat.Index4 : GVTextureFormat.Index8;
						texInfo.Attributes |= PAKTextureAttributes.Palettized;
						texInfo.BitDepth = 8;
					}
					else
					{
						texture.WriteImage(texStream, format);
						texInfo.BitDepth = format == ImageFormat.DDS ? 16u : 32u;
					}

					texData = texStream.ToArray();
				}

				string textureName = texInfo.Name + ".dds";
				result.Entries.Add(new(texData, textureName, $"{itemBasePath}\\{folderName}\\{textureName}"));
				textureInfo[i] = texInfo;
			}

			using MemoryStream stream = new();
			using BinaryObjectWriter writer = new(stream, StreamOwnership.Retain, Endianness.Little);
			writer.WriteObjectArray(textureInfo);

			string indexName = folderName + ".inf";
			result.Entries.Insert(0, new(stream.ToArray(), indexName, $"{itemBasePath}\\{folderName}\\{indexName}"));

			return result;
		}
	}
}
