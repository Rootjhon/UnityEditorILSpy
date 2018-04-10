using System;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal class PaintableSceneViewGrid : PaintableGrid
	{
		private SceneView activeSceneView = null;

		private Transform gridTransform
		{
			get
			{
				return (!(this.grid != null)) ? null : this.grid.transform;
			}
		}

		private Grid grid
		{
			get
			{
				return (!(this.brushTarget != null)) ? ((!(Selection.activeGameObject != null)) ? null : Selection.activeGameObject.GetComponentInParent<Grid>()) : this.brushTarget.GetComponentInParent<Grid>();
			}
		}

		private GridBrushBase gridBrush
		{
			get
			{
				return GridPaintingState.gridBrush;
			}
		}

		private GameObject brushTarget
		{
			get
			{
				return GridPaintingState.scenePaintTarget;
			}
		}

		public Tilemap tilemap
		{
			get
			{
				Tilemap result;
				if (this.brushTarget != null)
				{
					result = this.brushTarget.GetComponent<Tilemap>();
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			GridSelection.gridSelectionChanged += new Action(this.OnGridSelectionChanged);
		}

		protected override void OnDisable()
		{
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			GridSelection.gridSelectionChanged -= new Action(this.OnGridSelectionChanged);
			base.OnDisable();
		}

		private void OnGridSelectionChanged()
		{
			SceneView.RepaintAll();
		}

		public void OnSceneGUI(SceneView sceneView)
		{
			base.UpdateMouseGridPosition();
			base.OnGUI();
			if (PaintableGrid.InGridEditMode())
			{
				this.CallOnSceneGUI();
				if (this.grid != null && (GridPaintingState.activeGrid == this || GridSelection.active))
				{
					this.CallOnPaintSceneGUI();
				}
				if (Event.current.type == EventType.Repaint)
				{
					EditorGUIUtility.AddCursorRect(new Rect(0f, 17f, sceneView.position.width, sceneView.position.height - 17f), MouseCursor.CustomCursor);
				}
			}
			this.HandleMouseEnterLeave(sceneView);
		}

		private void HandleMouseEnterLeave(SceneView sceneView)
		{
			if (base.inEditMode)
			{
				if (Event.current.type == EventType.MouseEnterWindow)
				{
					this.OnMouseEnter(sceneView);
				}
				else if (Event.current.type == EventType.MouseLeaveWindow)
				{
					this.OnMouseLeave(sceneView);
				}
				else if (sceneView.docked && (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.LinuxEditor))
				{
					Vector2 point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
					if (sceneView.position.Contains(point))
					{
						if (GridPaintingState.activeGrid != this)
						{
							this.OnMouseEnter(sceneView);
						}
					}
					else if (this.activeSceneView == sceneView)
					{
						if (GridPaintingState.activeGrid == this)
						{
							this.OnMouseLeave(sceneView);
						}
					}
				}
			}
		}

		private void OnMouseEnter(SceneView sceneView)
		{
			if (GridPaintingState.activeBrushEditor != null)
			{
				GridPaintingState.activeBrushEditor.OnMouseEnter();
			}
			GridPaintingState.activeGrid = this;
			this.activeSceneView = sceneView;
		}

		private void OnMouseLeave(SceneView sceneView)
		{
			if (GridPaintingState.activeBrushEditor != null)
			{
				GridPaintingState.activeBrushEditor.OnMouseLeave();
			}
			GridPaintingState.activeGrid = null;
			this.activeSceneView = null;
		}

		private void UndoRedoPerformed()
		{
			this.RefreshAllTiles();
		}

		private void RefreshAllTiles()
		{
			if (this.tilemap != null)
			{
				this.tilemap.RefreshAllTiles();
			}
		}

		protected override void RegisterUndo()
		{
			if (GridPaintingState.activeBrushEditor != null)
			{
				GridPaintingState.activeBrushEditor.RegisterUndo(this.brushTarget, PaintableGrid.EditModeToBrushTool(EditMode.editMode));
			}
		}

		protected override void Paint(Vector3Int position)
		{
			if (this.grid != null)
			{
				this.gridBrush.Paint(this.grid, this.brushTarget, position);
			}
		}

		protected override void Erase(Vector3Int position)
		{
			if (this.grid != null)
			{
				this.gridBrush.Erase(this.grid, this.brushTarget, position);
			}
		}

		protected override void BoxFill(BoundsInt position)
		{
			if (this.grid != null)
			{
				this.gridBrush.BoxFill(this.grid, this.brushTarget, position);
			}
		}

		protected override void BoxErase(BoundsInt position)
		{
			if (this.grid != null)
			{
				this.gridBrush.BoxErase(this.grid, this.brushTarget, position);
			}
		}

		protected override void FloodFill(Vector3Int position)
		{
			if (this.grid != null)
			{
				this.gridBrush.FloodFill(this.grid, this.brushTarget, position);
			}
		}

		protected override void PickBrush(BoundsInt position, Vector3Int pickStart)
		{
			if (this.grid != null)
			{
				this.gridBrush.Pick(this.grid, this.brushTarget, position, pickStart);
			}
		}

		protected override void Select(BoundsInt position)
		{
			if (this.grid != null)
			{
				GridSelection.Select(this.brushTarget, position);
				this.gridBrush.Select(this.grid, this.brushTarget, position);
			}
		}

		protected override void Move(BoundsInt from, BoundsInt to)
		{
			if (this.grid != null)
			{
				this.gridBrush.Move(this.grid, this.brushTarget, from, to);
			}
		}

		protected override void MoveStart(BoundsInt position)
		{
			if (this.grid != null)
			{
				this.gridBrush.MoveStart(this.grid, this.brushTarget, position);
			}
		}

		protected override void MoveEnd(BoundsInt position)
		{
			if (this.grid != null)
			{
				this.gridBrush.MoveEnd(this.grid, this.brushTarget, position);
			}
		}

		protected override void ClearGridSelection()
		{
			GridSelection.Clear();
		}

		public override void Repaint()
		{
			SceneView.RepaintAll();
		}

		protected override bool ValidateFloodFillPosition(Vector3Int position)
		{
			return true;
		}

		protected override Vector2Int ScreenToGrid(Vector2 screenPosition)
		{
			Vector2Int result;
			if (this.tilemap != null)
			{
				Transform transform = this.tilemap.transform;
				Vector3 inNormal = this.tilemap.orientationMatrix.MultiplyVector(transform.forward) * -1f;
				Plane plane = new Plane(inNormal, transform.position);
				Vector3Int vector3Int = this.LocalToGrid(this.tilemap, GridEditorUtility.ScreenToLocal(transform, screenPosition, plane));
				result = new Vector2Int(vector3Int.x, vector3Int.y);
			}
			else if (this.grid)
			{
				Vector3Int vector3Int2 = this.LocalToGrid(this.grid, GridEditorUtility.ScreenToLocal(this.gridTransform, screenPosition, this.GetGridPlane(this.grid)));
				result = new Vector2Int(vector3Int2.x, vector3Int2.y);
			}
			else
			{
				result = Vector2Int.zero;
			}
			return result;
		}

		protected override bool PickingIsDefaultTool()
		{
			return false;
		}

		protected override bool CanPickOutsideEditMode()
		{
			return false;
		}

		protected override GridLayout.CellLayout CellLayout()
		{
			return this.grid.cellLayout;
		}

		private Vector3Int LocalToGrid(GridLayout gridLayout, Vector3 local)
		{
			return gridLayout.LocalToCell(local);
		}

		private Plane GetGridPlane(Grid grid)
		{
			Plane result;
			switch (grid.cellSwizzle)
			{
			case GridLayout.CellSwizzle.XYZ:
				result = new Plane(grid.transform.forward * -1f, grid.transform.position);
				break;
			case GridLayout.CellSwizzle.XZY:
				result = new Plane(grid.transform.up * -1f, grid.transform.position);
				break;
			case GridLayout.CellSwizzle.YXZ:
				result = new Plane(grid.transform.forward, grid.transform.position);
				break;
			case GridLayout.CellSwizzle.YZX:
				result = new Plane(grid.transform.up, grid.transform.position);
				break;
			case GridLayout.CellSwizzle.ZXY:
				result = new Plane(grid.transform.right, grid.transform.position);
				break;
			case GridLayout.CellSwizzle.ZYX:
				result = new Plane(grid.transform.right * -1f, grid.transform.position);
				break;
			default:
				result = new Plane(grid.transform.forward * -1f, grid.transform.position);
				break;
			}
			return result;
		}

		private void CallOnPaintSceneGUI()
		{
			bool flag = GridSelection.active && GridSelection.target == this.brushTarget;
			if (flag || !(GridPaintingState.activeGrid != this))
			{
				RectInt marqueeRect = new RectInt(base.mouseGridPosition, new Vector2Int(1, 1));
				if (this.m_MarqueeStart.HasValue)
				{
					marqueeRect = GridEditorUtility.GetMarqueeRect(base.mouseGridPosition, this.m_MarqueeStart.Value);
				}
				else if (flag)
				{
					marqueeRect = new RectInt(GridSelection.position.xMin, GridSelection.position.yMin, GridSelection.position.size.x, GridSelection.position.size.y);
				}
				GridLayout gridLayout = (!(this.tilemap != null)) ? this.grid : this.tilemap;
				if (GridPaintingState.activeBrushEditor != null)
				{
					GridPaintingState.activeBrushEditor.OnPaintSceneGUI(gridLayout, this.brushTarget, new BoundsInt(new Vector3Int(marqueeRect.x, marqueeRect.y, 0), new Vector3Int(marqueeRect.width, marqueeRect.height, 1)), PaintableGrid.EditModeToBrushTool(EditMode.editMode), this.m_MarqueeStart.HasValue || base.executing);
				}
				else
				{
					GridBrushEditorBase.OnPaintSceneGUIInternal(gridLayout, this.brushTarget, new BoundsInt(new Vector3Int(marqueeRect.x, marqueeRect.y, 0), new Vector3Int(marqueeRect.width, marqueeRect.height, 1)), PaintableGrid.EditModeToBrushTool(EditMode.editMode), this.m_MarqueeStart.HasValue || base.executing);
				}
			}
		}

		private void CallOnSceneGUI()
		{
			if (GridPaintingState.activeBrushEditor != null)
			{
				MethodInfo method = GridPaintingState.activeBrushEditor.GetType().GetMethod("OnSceneGUI");
				if (method != null)
				{
					method.Invoke(GridPaintingState.activeBrushEditor, null);
				}
			}
		}
	}
}
