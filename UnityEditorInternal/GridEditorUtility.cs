using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal static class GridEditorUtility
	{
		private const int k_GridGizmoVertexCount = 32000;

		private const float k_GridGizmoDistanceFalloff = 50f;

		public static Vector3Int ClampToGrid(Vector3Int p, Vector2Int origin, Vector2Int gridSize)
		{
			return new Vector3Int(Math.Max(Math.Min(p.x, origin.x + gridSize.x - 1), origin.x), Math.Max(Math.Min(p.y, origin.y + gridSize.y - 1), origin.y), p.z);
		}

		public static Vector3 ScreenToLocal(Transform transform, Vector2 screenPosition)
		{
			return GridEditorUtility.ScreenToLocal(transform, screenPosition, new Plane(transform.forward * -1f, transform.position));
		}

		public static Vector3 ScreenToLocal(Transform transform, Vector2 screenPosition, Plane plane)
		{
			Ray ray;
			if (Camera.current.orthographic)
			{
				Vector2 v = EditorGUIUtility.PointsToPixels(GUIClip.Unclip(screenPosition));
				v.y = (float)Screen.height - v.y;
				Vector3 origin = Camera.current.ScreenToWorldPoint(v);
				ray = new Ray(origin, Camera.current.transform.forward);
			}
			else
			{
				ray = HandleUtility.GUIPointToWorldRay(screenPosition);
			}
			float distance;
			plane.Raycast(ray, out distance);
			Vector3 point = ray.GetPoint(distance);
			return transform.InverseTransformPoint(point);
		}

		public static RectInt GetMarqueeRect(Vector2Int p1, Vector2Int p2)
		{
			return new RectInt(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y), Math.Abs(p2.x - p1.x) + 1, Math.Abs(p2.y - p1.y) + 1);
		}

		public static BoundsInt GetMarqueeBounds(Vector3Int p1, Vector3Int p2)
		{
			return new BoundsInt(Math.Min(p1.x, p2.x), Math.Min(p1.y, p2.y), Math.Min(p1.z, p2.z), Math.Abs(p2.x - p1.x) + 1, Math.Abs(p2.y - p1.y) + 1, Math.Abs(p2.z - p1.z) + 1);
		}

		[DebuggerHidden]
		public static IEnumerable<Vector2Int> GetPointsOnLine(Vector2Int p1, Vector2Int p2)
		{
			GridEditorUtility.<GetPointsOnLine>c__Iterator0 <GetPointsOnLine>c__Iterator = new GridEditorUtility.<GetPointsOnLine>c__Iterator0();
			<GetPointsOnLine>c__Iterator.p1 = p1;
			<GetPointsOnLine>c__Iterator.p2 = p2;
			GridEditorUtility.<GetPointsOnLine>c__Iterator0 expr_15 = <GetPointsOnLine>c__Iterator;
			expr_15.$PC = -2;
			return expr_15;
		}

		public static void DrawBatchedHorizontalLine(float x1, float x2, float y)
		{
			GL.Vertex3(x1, y, 0f);
			GL.Vertex3(x2, y, 0f);
			GL.Vertex3(x2, y + 1f, 0f);
			GL.Vertex3(x1, y + 1f, 0f);
		}

		public static void DrawBatchedVerticalLine(float y1, float y2, float x)
		{
			GL.Vertex3(x, y1, 0f);
			GL.Vertex3(x, y2, 0f);
			GL.Vertex3(x + 1f, y2, 0f);
			GL.Vertex3(x + 1f, y1, 0f);
		}

		public static void DrawBatchedLine(Vector3 p1, Vector3 p2)
		{
			GL.Vertex3(p1.x, p1.y, p1.z);
			GL.Vertex3(p2.x, p2.y, p2.z);
		}

		public static void DrawLine(Vector2 p1, Vector2 p2, Color color)
		{
			if (Event.current.type == EventType.Repaint)
			{
				HandleUtility.ApplyWireMaterial();
				GL.PushMatrix();
				GL.MultMatrix(GUI.matrix);
				GL.Begin(1);
				GL.Color(color);
				GridEditorUtility.DrawBatchedLine(p1, p2);
				GL.End();
				GL.PopMatrix();
			}
		}

		public static void DrawBox(Rect r, Color color)
		{
			if (Event.current.type == EventType.Repaint)
			{
				HandleUtility.ApplyWireMaterial();
				GL.PushMatrix();
				GL.MultMatrix(GUI.matrix);
				GL.Begin(1);
				GL.Color(color);
				GridEditorUtility.DrawBatchedLine(new Vector3(r.xMin, r.yMin, 0f), new Vector3(r.xMax, r.yMin, 0f));
				GridEditorUtility.DrawBatchedLine(new Vector3(r.xMax, r.yMin, 0f), new Vector3(r.xMax, r.yMax, 0f));
				GridEditorUtility.DrawBatchedLine(new Vector3(r.xMax, r.yMax, 0f), new Vector3(r.xMin, r.yMax, 0f));
				GridEditorUtility.DrawBatchedLine(new Vector3(r.xMin, r.yMax, 0f), new Vector3(r.xMin, r.yMin, 0f));
				GL.End();
				GL.PopMatrix();
			}
		}

		public static void DrawFilledBox(Rect r, Color color)
		{
			if (Event.current.type == EventType.Repaint)
			{
				HandleUtility.ApplyWireMaterial();
				GL.PushMatrix();
				GL.MultMatrix(GUI.matrix);
				GL.Begin(7);
				GL.Color(color);
				GL.Vertex3(r.xMin, r.yMin, 0f);
				GL.Vertex3(r.xMax, r.yMin, 0f);
				GL.Vertex3(r.xMax, r.yMax, 0f);
				GL.Vertex3(r.xMin, r.yMax, 0f);
				GL.End();
				GL.PopMatrix();
			}
		}

		public static void DrawGridMarquee(GridLayout gridLayout, BoundsInt area, Color color)
		{
			Vector3 vector = gridLayout.cellSize + gridLayout.cellGap;
			Vector3 one = Vector3.one;
			if (!Mathf.Approximately(vector.x, 0f))
			{
				one.x = gridLayout.cellSize.x / vector.x;
			}
			if (!Mathf.Approximately(vector.y, 0f))
			{
				one.y = gridLayout.cellSize.y / vector.y;
			}
			Vector3[] array = new Vector3[]
			{
				gridLayout.CellToLocal(new Vector3Int(area.xMin, area.yMin, 0)),
				gridLayout.CellToLocalInterpolated(new Vector3((float)(area.xMax - 1) + one.x, (float)area.yMin, 0f)),
				gridLayout.CellToLocalInterpolated(new Vector3((float)(area.xMax - 1) + one.x, (float)(area.yMax - 1) + one.y, 0f)),
				gridLayout.CellToLocalInterpolated(new Vector3((float)area.xMin, (float)(area.yMax - 1) + one.y, 0f))
			};
			HandleUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.MultMatrix(gridLayout.transform.localToWorldMatrix);
			GL.Begin(1);
			GL.Color(color);
			int i = 0;
			int num = array.Length - 1;
			while (i < array.Length)
			{
				GridEditorUtility.DrawBatchedLine(array[num], array[i]);
				num = i++;
			}
			GL.End();
			GL.PopMatrix();
		}

		public static void DrawGridGizmo(GridLayout gridLayout, Transform transform, Color color, ref Mesh gridMesh, ref Material gridMaterial)
		{
			if (Event.current.type == EventType.Repaint)
			{
				if (gridMesh == null)
				{
					gridMesh = GridEditorUtility.GenerateCachedGridMesh(gridLayout, color);
				}
				if (gridMaterial == null)
				{
					gridMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/GridGap.mat");
				}
				gridMaterial.SetVector("_Gap", gridLayout.cellSize);
				gridMaterial.SetVector("_Stride", gridLayout.cellGap + gridLayout.cellSize);
				gridMaterial.SetPass(0);
				GL.PushMatrix();
				if (gridMesh.GetTopology(0) == MeshTopology.Lines)
				{
					GL.Begin(1);
				}
				else
				{
					GL.Begin(7);
				}
				Graphics.DrawMeshNow(gridMesh, transform.localToWorldMatrix);
				GL.End();
				GL.PopMatrix();
			}
		}

		public static Vector3 GetSpriteWorldSize(Sprite sprite)
		{
			Vector3 result;
			if (sprite != null && sprite.rect.size.magnitude > 0f)
			{
				result = new Vector3(sprite.rect.size.x / sprite.pixelsPerUnit, sprite.rect.size.y / sprite.pixelsPerUnit, 1f);
			}
			else
			{
				result = Vector3.one;
			}
			return result;
		}

		private static Mesh GenerateCachedGridMesh(GridLayout gridLayout, Color color)
		{
			int num = -1000;
			int num2 = num * -1;
			int num3 = num2 - num;
			RectInt bounds = new RectInt(num, num, num3, num3);
			return GridEditorUtility.GenerateCachedGridMesh(gridLayout, color, 0f, bounds, MeshTopology.Lines);
		}

		public static Mesh GenerateCachedGridMesh(GridLayout gridLayout, Color color, float screenPixelSize, RectInt bounds, MeshTopology topology)
		{
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.HideAndDontSave;
			int num = 0;
			int num2 = (topology != MeshTopology.Quads) ? (4 * (bounds.size.x + bounds.size.y)) : (8 * (bounds.size.x + bounds.size.y));
			Vector3 b = new Vector3(screenPixelSize, 0f, 0f);
			Vector3 b2 = new Vector3(0f, screenPixelSize, 0f);
			Vector3[] array = new Vector3[num2];
			Vector2[] array2 = new Vector2[num2];
			Vector3 vector = gridLayout.cellSize + gridLayout.cellGap;
			Vector3Int vector3Int = new Vector3Int(0, bounds.min.y, 0);
			Vector3Int vector3Int2 = new Vector3Int(0, bounds.max.y, 0);
			Vector3 zero = Vector3.zero;
			if (!Mathf.Approximately(vector.x, 0f))
			{
				zero.x = gridLayout.cellSize.x / vector.x;
			}
			for (int i = bounds.min.x; i < bounds.max.x; i++)
			{
				vector3Int.x = i;
				vector3Int2.x = i;
				array[num] = gridLayout.CellToLocal(vector3Int);
				array[num + 1] = gridLayout.CellToLocal(vector3Int2);
				if (topology == MeshTopology.Quads)
				{
					array[num + 2] = gridLayout.CellToLocal(vector3Int2) + b;
					array[num + 3] = gridLayout.CellToLocal(vector3Int) + b;
					array2[num] = Vector2.zero;
					array2[num + 1] = new Vector2(0f, vector.y * (float)bounds.size.y);
					array2[num + 2] = new Vector2(0f, vector.y * (float)bounds.size.y);
					array2[num + 3] = Vector2.zero;
				}
				num += ((topology != MeshTopology.Quads) ? 2 : 4);
				array[num] = gridLayout.CellToLocalInterpolated(vector3Int + zero);
				array[num + 1] = gridLayout.CellToLocalInterpolated(vector3Int2 + zero);
				if (topology == MeshTopology.Quads)
				{
					array[num + 2] = gridLayout.CellToLocalInterpolated(vector3Int2 + zero) + b;
					array[num + 3] = gridLayout.CellToLocalInterpolated(vector3Int + zero) + b;
					array2[num] = Vector2.zero;
					array2[num + 1] = new Vector2(0f, vector.y * (float)bounds.size.y);
					array2[num + 2] = new Vector2(0f, vector.y * (float)bounds.size.y);
					array2[num + 3] = Vector2.zero;
				}
				num += ((topology != MeshTopology.Quads) ? 2 : 4);
			}
			vector3Int = new Vector3Int(bounds.min.x, 0, 0);
			vector3Int2 = new Vector3Int(bounds.max.x, 0, 0);
			zero = Vector3.zero;
			if (!Mathf.Approximately(vector.y, 0f))
			{
				zero.y = gridLayout.cellSize.y / vector.y;
			}
			for (int j = bounds.min.y; j < bounds.max.y; j++)
			{
				vector3Int.y = j;
				vector3Int2.y = j;
				array[num] = gridLayout.CellToLocal(vector3Int);
				array[num + 1] = gridLayout.CellToLocal(vector3Int2);
				if (topology == MeshTopology.Quads)
				{
					array[num + 2] = gridLayout.CellToLocal(vector3Int2) + b2;
					array[num + 3] = gridLayout.CellToLocal(vector3Int) + b2;
					array2[num] = Vector2.zero;
					array2[num + 1] = new Vector2(vector.x * (float)bounds.size.x, 0f);
					array2[num + 2] = new Vector2(vector.x * (float)bounds.size.x, 0f);
					array2[num + 3] = Vector2.zero;
				}
				num += ((topology != MeshTopology.Quads) ? 2 : 4);
				array[num] = gridLayout.CellToLocalInterpolated(vector3Int + zero);
				array[num + 1] = gridLayout.CellToLocalInterpolated(vector3Int2 + zero);
				if (topology == MeshTopology.Quads)
				{
					array[num + 2] = gridLayout.CellToLocalInterpolated(vector3Int2 + zero) + b2;
					array[num + 3] = gridLayout.CellToLocalInterpolated(vector3Int + zero) + b2;
					array2[num] = Vector2.zero;
					array2[num + 1] = new Vector2(vector.x * (float)bounds.size.x, 0f);
					array2[num + 2] = new Vector2(vector.x * (float)bounds.size.x, 0f);
					array2[num + 3] = Vector2.zero;
				}
				num += ((topology != MeshTopology.Quads) ? 2 : 4);
			}
			Vector2 vector2 = new Vector2(50f, 0f);
			Vector2[] array3 = new Vector2[num];
			int[] array4 = new int[num];
			Color[] array5 = new Color[num];
			for (int k = 0; k < num; k++)
			{
				array3[k] = vector2;
				array4[k] = k;
				array5[k] = color;
			}
			mesh.vertices = array;
			mesh.uv = array3;
			if (topology == MeshTopology.Quads)
			{
				mesh.uv2 = array2;
			}
			mesh.colors = array5;
			mesh.SetIndices(array4, topology, 0);
			return mesh;
		}
	}
}
