using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace UnityEditor
{
	[Serializable]
	internal class TextureImportPlatformSettings
	{
		[SerializeField]
		private TextureImporterPlatformSettings m_PlatformSettings = new TextureImporterPlatformSettings();

		[SerializeField]
		private bool m_OverriddenIsDifferent = false;

		[SerializeField]
		private bool m_MaxTextureSizeIsDifferent = false;

		[SerializeField]
		private bool m_ResizeAlgorithmIsDifferent = false;

		[SerializeField]
		private bool m_TextureCompressionIsDifferent = false;

		[SerializeField]
		private bool m_CompressionQualityIsDifferent = false;

		[SerializeField]
		private bool m_CrunchedCompressionIsDifferent = false;

		[SerializeField]
		private bool m_TextureFormatIsDifferent = false;

		[SerializeField]
		private bool m_AlphaSplitIsDifferent = false;

		[SerializeField]
		private bool m_AndroidETC2FallbackOverrideIsDifferent = false;

		[SerializeField]
		public BuildTarget m_Target;

		[SerializeField]
		private TextureImporter[] m_Importers;

		[SerializeField]
		private bool m_HasChanged = false;

		[SerializeField]
		private TextureImporterInspector m_Inspector;

		public static readonly int[] kTextureFormatsValueWiiU = new int[]
		{
			10,
			12,
			7,
			1,
			4,
			13
		};

		public static readonly int[] kTextureFormatsValuePSP2 = new int[]
		{
			10,
			12,
			7,
			3,
			13,
			4,
			1,
			17
		};

		public static readonly int[] kTextureFormatsValueSwitch = new int[]
		{
			10,
			12,
			28,
			29,
			7,
			3,
			1,
			2,
			4,
			17,
			26,
			27,
			48,
			49,
			50,
			51,
			52,
			53,
			54,
			55,
			56,
			57,
			58,
			59
		};

		public static readonly int[] kTextureFormatsValueApplePVR = new int[]
		{
			30,
			31,
			32,
			33,
			48,
			49,
			50,
			51,
			52,
			53,
			54,
			55,
			56,
			57,
			58,
			59,
			34,
			64,
			47,
			65,
			7,
			3,
			1,
			13,
			4
		};

		public static readonly int[] kTextureFormatsValueAndroid = new int[]
		{
			10,
			28,
			12,
			29,
			34,
			64,
			45,
			46,
			47,
			65,
			30,
			31,
			32,
			33,
			35,
			36,
			48,
			49,
			50,
			51,
			52,
			53,
			54,
			55,
			56,
			57,
			58,
			59,
			7,
			3,
			1,
			13,
			4
		};

		public static readonly int[] kTextureFormatsValueTizen = new int[]
		{
			34,
			7,
			3,
			1,
			13,
			4
		};

		public static readonly int[] kTextureFormatsValueWebGL = new int[]
		{
			10,
			12,
			28,
			29,
			7,
			3,
			1,
			2,
			4
		};

		public static readonly int[] kNormalFormatsValueDefault = new int[]
		{
			27,
			25,
			12,
			29,
			2,
			4
		};

		public static readonly int[] kTextureFormatsValueDefault = new int[]
		{
			10,
			12,
			28,
			29,
			7,
			3,
			1,
			2,
			4,
			17,
			26,
			27,
			24,
			25
		};

		public static readonly int[] kTextureFormatsValueSingleChannel = new int[]
		{
			1,
			26
		};

		public static readonly int[] kAndroidETC2FallbackOverrideValues = new int[]
		{
			0,
			1,
			2,
			3
		};

		public TextureImporterPlatformSettings platformTextureSettings
		{
			get
			{
				return this.m_PlatformSettings;
			}
		}

		public string name
		{
			get
			{
				return this.m_PlatformSettings.name;
			}
		}

		public bool overridden
		{
			get
			{
				return this.m_PlatformSettings.overridden;
			}
		}

		public bool overriddenIsDifferent
		{
			get
			{
				return this.m_OverriddenIsDifferent;
			}
		}

		public bool allAreOverridden
		{
			get
			{
				return this.isDefault || (this.overridden && !this.m_OverriddenIsDifferent);
			}
		}

		public int maxTextureSize
		{
			get
			{
				return this.m_PlatformSettings.maxTextureSize;
			}
		}

		public bool maxTextureSizeIsDifferent
		{
			get
			{
				return this.m_MaxTextureSizeIsDifferent;
			}
		}

		public TextureResizeAlgorithm resizeAlgorithm
		{
			get
			{
				return this.m_PlatformSettings.resizeAlgorithm;
			}
		}

		public bool resizeAlgorithmIsDifferent
		{
			get
			{
				return this.m_ResizeAlgorithmIsDifferent;
			}
		}

		public TextureImporterCompression textureCompression
		{
			get
			{
				return this.m_PlatformSettings.textureCompression;
			}
		}

		public bool textureCompressionIsDifferent
		{
			get
			{
				return this.m_TextureCompressionIsDifferent;
			}
		}

		public int compressionQuality
		{
			get
			{
				return this.m_PlatformSettings.compressionQuality;
			}
		}

		public bool compressionQualityIsDifferent
		{
			get
			{
				return this.m_CompressionQualityIsDifferent;
			}
		}

		public bool crunchedCompression
		{
			get
			{
				return this.m_PlatformSettings.crunchedCompression;
			}
		}

		public bool crunchedCompressionIsDifferent
		{
			get
			{
				return this.m_CrunchedCompressionIsDifferent;
			}
		}

		public TextureImporterFormat format
		{
			get
			{
				return this.m_PlatformSettings.format;
			}
		}

		public bool textureFormatIsDifferent
		{
			get
			{
				return this.m_TextureFormatIsDifferent;
			}
		}

		public bool allowsAlphaSplitting
		{
			get
			{
				return this.m_PlatformSettings.allowsAlphaSplitting;
			}
		}

		public bool allowsAlphaSplitIsDifferent
		{
			get
			{
				return this.m_AlphaSplitIsDifferent;
			}
		}

		public AndroidETC2FallbackOverride androidETC2FallbackOverride
		{
			get
			{
				return this.m_PlatformSettings.androidETC2FallbackOverride;
			}
		}

		public bool androidETC2FallbackOverrideIsDifferent
		{
			get
			{
				return this.m_AndroidETC2FallbackOverrideIsDifferent;
			}
		}

		public TextureImporter[] importers
		{
			get
			{
				return this.m_Importers;
			}
		}

		public bool isDefault
		{
			get
			{
				return this.name == TextureImporterInspector.s_DefaultPlatformName;
			}
		}

		public TextureImportPlatformSettings(string name, BuildTarget target, TextureImporterInspector inspector)
		{
			this.m_PlatformSettings.name = name;
			this.m_Target = target;
			this.m_Inspector = inspector;
			this.m_PlatformSettings.overridden = false;
			this.m_Importers = (from x in inspector.targets
			select x as TextureImporter).ToArray<TextureImporter>();
			for (int i = 0; i < this.importers.Length; i++)
			{
				TextureImporter textureImporter = this.importers[i];
				TextureImporterPlatformSettings platformTextureSettings = textureImporter.GetPlatformTextureSettings(name);
				if (i == 0)
				{
					this.m_PlatformSettings = platformTextureSettings;
				}
				else
				{
					if (platformTextureSettings.overridden != this.m_PlatformSettings.overridden)
					{
						this.m_OverriddenIsDifferent = true;
					}
					if (platformTextureSettings.format != this.m_PlatformSettings.format)
					{
						this.m_TextureFormatIsDifferent = true;
					}
					if (platformTextureSettings.maxTextureSize != this.m_PlatformSettings.maxTextureSize)
					{
						this.m_MaxTextureSizeIsDifferent = true;
					}
					if (platformTextureSettings.resizeAlgorithm != this.m_PlatformSettings.resizeAlgorithm)
					{
						this.m_ResizeAlgorithmIsDifferent = true;
					}
					if (platformTextureSettings.textureCompression != this.m_PlatformSettings.textureCompression)
					{
						this.m_TextureCompressionIsDifferent = true;
					}
					if (platformTextureSettings.compressionQuality != this.m_PlatformSettings.compressionQuality)
					{
						this.m_CompressionQualityIsDifferent = true;
					}
					if (platformTextureSettings.crunchedCompression != this.m_PlatformSettings.crunchedCompression)
					{
						this.m_CrunchedCompressionIsDifferent = true;
					}
					if (platformTextureSettings.allowsAlphaSplitting != this.m_PlatformSettings.allowsAlphaSplitting)
					{
						this.m_AlphaSplitIsDifferent = true;
					}
					if (platformTextureSettings.androidETC2FallbackOverride != this.m_PlatformSettings.androidETC2FallbackOverride)
					{
						this.m_AndroidETC2FallbackOverrideIsDifferent = true;
					}
				}
			}
			this.Sync();
		}

		public void SetOverriddenForAll(bool overridden)
		{
			this.m_PlatformSettings.overridden = overridden;
			this.m_OverriddenIsDifferent = false;
			this.SetChanged();
		}

		public void SetMaxTextureSizeForAll(int maxTextureSize)
		{
			this.m_PlatformSettings.maxTextureSize = maxTextureSize;
			this.m_MaxTextureSizeIsDifferent = false;
			this.SetChanged();
		}

		public void SetResizeAlgorithmForAll(TextureResizeAlgorithm algorithm)
		{
			this.m_PlatformSettings.resizeAlgorithm = algorithm;
			this.m_ResizeAlgorithmIsDifferent = false;
			this.SetChanged();
		}

		public void SetTextureCompressionForAll(TextureImporterCompression textureCompression)
		{
			this.m_PlatformSettings.textureCompression = textureCompression;
			this.m_TextureCompressionIsDifferent = false;
			this.m_HasChanged = true;
		}

		public void SetCompressionQualityForAll(int quality)
		{
			this.m_PlatformSettings.compressionQuality = quality;
			this.m_CompressionQualityIsDifferent = false;
			this.SetChanged();
		}

		public void SetCrunchedCompressionForAll(bool crunched)
		{
			this.m_PlatformSettings.crunchedCompression = crunched;
			this.m_CrunchedCompressionIsDifferent = false;
			this.SetChanged();
		}

		public void SetTextureFormatForAll(TextureImporterFormat format)
		{
			this.m_PlatformSettings.format = format;
			this.m_TextureFormatIsDifferent = false;
			this.SetChanged();
		}

		public void SetAllowsAlphaSplitForAll(bool value)
		{
			this.m_PlatformSettings.allowsAlphaSplitting = value;
			this.m_AlphaSplitIsDifferent = false;
			this.SetChanged();
		}

		public void SetAndroidETC2FallbackOverrideForAll(AndroidETC2FallbackOverride value)
		{
			this.m_PlatformSettings.androidETC2FallbackOverride = value;
			this.m_AndroidETC2FallbackOverrideIsDifferent = false;
			this.SetChanged();
		}

		public bool SupportsFormat(TextureImporterFormat format, TextureImporter importer)
		{
			TextureImporterSettings settings = this.GetSettings(importer);
			BuildTarget target = this.m_Target;
			int[] array;
			switch (target)
			{
			case BuildTarget.WiiU:
				array = TextureImportPlatformSettings.kTextureFormatsValueWiiU;
				goto IL_AD;
			case BuildTarget.tvOS:
				break;
			case BuildTarget.Switch:
				array = TextureImportPlatformSettings.kTextureFormatsValueSwitch;
				goto IL_AD;
			default:
				if (target == BuildTarget.Tizen)
				{
					array = TextureImportPlatformSettings.kTextureFormatsValueTizen;
					goto IL_AD;
				}
				if (target == BuildTarget.PSP2)
				{
					array = TextureImportPlatformSettings.kTextureFormatsValuePSP2;
					goto IL_AD;
				}
				if (target != BuildTarget.iOS)
				{
					if (target != BuildTarget.Android)
					{
						array = ((settings.textureType != TextureImporterType.NormalMap) ? TextureImportPlatformSettings.kTextureFormatsValueDefault : TextureImportPlatformSettings.kNormalFormatsValueDefault);
						goto IL_AD;
					}
					array = TextureImportPlatformSettings.kTextureFormatsValueAndroid;
					goto IL_AD;
				}
				break;
			}
			array = TextureImportPlatformSettings.kTextureFormatsValueApplePVR;
			IL_AD:
			return ((IList)array).Contains((int)format);
		}

		public TextureImporterSettings GetSettings(TextureImporter importer)
		{
			TextureImporterSettings textureImporterSettings = new TextureImporterSettings();
			importer.ReadTextureSettings(textureImporterSettings);
			this.m_Inspector.GetSerializedPropertySettings(textureImporterSettings);
			return textureImporterSettings;
		}

		public virtual void SetChanged()
		{
			this.m_HasChanged = true;
		}

		public virtual bool HasChanged()
		{
			return this.m_HasChanged;
		}

		public void Sync()
		{
			if (!this.isDefault && (!this.overridden || this.m_OverriddenIsDifferent))
			{
				TextureImportPlatformSettings textureImportPlatformSettings = this.m_Inspector.m_PlatformSettings[0];
				this.m_PlatformSettings.maxTextureSize = textureImportPlatformSettings.maxTextureSize;
				this.m_MaxTextureSizeIsDifferent = textureImportPlatformSettings.m_MaxTextureSizeIsDifferent;
				this.m_PlatformSettings.resizeAlgorithm = textureImportPlatformSettings.resizeAlgorithm;
				this.m_ResizeAlgorithmIsDifferent = textureImportPlatformSettings.m_ResizeAlgorithmIsDifferent;
				this.m_PlatformSettings.textureCompression = textureImportPlatformSettings.textureCompression;
				this.m_TextureCompressionIsDifferent = textureImportPlatformSettings.m_TextureCompressionIsDifferent;
				this.m_PlatformSettings.format = textureImportPlatformSettings.format;
				this.m_TextureFormatIsDifferent = textureImportPlatformSettings.m_TextureFormatIsDifferent;
				this.m_PlatformSettings.compressionQuality = textureImportPlatformSettings.compressionQuality;
				this.m_CompressionQualityIsDifferent = textureImportPlatformSettings.m_CompressionQualityIsDifferent;
				this.m_PlatformSettings.crunchedCompression = textureImportPlatformSettings.crunchedCompression;
				this.m_CrunchedCompressionIsDifferent = textureImportPlatformSettings.m_CrunchedCompressionIsDifferent;
				this.m_PlatformSettings.allowsAlphaSplitting = textureImportPlatformSettings.allowsAlphaSplitting;
				this.m_AlphaSplitIsDifferent = textureImportPlatformSettings.m_AlphaSplitIsDifferent;
				this.m_AndroidETC2FallbackOverrideIsDifferent = textureImportPlatformSettings.m_AndroidETC2FallbackOverrideIsDifferent;
			}
			if ((this.overridden || this.m_OverriddenIsDifferent) && this.m_PlatformSettings.format < (TextureImporterFormat)0)
			{
				this.m_PlatformSettings.format = TextureImporter.FormatFromTextureParameters(this.GetSettings(this.importers[0]), this.m_PlatformSettings, this.importers[0].DoesSourceTextureHaveAlpha(), this.importers[0].IsSourceTextureHDR(), this.m_Target);
				this.m_TextureFormatIsDifferent = false;
				for (int i = 1; i < this.importers.Length; i++)
				{
					TextureImporter textureImporter = this.importers[i];
					TextureImporterSettings settings = this.GetSettings(textureImporter);
					TextureImporterFormat textureImporterFormat = TextureImporter.FormatFromTextureParameters(settings, this.m_PlatformSettings, textureImporter.DoesSourceTextureHaveAlpha(), textureImporter.IsSourceTextureHDR(), this.m_Target);
					if (textureImporterFormat != this.m_PlatformSettings.format)
					{
						this.m_TextureFormatIsDifferent = true;
					}
				}
			}
		}

		private bool GetOverridden(TextureImporter importer)
		{
			bool overridden;
			if (!this.m_OverriddenIsDifferent)
			{
				overridden = this.overridden;
			}
			else
			{
				overridden = importer.GetPlatformTextureSettings(this.name).overridden;
			}
			return overridden;
		}

		public void Apply()
		{
			for (int i = 0; i < this.importers.Length; i++)
			{
				TextureImporter textureImporter = this.importers[i];
				TextureImporterPlatformSettings platformTextureSettings = textureImporter.GetPlatformTextureSettings(this.name);
				if (!this.m_OverriddenIsDifferent)
				{
					platformTextureSettings.overridden = this.m_PlatformSettings.overridden;
				}
				if (!this.m_TextureFormatIsDifferent)
				{
					platformTextureSettings.format = this.m_PlatformSettings.format;
				}
				if (!this.m_MaxTextureSizeIsDifferent)
				{
					platformTextureSettings.maxTextureSize = this.m_PlatformSettings.maxTextureSize;
				}
				if (!this.m_ResizeAlgorithmIsDifferent)
				{
					platformTextureSettings.resizeAlgorithm = this.m_PlatformSettings.resizeAlgorithm;
				}
				if (!this.m_TextureCompressionIsDifferent)
				{
					platformTextureSettings.textureCompression = this.m_PlatformSettings.textureCompression;
				}
				if (!this.m_CompressionQualityIsDifferent)
				{
					platformTextureSettings.compressionQuality = this.m_PlatformSettings.compressionQuality;
				}
				if (!this.m_CrunchedCompressionIsDifferent)
				{
					platformTextureSettings.crunchedCompression = this.m_PlatformSettings.crunchedCompression;
				}
				if (!this.m_AlphaSplitIsDifferent)
				{
					platformTextureSettings.allowsAlphaSplitting = this.m_PlatformSettings.allowsAlphaSplitting;
				}
				if (!this.m_AndroidETC2FallbackOverrideIsDifferent)
				{
					platformTextureSettings.androidETC2FallbackOverride = this.m_PlatformSettings.androidETC2FallbackOverride;
				}
				textureImporter.SetPlatformTextureSettings(platformTextureSettings);
			}
		}
	}
}
