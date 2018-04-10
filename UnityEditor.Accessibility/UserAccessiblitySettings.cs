using System;

namespace UnityEditor.Accessibility
{
	internal static class UserAccessiblitySettings
	{
		private const string k_ColorBlindConditionPrefKey = "AccessibilityColorBlindCondition";

		private static ColorBlindCondition s_ColorBlindCondition;

		public static Action colorBlindConditionChanged;

		public static ColorBlindCondition colorBlindCondition
		{
			get
			{
				return UserAccessiblitySettings.s_ColorBlindCondition;
			}
			set
			{
				if (UserAccessiblitySettings.s_ColorBlindCondition != value)
				{
					UserAccessiblitySettings.s_ColorBlindCondition = value;
					EditorPrefs.SetInt("AccessibilityColorBlindCondition", (int)value);
					if (UserAccessiblitySettings.colorBlindConditionChanged != null)
					{
						UserAccessiblitySettings.colorBlindConditionChanged();
					}
				}
			}
		}

		static UserAccessiblitySettings()
		{
			UserAccessiblitySettings.s_ColorBlindCondition = (ColorBlindCondition)EditorPrefs.GetInt("AccessibilityColorBlindCondition", 0);
		}
	}
}
