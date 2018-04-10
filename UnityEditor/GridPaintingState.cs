using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UnityEditor
{
	internal class GridPaintingState : ScriptableSingleton<GridPaintingState>, IToolModeOwner
	{
		[SerializeField]
		private GameObject m_ScenePaintTarget;

		[SerializeField]
		private GridBrushBase m_Brush;

		[SerializeField]
		private PaintableGrid m_ActiveGrid;

		[SerializeField]
		private HashSet<UnityEngine.Object> m_InterestedPainters = new HashSet<UnityEngine.Object>();

		private GameObject[] m_CachedPaintTargets = null;

		private bool m_FlushPaintTargetCache;

		private Editor m_CachedEditor;

		private bool m_SavingPalette;

		public static event Action<GameObject> scenePaintTargetChanged
		{
			add
			{
				Action<GameObject> action = GridPaintingState.scenePaintTargetChanged;
				Action<GameObject> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<GameObject>>(ref GridPaintingState.scenePaintTargetChanged, (Action<GameObject>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<GameObject> action = GridPaintingState.scenePaintTargetChanged;
				Action<GameObject> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<GameObject>>(ref GridPaintingState.scenePaintTargetChanged, (Action<GameObject>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public static event Action<GridBrushBase> brushChanged
		{
			add
			{
				Action<GridBrushBase> action = GridPaintingState.brushChanged;
				Action<GridBrushBase> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<GridBrushBase>>(ref GridPaintingState.brushChanged, (Action<GridBrushBase>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<GridBrushBase> action = GridPaintingState.brushChanged;
				Action<GridBrushBase> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<GridBrushBase>>(ref GridPaintingState.brushChanged, (Action<GridBrushBase>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public static GameObject scenePaintTarget
		{
			get
			{
				return ScriptableSingleton<GridPaintingState>.instance.m_ScenePaintTarget;
			}
			set
			{
				if (value != ScriptableSingleton<GridPaintingState>.instance.m_ScenePaintTarget)
				{
					ScriptableSingleton<GridPaintingState>.instance.m_ScenePaintTarget = value;
					if (GridPaintingState.scenePaintTargetChanged != null)
					{
						GridPaintingState.scenePaintTargetChanged(ScriptableSingleton<GridPaintingState>.instance.m_ScenePaintTarget);
					}
				}
			}
		}

		public static GridBrushBase gridBrush
		{
			get
			{
				if (ScriptableSingleton<GridPaintingState>.instance.m_Brush == null)
				{
					ScriptableSingleton<GridPaintingState>.instance.m_Brush = GridPaletteBrushes.brushes[0];
				}
				return ScriptableSingleton<GridPaintingState>.instance.m_Brush;
			}
			set
			{
				if (ScriptableSingleton<GridPaintingState>.instance.m_Brush != value)
				{
					ScriptableSingleton<GridPaintingState>.instance.m_Brush = value;
					ScriptableSingleton<GridPaintingState>.instance.m_FlushPaintTargetCache = true;
					if (GridPaintingState.scenePaintTarget != null && !GridPaintingState.ValidatePaintTarget(GridPaintingState.scenePaintTarget))
					{
						GridPaintingState.scenePaintTarget = null;
					}
					if (GridPaintingState.scenePaintTarget == null)
					{
						GridPaintingState.scenePaintTarget = ((!GridPaintingState.ValidatePaintTarget(Selection.activeGameObject)) ? null : Selection.activeGameObject);
					}
					if (GridPaintingState.scenePaintTarget == null)
					{
						GridPaintingState.AutoSelectPaintTarget();
					}
					if (GridPaintingState.brushChanged != null)
					{
						GridPaintingState.brushChanged(ScriptableSingleton<GridPaintingState>.instance.m_Brush);
					}
				}
			}
		}

		public static GridBrush defaultBrush
		{
			get
			{
				return GridPaintingState.gridBrush as GridBrush;
			}
			set
			{
				GridPaintingState.gridBrush = value;
			}
		}

		public static GridBrushEditorBase activeBrushEditor
		{
			get
			{
				Editor.CreateCachedEditor(GridPaintingState.gridBrush, null, ref ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor);
				return ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor as GridBrushEditorBase;
			}
		}

		public static Editor fallbackEditor
		{
			get
			{
				Editor.CreateCachedEditor(GridPaintingState.gridBrush, null, ref ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor);
				return ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor;
			}
		}

		public static PaintableGrid activeGrid
		{
			get
			{
				return ScriptableSingleton<GridPaintingState>.instance.m_ActiveGrid;
			}
			set
			{
				ScriptableSingleton<GridPaintingState>.instance.m_ActiveGrid = value;
			}
		}

		public static bool savingPalette
		{
			get
			{
				return ScriptableSingleton<GridPaintingState>.instance.m_SavingPalette;
			}
			set
			{
				ScriptableSingleton<GridPaintingState>.instance.m_SavingPalette = value;
			}
		}

		public static GameObject[] validTargets
		{
			get
			{
				if (ScriptableSingleton<GridPaintingState>.instance.m_FlushPaintTargetCache)
				{
					ScriptableSingleton<GridPaintingState>.instance.m_CachedPaintTargets = null;
					if (GridPaintingState.activeBrushEditor != null)
					{
						ScriptableSingleton<GridPaintingState>.instance.m_CachedPaintTargets = GridPaintingState.activeBrushEditor.validTargets;
					}
					ScriptableSingleton<GridPaintingState>.instance.m_FlushPaintTargetCache = false;
				}
				return ScriptableSingleton<GridPaintingState>.instance.m_CachedPaintTargets;
			}
		}

		public bool hasInterestedPainters
		{
			get
			{
				return this.m_InterestedPainters.Count > 0;
			}
		}

		public bool areToolModesAvailable
		{
			get
			{
				return true;
			}
		}

		private void OnEnable()
		{
			EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(this.HierarchyChanged));
			Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(this.OnSelectionChange));
			this.m_FlushPaintTargetCache = true;
		}

		private void OnDisable()
		{
			this.m_InterestedPainters.Clear();
			this.m_InterestedPainters = null;
			EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(this.HierarchyChanged));
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(this.OnSelectionChange));
			GridPaintingState.FlushCache();
		}

		private void OnSelectionChange()
		{
			if (this.hasInterestedPainters && GridPaintingState.validTargets == null && GridPaintingState.ValidatePaintTarget(Selection.activeGameObject))
			{
				GridPaintingState.scenePaintTarget = Selection.activeGameObject;
			}
		}

		private void HierarchyChanged()
		{
			if (this.hasInterestedPainters)
			{
				this.m_FlushPaintTargetCache = true;
				if (GridPaintingState.validTargets == null || !GridPaintingState.validTargets.Contains(GridPaintingState.scenePaintTarget))
				{
					GridPaintingState.AutoSelectPaintTarget();
				}
			}
		}

		public static void AutoSelectPaintTarget()
		{
			if (GridPaintingState.activeBrushEditor != null)
			{
				if (GridPaintingState.validTargets != null && GridPaintingState.validTargets.Length > 0)
				{
					GridPaintingState.scenePaintTarget = GridPaintingState.validTargets[0];
				}
			}
		}

		public static bool ValidatePaintTarget(GameObject candidate)
		{
			return !(candidate == null) && (!(candidate.GetComponentInParent<Grid>() == null) || !(candidate.GetComponent<Grid>() == null)) && (GridPaintingState.validTargets == null || GridPaintingState.validTargets.Contains(candidate));
		}

		public static void FlushCache()
		{
			if (ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor != null)
			{
				UnityEngine.Object.DestroyImmediate(ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor);
				ScriptableSingleton<GridPaintingState>.instance.m_CachedEditor = null;
			}
			ScriptableSingleton<GridPaintingState>.instance.m_FlushPaintTargetCache = true;
		}

		public static void RegisterPainterInterest(UnityEngine.Object painter)
		{
			ScriptableSingleton<GridPaintingState>.instance.m_InterestedPainters.Add(painter);
		}

		public static void UnregisterPainterInterest(UnityEngine.Object painter)
		{
			ScriptableSingleton<GridPaintingState>.instance.m_InterestedPainters.Remove(painter);
		}

		public Bounds GetWorldBoundsOfTargets()
		{
			return new Bounds(Vector3.zero, Vector3.positiveInfinity);
		}

		public bool ModeSurvivesSelectionChange(int toolMode)
		{
			return true;
		}

		int IToolModeOwner.GetInstanceID()
		{
			return base.GetInstanceID();
		}
	}
}
