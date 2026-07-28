using SA3D.Common.IO;
using SA3D.Texturing;
using SA3D.Texturing.MipMapping;

namespace SA3D.Archival.Textures
{
	/// <summary>
	/// Base class for texture archive entries.
	/// </summary>
	public interface ITextureArchiveEntry : IArchiveEntry, IFileSerializable<TextureIOContext>, ITexture, IIndexTexture, IMipMapped
	{
		TextureType ITexture.TextureType => TextureType.RGBA32;

		bool IIndexTexture.IsIndex4 => TextureType == TextureType.Index4;
	}
}
