using System;
using UnityEditor.U2D.Interface;

namespace UnityEditor.U2D.Common
{
	internal class TexturePlatformSettingsFormatHelper : ITexturePlatformSettingsFormatHelper
	{
		public void AcquireTextureFormatValuesAndStrings(BuildTarget buildTarget, out int[] formatValues, out string[] formatStrings)
		{
			if (TextureImporterInspector.IsGLESMobileTargetPlatform(buildTarget))
			{
				if (buildTarget == BuildTarget.iOS || buildTarget == BuildTarget.tvOS)
				{
					formatValues = TextureImportPlatformSettings.kTextureFormatsValueApplePVR;
					formatStrings = TextureImporterInspector.s_TextureFormatStringsApplePVR;
				}
				else
				{
					formatValues = TextureImportPlatformSettings.kTextureFormatsValueAndroid;
					formatStrings = TextureImporterInspector.s_TextureFormatStringsAndroid;
				}
			}
			else if (buildTarget == BuildTarget.WebGL)
			{
				formatValues = TextureImportPlatformSettings.kTextureFormatsValueWebGL;
				formatStrings = TextureImporterInspector.s_TextureFormatStringsWebGL;
			}
			else if (buildTarget == BuildTarget.WiiU)
			{
				formatValues = TextureImportPlatformSettings.kTextureFormatsValueWiiU;
				formatStrings = TextureImporterInspector.s_TextureFormatStringsWiiU;
			}
			else if (buildTarget == BuildTarget.PSP2)
			{
				formatValues = TextureImportPlatformSettings.kTextureFormatsValuePSP2;
				formatStrings = TextureImporterInspector.s_TextureFormatStringsPSP2;
			}
			else if (buildTarget == BuildTarget.Switch)
			{
				formatValues = TextureImportPlatformSettings.kTextureFormatsValueSwitch;
				formatStrings = TextureImporterInspector.s_TextureFormatStringsSwitch;
			}
			else
			{
				formatValues = TextureImportPlatformSettings.kTextureFormatsValueDefault;
				formatStrings = TextureImporterInspector.s_TextureFormatStringsDefault;
			}
		}

		public bool TextureFormatRequireCompressionQualityInput(TextureImporterFormat format)
		{
			return TextureImporterInspector.IsFormatRequireCompressionSetting(format);
		}
	}
}
