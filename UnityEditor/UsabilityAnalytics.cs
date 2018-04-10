using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityEditor
{
	internal static class UsabilityAnalytics
	{
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void Track(string page);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void Event(string category, string action, string label, int value);

		internal static void SendEvent(string subType, DateTime startTime, TimeSpan duration, bool isBlocking, object parameters)
		{
			if (startTime.Kind == DateTimeKind.Local)
			{
				throw new ArgumentException("Local DateTimes are not supported, use UTC instead.");
			}
			UsabilityAnalytics.SendEvent(subType, startTime.Ticks, duration.Ticks, isBlocking, parameters);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SendEvent(string subType, long startTimeTicks, long durationTicks, bool isBlocking, object parameters);
	}
}
