using System;
using UnityEngine;

namespace UnityEditor
{
	[CustomEditor(typeof(GridSelection))]
	internal class GridSelectionEditor : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle header = new GUIStyle("IN GameObjectHeader");

			public static readonly GUIContent gridSelectionLabel = EditorGUIUtility.TextContent("Grid Selection");
		}

		private const float iconSize = 32f;

		public override void OnInspectorGUI()
		{
			if (GridPaintingState.activeBrushEditor && GridSelection.active)
			{
				GridPaintingState.activeBrushEditor.OnSelectionInspectorGUI();
			}
		}

		protected override void OnHeaderGUI()
		{
			EditorGUILayout.BeginHorizontal(GridSelectionEditor.Styles.header, new GUILayoutOption[0]);
			Texture2D miniTypeThumbnail = AssetPreview.GetMiniTypeThumbnail(typeof(Grid));
			GUILayout.Label(miniTypeThumbnail, new GUILayoutOption[]
			{
				GUILayout.Width(32f),
				GUILayout.Height(32f)
			});
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Label(GridSelectionEditor.Styles.gridSelectionLabel, new GUILayoutOption[0]);
			GridSelection.position = EditorGUILayout.BoundsIntField(GUIContent.none, GridSelection.position, new GUILayoutOption[0]);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			this.DrawHeaderHelpAndSettingsGUI(GUILayoutUtility.GetLastRect());
		}

		public bool HasFrameBounds()
		{
			return GridSelection.active;
		}

		public Bounds OnGetFrameBounds()
		{
			Bounds result = default(Bounds);
			if (GridSelection.active)
			{
				Vector3Int min = GridSelection.position.min;
				Vector3Int max = GridSelection.position.max;
				Vector3 b = GridSelection.grid.CellToWorld(min);
				Vector3 a = GridSelection.grid.CellToWorld(max);
				result = new Bounds((a + b) * 0.5f, a - b);
			}
			return result;
		}
	}
}
