using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor
{
	internal class GUIViewDebuggerWindow : EditorWindow
	{
		private enum InstructionType
		{
			Draw,
			Clip,
			Layout,
			NamedControl,
			Property,
			Unified
		}

		internal static class Styles
		{
			public static readonly string defaultWindowPopupText;

			public static readonly GUIContent inspectedWindowLabel;

			public static readonly GUIStyle listItem;

			public static readonly GUIStyle listItemBackground;

			public static readonly GUIStyle listBackgroundStyle;

			public static readonly GUIStyle boxStyle;

			public static readonly GUIStyle stackframeStyle;

			public static readonly GUIStyle stacktraceBackground;

			public static readonly GUIStyle centeredText;

			public static readonly Color contentHighlighterColor;

			public static readonly Color paddingHighlighterColor;

			static Styles()
			{
				GUIViewDebuggerWindow.Styles.defaultWindowPopupText = "<Please Select>";
				GUIViewDebuggerWindow.Styles.inspectedWindowLabel = new GUIContent("Inspected View: ");
				GUIViewDebuggerWindow.Styles.listItem = new GUIStyle("PR Label");
				GUIViewDebuggerWindow.Styles.listItemBackground = new GUIStyle("CN EntryBackOdd");
				GUIViewDebuggerWindow.Styles.listBackgroundStyle = new GUIStyle("CN Box");
				GUIViewDebuggerWindow.Styles.boxStyle = new GUIStyle("CN Box");
				GUIViewDebuggerWindow.Styles.stackframeStyle = new GUIStyle(EditorStyles.label);
				GUIViewDebuggerWindow.Styles.stacktraceBackground = new GUIStyle("CN Box");
				GUIViewDebuggerWindow.Styles.centeredText = new GUIStyle("PR Label");
				GUIViewDebuggerWindow.Styles.contentHighlighterColor = new Color(0.62f, 0.77f, 0.9f, 0.5f);
				GUIViewDebuggerWindow.Styles.paddingHighlighterColor = new Color(0.76f, 0.87f, 0.71f, 0.5f);
				GUIViewDebuggerWindow.Styles.stackframeStyle.margin = new RectOffset(0, 0, 0, 0);
				GUIViewDebuggerWindow.Styles.stackframeStyle.padding = new RectOffset(0, 0, 0, 0);
				GUIViewDebuggerWindow.Styles.stacktraceBackground.padding = new RectOffset(5, 5, 5, 5);
				GUIViewDebuggerWindow.Styles.centeredText.alignment = TextAnchor.MiddleCenter;
				GUIViewDebuggerWindow.Styles.centeredText.stretchHeight = true;
				GUIViewDebuggerWindow.Styles.centeredText.stretchWidth = true;
			}
		}

		private static GUIViewDebuggerWindow s_ActiveInspector;

		[SerializeField]
		private GUIView m_Inspected;

		private EditorWindow m_InspectedEditorWindow;

		private IBaseInspectView m_InstructionModeView;

		[SerializeField]
		private GUIViewDebuggerWindow.InstructionType m_InstructionType = GUIViewDebuggerWindow.InstructionType.Draw;

		private VisualElement m_ContentHighlighter;

		private VisualElement m_PaddingHighlighter;

		private bool m_ShowHighlighter = true;

		[NonSerialized]
		private bool m_QueuedPointInspection = false;

		[NonSerialized]
		private Vector2 m_PointToInspect;

		private readonly SplitterState m_InstructionListDetailSplitter = new SplitterState(new float[]
		{
			30f,
			70f
		}, new int[]
		{
			32,
			32
		}, null);

		public GUIView inspected
		{
			get
			{
				GUIView result;
				if (this.m_Inspected != null || this.m_InspectedEditorWindow == null)
				{
					result = this.m_Inspected;
				}
				else
				{
					GUIView parent = this.m_InspectedEditorWindow.m_Parent;
					this.inspected = parent;
					result = parent;
				}
				return result;
			}
			private set
			{
				if (this.m_Inspected != value)
				{
					this.ClearInstructionHighlighter();
					this.m_Inspected = value;
					if (this.m_Inspected != null)
					{
						this.m_InspectedEditorWindow = ((!(this.m_Inspected is HostView)) ? null : ((HostView)this.m_Inspected).actualView);
						GUIViewDebuggerHelper.DebugWindow(this.m_Inspected);
						this.m_Inspected.Repaint();
					}
					else
					{
						GUIViewDebuggerHelper.StopDebugging();
					}
					if (this.instructionModeView != null)
					{
						this.instructionModeView.ClearRowSelection();
					}
					this.OnInspectedViewChanged();
				}
			}
		}

		public IBaseInspectView instructionModeView
		{
			get
			{
				return this.m_InstructionModeView;
			}
		}

		private GUIViewDebuggerWindow.InstructionType instructionType
		{
			get
			{
				return this.m_InstructionType;
			}
			set
			{
				if (this.m_InstructionType != value || this.m_InstructionModeView == null)
				{
					this.m_InstructionType = value;
					switch (this.m_InstructionType)
					{
					case GUIViewDebuggerWindow.InstructionType.Draw:
						this.m_InstructionModeView = new StyleDrawInspectView(this);
						break;
					case GUIViewDebuggerWindow.InstructionType.Clip:
						this.m_InstructionModeView = new GUIClipInspectView(this);
						break;
					case GUIViewDebuggerWindow.InstructionType.Layout:
						this.m_InstructionModeView = new GUILayoutInspectView(this);
						break;
					case GUIViewDebuggerWindow.InstructionType.NamedControl:
						this.m_InstructionModeView = new GUINamedControlInspectView(this);
						break;
					case GUIViewDebuggerWindow.InstructionType.Property:
						this.m_InstructionModeView = new GUIPropertyInspectView(this);
						break;
					case GUIViewDebuggerWindow.InstructionType.Unified:
						this.m_InstructionModeView = new UnifiedInspectView(this);
						break;
					}
					this.m_InstructionModeView.UpdateInstructions();
				}
			}
		}

		protected GUIViewDebuggerWindow()
		{
			this.m_InstructionModeView = new StyleDrawInspectView(this);
		}

		private static EditorWindow GetEditorWindow(GUIView view)
		{
			HostView hostView = view as HostView;
			EditorWindow result;
			if (hostView != null)
			{
				result = hostView.actualView;
			}
			else
			{
				result = null;
			}
			return result;
		}

		private static string GetViewName(GUIView view)
		{
			EditorWindow editorWindow = GUIViewDebuggerWindow.GetEditorWindow(view);
			string result;
			if (editorWindow != null)
			{
				result = editorWindow.titleContent.text;
			}
			else
			{
				result = view.GetType().Name;
			}
			return result;
		}

		public void ClearInstructionHighlighter()
		{
			if (this.m_PaddingHighlighter != null && this.m_PaddingHighlighter.shadow.parent != null)
			{
				VisualElement parent = this.m_PaddingHighlighter.shadow.parent;
				this.m_PaddingHighlighter.RemoveFromHierarchy();
				this.m_ContentHighlighter.RemoveFromHierarchy();
				parent.Dirty(ChangeType.Repaint);
			}
		}

		public void HighlightInstruction(GUIView view, Rect instructionRect, GUIStyle style)
		{
			if (this.m_ShowHighlighter)
			{
				this.ClearInstructionHighlighter();
				if (this.m_PaddingHighlighter == null)
				{
					this.m_PaddingHighlighter = new VisualElement();
					this.m_PaddingHighlighter.style.backgroundColor = GUIViewDebuggerWindow.Styles.paddingHighlighterColor;
					this.m_ContentHighlighter = new VisualElement();
					this.m_ContentHighlighter.style.backgroundColor = GUIViewDebuggerWindow.Styles.contentHighlighterColor;
				}
				this.m_PaddingHighlighter.layout = instructionRect;
				view.visualTree.Add(this.m_PaddingHighlighter);
				if (style != null)
				{
					instructionRect = style.padding.Remove(instructionRect);
				}
				this.m_ContentHighlighter.layout = instructionRect;
				view.visualTree.Add(this.m_ContentHighlighter);
			}
		}

		private static void Init()
		{
			if (GUIViewDebuggerWindow.s_ActiveInspector == null)
			{
				GUIViewDebuggerWindow gUIViewDebuggerWindow = (GUIViewDebuggerWindow)EditorWindow.GetWindow(typeof(GUIViewDebuggerWindow));
				GUIViewDebuggerWindow.s_ActiveInspector = gUIViewDebuggerWindow;
			}
			GUIViewDebuggerWindow.s_ActiveInspector.Show();
		}

		private void OnEnable()
		{
			base.titleContent = new GUIContent("GUI Inspector");
			GUIViewDebuggerHelper.onViewInstructionsChanged = (Action)Delegate.Combine(GUIViewDebuggerHelper.onViewInstructionsChanged, new Action(this.OnInspectedViewChanged));
			GUIView inspected = this.m_Inspected;
			this.inspected = null;
			this.inspected = inspected;
			this.m_InstructionModeView = null;
			this.instructionType = this.m_InstructionType;
		}

		private void OnDisable()
		{
			GUIViewDebuggerHelper.onViewInstructionsChanged = (Action)Delegate.Remove(GUIViewDebuggerHelper.onViewInstructionsChanged, new Action(this.OnInspectedViewChanged));
			GUIViewDebuggerHelper.StopDebugging();
			this.ClearInstructionHighlighter();
		}

		private void OnBecameVisible()
		{
			this.OnShowOverlayChanged();
		}

		private void OnBecameInvisible()
		{
			this.ClearInstructionHighlighter();
		}

		private void OnGUI()
		{
			this.DoToolbar();
			this.ShowDrawInstructions();
		}

		private void OnInspectedViewChanged()
		{
			this.RefreshData();
			base.Repaint();
		}

		private void DoToolbar()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[0]);
			this.DoWindowPopup();
			this.DoInspectTypePopup();
			this.DoInstructionOverlayToggle();
			GUILayout.EndHorizontal();
		}

		private bool CanInspectView(GUIView view)
		{
			bool result;
			if (view == null)
			{
				result = false;
			}
			else
			{
				EditorWindow editorWindow = GUIViewDebuggerWindow.GetEditorWindow(view);
				result = (editorWindow == null || !(editorWindow == this));
			}
			return result;
		}

		private void DoWindowPopup()
		{
			string t = (!(this.inspected == null)) ? GUIViewDebuggerWindow.GetViewName(this.inspected) : GUIViewDebuggerWindow.Styles.defaultWindowPopupText;
			GUILayout.Label(GUIViewDebuggerWindow.Styles.inspectedWindowLabel, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			Rect rect = GUILayoutUtility.GetRect(GUIContent.Temp(t), EditorStyles.toolbarDropDown, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true)
			});
			if (GUI.Button(rect, GUIContent.Temp(t), EditorStyles.toolbarDropDown))
			{
				List<GUIView> list = new List<GUIView>();
				GUIViewDebuggerHelper.GetViews(list);
				List<GUIContent> list2 = new List<GUIContent>(list.Count + 1);
				list2.Add(new GUIContent("None"));
				int selected = 0;
				List<GUIView> list3 = new List<GUIView>(list.Count + 1);
				for (int i = 0; i < list.Count; i++)
				{
					GUIView gUIView = list[i];
					if (this.CanInspectView(gUIView))
					{
						GUIContent item = new GUIContent(string.Format("{0}. {1}", list2.Count, GUIViewDebuggerWindow.GetViewName(gUIView)));
						list2.Add(item);
						list3.Add(gUIView);
						if (gUIView == this.inspected)
						{
							selected = list3.Count;
						}
					}
				}
				EditorUtility.DisplayCustomMenu(rect, list2.ToArray(), selected, new EditorUtility.SelectMenuItemFunction(this.OnWindowSelected), list3);
			}
		}

		private void DoInspectTypePopup()
		{
			EditorGUI.BeginChangeCheck();
			GUIViewDebuggerWindow.InstructionType instructionType = (GUIViewDebuggerWindow.InstructionType)EditorGUILayout.EnumPopup(this.m_InstructionType, EditorStyles.toolbarDropDown, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.instructionType = instructionType;
			}
		}

		private void DoInstructionOverlayToggle()
		{
			EditorGUI.BeginChangeCheck();
			this.m_ShowHighlighter = GUILayout.Toggle(this.m_ShowHighlighter, GUIContent.Temp("Show overlay"), EditorStyles.toolbarButton, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.OnShowOverlayChanged();
			}
		}

		private void OnShowOverlayChanged()
		{
			if (!this.m_ShowHighlighter)
			{
				this.ClearInstructionHighlighter();
			}
			else if (this.inspected != null)
			{
				this.instructionModeView.ShowOverlay();
			}
		}

		private void OnWindowSelected(object userdata, string[] options, int selected)
		{
			selected--;
			this.inspected = ((selected >= 0) ? ((List<GUIView>)userdata)[selected] : null);
		}

		private void RefreshData()
		{
			this.instructionModeView.UpdateInstructions();
		}

		private void ShowDrawInstructions()
		{
			if (this.inspected == null)
			{
				this.ClearInstructionHighlighter();
			}
			else
			{
				if (this.m_QueuedPointInspection)
				{
					this.instructionModeView.ClearRowSelection();
					this.instructionModeView.SelectRow(this.FindInstructionUnderPoint(this.m_PointToInspect));
					this.m_QueuedPointInspection = false;
				}
				SplitterGUILayout.BeginHorizontalSplit(this.m_InstructionListDetailSplitter, new GUILayoutOption[0]);
				this.instructionModeView.DrawInstructionList();
				EditorGUILayout.BeginVertical(new GUILayoutOption[0]);
				this.instructionModeView.DrawSelectedInstructionDetails();
				EditorGUILayout.EndVertical();
				SplitterGUILayout.EndHorizontalSplit();
			}
		}

		private void InspectPointAt(Vector2 point)
		{
			this.m_PointToInspect = point;
			this.m_QueuedPointInspection = true;
			this.inspected.Repaint();
			base.Repaint();
		}

		private int FindInstructionUnderPoint(Vector2 point)
		{
			List<IMGUIDrawInstruction> list = new List<IMGUIDrawInstruction>();
			GUIViewDebuggerHelper.GetDrawInstructions(list);
			int result;
			for (int i = 0; i < list.Count; i++)
			{
				Rect rect = list[i].rect;
				if (rect.Contains(point))
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}
	}
}
