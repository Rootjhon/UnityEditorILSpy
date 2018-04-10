using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class PanelPickerWindow : EditorWindow
	{
		private Action<UIElementsDebugger.ViewPanel?> m_Callback;

		private PickingData m_Data;

		internal static PanelPickerWindow Show(PickingData data, Action<UIElementsDebugger.ViewPanel?> callback)
		{
			PanelPickerWindow panelPickerWindow = ScriptableObject.CreateInstance<PanelPickerWindow>();
			panelPickerWindow.m_Data = data;
			panelPickerWindow.m_Pos = data.screenRect;
			panelPickerWindow.m_Callback = callback;
			panelPickerWindow.ShowPopup();
			panelPickerWindow.Focus();
			return panelPickerWindow;
		}

		public void OnGUI()
		{
			UIElementsDebugger.ViewPanel? obj = null;
			if (this.m_Data.Draw(ref obj, this.m_Data.screenRect))
			{
				base.Close();
				this.m_Callback(obj);
			}
			else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
			{
				base.Close();
				this.m_Callback(null);
			}
		}
	}
}
