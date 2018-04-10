using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor.CrashReporting
{
	public static class CrashReportingSettings
	{
		[ThreadAndSerializationSafe]
		public static extern bool enabled
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public static extern bool captureEditorExceptions
		{
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[GeneratedByOldBindingsGenerator]
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetEventUrl();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetEventUrl(string eventUrl);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern string GetNativeEventUrl();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void SetNativeEventUrl(string eventUrl);
	}
}
