using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Utils;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal class GridPaintPaletteWindow : EditorWindow
	{
		private static class Styles
		{
			public static readonly GUIContent[] toolContents;

			public static readonly EditMode.SceneViewEditMode[] sceneViewEditModes;

			public static readonly string[] mouseCursorOSPath;

			public static readonly Vector2[] mouseCursorOSHotspot;

			public static readonly string[] mouseCursorTexturePaths;

			public static readonly Texture2D[] mouseCursorTextures;

			public static readonly GUIContent emptyProjectInfo;

			public static readonly GUIContent emptyClipboardInfo;

			public static readonly GUIContent invalidClipboardInfo;

			public static readonly GUIContent selectPaintTarget;

			public static readonly GUIContent selectPalettePrefab;

			public static readonly GUIContent selectTileAsset;

			public static readonly GUIContent unlockPaletteEditing;

			public static readonly GUIContent lockPaletteEditing;

			public static readonly GUIContent createNewPalette;

			public static readonly GUIContent focusLabel;

			public static readonly GUIContent rendererOverlayTitleLabel;

			public static readonly GUIContent activeTargetLabel;

			public static readonly GUIContent edit;

			public static readonly GUIContent editModified;

			public static readonly GUIStyle ToolbarStyle;

			public static readonly GUIStyle ToolbarTitleStyle;

			public static float toolbarWidth;

			static Styles()
			{
				GridPaintPaletteWindow.Styles.toolContents = new GUIContent[]
				{
					EditorGUIUtility.IconContent("Grid.Default", "|Select an area of the grid (S)"),
					EditorGUIUtility.IconContent("Grid.MoveTool", "|Move selection with active brush (M)"),
					EditorGUIUtility.IconContent("Grid.PaintTool", "|Paint with active brush (B)"),
					EditorGUIUtility.IconContent("Grid.BoxTool", "|Paint a filled box with active brush (U)"),
					EditorGUIUtility.IconContent("Grid.PickingTool", "|Pick or marquee select new brush (Ctrl/CMD)."),
					EditorGUIUtility.IconContent("Grid.EraserTool", "|Erase with active brush (Shift)"),
					EditorGUIUtility.IconContent("Grid.FillTool", "|Flood fill with active brush (G)")
				};
				GridPaintPaletteWindow.Styles.sceneViewEditModes = new EditMode.SceneViewEditMode[]
				{
					EditMode.SceneViewEditMode.GridSelect,
					EditMode.SceneViewEditMode.GridMove,
					EditMode.SceneViewEditMode.GridPainting,
					EditMode.SceneViewEditMode.GridBox,
					EditMode.SceneViewEditMode.GridPicking,
					EditMode.SceneViewEditMode.GridEraser,
					EditMode.SceneViewEditMode.GridFloodFill
				};
				GridPaintPaletteWindow.Styles.mouseCursorOSPath = new string[]
				{
					"",
					"Cursors/macOS",
					"Cursors/Windows",
					"Cursors/Linux"
				};
				GridPaintPaletteWindow.Styles.mouseCursorOSHotspot = new Vector2[]
				{
					Vector2.zero,
					new Vector2(6f, 4f),
					new Vector2(6f, 4f),
					new Vector2(6f, 4f)
				};
				GridPaintPaletteWindow.Styles.mouseCursorTexturePaths = new string[]
				{
					"",
					"Grid.MoveTool.png",
					"Grid.PaintTool.png",
					"Grid.BoxTool.png",
					"Grid.PickingTool.png",
					"Grid.EraserTool.png",
					"Grid.FillTool.png"
				};
				GridPaintPaletteWindow.Styles.emptyProjectInfo = EditorGUIUtility.TextContent("Create a new palette in the dropdown above.");
				GridPaintPaletteWindow.Styles.emptyClipboardInfo = EditorGUIUtility.TextContent("Drag Tile, Sprite or Sprite Texture assets here.");
				GridPaintPaletteWindow.Styles.invalidClipboardInfo = EditorGUIUtility.TextContent("This is an invalid clipboard. Did you delete the clipboard asset?");
				GridPaintPaletteWindow.Styles.selectPaintTarget = EditorGUIUtility.TextContent("Select Paint Target");
				GridPaintPaletteWindow.Styles.selectPalettePrefab = EditorGUIUtility.TextContent("Select Palette Prefab");
				GridPaintPaletteWindow.Styles.selectTileAsset = EditorGUIUtility.TextContent("Select Tile Asset");
				GridPaintPaletteWindow.Styles.unlockPaletteEditing = EditorGUIUtility.TextContent("Unlock Palette Editing");
				GridPaintPaletteWindow.Styles.lockPaletteEditing = EditorGUIUtility.TextContent("Lock Palette Editing");
				GridPaintPaletteWindow.Styles.createNewPalette = EditorGUIUtility.TextContent("Create New Palette");
				GridPaintPaletteWindow.Styles.focusLabel = EditorGUIUtility.TextContent("Focus On");
				GridPaintPaletteWindow.Styles.rendererOverlayTitleLabel = EditorGUIUtility.TextContent("Tilemap");
				GridPaintPaletteWindow.Styles.activeTargetLabel = EditorGUIUtility.TextContent("Active Tilemap|Specifies the currently active Tilemap used for painting in the Scene View.");
				GridPaintPaletteWindow.Styles.edit = EditorGUIUtility.TextContent("Edit");
				GridPaintPaletteWindow.Styles.editModified = EditorGUIUtility.TextContent("Edit*");
				GridPaintPaletteWindow.Styles.ToolbarStyle = "preToolbar";
				GridPaintPaletteWindow.Styles.ToolbarTitleStyle = "preToolbar";
				GridPaintPaletteWindow.Styles.mouseCursorTextures = new Texture2D[GridPaintPaletteWindow.Styles.mouseCursorTexturePaths.Length];
				int operatingSystemFamily = (int)SystemInfo.operatingSystemFamily;
				for (int i = 0; i < GridPaintPaletteWindow.Styles.mouseCursorTexturePaths.Length; i++)
				{
					if (GridPaintPaletteWindow.Styles.mouseCursorOSPath[operatingSystemFamily] != null && GridPaintPaletteWindow.Styles.mouseCursorOSPath[operatingSystemFamily].Length > 0 && GridPaintPaletteWindow.Styles.mouseCursorTexturePaths[i] != null && GridPaintPaletteWindow.Styles.mouseCursorTexturePaths[i].Length > 0)
					{
						string path = Paths.Combine(new string[]
						{
							GridPaintPaletteWindow.Styles.mouseCursorOSPath[operatingSystemFamily],
							GridPaintPaletteWindow.Styles.mouseCursorTexturePaths[i]
						});
						GridPaintPaletteWindow.Styles.mouseCursorTextures[i] = (EditorGUIUtility.LoadRequired(path) as Texture2D);
					}
					else
					{
						GridPaintPaletteWindow.Styles.mouseCursorTextures[i] = null;
					}
				}
				GUIStyle toolbarStyle = "Command";
				GridPaintPaletteWindow.Styles.toolbarWidth = GridPaintPaletteWindow.Styles.toolContents.Sum((GUIContent x) => toolbarStyle.CalcSize(x).x);
			}
		}

		public class AssetProcessor : AssetPostprocessor
		{
			public override int GetPostprocessOrder()
			{
				return 2147483647;
			}

			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
			{
				if (!GridPaintingState.savingPalette)
				{
					foreach (GridPaintPaletteWindow current in GridPaintPaletteWindow.instances)
					{
						current.ResetPreviewInstance();
					}
				}
			}
		}

		public class PaletteAssetModificationProcessor : AssetModificationProcessor
		{
			private static string[] OnWillSaveAssets(string[] paths)
			{
				if (!GridPaintingState.savingPalette)
				{
					foreach (GridPaintPaletteWindow current in GridPaintPaletteWindow.instances)
					{
						if (current.clipboardView.isModified)
						{
							current.clipboardView.SavePaletteIfNecessary();
							current.Repaint();
						}
					}
				}
				return paths;
			}
		}

		private const float k_DropdownWidth = 200f;

		private const float k_ActiveTargetLabelWidth = 90f;

		private const float k_ActiveTargetDropdownWidth = 130f;

		private const float k_TopAreaHeight = 95f;

		private const float k_MinBrushInspectorHeight = 50f;

		private const float k_MinClipboardHeight = 200f;

		private const float k_ToolbarHeight = 17f;

		private const float k_ResizerDragRectPadding = 10f;

		public static readonly GUIContent tilePalette = EditorGUIUtility.TextContent("Tile Palette");

		private PaintableSceneViewGrid m_PaintableSceneViewGrid;

		private static PrefKey kGridSelectKey = new PrefKey("Grid Painting/Select", "s");

		private static PrefKey kGridMoveKey = new PrefKey("Grid Painting/Move", "m");

		private static PrefKey kGridBrushKey = new PrefKey("Grid Painting/Brush", "b");

		private static PrefKey kGridRectangleKey = new PrefKey("Grid Painting/Rectangle", "u");

		private static PrefKey kGridPickerKey = new PrefKey("Grid Painting/Picker", "i");

		private static PrefKey kGridEraseKey = new PrefKey("Grid Painting/Erase", "d");

		private static PrefKey kGridFillKey = new PrefKey("Grid Painting/Fill", "g");

		private static PrefKey kRotateClockwise = new PrefKey("Grid Painting/Rotate Clockwise", "[");

		private static PrefKey kRotateAntiClockwise = new PrefKey("Grid Painting/Rotate Anti-Clockwise", "]");

		private static PrefKey kFlipX = new PrefKey("Grid Painting/Flip X", "#[");

		private static PrefKey kFlipY = new PrefKey("Grid Painting/Flip Y", "#]");

		private static List<GridPaintPaletteWindow> s_Instances;

		[SerializeField]
		private PreviewResizer m_PreviewResizer;

		[SerializeField]
		private GameObject m_Palette;

		private GameObject m_PaletteInstance;

		private GridPalettesDropdown m_PaletteDropdown;

		[SerializeField]
		public GameObject m_Target;

		private Vector2 m_BrushScroll;

		private GridBrushEditorBase m_PreviousToolActivatedEditor;

		private GridBrushBase.Tool m_PreviousToolActivated;

		private PreviewRenderUtility m_PreviewUtility;

		public PaintableGrid paintableSceneViewGrid
		{
			get
			{
				return this.m_PaintableSceneViewGrid;
			}
		}

		public static List<GridPaintPaletteWindow> instances
		{
			get
			{
				if (GridPaintPaletteWindow.s_Instances == null)
				{
					GridPaintPaletteWindow.s_Instances = new List<GridPaintPaletteWindow>();
				}
				return GridPaintPaletteWindow.s_Instances;
			}
		}

		public GameObject palette
		{
			get
			{
				return this.m_Palette;
			}
			set
			{
				if (this.m_Palette != value)
				{
					this.clipboardView.OnBeforePaletteSelectionChanged();
					this.m_Palette = value;
					this.clipboardView.OnAfterPaletteSelectionChanged();
					TilemapEditorUserSettings.lastUsedPalette = this.m_Palette;
				}
			}
		}

		public GameObject paletteInstance
		{
			get
			{
				return this.m_PaletteInstance;
			}
		}

		public GridPaintPaletteClipboard clipboardView
		{
			get;
			private set;
		}

		public PreviewRenderUtility previewUtility
		{
			get
			{
				if (this.m_PreviewUtility == null)
				{
					this.InitPreviewUtility();
				}
				return this.m_PreviewUtility;
			}
		}

		private void OnSelectionChange()
		{
			GameObject activeGameObject = Selection.activeGameObject;
			if (activeGameObject != null)
			{
				bool flag = EditorUtility.IsPersistent(activeGameObject) || (activeGameObject.hideFlags & HideFlags.NotEditable) != HideFlags.None;
				if (flag)
				{
					string assetPath = AssetDatabase.GetAssetPath(activeGameObject);
					UnityEngine.Object[] array = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
					UnityEngine.Object[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						UnityEngine.Object @object = array2[i];
						if (@object.GetType() == typeof(GridPalette))
						{
							GameObject gameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(assetPath);
							if (gameObject != this.palette)
							{
								this.palette = gameObject;
								base.Repaint();
							}
							break;
						}
					}
				}
			}
		}

		private void OnGUI()
		{
			this.HandleContextMenu();
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			float pixels = ((float)Screen.width / EditorGUIUtility.pixelsPerPoint - GridPaintPaletteWindow.Styles.toolbarWidth) * 0.5f;
			GUILayout.Space(pixels);
			EditMode.DoInspectorToolbar(GridPaintPaletteWindow.Styles.sceneViewEditModes, GridPaintPaletteWindow.Styles.toolContents, ScriptableSingleton<GridPaintingState>.instance);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(pixels);
			this.DoActiveTargetsGUI();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(6f);
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			Rect rect = EditorGUILayout.BeginHorizontal(GUIContent.none, "Toolbar", new GUILayoutOption[0]);
			this.DoClipboardHeader();
			EditorGUILayout.EndHorizontal();
			Rect dragRect = new Rect(210f, 0f, base.position.width - 200f - 10f, 17f);
			float num = this.m_PreviewResizer.ResizeHandle(base.position, 50f, 200f, 17f, dragRect);
			float height = base.position.height - num - 95f;
			Rect position = new Rect(0f, rect.yMax, base.position.width, height);
			this.OnClipboardGUI(position);
			EditorGUILayout.EndVertical();
			GUILayout.Space(position.height);
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			EditorGUILayout.BeginHorizontal(GUIContent.none, "Toolbar", new GUILayoutOption[0]);
			this.DoBrushesDropdown();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			this.m_BrushScroll = GUILayout.BeginScrollView(this.m_BrushScroll, false, false, new GUILayoutOption[0]);
			GUILayout.Space(5f);
			this.OnBrushInspectorGUI();
			GUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
			Color color = Handles.color;
			Handles.color = Color.black;
			Handles.DrawLine(new Vector3(0f, position.yMax + 0.5f, 0f), new Vector3((float)Screen.width, position.yMax + 0.5f, 0f));
			Handles.color = Color.black.AlphaMultiplied(0.33f);
			Handles.DrawLine(new Vector3(0f, GUILayoutUtility.GetLastRect().yMax + 0.5f, 0f), new Vector3((float)Screen.width, GUILayoutUtility.GetLastRect().yMax + 0.5f, 0f));
			Handles.color = color;
			EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(2f);
			EditorGUILayout.EndVertical();
			if (AssetPreview.IsLoadingAssetPreviews(base.GetInstanceID()))
			{
				base.Repaint();
			}
			if (Event.current.type == EventType.MouseDown)
			{
				GUIUtility.keyboardControl = 0;
			}
		}

		public void ResetPreviewInstance()
		{
			if (this.m_PreviewUtility == null)
			{
				this.InitPreviewUtility();
			}
			this.DestroyPreviewInstance();
			if (this.palette != null)
			{
				this.m_PaletteInstance = this.previewUtility.InstantiatePrefabInScene(this.palette);
				PrefabUtility.DisconnectPrefabInstance(this.m_PaletteInstance);
				EditorUtility.InitInstantiatedPreviewRecursive(this.m_PaletteInstance);
				this.m_PaletteInstance.transform.position = new Vector3(0f, 0f, 0f);
				this.m_PaletteInstance.transform.rotation = Quaternion.identity;
				this.m_PaletteInstance.transform.localScale = Vector3.one;
				string assetPath = AssetDatabase.GetAssetPath(this.palette);
				GridPalette gridPalette = AssetDatabase.LoadAssetAtPath<GridPalette>(assetPath);
				if (gridPalette != null)
				{
					if (gridPalette.cellSizing == GridPalette.CellSizing.Automatic)
					{
						Grid component = this.m_PaletteInstance.GetComponent<Grid>();
						if (component != null)
						{
							component.cellSize = GridPaletteUtility.CalculateAutoCellSize(component, component.cellSize);
						}
						else
						{
							Debug.LogWarning("Grid component not found from: " + assetPath);
						}
					}
				}
				else
				{
					Debug.LogWarning("GridPalette subasset not found from: " + assetPath);
				}
				Renderer[] componentsInChildren = this.m_PaletteInstance.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Renderer renderer = componentsInChildren[i];
					renderer.gameObject.layer = Camera.PreviewCullingLayer;
					renderer.allowOcclusionWhenDynamic = false;
				}
				Transform[] componentsInChildren2 = this.m_PaletteInstance.GetComponentsInChildren<Transform>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Transform transform = componentsInChildren2[j];
					transform.gameObject.hideFlags = HideFlags.HideAndDontSave;
				}
				PreviewRenderUtility.SetEnabledRecursive(this.m_PaletteInstance, false);
				this.clipboardView.ResetPreviewMesh();
			}
		}

		public void DestroyPreviewInstance()
		{
			if (this.m_PaletteInstance != null)
			{
				UnityEngine.Object.DestroyImmediate(this.m_PaletteInstance);
			}
		}

		public void InitPreviewUtility()
		{
			this.m_PreviewUtility = new PreviewRenderUtility(true, true);
			this.m_PreviewUtility.camera.cullingMask = 1 << Camera.PreviewCullingLayer;
			this.m_PreviewUtility.camera.gameObject.layer = Camera.PreviewCullingLayer;
			this.m_PreviewUtility.lights[0].gameObject.layer = Camera.PreviewCullingLayer;
			this.m_PreviewUtility.camera.orthographic = true;
			this.m_PreviewUtility.camera.orthographicSize = 5f;
			this.m_PreviewUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
			this.m_PreviewUtility.ambientColor = new Color(1f, 1f, 1f, 0f);
			this.ResetPreviewInstance();
			this.clipboardView.SetupPreviewCameraOnInit();
		}

		private void HandleContextMenu()
		{
			if (Event.current.type == EventType.ContextClick)
			{
				this.DoContextMenu();
				Event.current.Use();
			}
		}

		public void SavePalette()
		{
			if (this.paletteInstance != null && this.palette != null)
			{
				GridPaintingState.savingPalette = true;
				this.SetHideFlagsRecursivelyIgnoringTilemapChildren(this.paletteInstance, HideFlags.HideInHierarchy);
				PrefabUtility.ReplacePrefab(this.paletteInstance, this.palette, ReplacePrefabOptions.ReplaceNameBased);
				this.SetHideFlagsRecursivelyIgnoringTilemapChildren(this.paletteInstance, HideFlags.HideAndDontSave);
				GridPaintingState.savingPalette = false;
			}
		}

		private void SetHideFlagsRecursivelyIgnoringTilemapChildren(GameObject root, HideFlags flags)
		{
			root.hideFlags = flags;
			if (root.GetComponent<Tilemap>() == null)
			{
				for (int i = 0; i < root.transform.childCount; i++)
				{
					this.SetHideFlagsRecursivelyIgnoringTilemapChildren(root.transform.GetChild(i).gameObject, flags);
				}
			}
		}

		private void DoContextMenu()
		{
			GenericMenu genericMenu = new GenericMenu();
			if (GridPaintingState.scenePaintTarget != null)
			{
				genericMenu.AddItem(GridPaintPaletteWindow.Styles.selectPaintTarget, false, new GenericMenu.MenuFunction(this.SelectPaintTarget));
			}
			else
			{
				genericMenu.AddDisabledItem(GridPaintPaletteWindow.Styles.selectPaintTarget);
			}
			if (this.palette != null)
			{
				genericMenu.AddItem(GridPaintPaletteWindow.Styles.selectPalettePrefab, false, new GenericMenu.MenuFunction(this.SelectPaletteAsset));
			}
			else
			{
				genericMenu.AddDisabledItem(GridPaintPaletteWindow.Styles.selectPalettePrefab);
			}
			if (this.clipboardView.activeTile != null)
			{
				genericMenu.AddItem(GridPaintPaletteWindow.Styles.selectTileAsset, false, new GenericMenu.MenuFunction(this.SelectTileAsset));
			}
			else
			{
				genericMenu.AddDisabledItem(GridPaintPaletteWindow.Styles.selectTileAsset);
			}
			genericMenu.AddSeparator("");
			if (this.clipboardView.unlocked)
			{
				genericMenu.AddItem(GridPaintPaletteWindow.Styles.lockPaletteEditing, false, new GenericMenu.MenuFunction(this.FlipLocked));
			}
			else
			{
				genericMenu.AddItem(GridPaintPaletteWindow.Styles.unlockPaletteEditing, false, new GenericMenu.MenuFunction(this.FlipLocked));
			}
			genericMenu.ShowAsContext();
		}

		private void FlipLocked()
		{
			this.clipboardView.unlocked = !this.clipboardView.unlocked;
		}

		private void SelectPaintTarget()
		{
			Selection.activeObject = GridPaintingState.scenePaintTarget;
		}

		private void SelectPaletteAsset()
		{
			Selection.activeObject = this.palette;
		}

		private void SelectTileAsset()
		{
			Selection.activeObject = this.clipboardView.activeTile;
		}

		private bool NotOverridingColor(GridBrush defaultGridBrush)
		{
			GridBrush.BrushCell[] cells = defaultGridBrush.cells;
			bool result;
			for (int i = 0; i < cells.Length; i++)
			{
				GridBrush.BrushCell brushCell = cells[i];
				TileBase tile = brushCell.tile;
				if (tile is Tile && ((tile as Tile).flags & TileFlags.LockColor) == TileFlags.None)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		private void DoBrushesDropdown()
		{
			GUIContent content = GUIContent.Temp(GridPaintingState.gridBrush.name);
			if (EditorGUILayout.DropdownButton(content, FocusType.Passive, EditorStyles.toolbarPopup, new GUILayoutOption[]
			{
				GUILayout.Width(200f)
			}))
			{
				GridBrushesDropdown.MenuItemProvider itemProvider = new GridBrushesDropdown.MenuItemProvider();
				GridBrushesDropdown windowContent = new GridBrushesDropdown(itemProvider, GridPaletteBrushes.brushes.IndexOf(GridPaintingState.gridBrush), null, new Action<int, object>(this.SelectBrush), 200f);
				PopupWindow.Show(GUILayoutUtility.topLevel.GetLast(), windowContent);
			}
		}

		private void SelectBrush(int i, object o)
		{
			GridPaintingState.gridBrush = GridPaletteBrushes.brushes[i];
		}

		public void OnEnable()
		{
			GridPaintPaletteWindow.instances.Add(this);
			if (this.clipboardView == null)
			{
				this.clipboardView = ScriptableObject.CreateInstance<GridPaintPaletteClipboard>();
				this.clipboardView.owner = this;
				this.clipboardView.hideFlags = HideFlags.HideAndDontSave;
				this.clipboardView.unlocked = false;
			}
			if (this.m_PaintableSceneViewGrid == null)
			{
				this.m_PaintableSceneViewGrid = ScriptableObject.CreateInstance<PaintableSceneViewGrid>();
				this.m_PaintableSceneViewGrid.hideFlags = HideFlags.HideAndDontSave;
			}
			GridPaletteBrushes.FlushCache();
			EditorApplication.globalEventHandler = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.globalEventHandler, new EditorApplication.CallbackFunction(this.HotkeyHandler));
			EditMode.editModeStarted += new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
			EditMode.editModeEnded += new Action<IToolModeOwner>(this.OnEditModeEnd);
			GridSelection.gridSelectionChanged += new Action(this.OnGridSelectionChanged);
			GridPaintingState.RegisterPainterInterest(this);
			GridPaintingState.scenePaintTargetChanged += new Action<GameObject>(this.OnScenePaintTargetChanged);
			GridPaintingState.brushChanged += new Action<GridBrushBase>(this.OnBrushChanged);
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneViewGUI));
			PrefabUtility.prefabInstanceUpdated = (PrefabUtility.PrefabInstanceUpdated)Delegate.Combine(PrefabUtility.prefabInstanceUpdated, new PrefabUtility.PrefabInstanceUpdated(this.PrefabInstanceUpdated));
			AssetPreview.SetPreviewTextureCacheSize(256, base.GetInstanceID());
			base.wantsMouseMove = true;
			base.wantsMouseEnterLeaveWindow = true;
			if (this.m_PreviewResizer == null)
			{
				this.m_PreviewResizer = new PreviewResizer();
				this.m_PreviewResizer.Init("TilemapBrushInspector");
			}
			base.minSize = new Vector2(240f, 200f);
			if (this.palette == null && TilemapEditorUserSettings.lastUsedPalette != null && GridPalettes.palettes.Contains(TilemapEditorUserSettings.lastUsedPalette))
			{
				this.palette = TilemapEditorUserSettings.lastUsedPalette;
			}
			Tools.onToolChanged = (Tools.OnToolChangedFunc)Delegate.Combine(Tools.onToolChanged, new Tools.OnToolChangedFunc(this.ToolChanged));
		}

		private void PrefabInstanceUpdated(GameObject updatedPrefab)
		{
			if (this.m_PaletteInstance != null && PrefabUtility.GetPrefabParent(updatedPrefab) == this.m_Palette && !GridPaintingState.savingPalette)
			{
				this.ResetPreviewInstance();
				base.Repaint();
			}
		}

		private void OnBrushChanged(GridBrushBase brush)
		{
			this.DisableFocus();
			if (brush is GridBrush)
			{
				this.EnableFocus();
			}
			SceneView.RepaintAll();
		}

		private void OnGridSelectionChanged()
		{
			base.Repaint();
		}

		private void ToolChanged(Tool from, Tool to)
		{
			if (to != Tool.None && PaintableGrid.InGridEditMode())
			{
				EditMode.QuitEditMode();
			}
			base.Repaint();
		}

		public void OnDisable()
		{
			this.CallOnToolDeactivated();
			GridPaintPaletteWindow.instances.Remove(this);
			this.DestroyPreviewInstance();
			EditorApplication.globalEventHandler = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.globalEventHandler, new EditorApplication.CallbackFunction(this.HotkeyHandler));
			EditMode.editModeStarted -= new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
			EditMode.editModeEnded -= new Action<IToolModeOwner>(this.OnEditModeEnd);
			Tools.onToolChanged = (Tools.OnToolChangedFunc)Delegate.Remove(Tools.onToolChanged, new Tools.OnToolChangedFunc(this.ToolChanged));
			GridSelection.gridSelectionChanged -= new Action(this.OnGridSelectionChanged);
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneViewGUI));
			GridPaintingState.scenePaintTargetChanged -= new Action<GameObject>(this.OnScenePaintTargetChanged);
			GridPaintingState.brushChanged -= new Action<GridBrushBase>(this.OnBrushChanged);
			GridPaintingState.UnregisterPainterInterest(this);
			PrefabUtility.prefabInstanceUpdated = (PrefabUtility.PrefabInstanceUpdated)Delegate.Remove(PrefabUtility.prefabInstanceUpdated, new PrefabUtility.PrefabInstanceUpdated(this.PrefabInstanceUpdated));
		}

		private void OnScenePaintTargetChanged(GameObject scenePaintTarget)
		{
			this.DisableFocus();
			this.EnableFocus();
			base.Repaint();
		}

		public void OnDestroy()
		{
			this.DestroyPreviewInstance();
			UnityEngine.Object.DestroyImmediate(this.clipboardView);
			UnityEngine.Object.DestroyImmediate(this.m_PaintableSceneViewGrid);
			if (this.m_PreviewUtility != null)
			{
				this.m_PreviewUtility.Cleanup();
			}
			this.m_PreviewUtility = null;
			if (PaintableGrid.InGridEditMode())
			{
				EditMode.QuitEditMode();
			}
			if (GridPaintPaletteWindow.instances.Count <= 1)
			{
				GridPaintingState.gridBrush = null;
			}
		}

		public void ChangeToTool(GridBrushBase.Tool tool)
		{
			EditMode.ChangeEditMode(PaintableGrid.BrushToolToEditMode(tool), new Bounds(Vector3.zero, Vector3.positiveInfinity), ScriptableSingleton<GridPaintingState>.instance);
			base.Repaint();
		}

		private void HotkeyHandler()
		{
			if (GridPaintPaletteWindow.kGridSelectKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridSelect)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridSelect, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridMoveKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridMove)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridMove, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridBrushKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridPainting)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPainting, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridEraseKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridEraser)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridEraser, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridFillKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridFloodFill)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridFloodFill, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridPickerKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridPicking)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridPicking, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintPaletteWindow.kGridRectangleKey.activated)
			{
				if (EditMode.editMode != EditMode.SceneViewEditMode.GridBox)
				{
					EditMode.ChangeEditMode(EditMode.SceneViewEditMode.GridBox, ScriptableSingleton<GridPaintingState>.instance);
				}
				else
				{
					EditMode.QuitEditMode();
				}
				Event.current.Use();
			}
			if (GridPaintingState.gridBrush != null && GridPaintingState.activeGrid != null)
			{
				if (GridPaintPaletteWindow.kRotateClockwise.activated)
				{
					GridPaintingState.gridBrush.Rotate(GridBrushBase.RotationDirection.Clockwise, GridPaintingState.activeGrid.cellLayout);
					GridPaintingState.activeGrid.Repaint();
					Event.current.Use();
				}
				if (GridPaintPaletteWindow.kRotateAntiClockwise.activated)
				{
					GridPaintingState.gridBrush.Rotate(GridBrushBase.RotationDirection.CounterClockwise, GridPaintingState.activeGrid.cellLayout);
					GridPaintingState.activeGrid.Repaint();
					Event.current.Use();
				}
				if (GridPaintPaletteWindow.kFlipX.activated)
				{
					GridPaintingState.gridBrush.Flip(GridBrushBase.FlipAxis.X, GridPaintingState.activeGrid.cellLayout);
					GridPaintingState.activeGrid.Repaint();
					Event.current.Use();
				}
				if (GridPaintPaletteWindow.kFlipY.activated)
				{
					GridPaintingState.gridBrush.Flip(GridBrushBase.FlipAxis.Y, GridPaintingState.activeGrid.cellLayout);
					GridPaintingState.activeGrid.Repaint();
					Event.current.Use();
				}
			}
		}

		public void OnEditModeStart(IToolModeOwner owner, EditMode.SceneViewEditMode editMode)
		{
			if (GridPaintingState.gridBrush != null && PaintableGrid.InGridEditMode() && GridPaintingState.activeBrushEditor != null)
			{
				GridBrushBase.Tool tool = PaintableGrid.EditModeToBrushTool(editMode);
				GridPaintingState.activeBrushEditor.OnToolActivated(tool);
				this.m_PreviousToolActivatedEditor = GridPaintingState.activeBrushEditor;
				this.m_PreviousToolActivated = tool;
				for (int i = 0; i < GridPaintPaletteWindow.Styles.sceneViewEditModes.Length; i++)
				{
					if (GridPaintPaletteWindow.Styles.sceneViewEditModes[i] == editMode)
					{
						Cursor.SetCursor(GridPaintPaletteWindow.Styles.mouseCursorTextures[i], (!(GridPaintPaletteWindow.Styles.mouseCursorTextures[i] != null)) ? Vector2.zero : GridPaintPaletteWindow.Styles.mouseCursorOSHotspot[(int)SystemInfo.operatingSystemFamily], CursorMode.Auto);
						break;
					}
				}
			}
			base.Repaint();
		}

		public void OnEditModeEnd(IToolModeOwner owner)
		{
			if (EditMode.editMode != EditMode.SceneViewEditMode.GridMove && EditMode.editMode != EditMode.SceneViewEditMode.GridSelect)
			{
				GridSelection.Clear();
			}
			this.CallOnToolDeactivated();
			base.Repaint();
		}

		private void CallOnToolDeactivated()
		{
			if (GridPaintingState.gridBrush != null && this.m_PreviousToolActivatedEditor != null)
			{
				this.m_PreviousToolActivatedEditor.OnToolDeactivated(this.m_PreviousToolActivated);
				this.m_PreviousToolActivatedEditor = null;
				if (!PaintableGrid.InGridEditMode())
				{
					Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
				}
			}
		}

		private void OnBrushInspectorGUI()
		{
			GridBrushBase gridBrush = GridPaintingState.gridBrush;
			if (!(gridBrush == null))
			{
				EditorGUI.BeginChangeCheck();
				if (GridPaintingState.activeBrushEditor != null)
				{
					GridPaintingState.activeBrushEditor.OnPaintInspectorGUI();
				}
				else if (GridPaintingState.fallbackEditor != null)
				{
					GridPaintingState.fallbackEditor.OnInspectorGUI();
				}
				if (EditorGUI.EndChangeCheck())
				{
					GridPaletteBrushes.ActiveGridBrushAssetChanged();
				}
			}
		}

		private void DoActiveTargetsGUI()
		{
			bool flag = GridPaintingState.scenePaintTarget != null;
			using (new EditorGUI.DisabledScope(!flag || GridPaintingState.validTargets == null))
			{
				GUILayout.Label(GridPaintPaletteWindow.Styles.activeTargetLabel, new GUILayoutOption[]
				{
					GUILayout.Width(90f)
				});
				GUIContent content = GUIContent.Temp((!flag) ? "Nothing" : GridPaintingState.scenePaintTarget.name);
				if (EditorGUILayout.DropdownButton(content, FocusType.Passive, EditorStyles.popup, new GUILayoutOption[]
				{
					GUILayout.Width(130f)
				}))
				{
					int selectionIndex = (!flag) ? 0 : Array.IndexOf<GameObject>(GridPaintingState.validTargets, GridPaintingState.scenePaintTarget);
					GridPaintTargetsDropdown.MenuItemProvider itemProvider = new GridPaintTargetsDropdown.MenuItemProvider();
					GridPaintTargetsDropdown windowContent = new GridPaintTargetsDropdown(itemProvider, selectionIndex, null, new Action<int, object>(this.SelectTarget), 130f);
					PopupWindow.Show(GUILayoutUtility.topLevel.GetLast(), windowContent);
				}
			}
		}

		private void SelectTarget(int i, object o)
		{
			GridPaintingState.scenePaintTarget = (o as GameObject);
			if (GridPaintingState.scenePaintTarget != null)
			{
				EditorGUIUtility.PingObject(GridPaintingState.scenePaintTarget);
			}
		}

		private void DoClipboardHeader()
		{
			if (!GridPalettes.palettes.Contains(this.palette) || this.palette == null)
			{
				GridPalettes.CleanCache();
				if (GridPalettes.palettes.Count > 0)
				{
					this.palette = GridPalettes.palettes.LastOrDefault<GameObject>();
				}
			}
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			this.DoPalettesDropdown();
			using (new EditorGUI.DisabledScope(this.palette == null))
			{
				this.clipboardView.unlocked = GUILayout.Toggle(this.clipboardView.unlocked, (!this.clipboardView.isModified) ? GridPaintPaletteWindow.Styles.edit : GridPaintPaletteWindow.Styles.editModified, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		private void DoPalettesDropdown()
		{
			string t = (!(this.palette != null)) ? GridPaintPaletteWindow.Styles.createNewPalette.text : this.palette.name;
			Rect rect = GUILayoutUtility.GetRect(GUIContent.Temp(t), EditorStyles.toolbarDropDown, new GUILayoutOption[]
			{
				GUILayout.Width(200f)
			});
			if (GridPalettes.palettes.Count == 0)
			{
				if (EditorGUI.DropdownButton(rect, GUIContent.Temp(t), FocusType.Passive, EditorStyles.toolbarDropDown))
				{
					this.OpenAddPalettePopup(rect);
				}
			}
			else
			{
				GUIContent content = GUIContent.Temp((GridPalettes.palettes.Count <= 0 || !(this.palette != null)) ? GridPaintPaletteWindow.Styles.createNewPalette.text : this.palette.name);
				if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, EditorStyles.toolbarPopup))
				{
					GridPalettesDropdown.MenuItemProvider itemProvider = new GridPalettesDropdown.MenuItemProvider();
					this.m_PaletteDropdown = new GridPalettesDropdown(itemProvider, GridPalettes.palettes.IndexOf(this.palette), null, new Action<int, object>(this.SelectPalette), 200f);
					PopupWindow.Show(GUILayoutUtility.topLevel.GetLast(), this.m_PaletteDropdown);
				}
			}
		}

		private void SelectPalette(int i, object o)
		{
			if (i < GridPalettes.palettes.Count)
			{
				this.palette = GridPalettes.palettes[i];
			}
			else
			{
				this.m_PaletteDropdown.editorWindow.Close();
				this.OpenAddPalettePopup(new Rect(0f, 0f, 0f, 0f));
			}
		}

		private void OpenAddPalettePopup(Rect rect)
		{
			bool flag = GridPaletteAddPopup.ShowAtPosition(rect, this);
			if (flag)
			{
				GUIUtility.ExitGUI();
			}
		}

		private void OnClipboardGUI(Rect position)
		{
			if (Event.current.type != EventType.Layout && position.Contains(Event.current.mousePosition) && GridPaintingState.activeGrid != this.clipboardView)
			{
				GridPaintingState.activeGrid = this.clipboardView;
				SceneView.RepaintAll();
			}
			if (this.palette == null)
			{
				Color color = GUI.color;
				GUI.color = Color.gray;
				if (GridPalettes.palettes.Count == 0)
				{
					GUI.Label(new Rect(position.center.x - GUI.skin.label.CalcSize(GridPaintPaletteWindow.Styles.emptyProjectInfo).x * 0.5f, position.center.y, 500f, 100f), GridPaintPaletteWindow.Styles.emptyProjectInfo);
				}
				else
				{
					GUI.Label(new Rect(position.center.x - GUI.skin.label.CalcSize(GridPaintPaletteWindow.Styles.invalidClipboardInfo).x * 0.5f, position.center.y, 500f, 100f), GridPaintPaletteWindow.Styles.invalidClipboardInfo);
				}
				GUI.color = color;
			}
			else
			{
				bool enabled = GUI.enabled;
				GUI.enabled = (!this.clipboardView.showNewEmptyClipboardInfo || DragAndDrop.objectReferences.Length > 0);
				if (Event.current.type == EventType.Repaint)
				{
					this.clipboardView.guiRect = position;
				}
				EditorGUI.BeginChangeCheck();
				this.clipboardView.OnGUI();
				if (EditorGUI.EndChangeCheck())
				{
					base.Repaint();
				}
				GUI.enabled = enabled;
				if (this.clipboardView.showNewEmptyClipboardInfo)
				{
					Color color2 = GUI.color;
					GUI.color = Color.gray;
					Rect position2 = new Rect(position.center.x - GUI.skin.label.CalcSize(GridPaintPaletteWindow.Styles.emptyClipboardInfo).x * 0.5f, position.center.y, 500f, 100f);
					GUI.Label(position2, GridPaintPaletteWindow.Styles.emptyClipboardInfo);
					GUI.color = color2;
				}
			}
		}

		private void OnSceneViewGUI(SceneView sceneView)
		{
			if (GridPaintingState.defaultBrush != null && GridPaintingState.scenePaintTarget != null)
			{
				SceneViewOverlay.Window(GridPaintPaletteWindow.Styles.rendererOverlayTitleLabel, new SceneViewOverlay.WindowFunction(this.DisplayFocusMode), 500, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);
			}
			else if (TilemapEditorUserSettings.focusMode != TilemapEditorUserSettings.FocusMode.None)
			{
				this.DisableFocus();
				TilemapEditorUserSettings.focusMode = TilemapEditorUserSettings.FocusMode.None;
			}
		}

		private void DisplayFocusMode(UnityEngine.Object displayTarget, SceneView sceneView)
		{
			TilemapEditorUserSettings.FocusMode focusMode = TilemapEditorUserSettings.focusMode;
			TilemapEditorUserSettings.FocusMode focusMode2 = (TilemapEditorUserSettings.FocusMode)EditorGUILayout.EnumPopup(GridPaintPaletteWindow.Styles.focusLabel, focusMode, new GUILayoutOption[0]);
			if (focusMode2 != focusMode)
			{
				this.DisableFocus();
				TilemapEditorUserSettings.focusMode = focusMode2;
				this.EnableFocus();
			}
		}

		private void EnableFocus()
		{
			TilemapEditorUserSettings.FocusMode focusMode = TilemapEditorUserSettings.focusMode;
			if (focusMode != TilemapEditorUserSettings.FocusMode.Tilemap)
			{
				if (focusMode == TilemapEditorUserSettings.FocusMode.Grid)
				{
					Tilemap component = GridPaintingState.scenePaintTarget.GetComponent<Tilemap>();
					if (component != null && component.layoutGrid != null)
					{
						if (SceneView.lastActiveSceneView != null)
						{
							SceneView.lastActiveSceneView.SetSceneViewFiltering(true);
						}
						HierarchyProperty.FilterSingleSceneObject(component.layoutGrid.gameObject.GetInstanceID(), false);
					}
				}
			}
			else
			{
				if (SceneView.lastActiveSceneView != null)
				{
					SceneView.lastActiveSceneView.SetSceneViewFiltering(true);
				}
				HierarchyProperty.FilterSingleSceneObject(GridPaintingState.scenePaintTarget.GetInstanceID(), false);
			}
		}

		private void DisableFocus()
		{
			if (TilemapEditorUserSettings.focusMode != TilemapEditorUserSettings.FocusMode.None)
			{
				HierarchyProperty.ClearSceneObjectsFilter();
				if (SceneView.lastActiveSceneView != null)
				{
					SceneView.lastActiveSceneView.SetSceneViewFiltering(false);
				}
			}
		}

		[MenuItem("Window/Tile Palette", false, 2015)]
		public static void OpenTilemapPalette()
		{
			GridPaintPaletteWindow window = EditorWindow.GetWindow<GridPaintPaletteWindow>();
			window.titleContent = GridPaintPaletteWindow.tilePalette;
		}
	}
}
