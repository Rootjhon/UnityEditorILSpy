using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal class GridPaintPaletteClipboard : PaintableGrid
	{
		private static class Styles
		{
			public static readonly GUIStyle background = new GUIStyle("CurveEditorBackground");
		}

		private bool m_PaletteNeedsSave;

		private const float k_ZoomSpeed = 7f;

		private const float k_MinZoom = 10f;

		private const float k_MaxZoom = 100f;

		private const float k_Padding = 0.75f;

		private int m_KeyboardPanningID;

		private int m_MousePanningID;

		private float k_KeyboardPanningSpeed = 3f;

		private Vector3 m_KeyboardPanning;

		private Rect m_GUIRect = new Rect(0f, 0f, 200f, 200f);

		private bool m_OldFog;

		[SerializeField]
		private GridPaintPaletteWindow m_Owner;

		[SerializeField]
		private bool m_CameraInitializedToBounds;

		[SerializeField]
		public bool m_CameraPositionSaved;

		[SerializeField]
		public Vector3 m_CameraPosition;

		[SerializeField]
		public float m_CameraOrthographicSize;

		private RectInt? m_ActivePick;

		private Dictionary<Vector2Int, UnityEngine.Object> m_HoverData;

		private bool m_Unlocked;

		private bool m_PingTileAsset = false;

		private Mesh m_GridMesh;

		private int m_LastGridHash;

		private Material m_GridMaterial;

		private static readonly Color k_GridColor = Color.white.AlphaMultiplied(0.1f);

		private bool m_PaletteUsed;

		private Vector2? m_PreviousMousePosition;

		public Rect guiRect
		{
			get
			{
				return this.m_GUIRect;
			}
			set
			{
				if (this.m_GUIRect != value)
				{
					Rect gUIRect = this.m_GUIRect;
					this.m_GUIRect = value;
					this.OnViewSizeChanged(gUIRect, this.m_GUIRect);
				}
			}
		}

		public bool activeDragAndDrop
		{
			get
			{
				return DragAndDrop.objectReferences.Length > 0 && this.guiRect.Contains(Event.current.mousePosition);
			}
		}

		public GameObject palette
		{
			get
			{
				return this.m_Owner.palette;
			}
		}

		public GameObject paletteInstance
		{
			get
			{
				return this.m_Owner.paletteInstance;
			}
		}

		public Tilemap tilemap
		{
			get
			{
				return (!(this.paletteInstance != null)) ? null : this.paletteInstance.GetComponentInChildren<Tilemap>();
			}
		}

		private Grid grid
		{
			get
			{
				return (!(this.paletteInstance != null)) ? null : this.paletteInstance.GetComponent<Grid>();
			}
		}

		private Grid prefabGrid
		{
			get
			{
				return (!(this.palette != null)) ? null : this.palette.GetComponent<Grid>();
			}
		}

		public PreviewRenderUtility previewUtility
		{
			get
			{
				return this.m_Owner.previewUtility;
			}
		}

		private GridBrushBase gridBrush
		{
			get
			{
				return GridPaintingState.gridBrush;
			}
		}

		public TileBase activeTile
		{
			get
			{
				TileBase result;
				if (this.m_ActivePick.HasValue && this.m_ActivePick.Value.size == Vector2Int.one && GridPaintingState.defaultBrush != null && GridPaintingState.defaultBrush.cellCount > 0)
				{
					result = GridPaintingState.defaultBrush.cells[0].tile;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		private RectInt bounds
		{
			get
			{
				RectInt result;
				if (this.tilemap == null)
				{
					result = default(RectInt);
				}
				else
				{
					RectInt rectInt = new RectInt(this.tilemap.origin.x, this.tilemap.origin.y, this.tilemap.size.x, this.tilemap.size.y);
					if (GridPaintPaletteClipboard.TilemapIsEmpty(this.tilemap))
					{
						result = rectInt;
					}
					else
					{
						int num = this.tilemap.origin.x + this.tilemap.size.x;
						int num2 = this.tilemap.origin.y + this.tilemap.size.y;
						int num3 = this.tilemap.origin.x;
						int num4 = this.tilemap.origin.y;
						foreach (Vector2Int current in rectInt.allPositionsWithin)
						{
							if (this.tilemap.GetTile(new Vector3Int(current.x, current.y, 0)) != null)
							{
								num = Math.Min(num, current.x);
								num2 = Math.Min(num2, current.y);
								num3 = Math.Max(num3, current.x);
								num4 = Math.Max(num4, current.y);
							}
						}
						result = new RectInt(num, num2, num3 - num + 1, num4 - num2 + 1);
					}
				}
				return result;
			}
		}

		private Rect paddedBounds
		{
			get
			{
				float num = this.m_GUIRect.width / this.m_GUIRect.height;
				float x = this.previewUtility.camera.orthographicSize * num * 0.75f * 2f;
				float y = this.previewUtility.camera.orthographicSize * 0.75f * 2f;
				RectInt bounds = this.bounds;
				Vector2 vector = this.grid.CellToLocal(new Vector3Int(bounds.xMin, bounds.yMin, 0));
				Vector2 a = this.grid.CellToLocal(new Vector3Int(bounds.xMax, bounds.yMax, 0));
				Rect result = new Rect(vector - new Vector2(x, y), a - vector + new Vector2(x, y) * 2f);
				return result;
			}
		}

		private RectInt paddedBoundsInt
		{
			get
			{
				Vector3Int vector3Int = this.grid.LocalToCell(this.paddedBounds.min);
				Vector3Int vector3Int2 = this.grid.LocalToCell(this.paddedBounds.max) + Vector3Int.one;
				return new RectInt(vector3Int.x, vector3Int.y, vector3Int2.x - vector3Int.x, vector3Int2.y - vector3Int.y);
			}
		}

		private GameObject brushTarget
		{
			get
			{
				return (!(this.tilemap != null)) ? null : this.tilemap.gameObject;
			}
		}

		public bool unlocked
		{
			get
			{
				return this.m_Unlocked;
			}
			set
			{
				if (!value && this.m_Unlocked && this.tilemap != null)
				{
					this.tilemap.ClearAllEditorPreviewTiles();
					this.SavePaletteIfNecessary();
				}
				this.m_Unlocked = value;
			}
		}

		public bool pingTileAsset
		{
			get
			{
				return this.m_PingTileAsset;
			}
			set
			{
				if (value && !this.m_PingTileAsset && this.m_ActivePick.HasValue)
				{
					this.PingTileAsset(this.m_ActivePick.Value);
				}
				this.m_PingTileAsset = value;
			}
		}

		public bool invalidClipboard
		{
			get
			{
				return this.m_Owner.palette == null;
			}
		}

		public bool isReceivingDragAndDrop
		{
			get
			{
				return this.m_HoverData != null && this.m_HoverData.Count > 0;
			}
		}

		public bool showNewEmptyClipboardInfo
		{
			get
			{
				return !(this.paletteInstance == null) && !(this.tilemap == null) && GridPaintPaletteClipboard.TilemapIsEmpty(this.tilemap) && !this.isReceivingDragAndDrop && !this.m_PaletteUsed;
			}
		}

		public bool isModified
		{
			get
			{
				return this.m_PaletteNeedsSave;
			}
		}

		public GridPaintPaletteWindow owner
		{
			set
			{
				this.m_Owner = value;
			}
		}

		public void OnBeforePaletteSelectionChanged()
		{
			this.SavePaletteIfNecessary();
			this.DestroyPreviewInstance();
			this.FlushHoverData();
		}

		private void FlushHoverData()
		{
			if (this.m_HoverData != null)
			{
				this.m_HoverData.Clear();
				this.m_HoverData = null;
			}
		}

		public void OnAfterPaletteSelectionChanged()
		{
			this.m_PaletteUsed = false;
			this.ResetPreviewInstance();
			if (this.palette != null)
			{
				this.ResetPreviewCamera();
			}
		}

		public void SetupPreviewCameraOnInit()
		{
			if (this.m_CameraPositionSaved)
			{
				this.LoadSavedCameraPosition();
			}
			else
			{
				this.ResetPreviewCamera();
			}
		}

		private void LoadSavedCameraPosition()
		{
			this.previewUtility.camera.transform.position = this.m_CameraPosition;
			this.previewUtility.camera.orthographicSize = this.m_CameraOrthographicSize;
			this.previewUtility.camera.nearClipPlane = 0.01f;
			this.previewUtility.camera.farClipPlane = 100f;
		}

		private void ResetPreviewCamera()
		{
			this.previewUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
			this.previewUtility.camera.transform.rotation = Quaternion.identity;
			this.previewUtility.camera.nearClipPlane = 0.01f;
			this.previewUtility.camera.farClipPlane = 100f;
			this.FrameEntirePalette();
		}

		private void DestroyPreviewInstance()
		{
			this.m_Owner.DestroyPreviewInstance();
		}

		private void ResetPreviewInstance()
		{
			this.m_Owner.ResetPreviewInstance();
		}

		public void ResetPreviewMesh()
		{
			if (this.m_GridMesh != null)
			{
				UnityEngine.Object.DestroyImmediate(this.m_GridMesh);
				this.m_GridMesh = null;
			}
			this.m_GridMaterial = null;
		}

		public void FrameEntirePalette()
		{
			this.Frame(this.bounds);
		}

		private void Frame(RectInt rect)
		{
			if (!(this.grid == null))
			{
				this.previewUtility.camera.transform.position = this.grid.CellToLocalInterpolated(new Vector3(rect.center.x, rect.center.y, 0f));
				this.previewUtility.camera.transform.position.Set(this.previewUtility.camera.transform.position.x, this.previewUtility.camera.transform.position.y, -10f);
				float num = (this.grid.CellToLocal(new Vector3Int(0, rect.yMax, 0)) - this.grid.CellToLocal(new Vector3Int(0, rect.yMin, 0))).magnitude;
				float num2 = (this.grid.CellToLocal(new Vector3Int(rect.xMax, 0, 0)) - this.grid.CellToLocal(new Vector3Int(rect.xMin, 0, 0))).magnitude;
				num += this.grid.cellSize.y;
				num2 += this.grid.cellSize.x;
				float num3 = this.m_GUIRect.width / this.m_GUIRect.height;
				float num4 = num2 / num;
				if (num3 > num4)
				{
					this.previewUtility.camera.orthographicSize = num / 2f;
				}
				else
				{
					this.previewUtility.camera.orthographicSize = num2 / num3 / 2f;
				}
				this.ClampZoomAndPan();
			}
		}

		private void RefreshAllTiles()
		{
			if (this.tilemap != null)
			{
				this.tilemap.RefreshAllTiles();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			EditorApplication.editorApplicationQuit = (UnityAction)Delegate.Combine(EditorApplication.editorApplicationQuit, new UnityAction(this.EditorApplicationQuit));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			this.m_KeyboardPanningID = GUIUtility.GetPermanentControlID();
			this.m_MousePanningID = GUIUtility.GetPermanentControlID();
		}

		protected override void OnDisable()
		{
			this.m_CameraPosition = this.previewUtility.camera.transform.position;
			this.m_CameraOrthographicSize = this.previewUtility.camera.orthographicSize;
			this.m_CameraPositionSaved = true;
			this.SavePaletteIfNecessary();
			this.DestroyPreviewInstance();
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			EditorApplication.editorApplicationQuit = (UnityAction)Delegate.Remove(EditorApplication.editorApplicationQuit, new UnityAction(this.EditorApplicationQuit));
			base.OnDisable();
		}

		private void OnDestroy()
		{
			this.previewUtility.Cleanup();
		}

		public override void OnGUI()
		{
			if (this.guiRect.width != 0f && this.guiRect.height != 0f)
			{
				base.UpdateMouseGridPosition();
				this.HandleDragAndDrop();
				if (!(this.palette == null))
				{
					this.HandlePanAndZoom();
					if (!this.showNewEmptyClipboardInfo)
					{
						if (Event.current.type == EventType.Repaint && !this.m_CameraInitializedToBounds)
						{
							this.Frame(this.bounds);
							this.m_CameraInitializedToBounds = true;
						}
						this.HandleMouseEnterLeave();
						if (this.guiRect.Contains(Event.current.mousePosition))
						{
							if ((this.m_PreviousMousePosition.HasValue && !this.guiRect.Contains(this.m_PreviousMousePosition.Value)) || !this.m_PreviousMousePosition.HasValue)
							{
								if (GridPaintingState.activeBrushEditor != null)
								{
									GridPaintingState.activeBrushEditor.OnMouseEnter();
								}
							}
							base.OnGUI();
						}
						else if (this.m_PreviousMousePosition.HasValue && this.guiRect.Contains(this.m_PreviousMousePosition.Value) && !this.guiRect.Contains(Event.current.mousePosition))
						{
							if (GridPaintingState.activeBrushEditor != null)
							{
								GridPaintingState.activeBrushEditor.OnMouseLeave();
								this.Repaint();
							}
						}
						if (Event.current.type == EventType.Repaint)
						{
							this.Render();
						}
						else
						{
							this.DoBrush();
						}
						this.m_PreviousMousePosition = new Vector2?(Event.current.mousePosition);
					}
				}
			}
		}

		public void OnViewSizeChanged(Rect oldSize, Rect newSize)
		{
			if (oldSize.height * oldSize.width * newSize.height * newSize.width != 0f)
			{
				Camera camera = this.previewUtility.camera;
				Vector2 a = new Vector2(newSize.width / this.LocalToScreenRatio(newSize.height) - oldSize.width / this.LocalToScreenRatio(oldSize.height), newSize.height / this.LocalToScreenRatio(newSize.height) - oldSize.height / this.LocalToScreenRatio(oldSize.height));
				camera.transform.Translate(a / 2f);
				this.ClampZoomAndPan();
			}
		}

		private void EditorApplicationQuit()
		{
			this.SavePaletteIfNecessary();
		}

		private void UndoRedoPerformed()
		{
			if (this.unlocked)
			{
				this.m_PaletteNeedsSave = true;
				this.RefreshAllTiles();
				this.Repaint();
			}
		}

		private void HandlePanAndZoom()
		{
			switch (Event.current.type)
			{
			case EventType.MouseDown:
				if (GridPaintPaletteClipboard.MousePanningEvent() && this.guiRect.Contains(Event.current.mousePosition) && GUIUtility.hotControl == 0)
				{
					GUIUtility.hotControl = this.m_MousePanningID;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == this.m_MousePanningID)
				{
					this.ClampZoomAndPan();
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			case EventType.MouseMove:
				if (GUIUtility.hotControl == this.m_MousePanningID && !GridPaintPaletteClipboard.MousePanningEvent())
				{
					GUIUtility.hotControl = 0;
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == this.m_MousePanningID)
				{
					Vector3 translation = new Vector3(-Event.current.delta.x, Event.current.delta.y, 0f) / this.LocalToScreenRatio();
					this.previewUtility.camera.transform.Translate(translation);
					this.ClampZoomAndPan();
					Event.current.Use();
				}
				break;
			case EventType.KeyDown:
				if (GUIUtility.hotControl == 0)
				{
					switch (Event.current.keyCode)
					{
					case KeyCode.UpArrow:
						this.m_KeyboardPanning = new Vector3(0f, this.k_KeyboardPanningSpeed) / this.LocalToScreenRatio();
						GUIUtility.hotControl = this.m_KeyboardPanningID;
						Event.current.Use();
						break;
					case KeyCode.DownArrow:
						this.m_KeyboardPanning = new Vector3(0f, -this.k_KeyboardPanningSpeed) / this.LocalToScreenRatio();
						GUIUtility.hotControl = this.m_KeyboardPanningID;
						Event.current.Use();
						break;
					case KeyCode.RightArrow:
						this.m_KeyboardPanning = new Vector3(this.k_KeyboardPanningSpeed, 0f) / this.LocalToScreenRatio();
						GUIUtility.hotControl = this.m_KeyboardPanningID;
						Event.current.Use();
						break;
					case KeyCode.LeftArrow:
						this.m_KeyboardPanning = new Vector3(-this.k_KeyboardPanningSpeed, 0f) / this.LocalToScreenRatio();
						GUIUtility.hotControl = this.m_KeyboardPanningID;
						Event.current.Use();
						break;
					}
				}
				break;
			case EventType.KeyUp:
				if (GUIUtility.hotControl == this.m_KeyboardPanningID)
				{
					this.m_KeyboardPanning = Vector3.zero;
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			case EventType.ScrollWheel:
				if (this.guiRect.Contains(Event.current.mousePosition))
				{
					float num = HandleUtility.niceMouseDeltaZoom * (float)((!Event.current.shift) ? -3 : -9) * 7f;
					Camera camera = this.previewUtility.camera;
					Vector3 b = this.ScreenToLocal(Event.current.mousePosition);
					camera.orthographicSize = Mathf.Max(0.0001f, camera.orthographicSize * (1f + num * 0.001f));
					this.ClampZoomAndPan();
					Vector3 a = this.ScreenToLocal(Event.current.mousePosition);
					Vector3 b2 = a - b;
					camera.transform.position = camera.transform.position - b2;
					this.ClampZoomAndPan();
					Event.current.Use();
				}
				break;
			case EventType.Repaint:
				if (GUIUtility.hotControl == this.m_KeyboardPanningID)
				{
					this.previewUtility.camera.transform.Translate(this.m_KeyboardPanning);
					this.ClampZoomAndPan();
					this.Repaint();
				}
				if (GUIUtility.hotControl == this.m_MousePanningID)
				{
					EditorGUIUtility.AddCursorRect(this.guiRect, MouseCursor.Pan);
				}
				break;
			case EventType.ValidateCommand:
				if (Event.current.commandName == "FrameSelected")
				{
					Event.current.Use();
				}
				break;
			case EventType.ExecuteCommand:
				if (Event.current.commandName == "FrameSelected")
				{
					if (this.m_ActivePick.HasValue)
					{
						this.Frame(this.m_ActivePick.Value);
					}
					else
					{
						this.FrameEntirePalette();
					}
					Event.current.Use();
				}
				break;
			}
		}

		private static bool MousePanningEvent()
		{
			return (Event.current.button == 0 && Event.current.alt) || Event.current.button > 0;
		}

		public void ClampZoomAndPan()
		{
			float num = this.grid.cellSize.y * this.LocalToScreenRatio();
			if (num < 10f)
			{
				this.previewUtility.camera.orthographicSize = this.grid.cellSize.y * this.guiRect.height / 20f;
			}
			else if (num > 100f)
			{
				this.previewUtility.camera.orthographicSize = this.grid.cellSize.y * this.guiRect.height / 200f;
			}
			Camera camera = this.previewUtility.camera;
			Rect paddedBounds = this.paddedBounds;
			Vector3 vector = camera.transform.position;
			Vector2 vector2 = vector - new Vector3(camera.orthographicSize * (this.guiRect.width / this.guiRect.height), camera.orthographicSize);
			Vector2 vector3 = vector + new Vector3(camera.orthographicSize * (this.guiRect.width / this.guiRect.height), camera.orthographicSize);
			if (vector2.x < paddedBounds.min.x)
			{
				vector += new Vector3(paddedBounds.min.x - vector2.x, 0f, 0f);
			}
			if (vector2.y < paddedBounds.min.y)
			{
				vector += new Vector3(0f, paddedBounds.min.y - vector2.y, 0f);
			}
			if (vector3.x > paddedBounds.max.x)
			{
				vector += new Vector3(paddedBounds.max.x - vector3.x, 0f, 0f);
			}
			if (vector3.y > paddedBounds.max.y)
			{
				vector += new Vector3(0f, paddedBounds.max.y - vector3.y, 0f);
			}
			vector.Set(vector.x, vector.y, -10f);
			camera.transform.position = vector;
			UnityEngine.Object.DestroyImmediate(this.m_GridMesh);
			this.m_GridMesh = null;
		}

		private void Render()
		{
			if (this.m_GridMesh != null && this.GetGridHash() != this.m_LastGridHash)
			{
				this.ResetPreviewInstance();
				this.ResetPreviewMesh();
			}
			this.previewUtility.BeginPreview(this.guiRect, GridPaintPaletteClipboard.Styles.background);
			this.BeginPreviewInstance();
			this.RenderGrid();
			this.EndPreviewInstance();
			this.RenderDragAndDropPreview();
			this.RenderSelectedBrushMarquee();
			this.DoBrush();
			this.previewUtility.EndAndDrawPreview(this.guiRect);
			this.m_LastGridHash = this.GetGridHash();
		}

		private int GetGridHash()
		{
			int result;
			if (this.prefabGrid == null)
			{
				result = 0;
			}
			else
			{
				int num = this.prefabGrid.GetHashCode();
				num = num * 33 + this.prefabGrid.cellGap.GetHashCode();
				num = num * 33 + this.prefabGrid.cellLayout.GetHashCode();
				num = num * 33 + this.prefabGrid.cellSize.GetHashCode();
				num = num * 33 + this.prefabGrid.cellSwizzle.GetHashCode();
				num = num * 33 + SceneViewGridManager.sceneViewGridComponentGizmo.Color.GetHashCode();
				result = num;
			}
			return result;
		}

		private void RenderDragAndDropPreview()
		{
			if (this.activeDragAndDrop && this.m_HoverData != null && this.m_HoverData.Count != 0)
			{
				RectInt minMaxRect = TileDragAndDrop.GetMinMaxRect(this.m_HoverData.Keys.ToList<Vector2Int>());
				minMaxRect.position += base.mouseGridPosition;
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				GridEditorUtility.DrawGridMarquee(this.grid, new BoundsInt(new Vector3Int(minMaxRect.xMin, minMaxRect.yMin, 0), new Vector3Int(minMaxRect.width, minMaxRect.height, 1)), Color.white);
			}
		}

		private void RenderGrid()
		{
			if (this.m_GridMesh == null && this.grid.cellLayout == GridLayout.CellLayout.Rectangle)
			{
				this.m_GridMesh = GridEditorUtility.GenerateCachedGridMesh(this.grid, GridPaintPaletteClipboard.k_GridColor, 1f / this.LocalToScreenRatio(), this.paddedBoundsInt, MeshTopology.Quads);
			}
			GridEditorUtility.DrawGridGizmo(this.grid, this.grid.transform, GridPaintPaletteClipboard.k_GridColor, ref this.m_GridMesh, ref this.m_GridMaterial);
		}

		private void DoBrush()
		{
			if (!this.activeDragAndDrop)
			{
				this.RenderSelectedBrushMarquee();
				this.CallOnPaintSceneGUI(base.mouseGridPosition);
			}
		}

		private void BeginPreviewInstance()
		{
			this.m_OldFog = RenderSettings.fog;
			Unsupported.SetRenderSettingsUseFogNoDirty(false);
			Handles.DrawCameraImpl(this.m_GUIRect, this.previewUtility.camera, DrawCameraMode.Textured, false, default(DrawGridParameters), true, false);
			PreviewRenderUtility.SetEnabledRecursive(this.paletteInstance, true);
		}

		private void EndPreviewInstance()
		{
			this.previewUtility.Render(false, true);
			PreviewRenderUtility.SetEnabledRecursive(this.paletteInstance, false);
			Unsupported.SetRenderSettingsUseFogNoDirty(this.m_OldFog);
		}

		public void HandleDragAndDrop()
		{
			if (DragAndDrop.objectReferences.Length != 0 && this.guiRect.Contains(Event.current.mousePosition))
			{
				EventType type = Event.current.type;
				if (type != EventType.DragUpdated)
				{
					if (type != EventType.DragPerform)
					{
						if (type != EventType.Repaint)
						{
						}
					}
					else
					{
						if (this.m_HoverData == null || this.m_HoverData.Count == 0)
						{
							return;
						}
						this.RegisterUndo();
						bool flag = GridPaintPaletteClipboard.TilemapIsEmpty(this.tilemap);
						Vector2Int mouseGridPosition = base.mouseGridPosition;
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Dictionary<Vector2Int, TileBase> dictionary = TileDragAndDrop.ConvertToTileSheet(this.m_HoverData);
						foreach (KeyValuePair<Vector2Int, TileBase> current in dictionary)
						{
							this.SetTile(this.tilemap, mouseGridPosition + current.Key, current.Value, Color.white, Matrix4x4.identity);
						}
						this.OnPaletteChanged();
						this.m_PaletteNeedsSave = true;
						this.FlushHoverData();
						GUI.changed = true;
						this.SavePaletteIfNecessary();
						if (flag)
						{
							this.ResetPreviewInstance();
							this.FrameEntirePalette();
						}
						Event.current.Use();
						GUIUtility.ExitGUI();
					}
				}
				else
				{
					List<Texture2D> validSpritesheets = TileDragAndDrop.GetValidSpritesheets(DragAndDrop.objectReferences);
					List<Sprite> validSingleSprites = TileDragAndDrop.GetValidSingleSprites(DragAndDrop.objectReferences);
					List<TileBase> validTiles = TileDragAndDrop.GetValidTiles(DragAndDrop.objectReferences);
					this.m_HoverData = TileDragAndDrop.CreateHoverData(validSpritesheets, validSingleSprites, validTiles);
					if (this.m_HoverData != null && this.m_HoverData.Count > 0)
					{
						DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						Event.current.Use();
						GUI.changed = true;
					}
				}
				if (this.m_HoverData != null && (Event.current.type == EventType.DragExited || (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)))
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.None;
					this.FlushHoverData();
					Event.current.Use();
				}
			}
		}

		private static bool GridSizeUninitialized(Grid grid)
		{
			return Mathf.Approximately(grid.cellSize.x, 1E-05f) && Mathf.Approximately(grid.cellSize.y, 1E-05f) && Mathf.Approximately(grid.cellSize.z, 1E-05f);
		}

		public void SetEditorPreviewTile(Tilemap tilemap, Vector2Int position, TileBase tile, Color color, Matrix4x4 matrix)
		{
			Vector3Int position2 = new Vector3Int(position.x, position.y, 0);
			tilemap.SetEditorPreviewTile(position2, tile);
			tilemap.SetEditorPreviewColor(position2, color);
			tilemap.SetEditorPreviewTransformMatrix(position2, matrix);
		}

		public void SetTile(Tilemap tilemap, Vector2Int position, TileBase tile, Color color, Matrix4x4 matrix)
		{
			Vector3Int position2 = new Vector3Int(position.x, position.y, 0);
			tilemap.SetTile(position2, tile);
			tilemap.SetColor(position2, color);
			tilemap.SetTransformMatrix(position2, matrix);
		}

		protected override void Paint(Vector3Int position)
		{
			if (!(this.gridBrush == null))
			{
				this.gridBrush.Paint(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		protected override void Erase(Vector3Int position)
		{
			if (!(this.gridBrush == null))
			{
				this.gridBrush.Erase(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		protected override void BoxFill(BoundsInt position)
		{
			if (!(this.gridBrush == null))
			{
				this.gridBrush.BoxFill(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		protected override void BoxErase(BoundsInt position)
		{
			if (!(this.gridBrush == null))
			{
				this.gridBrush.BoxErase(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		protected override void FloodFill(Vector3Int position)
		{
			if (!(this.gridBrush == null))
			{
				this.gridBrush.FloodFill(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		protected override void PickBrush(BoundsInt position, Vector3Int pickingStart)
		{
			if (!(this.grid == null) && !(this.gridBrush == null))
			{
				this.gridBrush.Pick(this.grid, this.brushTarget, position, pickingStart);
				if (!PaintableGrid.InGridEditMode())
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPainting, default(Bounds), ScriptableSingleton<GridPaintingState>.instance);
				}
				this.m_ActivePick = new RectInt?(new RectInt(position.min.x, position.min.y, position.size.x, position.size.y));
			}
		}

		protected override void Select(BoundsInt position)
		{
			if (this.grid)
			{
				GridSelection.Select(this.brushTarget, position);
				this.gridBrush.Select(this.grid, this.brushTarget, position);
			}
		}

		protected override void Move(BoundsInt from, BoundsInt to)
		{
			if (this.grid)
			{
				this.gridBrush.Move(this.grid, this.brushTarget, from, to);
			}
		}

		protected override void MoveStart(BoundsInt position)
		{
			if (this.grid)
			{
				this.gridBrush.MoveStart(this.grid, this.brushTarget, position);
			}
		}

		protected override void MoveEnd(BoundsInt position)
		{
			if (this.grid)
			{
				this.gridBrush.MoveEnd(this.grid, this.brushTarget, position);
				this.OnPaletteChanged();
			}
		}

		public override void Repaint()
		{
			this.m_Owner.Repaint();
		}

		protected override void ClearGridSelection()
		{
			GridSelection.Clear();
		}

		protected override void OnBrushPickStarted()
		{
		}

		protected override void OnBrushPickDragged(BoundsInt position)
		{
			this.m_ActivePick = new RectInt?(new RectInt(position.min.x, position.min.y, position.size.x, position.size.y));
		}

		private void PingTileAsset(RectInt rect)
		{
			if (rect.size == Vector2Int.zero && this.tilemap != null)
			{
				TileBase tile = this.tilemap.GetTile(new Vector3Int(rect.xMin, rect.yMin, 0));
				EditorGUIUtility.PingObject(tile);
				Selection.activeObject = tile;
			}
		}

		protected override bool ValidateFloodFillPosition(Vector3Int position)
		{
			return true;
		}

		protected override bool PickingIsDefaultTool()
		{
			return !this.m_Unlocked;
		}

		protected override bool CanPickOutsideEditMode()
		{
			return true;
		}

		protected override GridLayout.CellLayout CellLayout()
		{
			return this.grid.cellLayout;
		}

		protected override Vector2Int ScreenToGrid(Vector2 screenPosition)
		{
			Vector2 v = this.ScreenToLocal(screenPosition);
			Vector3Int vector3Int = this.grid.LocalToCell(v);
			Vector2Int result = new Vector2Int(vector3Int.x, vector3Int.y);
			return result;
		}

		private void RenderSelectedBrushMarquee()
		{
			if (!this.unlocked && this.m_ActivePick.HasValue)
			{
				this.DrawSelectionGizmo(this.m_ActivePick.Value);
			}
		}

		protected void DrawSelectionGizmo(RectInt rect)
		{
			if (Event.current.type == EventType.Repaint && GUI.enabled)
			{
				Color color = Color.white;
				if (base.isPicking)
				{
					color = Color.cyan;
				}
				GridEditorUtility.DrawGridMarquee(this.grid, new BoundsInt(new Vector3Int(rect.xMin, rect.yMin, 0), new Vector3Int(rect.width, rect.height, 1)), color);
			}
		}

		private void HandleMouseEnterLeave()
		{
			if (Event.current.type == EventType.MouseEnterWindow)
			{
				if (PaintableGrid.InGridEditMode())
				{
					GridPaintingState.activeGrid = this;
					Event.current.Use();
				}
			}
			else if (Event.current.type == EventType.MouseLeaveWindow)
			{
				if (this.m_PreviousMousePosition.HasValue && this.guiRect.Contains(this.m_PreviousMousePosition.Value) && GridPaintingState.activeBrushEditor != null)
				{
					GridPaintingState.activeBrushEditor.OnMouseLeave();
				}
				this.m_PreviousMousePosition = null;
				if (PaintableGrid.InGridEditMode())
				{
					GridPaintingState.activeGrid = null;
					Event.current.Use();
					this.Repaint();
				}
			}
		}

		private void CallOnPaintSceneGUI(Vector2Int position)
		{
			if (this.unlocked || EditMode.editMode == EditMode.SceneViewEditMode.GridSelect || EditMode.editMode == EditMode.SceneViewEditMode.GridPicking)
			{
				bool flag = GridSelection.active && GridSelection.target == this.brushTarget;
				if (flag || !(GridPaintingState.activeGrid != this))
				{
					GridBrushBase gridBrush = GridPaintingState.gridBrush;
					if (!(gridBrush == null))
					{
						RectInt marqueeRect = new RectInt(position, new Vector2Int(1, 1));
						if (this.m_MarqueeStart.HasValue)
						{
							marqueeRect = GridEditorUtility.GetMarqueeRect(position, this.m_MarqueeStart.Value);
						}
						else if (flag)
						{
							marqueeRect = new RectInt(GridSelection.position.xMin, GridSelection.position.yMin, GridSelection.position.size.x, GridSelection.position.size.y);
						}
						GridLayout gridLayout = (!(this.tilemap != null)) ? this.grid : this.tilemap;
						BoundsInt position2 = new BoundsInt(new Vector3Int(marqueeRect.x, marqueeRect.y, 0), new Vector3Int(marqueeRect.width, marqueeRect.height, 1));
						if (GridPaintingState.activeBrushEditor != null)
						{
							GridPaintingState.activeBrushEditor.OnPaintSceneGUI(gridLayout, this.brushTarget, position2, PaintableGrid.EditModeToBrushTool(EditMode.editMode), this.m_MarqueeStart.HasValue || base.executing);
						}
						else
						{
							GridBrushEditorBase.OnPaintSceneGUIInternal(gridLayout, Selection.activeGameObject, position2, PaintableGrid.EditModeToBrushTool(EditMode.editMode), this.m_MarqueeStart.HasValue || base.executing);
						}
					}
				}
			}
		}

		protected override void RegisterUndo()
		{
			if (!this.invalidClipboard)
			{
				Undo.RegisterFullObjectHierarchyUndo(this.paletteInstance, "Edit Palette");
			}
		}

		private void OnPaletteChanged()
		{
			this.m_PaletteUsed = true;
			this.m_PaletteNeedsSave = true;
			Undo.FlushUndoRecordObjects();
		}

		public void SavePaletteIfNecessary()
		{
			if (this.m_PaletteNeedsSave)
			{
				this.m_Owner.SavePalette();
				this.m_PaletteNeedsSave = false;
			}
		}

		private static RectInt SnapInsideBounds(RectInt rect, RectInt bounds)
		{
			if (rect.xMin < bounds.xMin)
			{
				rect.position += new Vector2Int(bounds.xMin - rect.xMin, 0);
			}
			if (rect.yMin < bounds.yMin)
			{
				rect.position += new Vector2Int(0, bounds.yMin - rect.yMin);
			}
			if (rect.xMax > bounds.xMax)
			{
				rect.position -= new Vector2Int(rect.xMax - bounds.xMax, 0);
			}
			if (rect.yMax > bounds.yMax)
			{
				rect.position -= new Vector2Int(0, rect.yMax - bounds.yMax);
			}
			return rect;
		}

		public Vector2 GridToScreen(Vector2 gridPosition)
		{
			Vector3 cellPosition = new Vector3(gridPosition.x, gridPosition.y, 0f);
			return this.LocalToScreen(this.grid.CellToLocalInterpolated(cellPosition));
		}

		protected Vector2 GridToScreen(Vector2Int gridPosition)
		{
			Vector3Int cellPosition = new Vector3Int(gridPosition.x, gridPosition.y, 0);
			return this.LocalToScreen(this.grid.CellToLocal(cellPosition));
		}

		public Vector2 ScreenToLocal(Vector2 screenPosition)
		{
			Vector2 a = this.previewUtility.camera.transform.position;
			screenPosition -= new Vector2(this.guiRect.xMin, this.guiRect.yMin);
			Vector2 a2 = new Vector2(screenPosition.x - this.guiRect.width * 0.5f, this.guiRect.height * 0.5f - screenPosition.y);
			return a + a2 / this.LocalToScreenRatio();
		}

		protected Vector2 LocalToScreen(Vector2 localPosition)
		{
			Vector2 vector = this.previewUtility.camera.transform.position;
			Vector2 a = new Vector2(localPosition.x - vector.x, vector.y - localPosition.y);
			return a * this.LocalToScreenRatio() + new Vector2(this.guiRect.width * 0.5f + this.guiRect.xMin, this.guiRect.height * 0.5f + this.guiRect.yMin);
		}

		private float LocalToScreenRatio()
		{
			return this.guiRect.height / (this.previewUtility.camera.orthographicSize * 2f);
		}

		private float LocalToScreenRatio(float viewHeight)
		{
			return viewHeight / (this.previewUtility.camera.orthographicSize * 2f);
		}

		protected Vector2Int GetPivot(Vector2Int corner, Vector2Int position)
		{
			return position - corner;
		}

		private static bool TilemapIsEmpty(Tilemap tilemap)
		{
			return tilemap.GetUsedTilesCount() == 0;
		}
	}
}
