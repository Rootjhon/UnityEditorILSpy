using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal abstract class PaintableGrid : ScriptableObject
	{
		public enum MarqueeType
		{
			None,
			Pick,
			Box,
			Select
		}

		private int m_PermanentControlID;

		internal static PaintableGrid s_LastActivePaintableGrid;

		private Vector2Int m_PreviousMouseGridPosition;

		private Vector2Int m_MouseGridPosition;

		private bool m_MouseGridPositionChanged;

		private bool m_PositionChangeRepaintDone;

		protected Vector2Int? m_PreviousMove = null;

		protected Vector2Int? m_MarqueeStart = null;

		private PaintableGrid.MarqueeType m_MarqueeType = PaintableGrid.MarqueeType.None;

		private bool m_IsExecuting;

		private EditMode.SceneViewEditMode m_ModeBeforePicking;

		public Vector2Int mouseGridPosition
		{
			get
			{
				return this.m_MouseGridPosition;
			}
		}

		public bool isPicking
		{
			get
			{
				return this.m_MarqueeType == PaintableGrid.MarqueeType.Pick;
			}
		}

		public bool isBoxing
		{
			get
			{
				return this.m_MarqueeType == PaintableGrid.MarqueeType.Box;
			}
		}

		public GridLayout.CellLayout cellLayout
		{
			get
			{
				return this.CellLayout();
			}
		}

		protected bool executing
		{
			get
			{
				return this.m_IsExecuting;
			}
			set
			{
				this.m_IsExecuting = (value && this.isHotControl);
			}
		}

		protected bool isHotControl
		{
			get
			{
				return GUIUtility.hotControl == this.m_PermanentControlID;
			}
		}

		protected bool mouseGridPositionChanged
		{
			get
			{
				return this.m_MouseGridPositionChanged;
			}
		}

		protected bool inEditMode
		{
			get
			{
				return PaintableGrid.InGridEditMode();
			}
		}

		public abstract void Repaint();

		protected abstract void RegisterUndo();

		protected abstract void Paint(Vector3Int position);

		protected abstract void Erase(Vector3Int position);

		protected abstract void BoxFill(BoundsInt position);

		protected abstract void BoxErase(BoundsInt position);

		protected abstract void FloodFill(Vector3Int position);

		protected abstract void PickBrush(BoundsInt position, Vector3Int pickStart);

		protected abstract void Select(BoundsInt position);

		protected abstract void Move(BoundsInt from, BoundsInt to);

		protected abstract void MoveStart(BoundsInt position);

		protected abstract void MoveEnd(BoundsInt position);

		protected abstract bool ValidateFloodFillPosition(Vector3Int position);

		protected abstract Vector2Int ScreenToGrid(Vector2 screenPosition);

		protected abstract bool PickingIsDefaultTool();

		protected abstract bool CanPickOutsideEditMode();

		protected abstract GridLayout.CellLayout CellLayout();

		protected abstract void ClearGridSelection();

		protected virtual void OnBrushPickStarted()
		{
		}

		protected virtual void OnBrushPickDragged(BoundsInt position)
		{
		}

		protected virtual void OnEnable()
		{
			this.m_PermanentControlID = GUIUtility.GetPermanentControlID();
		}

		protected virtual void OnDisable()
		{
		}

		public virtual void OnGUI()
		{
			if (this.CanPickOutsideEditMode() || this.inEditMode)
			{
				this.HandleBrushPicking();
			}
			if (this.inEditMode)
			{
				this.HandleBrushPaintAndErase();
				this.HandleSelectTool();
				this.HandleMoveTool();
				this.HandleEditModeChange();
				this.HandleFloodFill();
				this.HandleBoxTool();
			}
			else if (this.isHotControl && !this.IsPickingEvent(Event.current))
			{
				GUIUtility.hotControl = 0;
			}
			if (this.mouseGridPositionChanged && !this.m_PositionChangeRepaintDone)
			{
				this.Repaint();
				this.m_PositionChangeRepaintDone = true;
			}
		}

		protected void UpdateMouseGridPosition()
		{
			if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseMove || Event.current.type == EventType.DragUpdated)
			{
				this.m_MouseGridPositionChanged = false;
				Vector2Int vector2Int = this.ScreenToGrid(Event.current.mousePosition);
				if (vector2Int != this.m_MouseGridPosition)
				{
					this.m_PreviousMouseGridPosition = this.m_MouseGridPosition;
					this.m_MouseGridPosition = vector2Int;
					this.m_MouseGridPositionChanged = true;
					this.m_PositionChangeRepaintDone = false;
				}
			}
		}

		private void HandleEditModeChange()
		{
			if (this.isPicking && EditMode.editMode != EditMode.SceneViewEditMode.GridPicking)
			{
				this.m_MarqueeStart = null;
				this.m_MarqueeType = PaintableGrid.MarqueeType.None;
				if (this.isHotControl)
				{
					GUI.changed = true;
					GUIUtility.hotControl = 0;
				}
			}
			if (this.isBoxing && EditMode.editMode != EditMode.SceneViewEditMode.GridBox)
			{
				this.m_MarqueeStart = null;
				this.m_MarqueeType = PaintableGrid.MarqueeType.None;
				if (this.isHotControl)
				{
					GUI.changed = true;
					GUIUtility.hotControl = 0;
				}
			}
			if (EditMode.editMode != EditMode.SceneViewEditMode.GridSelect && EditMode.editMode != EditMode.SceneViewEditMode.GridMove)
			{
				this.ClearGridSelection();
			}
		}

		private void HandleBrushPicking()
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && this.IsPickingEvent(current) && !this.isHotControl)
			{
				this.m_ModeBeforePicking = EditMode.SceneViewEditMode.GridPainting;
				if (this.inEditMode && EditMode.editMode != EditMode.SceneViewEditMode.GridPicking)
				{
					this.m_ModeBeforePicking = EditMode.editMode;
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPicking, ScriptableSingleton<GridPaintingState>.instance);
				}
				this.m_MarqueeStart = new Vector2Int?(this.mouseGridPosition);
				this.m_MarqueeType = PaintableGrid.MarqueeType.Pick;
				PaintableGrid.s_LastActivePaintableGrid = this;
				Event.current.Use();
				GUI.changed = true;
				GUIUtility.hotControl = this.m_PermanentControlID;
				this.OnBrushPickStarted();
			}
			if (current.type == EventType.MouseDrag && this.isHotControl && this.m_MarqueeStart.HasValue && this.m_MarqueeType == PaintableGrid.MarqueeType.Pick && this.IsPickingEvent(current))
			{
				RectInt marqueeRect = GridEditorUtility.GetMarqueeRect(this.m_MarqueeStart.Value, this.mouseGridPosition);
				this.OnBrushPickDragged(new BoundsInt(new Vector3Int(marqueeRect.xMin, marqueeRect.yMin, 0), new Vector3Int(marqueeRect.size.x, marqueeRect.size.y, 1)));
				Event.current.Use();
				GUI.changed = true;
			}
			if (current.type == EventType.MouseUp && this.m_MarqueeStart.HasValue && this.m_MarqueeType == PaintableGrid.MarqueeType.Pick && this.IsPickingEvent(current))
			{
				RectInt marqueeRect2 = GridEditorUtility.GetMarqueeRect(this.m_MarqueeStart.Value, this.mouseGridPosition);
				if (this.isHotControl)
				{
					Vector2Int marqueePivot = this.GetMarqueePivot(this.m_MarqueeStart.Value, this.mouseGridPosition);
					this.PickBrush(new BoundsInt(new Vector3Int(marqueeRect2.xMin, marqueeRect2.yMin, 0), new Vector3Int(marqueeRect2.size.x, marqueeRect2.size.y, 1)), new Vector3Int(marqueePivot.x, marqueePivot.y, 0));
					if (this.inEditMode && EditMode.editMode != this.m_ModeBeforePicking)
					{
						EditMode.ChangeEditMode(this.m_ModeBeforePicking, ScriptableSingleton<GridPaintingState>.instance);
					}
					GridPaletteBrushes.ActiveGridBrushAssetChanged();
					PaintableGrid.s_LastActivePaintableGrid = this;
					InspectorWindow.RepaintAllInspectors();
					Event.current.Use();
					GUI.changed = true;
					GUIUtility.hotControl = 0;
				}
				this.m_MarqueeType = PaintableGrid.MarqueeType.None;
				this.m_MarqueeStart = null;
			}
		}

		private bool IsPickingEvent(Event evt)
		{
			return ((evt.control && EditMode.editMode != EditMode.SceneViewEditMode.GridMove) || EditMode.editMode == EditMode.SceneViewEditMode.GridPicking || (EditMode.editMode != EditMode.SceneViewEditMode.GridSelect && this.PickingIsDefaultTool())) && evt.button == 0 && !evt.alt;
		}

		private void HandleSelectTool()
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && current.button == 0 && !current.alt && (EditMode.editMode == EditMode.SceneViewEditMode.GridSelect || (EditMode.editMode == EditMode.SceneViewEditMode.GridMove && current.control)))
			{
				if (EditMode.editMode == EditMode.SceneViewEditMode.GridMove && current.control)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridSelect, ScriptableSingleton<GridPaintingState>.instance);
				}
				this.m_PreviousMove = null;
				this.m_MarqueeStart = new Vector2Int?(this.mouseGridPosition);
				this.m_MarqueeType = PaintableGrid.MarqueeType.Select;
				PaintableGrid.s_LastActivePaintableGrid = this;
				GUIUtility.hotControl = this.m_PermanentControlID;
				Event.current.Use();
			}
			if (current.type == EventType.MouseUp && current.button == 0 && !current.alt && this.m_MarqueeStart.HasValue && GUIUtility.hotControl == this.m_PermanentControlID && EditMode.editMode == EditMode.SceneViewEditMode.GridSelect)
			{
				if (this.m_MarqueeStart.HasValue && this.m_MarqueeType == PaintableGrid.MarqueeType.Select)
				{
					RectInt marqueeRect = GridEditorUtility.GetMarqueeRect(this.m_MarqueeStart.Value, this.mouseGridPosition);
					this.Select(new BoundsInt(new Vector3Int(marqueeRect.xMin, marqueeRect.yMin, 0), new Vector3Int(marqueeRect.size.x, marqueeRect.size.y, 1)));
					this.m_MarqueeStart = null;
					this.m_MarqueeType = PaintableGrid.MarqueeType.None;
					InspectorWindow.RepaintAllInspectors();
				}
				if (current.control)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridMove, ScriptableSingleton<GridPaintingState>.instance);
				}
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape && !this.m_MarqueeStart.HasValue && !this.m_PreviousMove.HasValue)
			{
				this.ClearGridSelection();
				Event.current.Use();
			}
		}

		private void HandleMoveTool()
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && current.button == 0 && EditMode.editMode == EditMode.SceneViewEditMode.GridMove)
			{
				this.RegisterUndo();
				Vector3Int position = new Vector3Int(this.mouseGridPosition.x, this.mouseGridPosition.y, GridSelection.position.zMin);
				if (GridSelection.active && GridSelection.position.Contains(position))
				{
					GUIUtility.hotControl = this.m_PermanentControlID;
					this.executing = true;
					this.m_MarqueeStart = null;
					this.m_MarqueeType = PaintableGrid.MarqueeType.None;
					this.m_PreviousMove = new Vector2Int?(this.mouseGridPosition);
					this.MoveStart(GridSelection.position);
					PaintableGrid.s_LastActivePaintableGrid = this;
				}
				Event.current.Use();
			}
			if (current.type == EventType.MouseDrag && current.button == 0 && EditMode.editMode == EditMode.SceneViewEditMode.GridMove && GUIUtility.hotControl == this.m_PermanentControlID)
			{
				if (this.m_MouseGridPositionChanged && this.m_PreviousMove.HasValue)
				{
					this.executing = true;
					BoundsInt position2 = GridSelection.position;
					BoundsInt from = new BoundsInt(new Vector3Int(position2.xMin, position2.yMin, 0), new Vector3Int(position2.size.x, position2.size.y, 1));
					Vector2Int vector2Int = this.mouseGridPosition - this.m_PreviousMove.Value;
					BoundsInt position3 = GridSelection.position;
					position3.position = new Vector3Int(position3.x + vector2Int.x, position3.y + vector2Int.y, position3.z);
					GridSelection.position = position3;
					this.Move(from, position3);
					this.m_PreviousMove = new Vector2Int?(this.mouseGridPosition);
					Event.current.Use();
				}
			}
			if (current.type == EventType.MouseUp && current.button == 0 && this.m_PreviousMove.HasValue && EditMode.editMode == EditMode.SceneViewEditMode.GridMove && GUIUtility.hotControl == this.m_PermanentControlID)
			{
				if (this.m_PreviousMove.HasValue)
				{
					this.m_PreviousMove = null;
					this.MoveEnd(GridSelection.position);
				}
				this.executing = false;
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
		}

		private void HandleBrushPaintAndErase()
		{
			Event current = Event.current;
			if (this.IsPaintingEvent(current) || this.IsErasingEvent(current))
			{
				EventType type = current.type;
				if (type != EventType.MouseDown)
				{
					if (type != EventType.MouseDrag)
					{
						if (type == EventType.MouseUp)
						{
							this.executing = false;
							if (this.isHotControl)
							{
								if (Event.current.shift && EditMode.editMode != EditMode.SceneViewEditMode.GridPainting)
								{
									EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPainting, ScriptableSingleton<GridPaintingState>.instance);
								}
								Event.current.Use();
								GUI.changed = true;
								GUIUtility.hotControl = 0;
							}
						}
					}
					else
					{
						if (this.isHotControl && this.mouseGridPositionChanged)
						{
							List<Vector2Int> list = GridEditorUtility.GetPointsOnLine(this.m_PreviousMouseGridPosition, this.mouseGridPosition).ToList<Vector2Int>();
							if (list[0] == this.mouseGridPosition)
							{
								list.Reverse();
							}
							for (int i = 1; i < list.Count; i++)
							{
								if (this.IsErasingEvent(current))
								{
									this.Erase(new Vector3Int(list[i].x, list[i].y, 0));
								}
								else
								{
									this.Paint(new Vector3Int(list[i].x, list[i].y, 0));
								}
							}
							Event.current.Use();
							GUI.changed = true;
						}
						this.executing = true;
					}
				}
				else
				{
					this.RegisterUndo();
					if (this.IsErasingEvent(current))
					{
						if (EditMode.editMode != EditMode.SceneViewEditMode.GridEraser)
						{
							EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridEraser, ScriptableSingleton<GridPaintingState>.instance);
						}
						this.Erase(new Vector3Int(this.mouseGridPosition.x, this.mouseGridPosition.y, 0));
					}
					else
					{
						if (EditMode.editMode != EditMode.SceneViewEditMode.GridPainting)
						{
							EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPainting, ScriptableSingleton<GridPaintingState>.instance);
						}
						this.Paint(new Vector3Int(this.mouseGridPosition.x, this.mouseGridPosition.y, 0));
					}
					Event.current.Use();
					GUIUtility.hotControl = this.m_PermanentControlID;
					GUI.changed = true;
					this.executing = true;
				}
			}
		}

		private bool IsPaintingEvent(Event evt)
		{
			return evt.button == 0 && !evt.control && !evt.alt && EditMode.editMode == EditMode.SceneViewEditMode.GridPainting;
		}

		private bool IsErasingEvent(Event evt)
		{
			return evt.button == 0 && ((!evt.control && !evt.alt && evt.shift && EditMode.editMode != EditMode.SceneViewEditMode.GridBox && EditMode.editMode != EditMode.SceneViewEditMode.GridFloodFill && EditMode.editMode != EditMode.SceneViewEditMode.GridSelect && EditMode.editMode != EditMode.SceneViewEditMode.GridMove) || EditMode.editMode == EditMode.SceneViewEditMode.GridEraser);
		}

		private void HandleFloodFill()
		{
			if (EditMode.editMode == EditMode.SceneViewEditMode.GridFloodFill && GridPaintingState.gridBrush != null && this.ValidateFloodFillPosition(new Vector3Int(this.mouseGridPosition.x, this.mouseGridPosition.y, 0)))
			{
				Event current = Event.current;
				if (current.type == EventType.MouseDown && current.button == 0)
				{
					GUIUtility.hotControl = this.m_PermanentControlID;
					GUI.changed = true;
					this.executing = true;
					Event.current.Use();
				}
				if (current.type == EventType.MouseUp && current.button == 0 && this.isHotControl)
				{
					this.executing = false;
					this.RegisterUndo();
					this.FloodFill(new Vector3Int(this.mouseGridPosition.x, this.mouseGridPosition.y, 0));
					GUI.changed = true;
					Event.current.Use();
					GUIUtility.hotControl = 0;
				}
			}
		}

		private void HandleBoxTool()
		{
			Event current = Event.current;
			if (current.type == EventType.MouseDown && current.button == 0 && EditMode.editMode == EditMode.SceneViewEditMode.GridBox)
			{
				this.m_MarqueeStart = new Vector2Int?(this.mouseGridPosition);
				this.m_MarqueeType = PaintableGrid.MarqueeType.Box;
				Event.current.Use();
				GUI.changed = true;
				this.executing = true;
				GUIUtility.hotControl = this.m_PermanentControlID;
			}
			if (current.type == EventType.MouseDrag && current.button == 0 && EditMode.editMode == EditMode.SceneViewEditMode.GridBox)
			{
				if (this.isHotControl && this.m_MarqueeStart.HasValue)
				{
					Event.current.Use();
					this.executing = true;
					GUI.changed = true;
				}
			}
			if (current.type == EventType.MouseUp && current.button == 0 && EditMode.editMode == EditMode.SceneViewEditMode.GridBox)
			{
				if (this.isHotControl && this.m_MarqueeStart.HasValue)
				{
					this.RegisterUndo();
					RectInt marqueeRect = GridEditorUtility.GetMarqueeRect(this.m_MarqueeStart.Value, this.mouseGridPosition);
					if (current.shift)
					{
						this.BoxErase(new BoundsInt(marqueeRect.x, marqueeRect.y, 0, marqueeRect.size.x, marqueeRect.size.y, 1));
					}
					else
					{
						this.BoxFill(new BoundsInt(marqueeRect.x, marqueeRect.y, 0, marqueeRect.size.x, marqueeRect.size.y, 1));
					}
					Event.current.Use();
					this.executing = false;
					GUI.changed = true;
					GUIUtility.hotControl = 0;
				}
				this.m_MarqueeStart = null;
				this.m_MarqueeType = PaintableGrid.MarqueeType.None;
			}
		}

		private Vector2Int GetMarqueePivot(Vector2Int start, Vector2Int end)
		{
			Vector2Int result = new Vector2Int(Math.Max(end.x - start.x, 0), Math.Max(end.y - start.y, 0));
			return result;
		}

		public static bool InGridEditMode()
		{
			return EditMode.editMode == EditMode.SceneViewEditMode.GridBox || EditMode.editMode == EditMode.SceneViewEditMode.GridEraser || EditMode.editMode == EditMode.SceneViewEditMode.GridFloodFill || EditMode.editMode == EditMode.SceneViewEditMode.GridPainting || EditMode.editMode == EditMode.SceneViewEditMode.GridPicking || EditMode.editMode == EditMode.SceneViewEditMode.GridSelect || EditMode.editMode == EditMode.SceneViewEditMode.GridMove;
		}

		public static GridBrushBase.Tool EditModeToBrushTool(EditMode.SceneViewEditMode editMode)
		{
			GridBrushBase.Tool result;
			switch (editMode)
			{
			case EditMode.SceneViewEditMode.GridPainting:
				result = GridBrushBase.Tool.Paint;
				break;
			case EditMode.SceneViewEditMode.GridPicking:
				result = GridBrushBase.Tool.Pick;
				break;
			case EditMode.SceneViewEditMode.GridEraser:
				result = GridBrushBase.Tool.Erase;
				break;
			case EditMode.SceneViewEditMode.GridFloodFill:
				result = GridBrushBase.Tool.FloodFill;
				break;
			case EditMode.SceneViewEditMode.GridBox:
				result = GridBrushBase.Tool.Box;
				break;
			case EditMode.SceneViewEditMode.GridSelect:
				result = GridBrushBase.Tool.Select;
				break;
			case EditMode.SceneViewEditMode.GridMove:
				result = GridBrushBase.Tool.Move;
				break;
			default:
				result = GridBrushBase.Tool.Paint;
				break;
			}
			return result;
		}

		public static EditMode.SceneViewEditMode BrushToolToEditMode(GridBrushBase.Tool tool)
		{
			EditMode.SceneViewEditMode result;
			switch (tool)
			{
			case GridBrushBase.Tool.Select:
				result = EditMode.SceneViewEditMode.GridSelect;
				break;
			case GridBrushBase.Tool.Move:
				result = EditMode.SceneViewEditMode.GridMove;
				break;
			case GridBrushBase.Tool.Paint:
				result = EditMode.SceneViewEditMode.GridPainting;
				break;
			case GridBrushBase.Tool.Box:
				result = EditMode.SceneViewEditMode.GridBox;
				break;
			case GridBrushBase.Tool.Pick:
				result = EditMode.SceneViewEditMode.GridPicking;
				break;
			case GridBrushBase.Tool.Erase:
				result = EditMode.SceneViewEditMode.GridEraser;
				break;
			case GridBrushBase.Tool.FloodFill:
				result = EditMode.SceneViewEditMode.GridFloodFill;
				break;
			default:
				result = EditMode.SceneViewEditMode.GridPainting;
				break;
			}
			return result;
		}
	}
}
