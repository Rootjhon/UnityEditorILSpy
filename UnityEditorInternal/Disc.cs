using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class Disc
	{
		private const int k_MaxSnapMarkers = 72;

		private const float k_RotationUnitSnapMajorMarkerStep = 45f;

		private const float k_RotationUnitSnapMarkerSize = 0.1f;

		private const float k_RotationUnitSnapMajorMarkerSize = 0.2f;

		private const float k_GrabZoneScale = 0.3f;

		private static Vector2 s_StartMousePosition;

		private static Vector2 s_CurrentMousePosition;

		private static Vector3 s_StartPosition;

		private static Vector3 s_StartAxis;

		private static Quaternion s_StartRotation;

		private static float s_RotationDist;

		public static Quaternion Do(int id, Quaternion rotation, Vector3 position, Vector3 axis, float size, bool cutoffPlane, float snap)
		{
			return Disc.Do(id, rotation, position, axis, size, cutoffPlane, snap, true, true, Handles.secondaryColor);
		}

		public static Quaternion Do(int id, Quaternion rotation, Vector3 position, Vector3 axis, float size, bool cutoffPlane, float snap, bool enableRayDrag, bool showHotArc, Color fillColor)
		{
			if (Mathf.Abs(Vector3.Dot(Camera.current.transform.forward, axis)) > 0.999f)
			{
				cutoffPlane = false;
			}
			Event current = Event.current;
			switch (current.GetTypeForControl(id))
			{
			case EventType.MouseDown:
				if (HandleUtility.nearestControl == id && current.button == 0)
				{
					GUIUtility.hotControl = id;
					Tools.LockHandlePosition();
					if (cutoffPlane)
					{
						Vector3 normalized = Vector3.Cross(axis, Camera.current.transform.forward).normalized;
						Disc.s_StartPosition = HandleUtility.ClosestPointToArc(position, axis, normalized, 180f, size);
					}
					else
					{
						Disc.s_StartPosition = HandleUtility.ClosestPointToDisc(position, axis, size);
					}
					Disc.s_RotationDist = 0f;
					Disc.s_StartRotation = rotation;
					Disc.s_StartAxis = axis;
					Disc.s_CurrentMousePosition = (Disc.s_StartMousePosition = Event.current.mousePosition);
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
					bool flag = EditorGUI.actionKey && current.shift && enableRayDrag;
					if (flag)
					{
						if (HandleUtility.ignoreRaySnapObjects == null)
						{
							Handles.SetupIgnoreRaySnapObjects();
						}
						object obj = HandleUtility.RaySnap(HandleUtility.GUIPointToWorldRay(current.mousePosition));
						if (obj != null && (double)Vector3.Dot(axis.normalized, rotation * Vector3.forward) < 0.999)
						{
							Vector3 vector = ((RaycastHit)obj).point - position;
							Vector3 forward = vector - Vector3.Dot(vector, axis.normalized) * axis.normalized;
							rotation = Quaternion.LookRotation(forward, rotation * Vector3.up);
						}
					}
					else
					{
						Vector3 normalized2 = Vector3.Cross(axis, position - Disc.s_StartPosition).normalized;
						Disc.s_CurrentMousePosition += current.delta;
						Disc.s_RotationDist = HandleUtility.CalcLineTranslation(Disc.s_StartMousePosition, Disc.s_CurrentMousePosition, Disc.s_StartPosition, normalized2) / size * 30f;
						Disc.s_RotationDist = Handles.SnapValue(Disc.s_RotationDist, snap);
						rotation = Quaternion.AngleAxis(Disc.s_RotationDist * -1f, Disc.s_StartAxis) * Disc.s_StartRotation;
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
				if (id == GUIUtility.hotControl)
				{
					color = Handles.color;
					Handles.color = Handles.selectedColor;
				}
				else if (id == HandleUtility.nearestControl && GUIUtility.hotControl == 0)
				{
					color = Handles.color;
					Handles.color = Handles.preselectionColor;
				}
				if (GUIUtility.hotControl == id)
				{
					Color color2 = Handles.color;
					Vector3 normalized3 = (Disc.s_StartPosition - position).normalized;
					Handles.color = fillColor;
					Handles.DrawLine(position, position + normalized3 * size);
					float angle = -Mathf.Sign(Disc.s_RotationDist) * Mathf.Repeat(Mathf.Abs(Disc.s_RotationDist), 360f);
					Vector3 a = Quaternion.AngleAxis(angle, axis) * normalized3;
					Handles.DrawLine(position, position + a * size);
					Handles.color = fillColor * new Color(1f, 1f, 1f, 0.2f);
					int i = 0;
					int num = (int)Mathf.Abs(Disc.s_RotationDist * 0.00277777785f);
					while (i < num)
					{
						Handles.DrawSolidDisc(position, axis, size);
						i++;
					}
					Handles.DrawSolidArc(position, axis, normalized3, angle, size);
					if (EditorGUI.actionKey && snap > 0f)
					{
						Disc.DrawRotationUnitSnapMarkers(position, axis, size, 0.1f, snap, normalized3);
						Disc.DrawRotationUnitSnapMarkers(position, axis, size, 0.2f, 45f, normalized3);
					}
					Handles.color = color2;
				}
				if ((showHotArc && GUIUtility.hotControl == id) || (GUIUtility.hotControl != id && !cutoffPlane))
				{
					Handles.DrawWireDisc(position, axis, size);
				}
				else if (GUIUtility.hotControl != id && cutoffPlane)
				{
					Vector3 normalized4 = Vector3.Cross(axis, Camera.current.transform.forward).normalized;
					Handles.DrawWireArc(position, axis, normalized4, 180f, size);
				}
				if (id == GUIUtility.hotControl || (id == HandleUtility.nearestControl && GUIUtility.hotControl == 0))
				{
					Handles.color = color;
				}
				break;
			}
			case EventType.Layout:
			{
				float distance;
				if (cutoffPlane)
				{
					Vector3 normalized5 = Vector3.Cross(axis, Camera.current.transform.forward).normalized;
					distance = HandleUtility.DistanceToArc(position, axis, normalized5, 180f, size) * 0.3f;
				}
				else
				{
					distance = HandleUtility.DistanceToDisc(position, axis, size) * 0.3f;
				}
				HandleUtility.AddControl(id, distance);
				break;
			}
			}
			return rotation;
		}

		private static void DrawRotationUnitSnapMarkers(Vector3 position, Vector3 axis, float handleSize, float markerSize, float snap, Vector3 from)
		{
			int num = Mathf.FloorToInt(360f / snap);
			bool flag = num > 72;
			int num2 = Mathf.Min(num, 72);
			int num3 = Mathf.RoundToInt((float)num2 * 0.5f);
			for (int i = -num3; i < num3; i++)
			{
				Quaternion rotation = Quaternion.AngleAxis((float)i * snap, axis);
				Vector3 a = rotation * from;
				Vector3 p = position + (1f - markerSize) * handleSize * a;
				Vector3 p2 = position + 1f * handleSize * a;
				Handles.color = Handles.selectedColor;
				if (flag)
				{
					float a2 = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Abs((float)i / ((float)num2 - 1f) - 0.5f) * 2f);
					Handles.color = new Color(Handles.color.r, Handles.color.g, Handles.color.b, a2);
				}
				Handles.DrawLine(p, p2);
			}
		}
	}
}
