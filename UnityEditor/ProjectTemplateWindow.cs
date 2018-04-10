using System;
using UnityEngine;

namespace UnityEditor
{
	internal class ProjectTemplateWindow : EditorWindow
	{
		private string m_Name;

		private string m_DisplayName;

		private string m_Description;

		private string m_Version;

		[MenuItem("internal:Project/Save As Template...")]
		internal static void SaveAsTemplate()
		{
			ProjectTemplateWindow window = EditorWindow.GetWindow<ProjectTemplateWindow>();
			window.Show();
		}

		private void OnEnable()
		{
			base.titleContent = new GUIContent("Save Template");
		}

		private void OnGUI()
		{
			this.m_Name = EditorGUILayout.TextField("Name:", this.m_Name, new GUILayoutOption[0]);
			this.m_DisplayName = EditorGUILayout.TextField("Display name:", this.m_DisplayName, new GUILayoutOption[0]);
			this.m_Description = EditorGUILayout.TextField("Description:", this.m_Description, new GUILayoutOption[0]);
			this.m_Version = EditorGUILayout.TextField("Version:", this.m_Version, new GUILayoutOption[0]);
			if (GUILayout.Button("Save As...", new GUILayoutOption[]
			{
				GUILayout.Width(100f)
			}))
			{
				string text = EditorUtility.SaveFolderPanel("Save template to folder", "", "");
				if (text.Length > 0)
				{
					AssetDatabase.SaveAssets();
					EditorUtility.SaveProjectAsTemplate(text, this.m_Name, this.m_DisplayName, this.m_Description, this.m_Version);
				}
			}
		}
	}
}
