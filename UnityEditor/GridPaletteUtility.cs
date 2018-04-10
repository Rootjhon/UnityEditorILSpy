using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal static class GridPaletteUtility
	{
		public static RectInt GetBounds(GameObject palette)
		{
			RectInt result;
			if (palette == null)
			{
				result = default(RectInt);
			}
			else
			{
				Vector2Int p = new Vector2Int(2147483647, 2147483647);
				Vector2Int p2 = new Vector2Int(-2147483648, -2147483648);
				Tilemap[] componentsInChildren = palette.GetComponentsInChildren<Tilemap>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Tilemap tilemap = componentsInChildren[i];
					Vector3Int editorPreviewOrigin = tilemap.editorPreviewOrigin;
					Vector3Int vector3Int = editorPreviewOrigin + tilemap.editorPreviewSize;
					Vector2Int vector2Int = new Vector2Int(Mathf.Min(editorPreviewOrigin.x, vector3Int.x), Mathf.Min(editorPreviewOrigin.y, vector3Int.y));
					Vector2Int vector2Int2 = new Vector2Int(Mathf.Max(editorPreviewOrigin.x, vector3Int.x), Mathf.Max(editorPreviewOrigin.y, vector3Int.y));
					p = new Vector2Int(Mathf.Min(p.x, vector2Int.x), Mathf.Min(p.y, vector2Int.y));
					p2 = new Vector2Int(Mathf.Max(p2.x, vector2Int2.x), Mathf.Max(p2.y, vector2Int2.y));
				}
				result = GridEditorUtility.GetMarqueeRect(p, p2);
			}
			return result;
		}

		public static GameObject CreateNewPaletteNamed(string name, GridLayout.CellLayout layout, GridPalette.CellSizing cellSizing, Vector3 cellSize)
		{
			string folder = (!ProjectBrowser.s_LastInteractedProjectBrowser) ? "Assets" : ProjectBrowser.s_LastInteractedProjectBrowser.GetActiveFolderPath();
			string text = EditorUtility.SaveFolderPanel("Create palette into folder ", folder, "");
			text = FileUtil.GetProjectRelativePath(text);
			GameObject result;
			if (string.IsNullOrEmpty(text))
			{
				result = null;
			}
			else
			{
				result = GridPaletteUtility.CreateNewPalette(text, name, layout, cellSizing, cellSize);
			}
			return result;
		}

		public static GameObject CreateNewPalette(string folderPath, string name, GridLayout.CellLayout layout, GridPalette.CellSizing cellSizing, Vector3 cellSize)
		{
			GameObject gameObject = new GameObject(name);
			Grid grid = gameObject.AddComponent<Grid>();
			grid.cellSize = cellSize;
			grid.cellLayout = layout;
			GridPaletteUtility.CreateNewLayer(gameObject, "Layer1", layout);
			string text = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + name + ".prefab");
			UnityEngine.Object @object = PrefabUtility.CreateEmptyPrefab(text);
			GridPalette gridPalette = ScriptableObject.CreateInstance<GridPalette>();
			gridPalette.name = "Palette Settings";
			gridPalette.cellSizing = cellSizing;
			AssetDatabase.AddObjectToAsset(gridPalette, @object);
			PrefabUtility.ReplacePrefab(gameObject, @object, ReplacePrefabOptions.Default);
			AssetDatabase.Refresh();
			UnityEngine.Object.DestroyImmediate(gameObject);
			return AssetDatabase.LoadAssetAtPath<GameObject>(text);
		}

		public static GameObject CreateNewLayer(GameObject paletteGO, string name, GridLayout.CellLayout layout)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.AddComponent<Tilemap>();
			gameObject.AddComponent<TilemapRenderer>();
			gameObject.transform.parent = paletteGO.transform;
			gameObject.layer = paletteGO.layer;
			if (layout == GridLayout.CellLayout.Rectangle)
			{
				paletteGO.GetComponent<Grid>().cellSize = new Vector3(1f, 1f, 0f);
			}
			return gameObject;
		}

		public static Vector3 CalculateAutoCellSize(Grid grid, Vector3 defaultValue)
		{
			Tilemap[] componentsInChildren = grid.GetComponentsInChildren<Tilemap>();
			Tilemap[] array = componentsInChildren;
			Vector3 result;
			for (int i = 0; i < array.Length; i++)
			{
				Tilemap tilemap = array[i];
				foreach (Vector3Int current in tilemap.cellBounds.allPositionsWithin)
				{
					Sprite sprite = tilemap.GetSprite(current);
					if (sprite != null)
					{
						result = new Vector3(sprite.rect.width, sprite.rect.height, 0f) / sprite.pixelsPerUnit;
						return result;
					}
				}
			}
			result = defaultValue;
			return result;
		}
	}
}
