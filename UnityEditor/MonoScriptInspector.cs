using System;
using UnityEditor.Compilation;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(MonoScript))]
	internal class MonoScriptInspector : TextAssetInspector
	{
		public override void OnInspectorGUI()
		{
			if (base.targets.Length == 1)
			{
				string assetPath = AssetDatabase.GetAssetPath(base.target);
				string assemblyNameFromScriptPath = CompilationPipeline.GetAssemblyNameFromScriptPath(assetPath);
				if (assemblyNameFromScriptPath != null)
				{
					GUILayout.Label("Assembly Information", EditorStyles.boldLabel, new GUILayoutOption[0]);
					EditorGUILayout.LabelField("Filename", assemblyNameFromScriptPath, new GUILayoutOption[0]);
					string assemblyDefinitionFilePathFromScriptPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath(assetPath);
					if (assemblyDefinitionFilePathFromScriptPath != null)
					{
						TextAsset obj = AssetDatabase.LoadAssetAtPath<TextAsset>(assemblyDefinitionFilePathFromScriptPath);
						using (new EditorGUI.DisabledScope(true))
						{
							EditorGUILayout.ObjectField("Definition File", obj, typeof(TextAsset), false, new GUILayoutOption[0]);
						}
					}
					EditorGUILayout.Space();
				}
			}
			base.OnInspectorGUI();
		}
	}
}
