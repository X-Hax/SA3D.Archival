using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;
using System.IO;

namespace SA3D.Archival.Textures
{
	/// <summary>
	/// Base class for texture archive entries.
	/// </summary>
	public interface ITextureArchiveEntry : IArchiveEntry, IFileSerializable<TextureIOContext>, ITexture, IIndexTexture, IMipMapped
	{
		TextureType ITexture.TextureType => TextureType.RGBA32;

		bool IIndexTexture.IsIndex4 => TextureType == TextureType.Index4;

		void IFileSerializable<TextureIOContext>.ReadFile(BinaryObjectReader reader, TextureIOContext context, FileIOInfo info)
		{
			if(!context.DataOnly && !string.IsNullOrEmpty(info.Filepath))
			{
				((IArchiveEntry)this).Name = Path.GetFileNameWithoutExtension(info.Filepath)!;
			}
		}
	}
}
