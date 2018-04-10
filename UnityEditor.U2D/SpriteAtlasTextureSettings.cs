using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEditor.U2D
{
	[StructLayout(LayoutKind.Sequential)]
	internal sealed class SpriteAtlasTextureSettings
	{
		internal uint m_AnisoLevel;

		internal uint m_CompressionQuality;

		internal uint m_MaxTextureSize;

		internal TextureImporterCompression m_TextureCompression;

		internal FilterMode m_FilterMode;

		internal int m_GenerateMipMaps;

		internal int m_Readable;

		internal int m_CrunchedCompression;

		internal int m_sRGB;

		public uint anisoLevel
		{
			get
			{
				return this.m_AnisoLevel;
			}
			set
			{
				this.m_AnisoLevel = value;
			}
		}

		public uint compressionQuality
		{
			get
			{
				return this.m_CompressionQuality;
			}
			set
			{
				this.m_CompressionQuality = value;
			}
		}

		public uint maxTextureSize
		{
			get
			{
				return this.m_MaxTextureSize;
			}
			set
			{
				this.m_MaxTextureSize = value;
			}
		}

		public TextureImporterCompression textureCompression
		{
			get
			{
				return this.m_TextureCompression;
			}
			set
			{
				this.m_TextureCompression = value;
			}
		}

		public FilterMode filterMode
		{
			get
			{
				return this.m_FilterMode;
			}
			set
			{
				this.m_FilterMode = value;
			}
		}

		public bool generateMipMaps
		{
			get
			{
				return this.m_GenerateMipMaps != 0;
			}
			set
			{
				this.m_GenerateMipMaps = ((!value) ? 0 : 1);
			}
		}

		public bool readable
		{
			get
			{
				return this.m_Readable != 0;
			}
			set
			{
				this.m_Readable = ((!value) ? 0 : 1);
			}
		}

		public bool crunchedCompression
		{
			get
			{
				return this.m_CrunchedCompression != 0;
			}
			set
			{
				this.m_CrunchedCompression = ((!value) ? 0 : 1);
			}
		}

		public bool sRGB
		{
			get
			{
				return this.m_sRGB != 0;
			}
			set
			{
				this.m_sRGB = ((!value) ? 0 : 1);
			}
		}
	}
}
