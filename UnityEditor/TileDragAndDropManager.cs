using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal class TileDragAndDropManager : ScriptableSingleton<TileDragAndDropManager>
	{
		private bool m_RegisteredEventHandlers;

		private Dictionary<Vector2Int, UnityEngine.Object> m_HoverData;

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			ScriptableSingleton<TileDragAndDropManager>.instance.RegisterEventHandlers();
		}

		private void OnEnable()
		{
			this.RegisterEventHandlers();
		}

		private void RegisterEventHandlers()
		{
			if (!this.m_RegisteredEventHandlers)
			{
				SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUIDelegate));
				this.m_RegisteredEventHandlers = true;
			}
		}

		private void OnDisable()
		{
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUIDelegate));
			this.m_RegisteredEventHandlers = false;
		}

		private void OnSceneGUIDelegate(SceneView sceneView)
		{
			Event current = Event.current;
			if (current.type == EventType.DragUpdated || current.type == EventType.DragPerform || current.type == EventType.DragExited || current.type == EventType.Repaint)
			{
				Grid activeGrid = TileDragAndDropManager.GetActiveGrid();
				if (!(activeGrid == null) && DragAndDrop.objectReferences.Length != 0)
				{
					Vector3 localPosition = GridEditorUtility.ScreenToLocal(activeGrid.transform, current.mousePosition);
					Vector3Int a = activeGrid.LocalToCell(localPosition);
					EventType type = current.type;
					if (type != EventType.DragUpdated)
					{
						if (type != EventType.DragPerform)
						{
							if (type == EventType.Repaint)
							{
								if (ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData != null)
								{
									Tilemap componentInParent = Selection.activeGameObject.GetComponentInParent<Tilemap>();
									if (componentInParent != null)
									{
										componentInParent.ClearAllEditorPreviewTiles();
									}
									DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
									foreach (KeyValuePair<Vector2Int, UnityEngine.Object> current2 in ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData)
									{
										Vector3Int position = a + new Vector3Int(current2.Key.x, current2.Key.y, 0);
										if (current2.Value is TileBase)
										{
											TileBase tile = current2.Value as TileBase;
											if (componentInParent != null)
											{
												componentInParent.SetEditorPreviewTile(position, tile);
											}
										}
									}
								}
							}
						}
						else if (ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData.Count > 0)
						{
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
							Dictionary<Vector2Int, TileBase> dictionary = TileDragAndDrop.ConvertToTileSheet(ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData);
							Tilemap orCreateActiveTilemap = TileDragAndDropManager.GetOrCreateActiveTilemap();
							orCreateActiveTilemap.ClearAllEditorPreviewTiles();
							foreach (KeyValuePair<Vector2Int, TileBase> current3 in dictionary)
							{
								Vector3Int position2 = new Vector3Int(a.x + current3.Key.x, a.y + current3.Key.y, 0);
								orCreateActiveTilemap.SetTile(position2, current3.Value);
							}
							ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData = null;
							GUI.changed = true;
							Event.current.Use();
						}
					}
					else
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						List<TileBase> validTiles = TileDragAndDrop.GetValidTiles(DragAndDrop.objectReferences);
						ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData = TileDragAndDrop.CreateHoverData(null, null, validTiles);
						if (ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData.Count > 0)
						{
							Event.current.Use();
							GUI.changed = true;
						}
					}
					if (ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData != null && (Event.current.type == EventType.DragExited || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)))
					{
						if (ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData.Count > 0)
						{
							Tilemap componentInParent2 = Selection.activeGameObject.GetComponentInParent<Tilemap>();
							if (componentInParent2 != null)
							{
								componentInParent2.ClearAllEditorPreviewTiles();
							}
							Event.current.Use();
						}
						ScriptableSingleton<TileDragAndDropManager>.instance.m_HoverData = null;
					}
				}
			}
		}

		private static Tilemap GetOrCreateActiveTilemap()
		{
			Tilemap result;
			if (Selection.activeGameObject != null)
			{
				Tilemap tilemap = Selection.activeGameObject.GetComponentInParent<Tilemap>();
				if (tilemap == null)
				{
					Grid componentInParent = Selection.activeGameObject.GetComponentInParent<Grid>();
					tilemap = TileDragAndDropManager.CreateNewTilemap(componentInParent);
				}
				result = tilemap;
			}
			else
			{
				result = null;
			}
			return result;
		}

		private static Tilemap CreateNewTilemap(Grid grid)
		{
			GameObject gameObject = new GameObject("Tilemap");
			gameObject.transform.SetParent(grid.gameObject.transform);
			Tilemap result = gameObject.AddComponent<Tilemap>();
			gameObject.AddComponent<TilemapRenderer>();
			return result;
		}

		public static Grid GetActiveGrid()
		{
			Grid result;
			if (Selection.activeGameObject != null)
			{
				result = Selection.activeGameObject.GetComponentInParent<Grid>();
			}
			else
			{
				result = null;
			}
			return result;
		}
	}
}
