using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[RequiredByNativeCode]
	public static class EditorAnalytics
	{
		public static extern bool enabled
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		internal static bool SendEventServiceInfo(object parameters)
		{
			return EditorAnalytics.SendEvent("serviceInfo", parameters);
		}

		internal static bool SendEventShowService(object parameters)
		{
			return EditorAnalytics.SendEvent("showService", parameters);
		}

		internal static bool SendEventTimelineInfo(object parameters)
		{
			return EditorAnalytics.SendEvent("timelineInfo", parameters);
		}

		internal static bool SendEventBuildTargetDevice(object parameters)
		{
			return EditorAnalytics.SendEvent("buildTargetDevice", parameters);
		}

		internal static bool SendEventSceneViewInfo(object parameters)
		{
			return EditorAnalytics.SendEvent("sceneViewInfo", parameters);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool SendEvent(string eventName, object parameters);
	}
}
