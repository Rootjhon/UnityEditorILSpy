using System;
using UnityEditor.Accessibility;
using UnityEngine;
using UnityEngine.Accessibility;

namespace UnityEditorInternal
{
	internal class ProfilerColors
	{
		private static readonly Color[] s_DefaultColors;

		private static readonly Color[] s_ColorBlindSafeColors;

		internal static Color allocationSample;

		internal static Color internalSample;

		public static Color[] currentColors
		{
			get
			{
				return (UserAccessiblitySettings.colorBlindCondition != ColorBlindCondition.Default) ? ProfilerColors.s_ColorBlindSafeColors : ProfilerColors.s_DefaultColors;
			}
		}

		static ProfilerColors()
		{
			ProfilerColors.allocationSample = new Color(0.7f, 0.1f, 0.3f, 1f);
			ProfilerColors.internalSample = new Color(0.392156869f, 0.392156869f, 0.392156869f, 0.75f);
			ProfilerColors.s_DefaultColors = new Color[]
			{
				new Color(0.4831376f, 0.6211768f, 0.0219608f, 1f),
				new Color(0.2070592f, 0.5333336f, 0.6556864f, 1f),
				new Color(0.8f, 0.4423528f, 0f, 1f),
				new Color(0.4486272f, 0.4078432f, 0.050196f, 1f),
				new Color(0.7749016f, 0.6368624f, 0.0250984f, 1f),
				new Color(0.5333336f, 0.16f, 0.0282352f, 1f),
				new Color(0.3827448f, 0.2886272f, 0.5239216f, 1f),
				new Color(0.478431374f, 0.482352942f, 0.117647059f, 1f),
				new Color(0.9411765f, 0.5019608f, 0.5019608f, 1f),
				new Color(0.6627451f, 0.6627451f, 0.6627451f, 1f),
				new Color(0.545098066f, 0f, 0.545098066f, 1f),
				new Color(255f, 0.894117653f, 0.709803939f, 1f),
				new Color(0.1254902f, 0.698039234f, 0.6666667f, 1f),
				new Color(0.4831376f, 0.6211768f, 0.0219608f, 1f),
				new Color(0.3827448f, 0.2886272f, 0.5239216f, 1f),
				new Color(0.8f, 0.4423528f, 0f, 1f),
				new Color(0.4486272f, 0.4078432f, 0.050196f, 1f),
				new Color(0.4831376f, 0.6211768f, 0.0219608f, 1f)
			};
			ProfilerColors.s_ColorBlindSafeColors = new Color[ProfilerColors.s_DefaultColors.Length];
			VisionUtility.GetColorBlindSafePalette(ProfilerColors.s_ColorBlindSafeColors, 0.3f, 1f);
		}
	}
}
