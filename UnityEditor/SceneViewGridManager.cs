using System;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class SceneViewGridManager : ScriptableSingleton<SceneViewGridManager>
	{
		internal static readonly PrefColor sceneViewGridComponentGizmo = new PrefColor("Scene/Grid Component", 1f, 1f, 1f, 0.1f);

		private static Mesh s_GridProxyMesh;

		private static Material s_GridProxyMaterial;

		private static Color s_LastGridProxyColor;

		[SerializeField]
		private GridLayout m_ActiveGridProxy;

		private bool m_RegisteredEventHandlers;

		private bool active
		{
			get
			{
				return this.m_ActiveGridProxy != null;
			}
		}

		private GridLayout activeGridProxy
		{
			get
			{
				return this.m_ActiveGridProxy;
			}
		}

		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			ScriptableSingleton<SceneViewGridManager>.instance.RegisterEventHandlers();
		}

		private void OnEnable()
		{
			this.RegisterEventHandlers();
		}

		private void RegisterEventHandlers()
		{
			if (!this.m_RegisteredEventHandlers)
			{
				SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGuiDelegate));
				Selection.selectionChanged = (Action)Delegate.Combine(Selection.selectionChanged, new Action(this.UpdateCache));
				EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(this.UpdateCache));
				EditMode.editModeStarted += new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
				EditMode.editModeEnded += new Action<IToolModeOwner>(this.OnEditModeEnd);
				GridPaintingState.brushChanged += new Action<GridBrushBase>(this.OnBrushChanged);
				GridPaintingState.scenePaintTargetChanged += new Action<GameObject>(this.OnScenePaintTargetChanged);
				Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.OnUndoRedoPerformed));
				GridSnapping.snapPosition = new Func<Vector3, Vector3>(this.OnSnapPosition);
				GridSnapping.activeFunc = new Func<bool>(this.GetActive);
				this.m_RegisteredEventHandlers = true;
			}
		}

		private void OnBrushChanged(GridBrushBase brush)
		{
			this.UpdateCache();
		}

		private void OnEditModeEnd(IToolModeOwner owner)
		{
			this.UpdateCache();
		}

		private void OnEditModeStart(IToolModeOwner owner, EditMode.SceneViewEditMode editMode)
		{
			this.UpdateCache();
		}

		private void OnScenePaintTargetChanged(GameObject scenePaintTarget)
		{
			this.UpdateCache();
		}

		private void OnUndoRedoPerformed()
		{
			SceneViewGridManager.FlushCachedGridProxy();
		}

		private void OnDisable()
		{
			SceneViewGridManager.FlushCachedGridProxy();
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGuiDelegate));
			Selection.selectionChanged = (Action)Delegate.Remove(Selection.selectionChanged, new Action(this.UpdateCache));
			EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(this.UpdateCache));
			EditMode.editModeStarted -= new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
			EditMode.editModeEnded -= new Action<IToolModeOwner>(this.OnEditModeEnd);
			GridPaintingState.brushChanged -= new Action<GridBrushBase>(this.OnBrushChanged);
			GridPaintingState.scenePaintTargetChanged -= new Action<GameObject>(this.OnScenePaintTargetChanged);
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.OnUndoRedoPerformed));
			GridSnapping.snapPosition = null;
			GridSnapping.activeFunc = null;
			this.m_RegisteredEventHandlers = false;
		}

		private void UpdateCache()
		{
			GridLayout gridLayout;
			if (PaintableGrid.InGridEditMode())
			{
				gridLayout = ((!(GridPaintingState.scenePaintTarget != null)) ? null : GridPaintingState.scenePaintTarget.GetComponentInParent<GridLayout>());
			}
			else
			{
				gridLayout = ((!(Selection.activeGameObject != null)) ? null : Selection.activeGameObject.GetComponentInParent<GridLayout>());
			}
			if (gridLayout != this.m_ActiveGridProxy)
			{
				this.m_ActiveGridProxy = gridLayout;
				SceneViewGridManager.FlushCachedGridProxy();
			}
			this.ShowGlobalGrid(!this.active);
		}

		private void OnSceneGuiDelegate(SceneView sceneView)
		{
			if (this.active && AnnotationUtility.showGrid)
			{
				SceneViewGridManager.DrawGrid(this.activeGridProxy);
			}
		}

		private static void DrawGrid(GridLayout gridLayout)
		{
			if (SceneViewGridManager.sceneViewGridComponentGizmo.Color != SceneViewGridManager.s_LastGridProxyColor)
			{
				SceneViewGridManager.FlushCachedGridProxy();
				SceneViewGridManager.s_LastGridProxyColor = SceneViewGridManager.sceneViewGridComponentGizmo.Color;
			}
			GridEditorUtility.DrawGridGizmo(gridLayout, gridLayout.transform, SceneViewGridManager.s_LastGridProxyColor, ref SceneViewGridManager.s_GridProxyMesh, ref SceneViewGridManager.s_GridProxyMaterial);
		}

		private void ShowGlobalGrid(bool value)
		{
			IEnumerator enumerator = SceneView.sceneViews.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					SceneView sceneView = (SceneView)enumerator.Current;
					sceneView.showGlobalGrid = value;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private bool GetActive()
		{
			return this.active;
		}

		private Vector3 OnSnapPosition(Vector3 position)
		{
			Vector3 result = position;
			if (this.active && !EditorGUI.actionKey)
			{
				Vector3 localPosition = this.activeGridProxy.WorldToLocal(position);
				Vector3 vector = this.activeGridProxy.LocalToCellInterpolated(localPosition);
				Vector3 cellPosition = new Vector3(Mathf.Round(2f * vector.x) / 2f, Mathf.Round(2f * vector.y) / 2f, Mathf.Round(2f * vector.z) / 2f);
				localPosition = this.activeGridProxy.CellToLocalInterpolated(cellPosition);
				result = this.activeGridProxy.LocalToWorld(localPosition);
			}
			return result;
		}

		internal static void FlushCachedGridProxy()
		{
			if (!(SceneViewGridManager.s_GridProxyMesh == null))
			{
				UnityEngine.Object.DestroyImmediate(SceneViewGridManager.s_GridProxyMesh);
				SceneViewGridManager.s_GridProxyMesh = null;
				SceneViewGridManager.s_GridProxyMaterial = null;
			}
		}
	}
}
