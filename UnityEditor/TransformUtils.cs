using System;
using UnityEngine;

namespace UnityEditor
{
	public static class TransformUtils
	{
		public static Vector3 GetInspectorRotation(Transform t)
		{
			return t.GetLocalEulerAngles(t.rotationOrder);
		}

		public static void SetInspectorRotation(Transform t, Vector3 r)
		{
			t.SetLocalEulerAngles(r, t.rotationOrder);
		}
	}
}
