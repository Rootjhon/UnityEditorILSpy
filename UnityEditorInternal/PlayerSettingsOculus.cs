using System;
using System.Runtime.CompilerServices;

namespace UnityEditorInternal
{
	internal class PlayerSettingsOculus
	{
		public static extern bool sharedDepthBuffer
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public static extern bool dashSupport
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}
	}
}
