using System;

namespace UnityEditor
{
	internal enum TextureUsageMode
	{
		Default,
		BakedLightmapDoubleLDR,
		BakedLightmapRGBM,
		NormalmapDXT5nm,
		NormalmapPlain,
		RGBMEncoded,
		AlwaysPadded,
		DoubleLDR,
		BakedLightmapFullHDR,
		RealtimeLightmapRGBM
	}
}
