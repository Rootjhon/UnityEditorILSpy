using System;

namespace UnityEditor
{
	internal class L10n
	{
		private L10n()
		{
		}

		public static string Tr(string str)
		{
			return LocalizationDatabase.GetLocalizedString(str);
		}
	}
}
