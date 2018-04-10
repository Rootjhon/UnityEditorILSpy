using System;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build
{
	[UsedByNativeCode]
	[Serializable]
	public struct BuildUsageTagGlobal
	{
		internal uint m_LightmapModesUsed;

		internal uint m_LegacyLightmapModesUsed;

		internal uint m_DynamicLightmapsUsed;

		internal uint m_FogModesUsed;

		internal bool m_ShadowMasksUsed;

		internal bool m_SubtractiveUsed;

		public static BuildUsageTagGlobal operator |(BuildUsageTagGlobal x, BuildUsageTagGlobal y)
		{
			return new BuildUsageTagGlobal
			{
				m_LightmapModesUsed = x.m_LightmapModesUsed | y.m_LightmapModesUsed,
				m_LegacyLightmapModesUsed = x.m_LegacyLightmapModesUsed | y.m_LegacyLightmapModesUsed,
				m_DynamicLightmapsUsed = x.m_LightmapModesUsed | y.m_DynamicLightmapsUsed,
				m_FogModesUsed = x.m_FogModesUsed | y.m_FogModesUsed,
				m_ShadowMasksUsed = x.m_ShadowMasksUsed | y.m_ShadowMasksUsed,
				m_SubtractiveUsed = x.m_SubtractiveUsed | y.m_SubtractiveUsed
			};
		}
	}
}
