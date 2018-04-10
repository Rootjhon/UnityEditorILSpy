using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.StyleSheets;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class UIElementsDebugger : EditorWindow
	{
		internal struct ViewPanel
		{
			public GUIView View;

			public Panel Panel;
		}

		internal static class Styles
		{
			internal const float LabelSizeSize = 50f;

			internal const float SizeRectLineSize = 3f;

			internal const float SizeRectBetweenSize = 35f;

			internal const float SizeRectHeight = 350f;

			internal const float SplitterSize = 2f;

			internal const float LabelWidth = 150f;

			public static GUIStyle KSizeLabel = new GUIStyle
			{
				alignment = TextAnchor.MiddleCenter
			};

			public static GUIStyle KInspectorTitle = new GUIStyle(EditorStyles.whiteLargeLabel)
			{
				alignment = TextAnchor.MiddleCenter
			};

			public static readonly GUIContent elementStylesContent = new GUIContent("Element styles");

			public static readonly GUIContent showDefaultsContent = new GUIContent("Show defaults");

			public static readonly GUIContent sortContent = new GUIContent("Sort");

			public static readonly GUIContent inlineContent = new GUIContent("INLINE");

			public static readonly GUIContent marginContent = new GUIContent("Margin");

			public static readonly GUIContent borderContent = new GUIContent("Border");

			public static readonly GUIContent paddingContent = new GUIContent("Padding");

			public static readonly GUIContent cancelPickingContent = new GUIContent("Cancel picking");

			public static readonly GUIContent pickPanelContent = new GUIContent("Pick Panel");

			public static readonly GUIContent pickElementInPanelContent = new GUIContent("Pick Element in panel");

			public static readonly GUIContent overlayContent = new GUIContent("Overlay");

			public static readonly GUIContent uxmlContent = new GUIContent("UXML Dump");

			public static readonly GUIContent stylesheetsContent = new GUIContent("Stylesheets");

			public static readonly GUIContent selectorsContent = new GUIContent("Matching Selectors");

			public static readonly GUIContent includeShadowHierarchyContent = new GUIContent("Include Shadow Hierarchy");

			private static readonly Color k_SeparatorColorPro = new Color(0.15f, 0.15f, 0.15f);

			private static readonly Color k_SeparatorColorNonPro = new Color(0.6f, 0.6f, 0.6f);

			internal static readonly Color kSizeMarginPrimaryColor = new Color(0f, 0f, 0f);

			internal static readonly Color kSizeMarginSecondaryColor = new Color(0.9764706f, 0.8f, 0.6156863f);

			internal static readonly Color kSizeBorderPrimaryColor = new Color(0f, 0f, 0f);

			internal static readonly Color kSizeBorderSecondaryColor = new Color(0.992156863f, 0.8666667f, 0.607843161f);

			internal static readonly Color kSizePaddingPrimaryColor = new Color(0f, 0f, 0f);

			internal static readonly Color kSizePaddingSecondaryColor = new Color(0.7607843f, 0.929411769f, 0.5411765f);

			internal static readonly Color kSizePrimaryColor = new Color(0f, 0f, 0f);

			internal static readonly Color kSizeSecondaryColor = new Color(0.545098066f, 0.709803939f, 0.7529412f);

			public static Color separatorColor
			{
				get
				{
					return (!EditorGUIUtility.isProSkin) ? UIElementsDebugger.Styles.k_SeparatorColorNonPro : UIElementsDebugger.Styles.k_SeparatorColorPro;
				}
			}
		}

		[SerializeField]
		private string m_LastWindowTitle;

		private bool m_ScheduleRestoreSelection;

		private HashSet<int> m_CurFoldout = new HashSet<int>();

		private string m_DumpId = "dump";

		private bool m_UxmlDumpExpanded;

		private bool m_UxmlDumpStyleFields;

		private bool m_NewLineOnAttributes;

		private bool m_AutoNameElements;

		private UIElementsDebugger.ViewPanel? m_CurPanel;

		private string m_DetailFilter = string.Empty;

		private Vector2 m_DetailScroll = Vector2.zero;

		private bool m_Overlay = true;

		private PickingData m_PickingData;

		private bool m_PickingElementInPanel;

		private VisualElement m_SelectedElement;

		private bool m_ShowDefaults;

		private bool m_Sort;

		private SplitterState m_SplitterState;

		private Texture2D m_TempTexture;

		private TreeViewState m_VisualTreeTreeViewState;

		private VisualTreeTreeView m_VisualTreeTreeView;

		private static readonly PropertyInfo[] k_FieldInfos = typeof(IStyle).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		private static readonly PropertyInfo[] k_SortedFieldInfos = (from f in UIElementsDebugger.k_FieldInfos
		orderby f.Name
		select f).ToArray<PropertyInfo>();

		private static MatchedRulesExtractor s_MatchedRulesExtractor = new MatchedRulesExtractor();

		private string m_SelectedElementUxml;

		private ReorderableList m_ClassList;

		private string m_NewClass;

		[MenuItem("Window/UI Debuggers/UIElements Debugger", false, 2013, true)]
		public static void Open()
		{
			EditorWindow.GetWindow<UIElementsDebugger>().Show();
		}

		private bool InterceptEvents(Event ev)
		{
			bool result;
			if (!this.m_CurPanel.HasValue)
			{
				result = false;
			}
			else if (!Event.current.isMouse)
			{
				result = false;
			}
			else
			{
				VisualElement e = this.m_CurPanel.Value.Panel.Pick(ev.mousePosition);
				if (e != null)
				{
					((PanelDebug)this.m_CurPanel.Value.Panel.panelDebug).highlightedElement = e.controlid;
				}
				if (ev.clickCount > 0 && ev.button == 0)
				{
					this.m_CurPanel.Value.Panel.panelDebug.interceptEvents = null;
					this.m_PickingElementInPanel = false;
					this.m_VisualTreeTreeView.ExpandAll();
					VisualTreeItem visualTreeItem = this.m_VisualTreeTreeView.GetRows().OfType<VisualTreeItem>().FirstOrDefault((VisualTreeItem vti) => e != null && vti.elt.controlid == e.controlid);
					if (visualTreeItem != null)
					{
						this.m_VisualTreeTreeView.SetSelection(new List<int>
						{
							visualTreeItem.id
						}, TreeViewSelectionOptions.RevealAndFrame);
					}
				}
				result = true;
			}
			return result;
		}

		public void OnGUI()
		{
			if (this.m_ScheduleRestoreSelection)
			{
				this.m_ScheduleRestoreSelection = false;
				if (this.m_PickingData.TryRestoreSelectedWindow(this.m_LastWindowTitle))
				{
					this.EndPicking(this.m_PickingData.Selected);
					this.m_VisualTreeTreeView.ExpandAll();
				}
				else
				{
					this.m_LastWindowTitle = string.Empty;
				}
			}
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true)
			});
			bool flag = false;
			EditorGUI.BeginChangeCheck();
			this.m_PickingData.DoSelectDropDown();
			if (EditorGUI.EndChangeCheck())
			{
				flag = true;
			}
			if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(50f)
			}))
			{
				this.m_PickingData.Refresh();
			}
			bool flag2 = GUILayout.Toggle(this.m_VisualTreeTreeView.includeShadowHierarchy, UIElementsDebugger.Styles.includeShadowHierarchyContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			if (flag2 != this.m_VisualTreeTreeView.includeShadowHierarchy)
			{
				this.m_VisualTreeTreeView.includeShadowHierarchy = flag2;
				flag = true;
			}
			if (this.m_CurPanel.HasValue)
			{
				bool flag3 = GUILayout.Toggle(this.m_PickingElementInPanel, UIElementsDebugger.Styles.pickElementInPanelContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
				if (flag3 != this.m_PickingElementInPanel)
				{
					this.m_PickingElementInPanel = flag3;
					if (this.m_PickingElementInPanel)
					{
						this.m_CurPanel.Value.Panel.panelDebug.interceptEvents = new Func<Event, bool>(this.InterceptEvents);
					}
				}
			}
			this.m_Overlay = GUILayout.Toggle(this.m_Overlay, UIElementsDebugger.Styles.overlayContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			EditorGUILayout.EndHorizontal();
			if (flag)
			{
				this.EndPicking(this.m_PickingData.Selected);
			}
			if (this.m_CurPanel.HasValue)
			{
				if (this.m_CurPanel.Value.Panel.panelDebug.enabled != this.m_Overlay)
				{
					this.m_CurPanel.Value.Panel.panelDebug.enabled = this.m_Overlay;
					this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Repaint);
				}
				SplitterGUILayout.BeginHorizontalSplit(this.m_SplitterState, new GUILayoutOption[]
				{
					GUILayout.ExpandWidth(true),
					GUILayout.ExpandHeight(true)
				});
				SplitterGUILayout.EndHorizontalSplit();
				float num = (float)((this.m_SplitterState.realSizes.Length <= 0) ? 150 : this.m_SplitterState.realSizes[0]);
				float width = base.position.width - num;
				Rect rect = new Rect(0f, 17f, num, base.position.height - 17f);
				Rect rect2 = new Rect(num, 17f, width, rect.height);
				this.m_VisualTreeTreeView.OnGUI(rect);
				this.DrawSelection(rect2);
				EditorGUI.DrawRect(new Rect(num + rect.xMin, rect.y, 1f, rect.height), UIElementsDebugger.Styles.separatorColor);
			}
		}

		private void EndPicking(UIElementsDebugger.ViewPanel? viewPanel)
		{
			Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
			while (panelsIterator.MoveNext())
			{
				KeyValuePair<int, Panel> current = panelsIterator.Current;
				current.Value.panelDebug = null;
			}
			this.m_CurPanel = viewPanel;
			if (this.m_CurPanel.HasValue)
			{
				this.m_LastWindowTitle = PickingData.GetName(this.m_CurPanel.Value);
				if (this.m_CurPanel.Value.Panel.panelDebug == null)
				{
					this.m_CurPanel.Value.Panel.panelDebug = new PanelDebug();
				}
				this.m_CurPanel.Value.Panel.panelDebug.enabled = true;
				this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Repaint);
				this.m_VisualTreeTreeView.panel = this.m_CurPanel.Value.Panel;
				this.m_VisualTreeTreeView.Reload();
			}
		}

		private void DrawSelection(Rect rect)
		{
			Event current = Event.current;
			if (current.type == EventType.Layout)
			{
				this.CacheData();
			}
			if (this.m_SelectedElement != null)
			{
				GUILayout.BeginArea(rect);
				EditorGUILayout.LabelField(this.m_SelectedElement.GetType().Name, UIElementsDebugger.Styles.KInspectorTitle, new GUILayoutOption[0]);
				this.m_DetailScroll = EditorGUILayout.BeginScrollView(this.m_DetailScroll, new GUILayoutOption[0]);
				Rect rect2 = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, new GUILayoutOption[]
				{
					GUILayout.ExpandWidth(true),
					GUILayout.Height(350f)
				});
				rect2.y += EditorGUIUtility.singleLineHeight;
				this.DrawSize(rect2, this.m_SelectedElement);
				this.DrawUxmlDump(this.m_SelectedElement);
				this.DrawMatchingRules();
				this.DrawProperties();
				EditorGUILayout.EndScrollView();
				GUILayout.EndArea();
			}
		}

		private void DrawUxmlDump(VisualElement selectedElement)
		{
			this.m_UxmlDumpExpanded = EditorGUILayout.Foldout(this.m_UxmlDumpExpanded, UIElementsDebugger.Styles.uxmlContent);
			if (this.m_UxmlDumpExpanded)
			{
				EditorGUI.BeginChangeCheck();
				this.m_DumpId = EditorGUILayout.TextField("Template id", this.m_DumpId, new GUILayoutOption[0]);
				this.m_UxmlDumpStyleFields = EditorGUILayout.Toggle("Include style fields", this.m_UxmlDumpStyleFields, new GUILayoutOption[0]);
				this.m_NewLineOnAttributes = EditorGUILayout.Toggle("Line breaks on attributes", this.m_NewLineOnAttributes, new GUILayoutOption[0]);
				this.m_AutoNameElements = EditorGUILayout.Toggle("Auto name elements", this.m_AutoNameElements, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					this.m_SelectedElementUxml = null;
				}
				if (this.m_SelectedElementUxml == null)
				{
					UxmlExporter.ExportOptions exportOptions = UxmlExporter.ExportOptions.None;
					if (this.m_UxmlDumpStyleFields)
					{
						exportOptions = UxmlExporter.ExportOptions.StyleFields;
					}
					if (this.m_NewLineOnAttributes)
					{
						exportOptions |= UxmlExporter.ExportOptions.NewLineOnAttributes;
					}
					if (this.m_AutoNameElements)
					{
						exportOptions |= UxmlExporter.ExportOptions.AutoNameElements;
					}
					this.m_SelectedElementUxml = UxmlExporter.Dump(selectedElement, this.m_DumpId ?? "template", exportOptions);
				}
				EditorGUILayout.TextArea(this.m_SelectedElementUxml, new GUILayoutOption[0]);
			}
		}

		private void CacheData()
		{
			if (!this.m_VisualTreeTreeView.HasSelection())
			{
				this.m_SelectedElement = null;
				this.m_UxmlDumpExpanded = false;
				this.m_ClassList = null;
				if (!this.m_PickingElementInPanel && this.m_CurPanel.HasValue && this.m_CurPanel.Value.Panel != null && this.m_CurPanel.Value.Panel.panelDebug != null)
				{
					((PanelDebug)this.m_CurPanel.Value.Panel.panelDebug).highlightedElement = 0u;
				}
			}
			else
			{
				int selectedId = this.m_VisualTreeTreeView.GetSelection().First<int>();
				VisualTreeItem nodeFor = this.m_VisualTreeTreeView.GetNodeFor(selectedId);
				if (nodeFor != null)
				{
					VisualElement elt = nodeFor.elt;
					if (elt != null)
					{
						if (this.m_CurPanel.HasValue)
						{
							if (this.m_SelectedElement != elt)
							{
								this.m_SelectedElement = elt;
								this.m_SelectedElementUxml = null;
								this.m_ClassList = null;
							}
							if (!this.m_PickingElementInPanel)
							{
								((PanelDebug)this.m_CurPanel.Value.Panel.panelDebug).highlightedElement = elt.controlid;
							}
							this.GetElementMatchers();
						}
					}
				}
			}
		}

		private void GetElementMatchers()
		{
			if (this.m_SelectedElement != null && this.m_SelectedElement.elementPanel != null)
			{
				UIElementsDebugger.s_MatchedRulesExtractor.selectedElementRules.Clear();
				UIElementsDebugger.s_MatchedRulesExtractor.selectedElementStylesheets.Clear();
				UIElementsDebugger.s_MatchedRulesExtractor.target = this.m_SelectedElement;
				UIElementsDebugger.s_MatchedRulesExtractor.Traverse(this.m_SelectedElement.elementPanel.visualTree, 0, UIElementsDebugger.s_MatchedRulesExtractor.ruleMatchers);
				UIElementsDebugger.s_MatchedRulesExtractor.ruleMatchers.Clear();
			}
		}

		private static int GetSpecificity<T>(StyleValue<T> style)
		{
			return style.specificity;
		}

		private void DrawProperties()
		{
			EditorGUILayout.LabelField(UIElementsDebugger.Styles.elementStylesContent, UIElementsDebugger.Styles.KInspectorTitle, new GUILayoutOption[0]);
			this.m_SelectedElement.name = EditorGUILayout.TextField("Name", this.m_SelectedElement.name, new GUILayoutOption[0]);
			this.m_SelectedElement.text = EditorGUILayout.TextField("Text", this.m_SelectedElement.text, new GUILayoutOption[0]);
			this.m_SelectedElement.clippingOptions = (VisualElement.ClippingOptions)EditorGUILayout.EnumPopup("Clipping Option", this.m_SelectedElement.clippingOptions, new GUILayoutOption[0]);
			this.m_SelectedElement.visible = EditorGUILayout.Toggle("Visible", this.m_SelectedElement.visible, new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Layout", this.m_SelectedElement.layout.ToString(), new GUILayoutOption[0]);
			EditorGUILayout.LabelField("World Bound", this.m_SelectedElement.worldBound.ToString(), new GUILayoutOption[0]);
			if (this.m_ClassList == null)
			{
				this.InitClassList();
			}
			this.m_ClassList.DoLayoutList();
			GUILayout.BeginHorizontal(EditorStyles.toolbar, new GUILayoutOption[0]);
			this.m_DetailFilter = EditorGUILayout.ToolbarSearchField(this.m_DetailFilter, new GUILayoutOption[0]);
			this.m_ShowDefaults = GUILayout.Toggle(this.m_ShowDefaults, UIElementsDebugger.Styles.showDefaultsContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			this.m_Sort = GUILayout.Toggle(this.m_Sort, UIElementsDebugger.Styles.sortContent, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			VisualElementStylesData effectiveStyle = this.m_SelectedElement.effectiveStyle;
			bool flag = false;
			PropertyInfo[] array = (!this.m_Sort) ? UIElementsDebugger.k_FieldInfos : UIElementsDebugger.k_SortedFieldInfos;
			for (int i = 0; i < array.Length; i++)
			{
				PropertyInfo propertyInfo = array[i];
				if (string.IsNullOrEmpty(this.m_DetailFilter) || propertyInfo.Name.IndexOf(this.m_DetailFilter, StringComparison.InvariantCultureIgnoreCase) != -1)
				{
					if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(StyleValue<>))
					{
						object obj = propertyInfo.GetValue(this.m_SelectedElement, null);
						EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
						EditorGUI.BeginChangeCheck();
						int num;
						if (obj is StyleValue<float>)
						{
							StyleValue<float> styleValue = (StyleValue<float>)obj;
							num = UIElementsDebugger.GetSpecificity<float>(styleValue);
							if (this.m_ShowDefaults || num > 0)
							{
								styleValue.specificity = 2147483647;
								styleValue.value = EditorGUILayout.FloatField(propertyInfo.Name, ((StyleValue<float>)obj).value, new GUILayoutOption[0]);
								obj = styleValue;
							}
						}
						else if (obj is StyleValue<int>)
						{
							StyleValue<int> styleValue2 = (StyleValue<int>)obj;
							num = UIElementsDebugger.GetSpecificity<int>(styleValue2);
							if (this.m_ShowDefaults || num > 0)
							{
								styleValue2.specificity = 2147483647;
								styleValue2.value = EditorGUILayout.IntField(propertyInfo.Name, ((StyleValue<int>)obj).value, new GUILayoutOption[0]);
								obj = styleValue2;
							}
						}
						else if (obj is StyleValue<bool>)
						{
							StyleValue<bool> styleValue3 = (StyleValue<bool>)obj;
							num = UIElementsDebugger.GetSpecificity<bool>(styleValue3);
							if (this.m_ShowDefaults || num > 0)
							{
								styleValue3.specificity = 2147483647;
								styleValue3.value = EditorGUILayout.Toggle(propertyInfo.Name, ((StyleValue<bool>)obj).value, new GUILayoutOption[0]);
								obj = styleValue3;
							}
						}
						else if (obj is StyleValue<Color>)
						{
							StyleValue<Color> styleValue4 = (StyleValue<Color>)obj;
							num = UIElementsDebugger.GetSpecificity<Color>(styleValue4);
							if (this.m_ShowDefaults || num > 0)
							{
								styleValue4.specificity = 2147483647;
								styleValue4.value = EditorGUILayout.ColorField(propertyInfo.Name, ((StyleValue<Color>)obj).value, new GUILayoutOption[0]);
								obj = styleValue4;
							}
						}
						else if (obj is StyleValue<Font>)
						{
							num = this.HandleReferenceProperty<Font>(propertyInfo, ref obj);
						}
						else if (obj is StyleValue<Texture2D>)
						{
							num = this.HandleReferenceProperty<Texture2D>(propertyInfo, ref obj);
						}
						else
						{
							Type type = obj.GetType();
							if (type.GetGenericArguments()[0].IsEnum)
							{
								num = (int)type.GetField("specificity", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj);
								if (this.m_ShowDefaults || num > 0)
								{
									FieldInfo field = type.GetField("value");
									Enum @enum = field.GetValue(obj) as Enum;
									Enum enum2 = EditorGUILayout.EnumPopup(propertyInfo.Name, @enum, new GUILayoutOption[0]);
									if (!object.Equals(@enum, enum2))
									{
										field.SetValue(obj, enum2);
									}
								}
							}
							else
							{
								EditorGUILayout.LabelField(propertyInfo.Name, (obj != null) ? obj.ToString() : "null", new GUILayoutOption[0]);
								num = -1;
							}
						}
						if (EditorGUI.EndChangeCheck())
						{
							flag = true;
							propertyInfo.SetValue(this.m_SelectedElement, obj, null);
						}
						if (num > 0)
						{
							GUILayout.Label((num != 2147483647) ? num.ToString() : "inline", new GUILayoutOption[0]);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}
			if (flag)
			{
				this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Transform);
				this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Styles);
				this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Layout);
				this.m_CurPanel.Value.Panel.visualTree.Dirty(ChangeType.Repaint);
				this.m_CurPanel.Value.View.RepaintImmediately();
			}
		}

		private void InitClassList()
		{
			Action refresh = delegate
			{
				this.m_ClassList.list = this.m_SelectedElement.GetClasses().ToList<string>();
			};
			this.m_ClassList = new ReorderableList(this.m_SelectedElement.GetClasses().ToList<string>(), typeof(string), false, true, true, true);
			this.m_ClassList.onRemoveCallback = delegate(ReorderableList _)
			{
				this.m_SelectedElement.RemoveFromClassList((string)this.m_ClassList.list[this.m_ClassList.index]);
				refresh();
			};
			this.m_ClassList.drawHeaderCallback = delegate(Rect r)
			{
				r.width /= 2f;
				EditorGUI.LabelField(r, "Classes");
				r.x += r.width;
				this.m_NewClass = EditorGUI.TextField(r, this.m_NewClass);
			};
			this.m_ClassList.onCanAddCallback = ((ReorderableList _) => !string.IsNullOrEmpty(this.m_NewClass) && !this.m_SelectedElement.ClassListContains(this.m_NewClass));
			this.m_ClassList.onAddCallback = delegate(ReorderableList _)
			{
				this.m_SelectedElement.AddToClassList(this.m_NewClass);
				this.m_NewClass = "";
				refresh();
			};
		}

		private int HandleReferenceProperty<T>(PropertyInfo field, ref object val) where T : UnityEngine.Object
		{
			StyleValue<T> styleValue = (StyleValue<T>)val;
			int specificity = UIElementsDebugger.GetSpecificity<T>(styleValue);
			if (this.m_ShowDefaults || specificity > 0)
			{
				styleValue.specificity = 2147483647;
				styleValue.value = (EditorGUILayout.ObjectField(field.Name, ((StyleValue<T>)val).value, typeof(T), false, new GUILayoutOption[0]) as T);
				val = styleValue;
			}
			return specificity;
		}

		private void DrawMatchingRules()
		{
			if (UIElementsDebugger.s_MatchedRulesExtractor.selectedElementStylesheets != null && UIElementsDebugger.s_MatchedRulesExtractor.selectedElementStylesheets.Count > 0)
			{
				EditorGUILayout.LabelField(UIElementsDebugger.Styles.stylesheetsContent, UIElementsDebugger.Styles.KInspectorTitle, new GUILayoutOption[0]);
				foreach (string current in UIElementsDebugger.s_MatchedRulesExtractor.selectedElementStylesheets)
				{
					if (GUILayout.Button(current, new GUILayoutOption[0]))
					{
						InternalEditorUtility.OpenFileAtLineExternal(current, 0);
					}
				}
			}
			if (UIElementsDebugger.s_MatchedRulesExtractor.selectedElementRules != null && UIElementsDebugger.s_MatchedRulesExtractor.selectedElementRules.Count > 0)
			{
				EditorGUILayout.LabelField(UIElementsDebugger.Styles.selectorsContent, UIElementsDebugger.Styles.KInspectorTitle, new GUILayoutOption[0]);
				int num = 0;
				foreach (MatchedRulesExtractor.MatchedRule current2 in UIElementsDebugger.s_MatchedRulesExtractor.selectedElementRules)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < current2.ruleMatcher.complexSelector.selectors.Length; i++)
					{
						StyleSelector styleSelector = current2.ruleMatcher.complexSelector.selectors[i];
						StyleSelectorRelationship previousRelationship = styleSelector.previousRelationship;
						if (previousRelationship != StyleSelectorRelationship.Child)
						{
							if (previousRelationship == StyleSelectorRelationship.Descendent)
							{
								stringBuilder.Append(" > ");
							}
						}
						else
						{
							stringBuilder.Append(" ");
						}
						for (int j = 0; j < styleSelector.parts.Length; j++)
						{
							StyleSelectorPart styleSelectorPart = styleSelector.parts[j];
							switch (styleSelectorPart.type)
							{
							case StyleSelectorType.Class:
								stringBuilder.Append(".");
								break;
							case StyleSelectorType.PseudoClass:
							case StyleSelectorType.RecursivePseudoClass:
								stringBuilder.Append(":");
								break;
							case StyleSelectorType.ID:
								stringBuilder.Append("#");
								break;
							}
							stringBuilder.Append(styleSelectorPart.value);
						}
					}
					StyleProperty[] properties = current2.ruleMatcher.complexSelector.rule.properties;
					bool flag = this.m_CurFoldout.Contains(num);
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					bool flag2 = EditorGUILayout.Foldout(this.m_CurFoldout.Contains(num), new GUIContent(stringBuilder.ToString()), true);
					if (current2.displayPath != null && GUILayout.Button(current2.displayPath, EditorStyles.miniButton, new GUILayoutOption[]
					{
						GUILayout.MaxWidth(150f)
					}))
					{
						InternalEditorUtility.OpenFileAtLineExternal(current2.fullPath, current2.lineNumber);
					}
					EditorGUILayout.EndHorizontal();
					if (flag && !flag2)
					{
						this.m_CurFoldout.Remove(num);
					}
					else if (!flag && flag2)
					{
						this.m_CurFoldout.Add(num);
					}
					if (flag2)
					{
						EditorGUI.indentLevel++;
						for (int k = 0; k < properties.Length; k++)
						{
							string text = current2.ruleMatcher.sheet.ReadAsString(properties[k].values[0]);
							EditorGUILayout.LabelField(new GUIContent(properties[k].name), new GUIContent(text), new GUILayoutOption[0]);
						}
						EditorGUI.indentLevel--;
					}
					num++;
				}
			}
		}

		private void DrawSize(Rect rect, VisualElement element)
		{
			Rect rect2 = new Rect(rect);
			rect2.x += 35f;
			rect2.y += 35f;
			rect2.width -= 70f;
			rect2.height -= 70f;
			this.DrawRect(rect2, 3f, UIElementsDebugger.Styles.kSizeMarginPrimaryColor, UIElementsDebugger.Styles.kSizeMarginSecondaryColor);
			UIElementsDebugger.DrawSizeLabels(rect2, UIElementsDebugger.Styles.marginContent, element.style.marginTop, element.style.marginRight, element.style.marginBottom, element.style.marginLeft);
			rect2.x += 35f;
			rect2.y += 35f;
			rect2.width -= 70f;
			rect2.height -= 70f;
			this.DrawRect(rect2, 3f, UIElementsDebugger.Styles.kSizeBorderPrimaryColor, UIElementsDebugger.Styles.kSizeBorderSecondaryColor);
			UIElementsDebugger.DrawSizeLabels(rect2, UIElementsDebugger.Styles.borderContent, element.style.borderTop, element.style.borderRight, element.style.borderBottom, element.style.borderLeft);
			rect2.x += 35f;
			rect2.y += 35f;
			rect2.width -= 70f;
			rect2.height -= 70f;
			this.DrawRect(rect2, 3f, UIElementsDebugger.Styles.kSizePaddingPrimaryColor, UIElementsDebugger.Styles.kSizePaddingSecondaryColor);
			UIElementsDebugger.DrawSizeLabels(rect2, UIElementsDebugger.Styles.paddingContent, element.style.paddingTop, element.style.paddingRight, element.style.paddingBottom, element.style.paddingLeft);
			rect2.x += 35f;
			rect2.y += 35f;
			rect2.width -= 70f;
			rect2.height -= 70f;
			this.DrawRect(rect2, 3f, UIElementsDebugger.Styles.kSizePrimaryColor, UIElementsDebugger.Styles.kSizeSecondaryColor);
			EditorGUI.LabelField(rect2, string.Format("{0:F2} x {1:F2}", element.layout.width, element.layout.height), UIElementsDebugger.Styles.KSizeLabel);
		}

		private static void DrawSizeLabels(Rect cursor, GUIContent label, float top, float right, float bottom, float left)
		{
			Rect position = new Rect(cursor.x + (cursor.width - 50f) * 0.5f, cursor.y + 2f, 50f, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(position, top.ToString("F2"), UIElementsDebugger.Styles.KSizeLabel);
			position.y = cursor.y + cursor.height + 2f - 35f;
			EditorGUI.LabelField(position, bottom.ToString("F2"), UIElementsDebugger.Styles.KSizeLabel);
			position.x = cursor.x;
			position.y = cursor.y + (cursor.height - EditorGUIUtility.singleLineHeight) * 0.5f;
			EditorGUI.LabelField(position, left.ToString("F2"), UIElementsDebugger.Styles.KSizeLabel);
			position.x = cursor.x + cursor.width - 35f - 4f;
			EditorGUI.LabelField(position, right.ToString("F2"), UIElementsDebugger.Styles.KSizeLabel);
			position.x = cursor.x + 2f;
			position.y = cursor.y + 2f;
			position.width = 50f;
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, label, UIElementsDebugger.Styles.KSizeLabel);
		}

		public void OnEnable()
		{
			this.m_PickingData = new PickingData();
			base.titleContent = new GUIContent("UIElements Debugger");
			this.m_VisualTreeTreeViewState = new TreeViewState();
			this.m_VisualTreeTreeView = new VisualTreeTreeView(this.m_VisualTreeTreeViewState);
			if (this.m_SplitterState == null)
			{
				this.m_SplitterState = new SplitterState(new float[]
				{
					1f,
					2f
				});
			}
			this.m_TempTexture = new Texture2D(2, 2);
			if (!string.IsNullOrEmpty(this.m_LastWindowTitle))
			{
				this.m_ScheduleRestoreSelection = true;
			}
		}

		public void OnDisable()
		{
			Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
			while (panelsIterator.MoveNext())
			{
				KeyValuePair<int, Panel> current = panelsIterator.Current;
				current.Value.panelDebug = null;
			}
		}

		private void DrawRect(Rect rect, float borderSize, Color borderColor, Color fillingColor)
		{
			this.m_TempTexture.SetPixel(0, 0, fillingColor);
			this.m_TempTexture.SetPixel(1, 0, fillingColor);
			this.m_TempTexture.SetPixel(0, 1, fillingColor);
			this.m_TempTexture.SetPixel(1, 1, fillingColor);
			this.m_TempTexture.Apply();
			GUI.DrawTexture(rect, this.m_TempTexture);
			this.m_TempTexture.SetPixel(0, 0, borderColor);
			this.m_TempTexture.SetPixel(1, 0, borderColor);
			this.m_TempTexture.SetPixel(0, 1, borderColor);
			this.m_TempTexture.SetPixel(1, 1, borderColor);
			this.m_TempTexture.Apply();
			Rect position = new Rect(rect.x, rect.y, rect.width, borderSize);
			GUI.DrawTexture(position, this.m_TempTexture);
			position.y = rect.y + rect.height - borderSize;
			GUI.DrawTexture(position, this.m_TempTexture);
			position.width = borderSize;
			position.height = rect.height;
			position.y = rect.y;
			GUI.DrawTexture(position, this.m_TempTexture);
			position.x = rect.x + rect.width - borderSize;
			GUI.DrawTexture(position, this.m_TempTexture);
		}
	}
}
