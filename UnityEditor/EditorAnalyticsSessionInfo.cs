using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[RequiredByNativeCode]
	public static class EditorAnalyticsSessionInfo
	{
		public static extern long id
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static extern long elapsedTime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static extern long focusedElapsedTime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static extern long playbackElapsedTime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static extern long activeElapsedTime
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static extern string userId
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}
	}
}
