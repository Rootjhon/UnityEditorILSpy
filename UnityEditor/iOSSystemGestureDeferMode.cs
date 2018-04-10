using System;

namespace UnityEditor
{
	[Flags]
	public enum iOSSystemGestureDeferMode : uint
	{
		None = 0u,
		TopEdge = 1u,
		LeftEdge = 2u,
		BottomEdge = 4u,
		RightEdge = 8u,
		All = 15u
	}
}
