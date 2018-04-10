using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class FreeRotate
	{
		private static readonly Color s_DimmingColor = new Color(0f, 0f, 0f, 0.078f);

		private static Vector2 s_CurrentMousePosition;

		public static Quaternion Do(int id, Quaternion rotation, Vector3 position, float size)
		{
			return FreeRotate.Do(id, rotation, position, size, true);
		}

		internal static Quaternion Do(int id, Quaternion rotation, Vector3 position, float size, bool drawCircle)
		{
			Vector3 vector = Handles.matrix.MultiplyPoint(position);
			Matrix4x4 matrix = Handles.matrix;
			Event current = Event.current;
			switch (current.GetTypeForControl(id))
			{
			case EventType.MouseDown:
				if (HandleUtility.nearestControl == id && current.button == 0)
				{
					GUIUtility.hotControl = id;
					Tools.LockHandlePosition();
					FreeRotate.s_CurrentMousePosition = current.mousePosition;
					HandleUtility.ignoreRaySnapObjects = null;
					current.Use();
					EditorGUIUtility.SetWantsMouseJumping(1);
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == id && (current.button == 0 || current.button == 2))
				{
					Tools.UnlockHandlePosition();
					GUIUtility.hotControl = 0;
					current.Use();
					EditorGUIUtility.SetWantsMouseJumping(0);
				}
				break;
			case EventType.MouseMove:
				if (id == HandleUtility.nearestControl)
				{
					HandleUtility.Repaint();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					bool flag = EditorGUI.actionKey && current.shift;
					if (flag)
					{
						if (HandleUtility.ignoreRaySnapObjects == null)
						{
							Handles.SetupIgnoreRaySnapObjects();
						}
						object obj = HandleUtility.RaySnap(HandleUtility.GUIPointToWorldRay(current.mousePosition));
						if (obj != null)
						{
							Quaternion quaternion = Quaternion.LookRotation(((RaycastHit)obj).point - position);
							if (Tools.pivotRotation == PivotRotation.Global)
							{
								Transform activeTransform = Selection.activeTransform;
								if (activeTransform)
								{
									Quaternion rhs = Quaternion.Inverse(activeTransform.rotation) * rotation;
									quaternion *= rhs;
								}
							}
							rotation = quaternion;
						}
					}
					else
					{
						FreeRotate.s_CurrentMousePosition += current.delta;
						Vector3 vector2 = Camera.current.transform.TransformDirection(new Vector3(-current.delta.y, -current.delta.x, 0f));
						rotation = Quaternion.AngleAxis(current.delta.magnitude, vector2.normalized) * rotation;
					}
					GUI.changed = true;
					current.Use();
				}
				break;
			case EventType.KeyDown:
				if (current.keyCode == KeyCode.Escape && GUIUtility.hotControl == id)
				{
					Tools.UnlockHandlePosition();
					EditorGUIUtility.SetWantsMouseJumping(0);
				}
				break;
			case EventType.Repaint:
			{
				Color color = Color.white;
				bool flag2 = id == GUIUtility.hotControl;
				bool flag3 = id == HandleUtility.nearestControl && GUIUtility.hotControl == 0;
				if (flag2)
				{
					color = Handles.color;
					Handles.color = Handles.selectedColor;
				}
				else if (flag3)
				{
					color = Handles.color;
					Handles.color = Handles.preselectionColor;
				}
				Handles.matrix = Matrix4x4.identity;
				if (drawCircle)
				{
					Handles.DrawWireDisc(vector, Camera.current.transform.forward, size);
				}
				if (flag3 || flag2)
				{
					Handles.color = FreeRotate.s_DimmingColor;
					Handles.DrawSolidDisc(vector, Camera.current.transform.forward, size);
				}
				Handles.matrix = matrix;
				if (flag2 || flag3)
				{
					Handles.color = color;
				}
				break;
			}
			case EventType.Layout:
				Handles.matrix = Matrix4x4.identity;
				HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(vector, size) + 5f);
				Handles.matrix = matrix;
				break;
			}
			return rotation;
		}
	}
}
