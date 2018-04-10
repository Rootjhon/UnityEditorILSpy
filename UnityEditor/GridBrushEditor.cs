using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[CustomEditor(typeof(GridBrush))]
	public class GridBrushEditor : GridBrushEditorBase
	{
		private static class Styles
		{
			public static readonly GUIContent multieditingNotSupported = EditorGUIUtility.TextContent("Multi-editing cells not supported");

			public static readonly GUIContent tileLabel = EditorGUIUtility.TextContent("Tile|Tile set in tilemap");

			public static readonly GUIContent spriteLabel = EditorGUIUtility.TextContent("Sprite|Sprite set when tile is set in tilemap");

			public static readonly GUIContent colorLabel = EditorGUIUtility.TextContent("Color|Color set when tile is set in tilemap");

			public static readonly GUIContent gameObjectLabel = EditorGUIUtility.TextContent("GameObject|Game Object instantiated when tile is set in tilemap");

			public static readonly GUIContent colliderTypeLabel = EditorGUIUtility.TextContent("Collider Type|Collider shape used for tile");

			public static readonly GUIContent gridPositionLabel = EditorGUIUtility.TextContent("Grid Position|Position of the tile in the tilemap");

			public static readonly GUIContent lockColorLabel = EditorGUIUtility.TextContent("Lock Color|Prevents tilemap from changing color of tile");

			public static readonly GUIContent lockTransformLabel = EditorGUIUtility.TextContent("Lock Transform|Prevents tilemap from changing transform of tile");

			public static readonly GUIContent instantiateGameObjectRuntimeOnlyLabel = EditorGUIUtility.TextContent("Instantiate GameObject Runtime Only|Instantiates GameObject in runtime play mode only");

			public static readonly GUIContent floodFillPreviewLabel = EditorGUIUtility.TextContent("Show Flood Fill Preview|Whether a preview is shown while painting a Tilemap when Flood Fill mode is enabled");

			public static readonly string floodFillPreviewEditorPref = "GridBrush.EnableFloodFillPreview";
		}

		private int m_LastPreviewRefreshHash;

		private GridLayout m_LastGrid = null;

		private GameObject m_LastBrushTarget = null;

		private BoundsInt? m_LastBounds = null;

		private GridBrushBase.Tool? m_LastTool = null;

		private TileBase[] m_SelectionTiles;

		private Color[] m_SelectionColors;

		private Matrix4x4[] m_SelectionMatrices;

		private TileFlags[] m_SelectionFlagsArray;

		private Sprite[] m_SelectionSprites;

		private Tile.ColliderType[] m_SelectionColliderTypes;

		public GridBrush brush
		{
			get
			{
				return base.target as GridBrush;
			}
		}

		public override GameObject[] validTargets
		{
			get
			{
				return (from x in UnityEngine.Object.FindObjectsOfType<Tilemap>()
				select x.gameObject).ToArray<GameObject>();
			}
		}

		protected virtual void OnEnable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
		}

		protected virtual void OnDisable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
		}

		private void UndoRedoPerformed()
		{
			this.ClearPreview();
			this.m_LastPreviewRefreshHash = 0;
		}

		public override void OnPaintSceneGUI(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, bool executing)
		{
			BoundsInt position2 = position;
			bool flag = false;
			if (Event.current.type == EventType.Layout)
			{
				int hash = GridBrushEditor.GetHash(gridLayout, brushTarget, position, tool, this.brush);
				flag = (hash != this.m_LastPreviewRefreshHash);
				if (flag)
				{
					this.m_LastPreviewRefreshHash = hash;
				}
			}
			if (tool == GridBrushBase.Tool.Move)
			{
				if (flag && executing)
				{
					this.ClearPreview();
					this.PaintPreview(gridLayout, brushTarget, position.min);
				}
			}
			else if (tool == GridBrushBase.Tool.Paint || tool == GridBrushBase.Tool.Erase)
			{
				if (flag)
				{
					this.ClearPreview();
					if (tool != GridBrushBase.Tool.Erase)
					{
						this.PaintPreview(gridLayout, brushTarget, position.min);
					}
				}
				position2 = new BoundsInt(position.min - this.brush.pivot, this.brush.size);
			}
			else if (tool == GridBrushBase.Tool.Box)
			{
				if (flag)
				{
					this.ClearPreview();
					this.BoxFillPreview(gridLayout, brushTarget, position);
				}
			}
			else if (tool == GridBrushBase.Tool.FloodFill)
			{
				if (flag)
				{
					this.ClearPreview();
					this.FloodFillPreview(gridLayout, brushTarget, position.min);
				}
			}
			base.OnPaintSceneGUI(gridLayout, brushTarget, position2, tool, executing);
		}

		public override void OnSelectionInspectorGUI()
		{
			BoundsInt position = GridSelection.position;
			Tilemap component = GridSelection.target.GetComponent<Tilemap>();
			int num = position.size.x * position.size.y * position.size.z;
			if (component != null && num > 0)
			{
				base.OnSelectionInspectorGUI();
				GUILayout.Space(10f);
				if (this.m_SelectionTiles == null || this.m_SelectionTiles.Length != num)
				{
					this.m_SelectionTiles = new TileBase[num];
					this.m_SelectionColors = new Color[num];
					this.m_SelectionMatrices = new Matrix4x4[num];
					this.m_SelectionFlagsArray = new TileFlags[num];
					this.m_SelectionSprites = new Sprite[num];
					this.m_SelectionColliderTypes = new Tile.ColliderType[num];
				}
				int num2 = 0;
				foreach (Vector3Int current in position.allPositionsWithin)
				{
					this.m_SelectionTiles[num2] = component.GetTile(current);
					this.m_SelectionColors[num2] = component.GetColor(current);
					this.m_SelectionMatrices[num2] = component.GetTransformMatrix(current);
					this.m_SelectionFlagsArray[num2] = component.GetTileFlags(current);
					this.m_SelectionSprites[num2] = component.GetSprite(current);
					this.m_SelectionColliderTypes[num2] = component.GetColliderType(current);
					num2++;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = this.m_SelectionTiles.Any((TileBase tile) => tile != this.m_SelectionTiles.First<TileBase>());
				Vector3Int position2 = new Vector3Int(position.xMin, position.yMin, position.zMin);
				TileBase tile2 = EditorGUILayout.ObjectField(GridBrushEditor.Styles.tileLabel, component.GetTile(position2), typeof(TileBase), false, new GUILayoutOption[0]) as TileBase;
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(component, "Edit Tilemap");
					foreach (Vector3Int current2 in position.allPositionsWithin)
					{
						component.SetTile(current2, tile2);
					}
				}
				bool flag = this.m_SelectionFlagsArray.All((TileFlags flags) => (flags & TileFlags.LockColor) == (this.m_SelectionFlagsArray.First<TileFlags>() & TileFlags.LockColor));
				using (new EditorGUI.DisabledScope(!flag || (this.m_SelectionFlagsArray[0] & TileFlags.LockColor) != TileFlags.None))
				{
					EditorGUI.showMixedValue = this.m_SelectionColors.Any((Color color) => color != this.m_SelectionColors.First<Color>());
					EditorGUI.BeginChangeCheck();
					Color color2 = EditorGUILayout.ColorField(GridBrushEditor.Styles.colorLabel, this.m_SelectionColors[0], new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(component, "Edit Tilemap");
						foreach (Vector3Int current3 in position.allPositionsWithin)
						{
							component.SetColor(current3, color2);
						}
					}
				}
				bool flag2 = this.m_SelectionFlagsArray.All((TileFlags flags) => (flags & TileFlags.LockTransform) == (this.m_SelectionFlagsArray.First<TileFlags>() & TileFlags.LockTransform));
				using (new EditorGUI.DisabledScope(!flag2 || (this.m_SelectionFlagsArray[0] & TileFlags.LockTransform) != TileFlags.None))
				{
					EditorGUI.showMixedValue = this.m_SelectionMatrices.Any((Matrix4x4 matrix) => matrix != this.m_SelectionMatrices.First<Matrix4x4>());
					EditorGUI.BeginChangeCheck();
					Matrix4x4 transform = TileEditor.TransformMatrixOnGUI(this.m_SelectionMatrices[0]);
					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(component, "Edit Tilemap");
						foreach (Vector3Int current4 in position.allPositionsWithin)
						{
							component.SetTransformMatrix(current4, transform);
						}
					}
				}
				using (new EditorGUI.DisabledScope(true))
				{
					EditorGUI.showMixedValue = this.m_SelectionSprites.Any((Sprite sprite) => sprite != this.m_SelectionSprites.First<Sprite>());
					EditorGUILayout.ObjectField(GridBrushEditor.Styles.spriteLabel, this.m_SelectionSprites[0], typeof(Sprite), false, new GUILayoutOption[]
					{
						GUILayout.Height(16f)
					});
					EditorGUI.showMixedValue = this.m_SelectionColliderTypes.Any((Tile.ColliderType colliderType) => colliderType != this.m_SelectionColliderTypes.First<Tile.ColliderType>());
					EditorGUILayout.EnumPopup(GridBrushEditor.Styles.colliderTypeLabel, this.m_SelectionColliderTypes[0], new GUILayoutOption[0]);
					EditorGUI.showMixedValue = !flag;
					EditorGUILayout.Toggle(GridBrushEditor.Styles.lockColorLabel, (this.m_SelectionFlagsArray[0] & TileFlags.LockColor) != TileFlags.None, new GUILayoutOption[0]);
					EditorGUI.showMixedValue = !flag2;
					EditorGUILayout.Toggle(GridBrushEditor.Styles.lockTransformLabel, (this.m_SelectionFlagsArray[0] & TileFlags.LockTransform) != TileFlags.None, new GUILayoutOption[0]);
				}
				EditorGUI.showMixedValue = false;
			}
		}

		public override void OnMouseLeave()
		{
			this.ClearPreview();
		}

		public override void OnToolDeactivated(GridBrushBase.Tool tool)
		{
			this.ClearPreview();
		}

		public override void RegisterUndo(GameObject brushTarget, GridBrushBase.Tool tool)
		{
			if (brushTarget != null)
			{
				Undo.RegisterFullObjectHierarchyUndo(brushTarget, tool.ToString());
			}
		}

		public virtual void PaintPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			Vector3Int vector3Int = position - this.brush.pivot;
			Vector3Int a = vector3Int + this.brush.size;
			BoundsInt value = new BoundsInt(vector3Int, a - vector3Int);
			if (brushTarget != null)
			{
				Tilemap component = brushTarget.GetComponent<Tilemap>();
				foreach (Vector3Int current in value.allPositionsWithin)
				{
					Vector3Int brushPosition = current - vector3Int;
					GridBrush.BrushCell brushCell = this.brush.cells[this.brush.GetCellIndex(brushPosition)];
					if (brushCell.tile != null && component != null)
					{
						GridBrushEditor.SetTilemapPreviewCell(component, current, brushCell.tile, brushCell.matrix, brushCell.color);
					}
				}
			}
			this.m_LastGrid = gridLayout;
			this.m_LastBounds = new BoundsInt?(value);
			this.m_LastBrushTarget = brushTarget;
			this.m_LastTool = new GridBrushBase.Tool?(GridBrushBase.Tool.Paint);
		}

		public virtual void BoxFillPreview(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			if (brushTarget != null)
			{
				Tilemap component = brushTarget.GetComponent<Tilemap>();
				if (component != null)
				{
					foreach (Vector3Int current in position.allPositionsWithin)
					{
						Vector3Int vector3Int = current - position.min;
						GridBrush.BrushCell brushCell = this.brush.cells[this.brush.GetCellIndexWrapAround(vector3Int.x, vector3Int.y, vector3Int.z)];
						if (brushCell.tile != null)
						{
							GridBrushEditor.SetTilemapPreviewCell(component, current, brushCell.tile, brushCell.matrix, brushCell.color);
						}
					}
				}
			}
			this.m_LastGrid = gridLayout;
			this.m_LastBounds = new BoundsInt?(position);
			this.m_LastBrushTarget = brushTarget;
			this.m_LastTool = new GridBrushBase.Tool?(GridBrushBase.Tool.Box);
		}

		public virtual void FloodFillPreview(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (EditorPrefs.GetBool(GridBrushEditor.Styles.floodFillPreviewEditorPref, true))
			{
				BoundsInt value = new BoundsInt(position, Vector3Int.one);
				if (brushTarget != null && this.brush.cellCount > 0)
				{
					Tilemap component = brushTarget.GetComponent<Tilemap>();
					if (component != null)
					{
						GridBrush.BrushCell brushCell = this.brush.cells[0];
						component.EditorPreviewFloodFill(position, brushCell.tile);
						value.min = component.origin;
						value.max = component.origin + component.size;
					}
				}
				this.m_LastGrid = gridLayout;
				this.m_LastBounds = new BoundsInt?(value);
				this.m_LastBrushTarget = brushTarget;
				this.m_LastTool = new GridBrushBase.Tool?(GridBrushBase.Tool.FloodFill);
			}
		}

		[PreferenceItem("2D")]
		private static void PreferencesGUI()
		{
			EditorGUI.BeginChangeCheck();
			bool value = EditorGUILayout.Toggle(GridBrushEditor.Styles.floodFillPreviewLabel, EditorPrefs.GetBool(GridBrushEditor.Styles.floodFillPreviewEditorPref, true), new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetBool(GridBrushEditor.Styles.floodFillPreviewEditorPref, value);
			}
		}

		public virtual void ClearPreview()
		{
			if (!(this.m_LastGrid == null))
			{
				BoundsInt? lastBounds = this.m_LastBounds;
				if (lastBounds.HasValue && !(this.m_LastBrushTarget == null))
				{
					GridBrushBase.Tool? lastTool = this.m_LastTool;
					if (lastTool.HasValue)
					{
						Tilemap component = this.m_LastBrushTarget.GetComponent<Tilemap>();
						if (component != null)
						{
							GridBrushBase.Tool? lastTool2 = this.m_LastTool;
							if (lastTool2.HasValue)
							{
								GridBrushBase.Tool value = lastTool2.Value;
								if (value != GridBrushBase.Tool.FloodFill)
								{
									if (value != GridBrushBase.Tool.Box)
									{
										if (value == GridBrushBase.Tool.Paint)
										{
											foreach (Vector3Int current in this.m_LastBounds.Value.allPositionsWithin)
											{
												GridBrushEditor.ClearTilemapPreview(component, current);
											}
										}
									}
									else
									{
										Vector3Int position = this.m_LastBounds.Value.position;
										Vector3Int a = position + this.m_LastBounds.Value.size;
										BoundsInt boundsInt = new BoundsInt(position, a - position);
										foreach (Vector3Int current2 in boundsInt.allPositionsWithin)
										{
											GridBrushEditor.ClearTilemapPreview(component, current2);
										}
									}
								}
								else
								{
									component.ClearAllEditorPreviewTiles();
								}
							}
						}
						this.m_LastBrushTarget = null;
						this.m_LastGrid = null;
						this.m_LastBounds = null;
						this.m_LastTool = null;
					}
				}
			}
		}

		private static void SetTilemapPreviewCell(Tilemap map, Vector3Int location, TileBase tile, Matrix4x4 transformMatrix, Color color)
		{
			if (!(map == null))
			{
				map.SetEditorPreviewTile(location, tile);
				map.SetEditorPreviewTransformMatrix(location, transformMatrix);
				map.SetEditorPreviewColor(location, color);
			}
		}

		private static void ClearTilemapPreview(Tilemap map, Vector3Int location)
		{
			if (!(map == null))
			{
				map.SetEditorPreviewTile(location, null);
				map.SetEditorPreviewTransformMatrix(location, Matrix4x4.identity);
				map.SetEditorPreviewColor(location, Color.white);
			}
		}

		private static int GetHash(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, GridBrushBase.Tool tool, GridBrush brush)
		{
			int num = 0;
			num = num * 33 + ((!(gridLayout != null)) ? 0 : gridLayout.GetHashCode());
			num = num * 33 + ((!(brushTarget != null)) ? 0 : brushTarget.GetHashCode());
			num = num * 33 + position.GetHashCode();
			num = num * 33 + tool.GetHashCode();
			return num * 33 + ((!(brush != null)) ? 0 : brush.GetHashCode());
		}
	}
}
