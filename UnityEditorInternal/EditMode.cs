using System;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	[InitializeOnLoad]
	public class EditMode
	{
		private static class Styles
		{
			public static readonly GUIStyle multiButtonStyle;

			public static readonly GUIStyle singleButtonStyle;

			static Styles()
			{
				EditMode.Styles.multiButtonStyle = "Command";
				EditMode.Styles.singleButtonStyle = new GUIStyle("Button");
				EditMode.Styles.singleButtonStyle.padding = EditMode.Styles.multiButtonStyle.padding;
				EditMode.Styles.singleButtonStyle.margin = EditMode.Styles.multiButtonStyle.margin;
			}
		}

		public delegate void OnEditModeStopFunc(Editor editor);

		public delegate void OnEditModeStartFunc(Editor editor, EditMode.SceneViewEditMode mode);

		public enum SceneViewEditMode
		{
			None,
			Collider,
			ClothConstraints,
			ClothSelfAndInterCollisionParticles,
			ReflectionProbeBox,
			ReflectionProbeOrigin,
			LightProbeProxyVolumeBox,
			LightProbeProxyVolumeOrigin,
			LightProbeGroup,
			ParticleSystemCollisionModulePlanesMove,
			ParticleSystemCollisionModulePlanesRotate,
			JointAngularLimits,
			GridPainting,
			GridPicking,
			GridEraser,
			GridFloodFill,
			GridBox,
			GridSelect,
			GridMove
		}

		private const string kEditModeStringKey = "EditModeState";

		private const string kPrevToolStringKey = "EditModePrevTool";

		private const string kOwnerStringKey = "EditModeOwner";

		private static bool s_Debug;

		private const float k_EditColliderbuttonWidth = 33f;

		private const float k_EditColliderbuttonHeight = 23f;

		private const float k_SpaceBetweenLabelAndButton = 5f;

		public static EditMode.OnEditModeStopFunc onEditModeEndDelegate;

		public static EditMode.OnEditModeStartFunc onEditModeStartDelegate;

		private static Tool s_ToolBeforeEnteringEditMode;

		private static int s_OwnerID;

		private static EditMode.SceneViewEditMode s_EditMode;

		[CompilerGenerated]
		private static Action <>f__mg$cache0;

		internal static event Action<IToolModeOwner> editModeEnded
		{
			add
			{
				Action<IToolModeOwner> action = EditMode.editModeEnded;
				Action<IToolModeOwner> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<IToolModeOwner>>(ref EditMode.editModeEnded, (Action<IToolModeOwner>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<IToolModeOwner> action = EditMode.editModeEnded;
				Action<IToolModeOwner> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<IToolModeOwner>>(ref EditMode.editModeEnded, (Action<IToolModeOwner>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		internal static event Action<IToolModeOwner, EditMode.SceneViewEditMode> editModeStarted
		{
			add
			{
				Action<IToolModeOwner, EditMode.SceneViewEditMode> action = EditMode.editModeStarted;
				Action<IToolModeOwner, EditMode.SceneViewEditMode> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<IToolModeOwner, EditMode.SceneViewEditMode>>(ref EditMode.editModeStarted, (Action<IToolModeOwner, EditMode.SceneViewEditMode>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<IToolModeOwner, EditMode.SceneViewEditMode> action = EditMode.editModeStarted;
				Action<IToolModeOwner, EditMode.SceneViewEditMode> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<IToolModeOwner, EditMode.SceneViewEditMode>>(ref EditMode.editModeStarted, (Action<IToolModeOwner, EditMode.SceneViewEditMode>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		private static Tool toolBeforeEnteringEditMode
		{
			get
			{
				return EditMode.s_ToolBeforeEnteringEditMode;
			}
			set
			{
				EditMode.s_ToolBeforeEnteringEditMode = value;
				SessionState.SetInt("EditModePrevTool", (int)EditMode.s_ToolBeforeEnteringEditMode);
				if (EditMode.s_Debug)
				{
					Debug.Log("Set toolBeforeEnteringEditMode " + value);
				}
			}
		}

		private static int ownerID
		{
			get
			{
				return EditMode.s_OwnerID;
			}
			set
			{
				EditMode.s_OwnerID = value;
				SessionState.SetInt("EditModeOwner", EditMode.s_OwnerID);
				if (EditMode.s_Debug)
				{
					Debug.Log("Set ownerID " + value);
				}
			}
		}

		public static EditMode.SceneViewEditMode editMode
		{
			get
			{
				return EditMode.s_EditMode;
			}
			private set
			{
				if (EditMode.s_EditMode == EditMode.SceneViewEditMode.None && value != EditMode.SceneViewEditMode.None)
				{
					EditMode.toolBeforeEnteringEditMode = ((Tools.current == Tool.None) ? Tool.Move : Tools.current);
					Tools.current = Tool.None;
				}
				else if (EditMode.s_EditMode != EditMode.SceneViewEditMode.None && value == EditMode.SceneViewEditMode.None)
				{
					EditMode.ResetToolToPrevious();
				}
				EditMode.s_EditMode = value;
				SessionState.SetInt("EditModeState", (int)EditMode.s_EditMode);
				if (EditMode.s_Debug)
				{
					Debug.Log("Set editMode " + EditMode.s_EditMode);
				}
			}
		}

		static EditMode()
		{
			EditMode.s_Debug = false;
			EditMode.s_ToolBeforeEnteringEditMode = Tool.Move;
			EditMode.ownerID = SessionState.GetInt("EditModeOwner", EditMode.ownerID);
			EditMode.editMode = (EditMode.SceneViewEditMode)SessionState.GetInt("EditModeState", (int)EditMode.editMode);
			EditMode.toolBeforeEnteringEditMode = (Tool)SessionState.GetInt("EditModePrevTool", (int)EditMode.toolBeforeEnteringEditMode);
			Delegate arg_6B_0 = Selection.selectionChanged;
			if (EditMode.<>f__mg$cache0 == null)
			{
				EditMode.<>f__mg$cache0 = new Action(EditMode.OnSelectionChange);
			}
			Selection.selectionChanged = (Action)Delegate.Combine(arg_6B_0, EditMode.<>f__mg$cache0);
			if (EditMode.s_Debug)
			{
				Debug.Log(string.Concat(new object[]
				{
					"EditMode static constructor: ",
					EditMode.ownerID,
					" ",
					EditMode.editMode,
					" ",
					EditMode.toolBeforeEnteringEditMode
				}));
			}
		}

		public static bool IsOwner(Editor editor)
		{
			return EditMode.IsOwner(editor);
		}

		internal static bool IsOwner(IToolModeOwner owner)
		{
			return owner.GetInstanceID() == EditMode.s_OwnerID;
		}

		public static void ResetToolToPrevious()
		{
			if (Tools.current == Tool.None)
			{
				Tools.current = EditMode.toolBeforeEnteringEditMode;
			}
		}

		private static void EndSceneViewEditing()
		{
			EditMode.ChangeEditMode(EditMode.SceneViewEditMode.None, new Bounds(Vector3.zero, Vector3.positiveInfinity), null);
		}

		public static void OnSelectionChange()
		{
			IToolModeOwner toolModeOwner = InternalEditorUtility.GetObjectFromInstanceID(EditMode.ownerID) as IToolModeOwner;
			if (toolModeOwner == null || !toolModeOwner.ModeSurvivesSelectionChange((int)EditMode.s_EditMode))
			{
				EditMode.QuitEditMode();
			}
		}

		public static void QuitEditMode()
		{
			if (Tools.current == Tool.None && EditMode.editMode != EditMode.SceneViewEditMode.None)
			{
				EditMode.ResetToolToPrevious();
			}
			EditMode.EndSceneViewEditing();
		}

		private static void DetectMainToolChange()
		{
			if (Tools.current != Tool.None && EditMode.editMode != EditMode.SceneViewEditMode.None)
			{
				EditMode.EndSceneViewEditing();
			}
		}

		[Obsolete("Use signature passing Func<Bounds> rather than Bounds.")]
		public static void DoEditModeInspectorModeButton(EditMode.SceneViewEditMode mode, string label, GUIContent icon, Bounds bounds, Editor caller)
		{
			EditMode.DoEditModeInspectorModeButton(mode, label, icon, () => bounds, caller);
		}

		public static void DoEditModeInspectorModeButton(EditMode.SceneViewEditMode mode, string label, GUIContent icon, Func<Bounds> getBoundsOfTargets, Editor caller)
		{
			EditMode.DoEditModeInspectorModeButton(mode, label, icon, getBoundsOfTargets, caller);
		}

		internal static void DoEditModeInspectorModeButton(EditMode.SceneViewEditMode mode, string label, GUIContent icon, IToolModeOwner owner)
		{
			EditMode.DoEditModeInspectorModeButton(mode, label, icon, null, owner);
		}

		private static void DoEditModeInspectorModeButton(EditMode.SceneViewEditMode mode, string label, GUIContent icon, Func<Bounds> getBoundsOfTargets, IToolModeOwner owner)
		{
			EditMode.DetectMainToolChange();
			Rect controlRect = EditorGUILayout.GetControlRect(true, 23f, EditMode.Styles.singleButtonStyle, new GUILayoutOption[0]);
			Rect position = new Rect(controlRect.xMin + EditorGUIUtility.labelWidth, controlRect.yMin, 33f, 23f);
			GUIContent content = new GUIContent(label);
			Vector2 vector = GUI.skin.label.CalcSize(content);
			Rect position2 = new Rect(position.xMax + 5f, controlRect.yMin + (controlRect.height - vector.y) * 0.5f, vector.x, controlRect.height);
			int instanceID = owner.GetInstanceID();
			bool value = EditMode.editMode == mode && EditMode.ownerID == instanceID;
			EditorGUI.BeginChangeCheck();
			bool flag = false;
			using (new EditorGUI.DisabledScope(!owner.areToolModesAvailable))
			{
				flag = GUI.Toggle(position, value, icon, EditMode.Styles.singleButtonStyle);
				GUI.Label(position2, label);
			}
			if (EditorGUI.EndChangeCheck())
			{
				EditMode.ChangeEditMode((!flag) ? EditMode.SceneViewEditMode.None : mode, (getBoundsOfTargets != null) ? getBoundsOfTargets() : owner.GetWorldBoundsOfTargets(), owner);
			}
		}

		[Obsolete("Use signature passing Func<Bounds> rather than Bounds.")]
		public static void DoInspectorToolbar(EditMode.SceneViewEditMode[] modes, GUIContent[] guiContents, Bounds bounds, Editor caller)
		{
			EditMode.DoInspectorToolbar(modes, guiContents, () => bounds, caller);
		}

		public static void DoInspectorToolbar(EditMode.SceneViewEditMode[] modes, GUIContent[] guiContents, Func<Bounds> getBoundsOfTargets, Editor caller)
		{
			EditMode.DoInspectorToolbar(modes, guiContents, getBoundsOfTargets, caller);
		}

		internal static void DoInspectorToolbar(EditMode.SceneViewEditMode[] modes, GUIContent[] guiContents, IToolModeOwner owner)
		{
			EditMode.DoInspectorToolbar(modes, guiContents, null, owner);
		}

		private static void DoInspectorToolbar(EditMode.SceneViewEditMode[] modes, GUIContent[] guiContents, Func<Bounds> getBoundsOfTargets, IToolModeOwner owner)
		{
			EditMode.DetectMainToolChange();
			int instanceID = owner.GetInstanceID();
			int num = ArrayUtility.IndexOf<EditMode.SceneViewEditMode>(modes, EditMode.editMode);
			if (EditMode.ownerID != instanceID)
			{
				num = -1;
			}
			EditorGUI.BeginChangeCheck();
			int num2 = num;
			using (new EditorGUI.DisabledScope(!owner.areToolModesAvailable))
			{
				num2 = GUILayout.Toolbar(num, guiContents, EditMode.Styles.multiButtonStyle, new GUILayoutOption[0]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				EditMode.SceneViewEditMode mode = (num2 != num) ? modes[num2] : EditMode.SceneViewEditMode.None;
				EditMode.ChangeEditMode(mode, (getBoundsOfTargets != null) ? getBoundsOfTargets() : owner.GetWorldBoundsOfTargets(), owner);
			}
		}

		public static void ChangeEditMode(EditMode.SceneViewEditMode mode, Bounds bounds, Editor caller)
		{
			EditMode.ChangeEditMode(mode, bounds, caller);
		}

		internal static void ChangeEditMode(EditMode.SceneViewEditMode mode, IToolModeOwner owner)
		{
			EditMode.ChangeEditMode(mode, owner.GetWorldBoundsOfTargets(), owner);
		}

		internal static void ChangeEditMode(EditMode.SceneViewEditMode mode, Bounds bounds, IToolModeOwner owner)
		{
			IToolModeOwner toolModeOwner = InternalEditorUtility.GetObjectFromInstanceID(EditMode.ownerID) as IToolModeOwner;
			EditMode.editMode = mode;
			EditMode.ownerID = ((mode == EditMode.SceneViewEditMode.None) ? 0 : owner.GetInstanceID());
			if (EditMode.onEditModeEndDelegate != null && toolModeOwner is Editor)
			{
				EditMode.onEditModeEndDelegate(toolModeOwner as Editor);
			}
			if (EditMode.editModeEnded != null)
			{
				EditMode.editModeEnded(toolModeOwner);
			}
			if (EditMode.editMode != EditMode.SceneViewEditMode.None)
			{
				if (EditMode.onEditModeStartDelegate != null && owner is Editor)
				{
					EditMode.onEditModeStartDelegate(owner as Editor, EditMode.editMode);
				}
				if (EditMode.editModeStarted != null)
				{
					EditMode.editModeStarted(owner, EditMode.editMode);
				}
			}
			EditMode.EditModeChanged(bounds);
			InspectorWindow.RepaintAllInspectors();
		}

		private static void EditModeChanged(Bounds bounds)
		{
			if (EditMode.editMode != EditMode.SceneViewEditMode.None && SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null && !EditMode.SeenByCamera(SceneView.lastActiveSceneView.camera, bounds))
			{
				SceneView.lastActiveSceneView.Frame(bounds, EditorApplication.isPlaying);
			}
			SceneView.RepaintAll();
		}

		private static bool SeenByCamera(Camera camera, Bounds bounds)
		{
			return EditMode.AnyPointSeenByCamera(camera, EditMode.GetPoints(bounds));
		}

		private static Vector3[] GetPoints(Bounds bounds)
		{
			return EditMode.BoundsToPoints(bounds);
		}

		private static Vector3[] BoundsToPoints(Bounds bounds)
		{
			return new Vector3[]
			{
				new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
				new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
				new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
				new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
				new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)
			};
		}

		private static bool AnyPointSeenByCamera(Camera camera, Vector3[] points)
		{
			bool result;
			for (int i = 0; i < points.Length; i++)
			{
				Vector3 point = points[i];
				if (EditMode.PointSeenByCamera(camera, point))
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		private static bool PointSeenByCamera(Camera camera, Vector3 point)
		{
			Vector3 vector = camera.WorldToViewportPoint(point);
			return vector.x > 0f && vector.x < 1f && vector.y > 0f && vector.y < 1f;
		}
	}
}
