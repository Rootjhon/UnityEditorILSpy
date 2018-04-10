using System;
using UnityEngine;

namespace UnityEditor
{
	internal class TransformTool : ManipulationTool
	{
		private static TransformTool s_Instance;

		private static Vector3 s_Scale;

		public static void OnGUI(SceneView view)
		{
			if (TransformTool.s_Instance == null)
			{
				TransformTool.s_Instance = new TransformTool();
			}
			TransformTool.s_Instance.OnToolGUI(view);
		}

		public override void ToolGUI(SceneView view, Vector3 handlePosition, bool isStatic)
		{
			Handles.TransformHandleIds @default = Handles.TransformHandleIds.Default;
			TransformManipulator.BeginManipulationHandling(false);
			if (@default.scale.Has(GUIUtility.hotControl) || @default.rotation.Has(GUIUtility.hotControl))
			{
				Tools.LockHandlePosition();
			}
			else
			{
				Tools.UnlockHandlePosition();
			}
			EditorGUI.BeginChangeCheck();
			if (Event.current.type == EventType.MouseDown)
			{
				TransformTool.s_Scale = Vector3.one;
			}
			Vector3 a = handlePosition;
			Quaternion handleRotation = Tools.handleRotation;
			Quaternion quaternion = handleRotation;
			Vector3 vector = TransformTool.s_Scale;
			Vector3 vector2 = vector;
			Handles.TransformHandle(@default, ref a, ref quaternion, ref vector2, Handles.TransformHandleParam.Default);
			TransformTool.s_Scale = vector2;
			if (EditorGUI.EndChangeCheck() && !isStatic)
			{
				Undo.RecordObjects(Selection.transforms, "Transform Manipulation");
				Vector3 vector3 = a - TransformManipulator.mouseDownHandlePosition;
				if (vector3 != Vector3.zero)
				{
					ManipulationToolUtility.SetMinDragDifferenceForPos(handlePosition);
					TransformManipulator.SetPositionDelta(vector3);
				}
				float num;
				Vector3 point;
				(Quaternion.Inverse(handleRotation) * quaternion).ToAngleAxis(out num, out point);
				if (!Mathf.Approximately(num, 0f))
				{
					Transform[] transforms = Selection.transforms;
					for (int i = 0; i < transforms.Length; i++)
					{
						Transform transform = transforms[i];
						if (Tools.pivotMode == PivotMode.Center)
						{
							transform.RotateAround(handlePosition, handleRotation * point, num);
						}
						else if (TransformManipulator.individualSpace)
						{
							transform.Rotate(transform.rotation * point, num, Space.World);
						}
						else
						{
							transform.Rotate(handleRotation * point, num, Space.World);
						}
						transform.SetLocalEulerHint(transform.GetLocalEulerAngles(transform.rotationOrder));
						if (transform.parent != null)
						{
							transform.SendTransformChangedScale();
						}
					}
					Tools.handleRotation = quaternion;
				}
				if (vector2 != vector)
				{
					TransformManipulator.SetScaleDelta(vector2, quaternion);
				}
			}
			TransformManipulator.EndManipulationHandling();
		}
	}
}
