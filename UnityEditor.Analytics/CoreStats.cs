using System;
using System.Threading;
using UnityEngine.Scripting;

namespace UnityEditor.Analytics
{
	internal static class CoreStats
	{
		public delegate bool RequireInBuildDelegate();

		public static event CoreStats.RequireInBuildDelegate OnRequireInBuildHandler
		{
			add
			{
				CoreStats.RequireInBuildDelegate requireInBuildDelegate = CoreStats.OnRequireInBuildHandler;
				CoreStats.RequireInBuildDelegate requireInBuildDelegate2;
				do
				{
					requireInBuildDelegate2 = requireInBuildDelegate;
					requireInBuildDelegate = Interlocked.CompareExchange<CoreStats.RequireInBuildDelegate>(ref CoreStats.OnRequireInBuildHandler, (CoreStats.RequireInBuildDelegate)Delegate.Combine(requireInBuildDelegate2, value), requireInBuildDelegate);
				}
				while (requireInBuildDelegate != requireInBuildDelegate2);
			}
			remove
			{
				CoreStats.RequireInBuildDelegate requireInBuildDelegate = CoreStats.OnRequireInBuildHandler;
				CoreStats.RequireInBuildDelegate requireInBuildDelegate2;
				do
				{
					requireInBuildDelegate2 = requireInBuildDelegate;
					requireInBuildDelegate = Interlocked.CompareExchange<CoreStats.RequireInBuildDelegate>(ref CoreStats.OnRequireInBuildHandler, (CoreStats.RequireInBuildDelegate)Delegate.Remove(requireInBuildDelegate2, value), requireInBuildDelegate);
				}
				while (requireInBuildDelegate != requireInBuildDelegate2);
			}
		}

		[RequiredByNativeCode]
		public static bool RequiresCoreStatsInBuild()
		{
			bool result;
			if (CoreStats.OnRequireInBuildHandler != null)
			{
				Delegate[] invocationList = CoreStats.OnRequireInBuildHandler.GetInvocationList();
				for (int i = 0; i < invocationList.Length; i++)
				{
					CoreStats.RequireInBuildDelegate requireInBuildDelegate = (CoreStats.RequireInBuildDelegate)invocationList[i];
					if (requireInBuildDelegate())
					{
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		static CoreStats()
		{
			// Note: this type is marked as 'beforefieldinit'.
			CoreStats.OnRequireInBuildHandler = null;
		}
	}
}
