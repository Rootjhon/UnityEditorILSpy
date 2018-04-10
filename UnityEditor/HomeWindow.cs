using System;
using System.Runtime.CompilerServices;

namespace UnityEditor
{
	internal static class HomeWindow
	{
		public enum HomeMode
		{
			Login,
			License,
			Launching,
			NewProjectOnly,
			OpenProjectOnly,
			ManageLicense,
			Welcome,
			Tutorial
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool Show(HomeWindow.HomeMode mode);
	}
}
