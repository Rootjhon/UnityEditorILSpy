using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Compilation;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(AssemblyDefinitionImporter))]
	internal class AssemblyDefinitionImporterInspector : AssetImporterEditor
	{
		internal class Styles
		{
			public static readonly GUIContent name = EditorGUIUtility.TrTextContent("Name");

			public static readonly GUIContent references = EditorGUIUtility.TrTextContent("References");

			public static readonly GUIContent platforms = EditorGUIUtility.TrTextContent("Platforms");

			public static readonly GUIContent anyPlatform = EditorGUIUtility.TrTextContent("Any Platform");

			public static readonly GUIContent includePlatforms = EditorGUIUtility.TrTextContent("Include Platforms");

			public static readonly GUIContent excludePlatforms = EditorGUIUtility.TrTextContent("Exclude Platforms");

			public static readonly GUIContent selectAll = EditorGUIUtility.TrTextContent("Select all");

			public static readonly GUIContent deselectAll = EditorGUIUtility.TrTextContent("Deselect all");

			public static readonly GUIContent apply = EditorGUIUtility.TrTextContent("Apply");

			public static readonly GUIContent revert = EditorGUIUtility.TrTextContent("Revert");

			public static readonly GUIContent loadError = EditorGUIUtility.TrTextContent("Load error");
		}

		internal enum MixedBool
		{
			Mixed = -1,
			False,
			True
		}

		internal class AssemblyDefinitionReference
		{
			public AssemblyDefinitionAsset asset;

			public CustomScriptAssemblyData data;

			public AssemblyDefinitionImporterInspector.MixedBool displayValue;

			public string path
			{
				get
				{
					return AssetDatabase.GetAssetPath(this.asset);
				}
			}
		}

		internal class AssemblyDefintionState
		{
			public AssemblyDefinitionAsset asset;

			public string name;

			public List<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference> references;

			public AssemblyDefinitionImporterInspector.MixedBool compatibleWithAnyPlatform;

			public AssemblyDefinitionImporterInspector.MixedBool[] platformCompatibility;

			public bool modified;

			public string path
			{
				get
				{
					return AssetDatabase.GetAssetPath(this.asset);
				}
			}
		}

		private GUIStyle m_TextStyle;

		private AssemblyDefinitionImporterInspector.AssemblyDefintionState[] m_TargetStates;

		private AssemblyDefinitionImporterInspector.AssemblyDefintionState m_State;

		private ReorderableList m_ReferencesList;

		public override bool showImportedObject
		{
			get
			{
				return false;
			}
		}

		public override void OnInspectorGUI()
		{
			if (this.m_State == null)
			{
				try
				{
					this.LoadAssemblyDefinitionFiles();
				}
				catch (Exception e)
				{
					this.ShowLoadErrorExceptionGUI(e);
					return;
				}
			}
			AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUI.BeginChangeCheck();
				if (base.targets.Length > 1)
				{
					using (new EditorGUI.DisabledScope(true))
					{
						string text = string.Join(", ", (from t in this.m_TargetStates
						select t.name).ToArray<string>());
						EditorGUILayout.TextField(AssemblyDefinitionImporterInspector.Styles.name, text, EditorStyles.textField, new GUILayoutOption[0]);
					}
				}
				else
				{
					this.m_State.name = EditorGUILayout.TextField(AssemblyDefinitionImporterInspector.Styles.name, this.m_State.name, EditorStyles.textField, new GUILayoutOption[0]);
				}
				GUILayout.Label(AssemblyDefinitionImporterInspector.Styles.references, EditorStyles.boldLabel, new GUILayoutOption[0]);
				this.m_ReferencesList.DoLayoutList();
				GUILayout.Label(AssemblyDefinitionImporterInspector.Styles.platforms, EditorStyles.boldLabel, new GUILayoutOption[0]);
				EditorGUILayout.BeginVertical(GUI.skin.box, new GUILayoutOption[0]);
				AssemblyDefinitionImporterInspector.MixedBool compatibleWithAnyPlatform = this.m_State.compatibleWithAnyPlatform;
				this.m_State.compatibleWithAnyPlatform = AssemblyDefinitionImporterInspector.ToggleWithMixedValue(AssemblyDefinitionImporterInspector.Styles.anyPlatform, this.m_State.compatibleWithAnyPlatform);
				if (compatibleWithAnyPlatform == AssemblyDefinitionImporterInspector.MixedBool.Mixed && this.m_State.compatibleWithAnyPlatform != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
				{
					AssemblyDefinitionImporterInspector.UpdatePlatformCompatibility(this.m_State.compatibleWithAnyPlatform, this.m_TargetStates);
					this.UpdateCombinedCompatibility();
				}
				else if (this.m_State.compatibleWithAnyPlatform != compatibleWithAnyPlatform)
				{
					AssemblyDefinitionImporterInspector.InversePlatformCompatibility(this.m_State);
				}
				if (this.m_State.compatibleWithAnyPlatform != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
				{
					GUILayout.Label((this.m_State.compatibleWithAnyPlatform != AssemblyDefinitionImporterInspector.MixedBool.True) ? AssemblyDefinitionImporterInspector.Styles.includePlatforms : AssemblyDefinitionImporterInspector.Styles.excludePlatforms, EditorStyles.boldLabel, new GUILayoutOption[0]);
					for (int i = 0; i < assemblyDefinitionPlatforms.Length; i++)
					{
						this.m_State.platformCompatibility[i] = AssemblyDefinitionImporterInspector.ToggleWithMixedValue(new GUIContent(assemblyDefinitionPlatforms[i].DisplayName), this.m_State.platformCompatibility[i]);
					}
					EditorGUILayout.Space();
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					if (GUILayout.Button(AssemblyDefinitionImporterInspector.Styles.selectAll, new GUILayoutOption[0]))
					{
						AssemblyDefinitionImporterInspector.SetPlatformCompatibility(this.m_State, AssemblyDefinitionImporterInspector.MixedBool.True);
					}
					if (GUILayout.Button(AssemblyDefinitionImporterInspector.Styles.deselectAll, new GUILayoutOption[0]))
					{
						AssemblyDefinitionImporterInspector.SetPlatformCompatibility(this.m_State, AssemblyDefinitionImporterInspector.MixedBool.False);
					}
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
				GUILayout.Space(10f);
				if (EditorGUI.EndChangeCheck())
				{
					this.m_State.modified = true;
				}
			}
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			using (new EditorGUI.DisabledScope(!this.m_State.modified))
			{
				if (GUILayout.Button(AssemblyDefinitionImporterInspector.Styles.revert, new GUILayoutOption[0]))
				{
					this.LoadAssemblyDefinitionFiles();
				}
				if (GUILayout.Button(AssemblyDefinitionImporterInspector.Styles.apply, new GUILayoutOption[0]))
				{
					AssemblyDefinitionImporterInspector.SaveAndUpdateAssemblyDefinitionStates(this.m_State, this.m_TargetStates);
				}
			}
			GUILayout.EndHorizontal();
		}

		public override void OnDisable()
		{
			if (this.m_State != null && this.m_State.modified)
			{
				AssetImporter assetImporter = base.target as AssetImporter;
				string message = "Unapplied import settings for '" + assetImporter.assetPath + "'";
				if (base.targets.Length > 1)
				{
					message = "Unapplied import settings for '" + base.targets.Length + "' files";
				}
				if (EditorUtility.DisplayDialog("Unapplied import settings", message, "Apply", "Revert"))
				{
					AssemblyDefinitionImporterInspector.SaveAndUpdateAssemblyDefinitionStates(this.m_State, this.m_TargetStates);
				}
			}
		}

		private static void UpdatePlatformCompatibility(AssemblyDefinitionImporterInspector.MixedBool compatibleWithAnyPlatform, AssemblyDefinitionImporterInspector.AssemblyDefintionState[] states)
		{
			if (compatibleWithAnyPlatform == AssemblyDefinitionImporterInspector.MixedBool.Mixed)
			{
				throw new ArgumentOutOfRangeException("compatibleWithAnyPlatform");
			}
			for (int i = 0; i < states.Length; i++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = states[i];
				if (assemblyDefintionState.compatibleWithAnyPlatform != compatibleWithAnyPlatform)
				{
					assemblyDefintionState.compatibleWithAnyPlatform = compatibleWithAnyPlatform;
					AssemblyDefinitionImporterInspector.InversePlatformCompatibility(assemblyDefintionState);
				}
			}
		}

		private static AssemblyDefinitionImporterInspector.MixedBool ToggleWithMixedValue(GUIContent title, AssemblyDefinitionImporterInspector.MixedBool value)
		{
			EditorGUI.showMixedValue = (value == AssemblyDefinitionImporterInspector.MixedBool.Mixed);
			EditorGUI.BeginChangeCheck();
			bool flag = EditorGUILayout.Toggle(title, value == AssemblyDefinitionImporterInspector.MixedBool.True, new GUILayoutOption[0]);
			AssemblyDefinitionImporterInspector.MixedBool result;
			if (EditorGUI.EndChangeCheck())
			{
				result = ((!flag) ? AssemblyDefinitionImporterInspector.MixedBool.False : AssemblyDefinitionImporterInspector.MixedBool.True);
			}
			else
			{
				EditorGUI.showMixedValue = false;
				result = value;
			}
			return result;
		}

		private static void InversePlatformCompatibility(AssemblyDefinitionImporterInspector.AssemblyDefintionState state)
		{
			AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
			for (int i = 0; i < assemblyDefinitionPlatforms.Length; i++)
			{
				state.platformCompatibility[i] = AssemblyDefinitionImporterInspector.InverseCompability(state.platformCompatibility[i]);
			}
		}

		private static void SetPlatformCompatibility(AssemblyDefinitionImporterInspector.AssemblyDefintionState state, AssemblyDefinitionImporterInspector.MixedBool compatibility)
		{
			AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
			for (int i = 0; i < assemblyDefinitionPlatforms.Length; i++)
			{
				state.platformCompatibility[i] = compatibility;
			}
		}

		private static AssemblyDefinitionImporterInspector.MixedBool InverseCompability(AssemblyDefinitionImporterInspector.MixedBool compatibility)
		{
			AssemblyDefinitionImporterInspector.MixedBool result;
			if (compatibility == AssemblyDefinitionImporterInspector.MixedBool.True)
			{
				result = AssemblyDefinitionImporterInspector.MixedBool.False;
			}
			else if (compatibility == AssemblyDefinitionImporterInspector.MixedBool.False)
			{
				result = AssemblyDefinitionImporterInspector.MixedBool.True;
			}
			else
			{
				result = AssemblyDefinitionImporterInspector.MixedBool.Mixed;
			}
			return result;
		}

		private void ShowLoadErrorExceptionGUI(Exception e)
		{
			if (this.m_TextStyle == null)
			{
				this.m_TextStyle = "ScriptText";
			}
			GUILayout.Label(AssemblyDefinitionImporterInspector.Styles.loadError, EditorStyles.boldLabel, new GUILayoutOption[0]);
			Rect rect = GUILayoutUtility.GetRect(EditorGUIUtility.TempContent(e.Message), this.m_TextStyle);
			EditorGUI.HelpBox(rect, e.Message, MessageType.Error);
		}

		private void LoadAssemblyDefinitionFiles()
		{
			this.m_TargetStates = new AssemblyDefinitionImporterInspector.AssemblyDefintionState[base.targets.Length];
			for (int i = 0; i < base.targets.Length; i++)
			{
				AssetImporter assetImporter = base.targets[i] as AssetImporter;
				if (!(assetImporter == null))
				{
					this.m_TargetStates[i] = AssemblyDefinitionImporterInspector.LoadAssemblyDefintionState(assetImporter.assetPath);
				}
			}
			int num = this.m_TargetStates.Min((AssemblyDefinitionImporterInspector.AssemblyDefintionState t) => t.references.Count<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference>());
			this.m_State = new AssemblyDefinitionImporterInspector.AssemblyDefintionState();
			this.m_State.name = this.m_TargetStates[0].name;
			this.m_State.references = new List<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference>();
			this.m_State.modified = this.m_TargetStates[0].modified;
			for (int j = 0; j < num; j++)
			{
				this.m_State.references.Add(this.m_TargetStates[0].references[j]);
			}
			for (int k = 1; k < this.m_TargetStates.Length; k++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = this.m_TargetStates[k];
				for (int l = 0; l < num; l++)
				{
					if (this.m_State.references[l].displayValue != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
					{
						if (this.m_State.references[l].path != assemblyDefintionState.references[l].path)
						{
							this.m_State.references[l].displayValue = AssemblyDefinitionImporterInspector.MixedBool.Mixed;
						}
					}
				}
				this.m_State.modified |= assemblyDefintionState.modified;
			}
			this.UpdateCombinedCompatibility();
			this.m_ReferencesList = new ReorderableList(this.m_State.references, typeof(AssemblyDefinitionImporterInspector.AssemblyDefinitionReference), false, false, true, true);
			this.m_ReferencesList.drawElementCallback = new ReorderableList.ElementCallbackDelegate(this.DrawReferenceListElement);
			this.m_ReferencesList.onAddCallback = new ReorderableList.AddCallbackDelegate(this.AddReferenceListElement);
			this.m_ReferencesList.onRemoveCallback = new ReorderableList.RemoveCallbackDelegate(this.RemoveReferenceListElement);
			this.m_ReferencesList.elementHeight = EditorGUIUtility.singleLineHeight + 2f;
			this.m_ReferencesList.headerHeight = 3f;
		}

		private void UpdateCombinedCompatibility()
		{
			this.m_State.compatibleWithAnyPlatform = this.m_TargetStates[0].compatibleWithAnyPlatform;
			AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
			this.m_State.platformCompatibility = new AssemblyDefinitionImporterInspector.MixedBool[assemblyDefinitionPlatforms.Length];
			Array.Copy(this.m_TargetStates[0].platformCompatibility, this.m_State.platformCompatibility, assemblyDefinitionPlatforms.Length);
			for (int i = 1; i < this.m_TargetStates.Length; i++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = this.m_TargetStates[i];
				if (this.m_State.compatibleWithAnyPlatform != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
				{
					if (this.m_State.compatibleWithAnyPlatform != assemblyDefintionState.compatibleWithAnyPlatform)
					{
						this.m_State.compatibleWithAnyPlatform = AssemblyDefinitionImporterInspector.MixedBool.Mixed;
					}
				}
				for (int j = 0; j < assemblyDefinitionPlatforms.Length; j++)
				{
					if (this.m_State.platformCompatibility[j] != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
					{
						if (this.m_State.platformCompatibility[j] != assemblyDefintionState.platformCompatibility[j])
						{
							this.m_State.platformCompatibility[j] = AssemblyDefinitionImporterInspector.MixedBool.Mixed;
						}
					}
				}
			}
		}

		private static AssemblyDefinitionImporterInspector.AssemblyDefintionState LoadAssemblyDefintionState(string path)
		{
			AssemblyDefinitionAsset assemblyDefinitionAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(path);
			AssemblyDefinitionImporterInspector.AssemblyDefintionState result;
			if (assemblyDefinitionAsset == null)
			{
				result = null;
			}
			else
			{
				CustomScriptAssemblyData customScriptAssemblyData = CustomScriptAssemblyData.FromJson(assemblyDefinitionAsset.text);
				if (customScriptAssemblyData == null)
				{
					result = null;
				}
				else
				{
					AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = new AssemblyDefinitionImporterInspector.AssemblyDefintionState();
					assemblyDefintionState.asset = assemblyDefinitionAsset;
					assemblyDefintionState.name = customScriptAssemblyData.name;
					assemblyDefintionState.references = new List<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference>();
					if (customScriptAssemblyData.references != null)
					{
						string[] references = customScriptAssemblyData.references;
						for (int i = 0; i < references.Length; i++)
						{
							string text = references[i];
							try
							{
								AssemblyDefinitionImporterInspector.AssemblyDefinitionReference assemblyDefinitionReference = new AssemblyDefinitionImporterInspector.AssemblyDefinitionReference();
								string assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(text);
								if (string.IsNullOrEmpty(assemblyDefinitionFilePathFromAssemblyName))
								{
									throw new AssemblyDefinitionException(string.Format("Could not find assembly reference '{0}'", text), new string[]
									{
										path
									});
								}
								assemblyDefinitionReference.asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyDefinitionFilePathFromAssemblyName);
								if (assemblyDefinitionReference.asset == null)
								{
									throw new AssemblyDefinitionException(string.Format("Reference assembly definition file '{0}' not found", assemblyDefinitionFilePathFromAssemblyName), new string[]
									{
										path
									});
								}
								assemblyDefinitionReference.data = CustomScriptAssemblyData.FromJson(assemblyDefinitionReference.asset.text);
								assemblyDefinitionReference.displayValue = AssemblyDefinitionImporterInspector.MixedBool.False;
								assemblyDefintionState.references.Add(assemblyDefinitionReference);
							}
							catch (AssemblyDefinitionException exception)
							{
								Debug.LogException(exception, assemblyDefinitionAsset);
								assemblyDefintionState.references.Add(new AssemblyDefinitionImporterInspector.AssemblyDefinitionReference());
								assemblyDefintionState.modified = true;
							}
						}
					}
					AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
					assemblyDefintionState.platformCompatibility = new AssemblyDefinitionImporterInspector.MixedBool[assemblyDefinitionPlatforms.Length];
					assemblyDefintionState.compatibleWithAnyPlatform = AssemblyDefinitionImporterInspector.MixedBool.True;
					string[] array = null;
					if (customScriptAssemblyData.includePlatforms != null && customScriptAssemblyData.includePlatforms.Length > 0)
					{
						assemblyDefintionState.compatibleWithAnyPlatform = AssemblyDefinitionImporterInspector.MixedBool.False;
						array = customScriptAssemblyData.includePlatforms;
					}
					else if (customScriptAssemblyData.excludePlatforms != null && customScriptAssemblyData.excludePlatforms.Length > 0)
					{
						assemblyDefintionState.compatibleWithAnyPlatform = AssemblyDefinitionImporterInspector.MixedBool.True;
						array = customScriptAssemblyData.excludePlatforms;
					}
					if (array != null)
					{
						string[] array2 = array;
						for (int j = 0; j < array2.Length; j++)
						{
							string name = array2[j];
							int platformIndex = AssemblyDefinitionImporterInspector.GetPlatformIndex(assemblyDefinitionPlatforms, name);
							assemblyDefintionState.platformCompatibility[platformIndex] = AssemblyDefinitionImporterInspector.MixedBool.True;
						}
					}
					result = assemblyDefintionState;
				}
			}
			return result;
		}

		private static AssemblyDefinitionImporterInspector.AssemblyDefinitionReference CreateAssemblyDefinitionReference(string assemblyName)
		{
			AssemblyDefinitionImporterInspector.AssemblyDefinitionReference assemblyDefinitionReference = new AssemblyDefinitionImporterInspector.AssemblyDefinitionReference();
			string assemblyDefinitionFilePathFromAssemblyName = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assemblyName);
			if (string.IsNullOrEmpty(assemblyDefinitionFilePathFromAssemblyName))
			{
				throw new Exception(string.Format("Could not get assembly definition filename for assembly '{0}'", assemblyName));
			}
			assemblyDefinitionReference.asset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(assemblyDefinitionFilePathFromAssemblyName);
			if (assemblyDefinitionReference.asset == null)
			{
				throw new FileNotFoundException(string.Format("Assembly definition file '{0}' not found", assemblyDefinitionReference.path), assemblyDefinitionReference.path);
			}
			assemblyDefinitionReference.data = CustomScriptAssemblyData.FromJson(assemblyDefinitionReference.asset.text);
			return assemblyDefinitionReference;
		}

		private static void SaveAndUpdateAssemblyDefinitionStates(AssemblyDefinitionImporterInspector.AssemblyDefintionState combinedState, AssemblyDefinitionImporterInspector.AssemblyDefintionState[] states)
		{
			int num = combinedState.references.Count<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference>();
			if (states.Length == 1)
			{
				states[0].name = combinedState.name;
			}
			for (int i = 0; i < states.Length; i++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = states[i];
				for (int j = 0; j < num; j++)
				{
					if (combinedState.references[j].displayValue != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
					{
						assemblyDefintionState.references[j] = combinedState.references[j];
					}
				}
				if (combinedState.compatibleWithAnyPlatform != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
				{
					assemblyDefintionState.compatibleWithAnyPlatform = combinedState.compatibleWithAnyPlatform;
				}
				for (int k = 0; k < combinedState.platformCompatibility.Length; k++)
				{
					if (combinedState.platformCompatibility[k] != AssemblyDefinitionImporterInspector.MixedBool.Mixed)
					{
						assemblyDefintionState.platformCompatibility[k] = combinedState.platformCompatibility[k];
					}
				}
				AssemblyDefinitionImporterInspector.SaveAssemblyDefinitionState(assemblyDefintionState);
			}
			combinedState.modified = false;
		}

		private static void SaveAssemblyDefinitionState(AssemblyDefinitionImporterInspector.AssemblyDefintionState state)
		{
			IEnumerable<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference> source = from r in state.references
			where r.asset != null
			select r;
			AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms = CompilationPipeline.GetAssemblyDefinitionPlatforms();
			CustomScriptAssemblyData customScriptAssemblyData = new CustomScriptAssemblyData();
			customScriptAssemblyData.name = state.name;
			customScriptAssemblyData.references = (from r in source
			select r.data.name).ToArray<string>();
			List<string> list = new List<string>();
			for (int i = 0; i < assemblyDefinitionPlatforms.Length; i++)
			{
				if (state.platformCompatibility[i] == AssemblyDefinitionImporterInspector.MixedBool.True)
				{
					list.Add(assemblyDefinitionPlatforms[i].Name);
				}
			}
			if (list.Any<string>())
			{
				if (state.compatibleWithAnyPlatform == AssemblyDefinitionImporterInspector.MixedBool.True)
				{
					customScriptAssemblyData.excludePlatforms = list.ToArray();
				}
				else
				{
					customScriptAssemblyData.includePlatforms = list.ToArray();
				}
			}
			string contents = CustomScriptAssemblyData.ToJson(customScriptAssemblyData);
			File.WriteAllText(state.path, contents);
			state.modified = false;
			AssetDatabase.ImportAsset(state.path);
		}

		private static int GetPlatformIndex(AssemblyDefinitionPlatform[] platforms, string name)
		{
			for (int i = 0; i < platforms.Length; i++)
			{
				if (string.Equals(platforms[i].Name, name, StringComparison.InvariantCultureIgnoreCase))
				{
					return i;
				}
			}
			throw new ArgumentException(string.Format("Unknown platform '{0}'", name), name);
		}

		private void DrawReferenceListElement(Rect rect, int index, bool selected, bool focused)
		{
			IList list = this.m_ReferencesList.list;
			AssemblyDefinitionImporterInspector.AssemblyDefinitionReference assemblyDefinitionReference = list[index] as AssemblyDefinitionImporterInspector.AssemblyDefinitionReference;
			rect.height -= 2f;
			string text = (assemblyDefinitionReference.data == null) ? "(Missing Reference)" : assemblyDefinitionReference.data.name;
			AssemblyDefinitionAsset asset = assemblyDefinitionReference.asset;
			bool flag = assemblyDefinitionReference.displayValue == AssemblyDefinitionImporterInspector.MixedBool.Mixed;
			EditorGUI.showMixedValue = flag;
			assemblyDefinitionReference.asset = (EditorGUI.ObjectField(rect, (!flag) ? text : "(Multiple Values)", asset, typeof(AssemblyDefinitionAsset), false) as AssemblyDefinitionAsset);
			EditorGUI.showMixedValue = false;
			if (asset != assemblyDefinitionReference.asset && assemblyDefinitionReference.asset != null)
			{
				assemblyDefinitionReference.data = CustomScriptAssemblyData.FromJson(assemblyDefinitionReference.asset.text);
				AssemblyDefinitionImporterInspector.AssemblyDefintionState[] targetStates = this.m_TargetStates;
				for (int i = 0; i < targetStates.Length; i++)
				{
					AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = targetStates[i];
					assemblyDefintionState.references[index] = assemblyDefinitionReference;
				}
			}
		}

		private void AddReferenceListElement(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoAddButton(list);
			AssemblyDefinitionImporterInspector.AssemblyDefintionState[] targetStates = this.m_TargetStates;
			for (int i = 0; i < targetStates.Length; i++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = targetStates[i];
				if (assemblyDefintionState.references.Count <= list.count)
				{
					int index = Math.Min(list.index, assemblyDefintionState.references.Count<AssemblyDefinitionImporterInspector.AssemblyDefinitionReference>());
					assemblyDefintionState.references.Insert(index, list.list[list.index] as AssemblyDefinitionImporterInspector.AssemblyDefinitionReference);
				}
			}
		}

		private void RemoveReferenceListElement(ReorderableList list)
		{
			AssemblyDefinitionImporterInspector.AssemblyDefintionState[] targetStates = this.m_TargetStates;
			for (int i = 0; i < targetStates.Length; i++)
			{
				AssemblyDefinitionImporterInspector.AssemblyDefintionState assemblyDefintionState = targetStates[i];
				assemblyDefintionState.references.RemoveAt(list.index);
			}
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
		}
	}
}
