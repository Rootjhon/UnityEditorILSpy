using System;
using UnityEngine;

namespace UnityEditor
{
	internal static class GridSnapping
	{
		public static Func<Vector3, Vector3> snapPosition;

		public static Func<bool> activeFunc;

		public static bool active
		{
			get
			{
				return GridSnapping.activeFunc != null && GridSnapping.activeFunc();
			}
		}

		public static Vector3 Snap(Vector3 position)
		{
			Vector3 result;
			if (GridSnapping.snapPosition != null)
			{
				result = GridSnapping.snapPosition(position);
			}
			else
			{
				result = position;
			}
			return result;
		}
	}
}
