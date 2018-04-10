using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine.Scripting;

namespace UnityEditor
{
	public sealed class EditorBuildSettings
	{
		public static event Action sceneListChanged
		{
			add
			{
				Action action = EditorBuildSettings.sceneListChanged;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref EditorBuildSettings.sceneListChanged, (Action)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action action = EditorBuildSettings.sceneListChanged;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref EditorBuildSettings.sceneListChanged, (Action)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public static extern EditorBuildSettingsScene[] scenes
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[RequiredByNativeCode]
		private static void SceneListChanged()
		{
			if (EditorBuildSettings.sceneListChanged != null)
			{
				EditorBuildSettings.sceneListChanged();
			}
		}
	}
}
