using SA3D.Archival.Tex.GV;
using SA3D.Common.Ini;
using SA3D.Common.IO;
using SA3D.Texturing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SA3D.Archival.PAK
{
	/// <summary>
	/// Generic archive format used in Sonic Adventure 2 PC.
	/// </summary>
	public class PAKArchive : Archive
	{
		/// <summary>
		/// PAK File header (START PAK)
		/// </summary>
		private const uint _header = 0x6B617001;

		private string _foldername = string.Empty;

		/// <summary>
		/// The name of the folder that the PAK archive compresses.
		/// </summary>
		public string FolderName
		{
			get => _foldername;
			set => _foldername = value.ToLowerInvariant();
		}

		/// <summary>
		/// PAK files in the archive.
		/// </summary>
		public List<PAKEntry> PAKEntries { get; }

		/// <inheritdoc/>
		public override IReadOnlyList<ArchiveEntry> Entries => PAKEntries;


		/// <summary>
		/// Creates a new PAK archive.
		/// </summary>
		/// <param name="folderName">The name of the folder that the PAK archive compresses</param>
		public PAKArchive(string folderName) : base()
		{
			FolderName = folderName;
			PAKEntries = new();
		}

		/// <summary>
		/// Creates a new PAK archive with no folder name.
		/// </summary>
		public PAKArchive() : this(string.Empty) { }


		/// <inheritdoc/>
		public override TextureSet ToTextureSet()
		{
			ArchiveEntry? infFile = Entries.FirstOrDefault(x => x.Name.EndsWith(".inf"));
			if(infFile == null)
			{
				return base.ToTextureSet();
			}

			List<Texture> textures = new();
			EndianStackReader infReader = infFile.CreateDataReader();
			PAKTextureInfo[] info = new PAKTextureInfo[infReader.Length / PAKTextureInfo.StructSize];
			uint addr = 0;
			for(int i = 0; i < info.Length; i++)
			{
				info[i] = PAKTextureInfo.Read(infReader, addr);
				addr += PAKTextureInfo.StructSize;
			}

			for(int i = 0; i < info.Length; i++)
			{
				PAKTextureInfo texInfo = info[i];
				PAKEntry entry = PAKEntries.First(x => x.Name.StartsWith(texInfo.Name));

				Texture tex = entry.ToTexture();

				tex.GlobalIndex = texInfo.GlobalIndex;
				tex.Name = Path.GetFileNameWithoutExtension(texInfo.Name);
				tex.OverrideWidth = (int)texInfo.Width;
				tex.OverrideHeight = (int)texInfo.Height;
				textures.Add(tex);
			}

			return new(textures.ToArray());
		}

		/// <inheritdoc/>
		public override void WriteContentIndex(TextWriter writer)
		{
			Dictionary<string, PAKIniItem> list = new(PAKEntries.Count);
			foreach(PAKEntry item in PAKEntries)
			{
				list.Add($"{FolderName}\\{item.Name}", new(item.LongPath));
			}

			IniSerializer.Serialize(list).Write(writer);
		}

		/// <inheritdoc/>
		public override void WriteArchive(EndianStackWriter writer)
		{
			int totalLength = PAKEntries.Sum((a) => a.CreateDataReader().Length);

			writer.WriteUInt(_header);
			writer.WriteEmpty(33);
			writer.WriteInt(PAKEntries.Count);
			writer.WriteInt(totalLength);
			writer.WriteInt(totalLength);
			writer.WriteEmpty(8);
			writer.WriteInt(PAKEntries.Count);

			foreach(PAKEntry item in PAKEntries)
			{
				string fullname = $"{FolderName}\\{item.Name}".ToLower();
				writer.WriteInt(item.LongPath.Length);
				writer.WriteString(item.LongPath.ToLower());
				writer.WriteInt(fullname.Length);
				writer.WriteString(fullname);
				writer.WriteInt(item.CreateDataReader().Length);
				writer.WriteInt(item.CreateDataReader().Length);
			}

			foreach(PAKEntry item in PAKEntries)
			{
				writer.Write(item.CreateDataReader().Source);
			}
		}


		/// <summary>
		/// Converts a texture set to a PAK archive.
		/// </summary>
		/// <param name="textureSet">The texture set to convert.</param>
		/// <param name="folderName">The name of the folder that the archive compresses.</param>
		/// <param name="itemBasePath">The item base path / The long path up until <paramref name="folderName"/> (exclusive). </param>
		/// <param name="storeIndexInAlpha">Whether the index for indexed textures should be stored in the alpha channel, instead of outputing a grayscale image.</param>
		/// <param name="useDDS">Whether to convert textures to DDS images, instead of PNG images.</param>
		/// <returns>The converted PAK archive.</returns>
		public static PAKArchive FromTextureSet(TextureSet textureSet, string folderName, string itemBasePath, bool storeIndexInAlpha = false, bool useDDS = false)
		{
			PAKTextureInfo[] textureInfo = new PAKTextureInfo[textureSet.Textures.Count];
			PAKArchive result = new(folderName);

			for(int i = 0; i < textureSet.Textures.Count; i++)
			{
				Texture texture = textureSet.Textures[i];
				PAKTextureInfo texInfo = new(
					texture.Name.ToLower(),
					texture.GlobalIndex,
					GVRPixelFormat.DXT1,
					0,
					GVRPixelFormat.DXT1,
					(ushort)texture.Width,
					(ushort)texture.Height,
					0,
					default);

				byte[] texData;
				using(MemoryStream texStream = new())
				{
					if(texture is IndexTexture indexTex && indexTex.Palette == null)
					{
						if(useDDS)
						{
							indexTex.WriteIndexedAsDDS(texStream);
						}
						else
						{
							indexTex.WriteIndexedAsPNG(texStream, storeIndexInAlpha);
						}

						texInfo.PixelFormat = texInfo.Type = indexTex.IsIndex4 ? GVRPixelFormat.Index4 : GVRPixelFormat.Index8;
						texInfo.Attributes |= PAKTextureAttributes.Palettized;
						texInfo.BitDepth = 8;
					}
					else
					{
						if(useDDS)
						{
							texture.WriteColoredAsDDS(texStream);
						}
						else
						{
							texture.WriteColoredAsPNG(texStream);
						}

						texInfo.BitDepth = useDDS ? 16u : 32u;
					}

					texData = texStream.ToArray();
				}

				string textureName = texInfo.Name + ".dds";
				result.PAKEntries.Add(new(texData, textureName, $"{itemBasePath}\\{folderName}\\{textureName}"));
				textureInfo[i] = texInfo;
			}

			using MemoryStream stream = new();
			EndianStackWriter writer = new(stream);
			foreach(PAKTextureInfo texInfo in textureInfo)
			{
				texInfo.Write(writer);
			}

			string indexName = folderName + ".inf";
			result.PAKEntries.Insert(0, new(stream.ToArray(), indexName, $"{itemBasePath}\\{folderName}\\{indexName}"));

			return result;
		}


		/// <summary>
		/// Checks whether the data at specified address can be read as a PAK archive.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which</param>
		/// <returns>Whether the data is a readable PAK archive.</returns>
		public static bool CheckIsPAKArchive(EndianStackReader reader, uint address)
		{
			return reader.ReadUInt(address) == _header;
		}

		/// <summary>
		///Checks whether the data at specified address can be read as a PAK archive.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <param name="address">The address at which</param>
		/// <returns></returns>
		public static bool CheckIsPAKArchive(byte[] data, uint address)
		{
			return CheckIsPAKArchive(new EndianStackReader(data), address);
		}


		/// <summary>
		/// Reads a PAK archive from an endian reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which the archive is located.</param>
		/// <param name="folderName">Name of the folder that the archive compressed.</param>
		/// <returns>The PAK Archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"/>
		public static PAKArchive ReadPAKArchive(EndianStackReader reader, uint address, string folderName)
		{
			if(!CheckIsPAKArchive(reader, address))
			{
				throw new InvalidArchiveException("Data is not a PAK archive.");
			}

			PAKArchive result = new(folderName);

			int numfiles = reader.ReadInt(address + 0x39);
			string[] longpaths = new string[numfiles];
			string[] names = new string[numfiles];
			uint[] lengths = new uint[numfiles];
			uint tmpaddr = address + 0x3D;

			for(int i = 0; i < numfiles; i++)
			{
				uint stringLength = reader.ReadUInt(tmpaddr);
				longpaths[i] = reader.ReadString(tmpaddr += 4, Encoding.ASCII, stringLength);

				stringLength = reader.ReadUInt(tmpaddr += stringLength);
				names[i] = reader.ReadString(tmpaddr += 4, Encoding.ASCII, stringLength);

				lengths[i] = reader.ReadUInt(tmpaddr += stringLength);
				tmpaddr += 8; // skipping an integer here
			}

			for(int i = 0; i < numfiles; i++)
			{
				byte[] entryData = reader.Source.Slice((int)tmpaddr, (int)lengths[i]).ToArray();

				result.PAKEntries.Add(new(
					entryData,
					Path.GetFileName(names[i]),
					longpaths[i]));

				tmpaddr += lengths[i];
			}

			return result;
		}

		/// <summary>
		/// Reads a PAK archive from an endian reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which the archive is located.</param>
		/// <returns>The PAK Archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"/>
		public static PAKArchive ReadPAKArchive(EndianStackReader reader, uint address)
		{
			return ReadPAKArchive(reader, address, string.Empty);
		}

		/// <summary>
		/// Reads a PAK archive from byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which the archive is located.</param>
		/// <param name="folderName">Name of the folder that the archive compressed.</param>
		/// <returns>The PAK Archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"/>
		public static PAKArchive ReadPAKArchive(byte[] data, uint address, string folderName)
		{
			return ReadPAKArchive(new EndianStackReader(data), address, folderName);
		}

		/// <summary>
		/// Reads a PAK archive from byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">The address at which the archive is located.</param>
		/// <returns>The PAK Archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"/>
		public static PAKArchive ReadPAKArchive(byte[] data, uint address)
		{
			return ReadPAKArchive(data, address, string.Empty);
		}

		/// <summary>
		/// Reads a PAK archive from a file.
		/// </summary>
		/// <param name="filePath">Path to the file to read.</param>
		/// <returns>The PAK Archive that was read.</returns>
		/// <exception cref="InvalidArchiveException"/>
		public static PAKArchive ReadPAKArchiveFromFile(string filePath)
		{
			return ReadPAKArchive(File.ReadAllBytes(filePath), 0, Path.GetFileNameWithoutExtension(filePath));
		}

	}
}
