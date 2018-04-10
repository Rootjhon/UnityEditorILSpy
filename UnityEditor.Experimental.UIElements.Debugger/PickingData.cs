using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class PickingData
	{
		private readonly List<UIElementsDebugger.ViewPanel> m_Panels;

		private GUIContent[] m_Labels;

		public Rect screenRect;

		private int m_Selected;

		internal UIElementsDebugger.ViewPanel? Selected
		{
			get
			{
				UIElementsDebugger.ViewPanel? result;
				if (this.m_Selected == 0 || this.m_Selected > this.m_Panels.Count)
				{
					result = null;
				}
				else
				{
					result = new UIElementsDebugger.ViewPanel?(this.m_Panels[this.m_Selected - 1]);
				}
				return result;
			}
		}

		public PickingData()
		{
			this.m_Panels = new List<UIElementsDebugger.ViewPanel>();
			this.Refresh();
		}

		internal bool Draw(ref UIElementsDebugger.ViewPanel? selectedPanel, Rect dataScreenRect)
		{
			bool result;
			foreach (UIElementsDebugger.ViewPanel current in this.m_Panels)
			{
				Rect screenPosition = current.View.screenPosition;
				screenPosition.x -= dataScreenRect.xMin;
				screenPosition.y -= dataScreenRect.yMin;
				if (GUI.Button(screenPosition, string.Format("{0}({1})", current.Panel.visualTree.name, current.View.GetInstanceID()), EditorStyles.miniButton))
				{
					selectedPanel = new UIElementsDebugger.ViewPanel?(current);
					result = true;
					return result;
				}
				PickingData.DrawRect(screenPosition, Color.white);
			}
			result = false;
			return result;
		}

		public static void DrawRect(Rect sp, Color c)
		{
			sp.xMin += 1f;
			sp.xMax -= 1f;
			sp.yMin += 1f;
			sp.yMax -= 1f;
			HandleUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.Begin(1);
			GL.Color(c);
			GL.Vertex3(sp.xMin, sp.yMin, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMax, sp.yMin, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMax, sp.yMin, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMax, sp.yMax, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMax, sp.yMax, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMin, sp.yMax, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMin, sp.yMax, 0f);
			GL.Color(c);
			GL.Vertex3(sp.xMin, sp.yMin, 0f);
			GL.End();
			GL.PopMatrix();
		}

		public void Refresh()
		{
			this.m_Panels.Clear();
			Dictionary<int, Panel>.Enumerator it = UIElementsUtility.GetPanelsIterator();
			List<GUIView> list = new List<GUIView>();
			GUIViewDebuggerHelper.GetViews(list);
			bool flag = false;
			Rect rect = new Rect(3.40282347E+38f, 3.40282347E+38f, 0f, 0f);
			while (it.MoveNext())
			{
				GUIView gUIView = list.FirstOrDefault(delegate(GUIView v)
				{
					int arg_19_0 = v.GetInstanceID();
					KeyValuePair<int, Panel> current2 = it.Current;
					return arg_19_0 == current2.Key;
				});
				if (!(gUIView == null))
				{
					List<UIElementsDebugger.ViewPanel> arg_A6_0 = this.m_Panels;
					UIElementsDebugger.ViewPanel item = default(UIElementsDebugger.ViewPanel);
					KeyValuePair<int, Panel> current = it.Current;
					item.Panel = current.Value;
					item.View = gUIView;
					arg_A6_0.Add(item);
					if (rect.xMin > gUIView.screenPosition.xMin)
					{
						rect.xMin = gUIView.screenPosition.xMin;
					}
					if (rect.yMin > gUIView.screenPosition.yMin)
					{
						rect.yMin = gUIView.screenPosition.yMin;
					}
					if (rect.xMax < gUIView.screenPosition.xMax || !flag)
					{
						rect.xMax = gUIView.screenPosition.xMax;
					}
					if (rect.yMax < gUIView.screenPosition.yMax || !flag)
					{
						rect.yMax = gUIView.screenPosition.yMax;
					}
					flag = true;
				}
			}
			this.m_Labels = new GUIContent[this.m_Panels.Count + 1];
			this.m_Labels[0] = new GUIContent("Select a panel");
			for (int i = 0; i < this.m_Panels.Count; i++)
			{
				this.m_Labels[i + 1] = new GUIContent(PickingData.GetName(this.m_Panels[i]));
			}
			this.screenRect = rect;
		}

		internal static string GetName(UIElementsDebugger.ViewPanel viewPanel)
		{
			HostView hostView = viewPanel.View as HostView;
			string name;
			if (hostView != null)
			{
				EditorWindow actualView = hostView.actualView;
				if (actualView != null)
				{
					if (!string.IsNullOrEmpty(actualView.name))
					{
						name = actualView.name;
						return name;
					}
					name = actualView.GetType().Name;
					return name;
				}
				else if (!string.IsNullOrEmpty(hostView.name))
				{
					name = hostView.name;
					return name;
				}
			}
			name = viewPanel.Panel.visualTree.name;
			return name;
		}

		public void DoSelectDropDown()
		{
			this.m_Selected = EditorGUILayout.Popup(this.m_Selected, this.m_Labels, EditorStyles.popup, new GUILayoutOption[0]);
		}

		public bool TryRestoreSelectedWindow(string lastWindowTitle)
		{
			bool result;
			for (int i = 0; i < this.m_Labels.Length; i++)
			{
				if (this.m_Labels[i].text == lastWindowTitle)
				{
					this.m_Selected = i;
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
	}
}
