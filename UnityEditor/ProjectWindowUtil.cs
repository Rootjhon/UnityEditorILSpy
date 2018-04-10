using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor.ProjectWindowCallback;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	public class ProjectWindowUtil
	{
		internal static int k_FavoritesStartInstanceID = 1000000000;

		internal static string k_DraggingFavoriteGenericData = "DraggingFavorite";

		internal static string k_IsFolderGenericData = "IsFolder";

		[CompilerGenerated]
		private static Func<string, UnityEngine.Object> <>f__mg$cache0;

		[MenuItem("Assets/Create/GUI Skin", false, 601)]
		public static void CreateNewGUISkin()
		{
			GUISkin gUISkin = ScriptableObject.CreateInstance<GUISkin>();
			GUISkin gUISkin2 = Resources.GetBuiltinResource(typeof(GUISkin), "GameSkin/GameSkin.guiskin") as GUISkin;
			if (gUISkin2)
			{
				EditorUtility.CopySerialized(gUISkin2, gUISkin);
			}
			else
			{
				UnityEngine.Debug.LogError("Internal error: unable to load builtin GUIskin");
			}
			ProjectWindowUtil.CreateAsset(gUISkin, "New GUISkin.guiskin");
		}

		internal static string GetActiveFolderPath()
		{
			ProjectBrowser projectBrowserIfExists = ProjectWindowUtil.GetProjectBrowserIfExists();
			string result;
			if (projectBrowserIfExists == null)
			{
				result = "Assets";
			}
			else
			{
				result = projectBrowserIfExists.GetActiveFolderPath();
			}
			return result;
		}

		internal static void EndNameEditAction(EndNameEditAction action, int instanceId, string pathName, string resourceFile)
		{
			pathName = AssetDatabase.GenerateUniqueAssetPath(pathName);
			if (action != null)
			{
				action.Action(instanceId, pathName, resourceFile);
				action.CleanUp();
			}
		}

		public static void CreateAsset(UnityEngine.Object asset, string pathName)
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), ScriptableObject.CreateInstance<DoCreateNewAsset>(), pathName, AssetPreview.GetMiniThumbnail(asset), null);
		}

		public static void CreateFolder()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateFolder>(), "New Folder", EditorGUIUtility.IconContent(EditorResourcesUtility.emptyFolderIconName).image as Texture2D, null);
		}

		public static void CreateScene()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateScene>(), "New Scene.unity", EditorGUIUtility.FindTexture("SceneAsset Icon"), null);
		}

		public static void CreatePrefab()
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreatePrefab>(), "New Prefab.prefab", EditorGUIUtility.IconContent("Prefab Icon").image as Texture2D, null);
		}

		internal static void CreateAssetWithContent(string filename, string content)
		{
			DoCreateAssetWithContent doCreateAssetWithContent = ScriptableObject.CreateInstance<DoCreateAssetWithContent>();
			doCreateAssetWithContent.filecontent = content;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, doCreateAssetWithContent, filename, null, null);
		}

		private static void CreateScriptAsset(string templatePath, string destName)
		{
			string fileName = Path.GetFileName(templatePath);
			if (fileName.ToLower().Contains("editortest") || fileName.ToLower().Contains("editmode"))
			{
				string text = AssetDatabase.GetUniquePathNameAtSelectedPath(destName);
				if (!text.ToLower().Contains("/editor/"))
				{
					text = text.Substring(0, text.Length - destName.Length - 1);
					string text2 = Path.Combine(text, "Editor");
					if (!Directory.Exists(text2))
					{
						AssetDatabase.CreateFolder(text, "Editor");
					}
					text = Path.Combine(text2, destName);
					text = text.Replace("\\", "/");
				}
				destName = text;
			}
			string extension = Path.GetExtension(destName);
			Texture2D icon;
			if (extension != null)
			{
				if (extension == ".js")
				{
					icon = (EditorGUIUtility.IconContent("js Script Icon").image as Texture2D);
					goto IL_1AF;
				}
				if (extension == ".cs")
				{
					icon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);
					goto IL_1AF;
				}
				if (extension == ".boo")
				{
					icon = (EditorGUIUtility.IconContent("boo Script Icon").image as Texture2D);
					goto IL_1AF;
				}
				if (extension == ".shader")
				{
					icon = (EditorGUIUtility.IconContent("Shader Icon").image as Texture2D);
					goto IL_1AF;
				}
				if (extension == ".asmdef")
				{
					icon = (EditorGUIUtility.IconContent("AssemblyDefinitionAsset Icon").image as Texture2D);
					goto IL_1AF;
				}
			}
			icon = (EditorGUIUtility.IconContent("TextAsset Icon").image as Texture2D);
			IL_1AF:
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateScriptAsset>(), destName, icon, templatePath);
		}

		public static void ShowCreatedAsset(UnityEngine.Object o)
		{
			Selection.activeObject = o;
			if (o)
			{
				ProjectWindowUtil.FrameObjectInProjectWindow(o.GetInstanceID());
			}
		}

		private static void CreateAnimatorController()
		{
			Texture2D icon = EditorGUIUtility.IconContent("AnimatorController Icon").image as Texture2D;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateAnimatorController>(), "New Animator Controller.controller", icon, null);
		}

		private static void CreateAudioMixer()
		{
			Texture2D icon = EditorGUIUtility.IconContent("AudioMixerController Icon").image as Texture2D;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateAudioMixer>(), "NewAudioMixer.mixer", icon, null);
		}

		private static void CreateSpritePolygon(int sides)
		{
			string str;
			switch (sides)
			{
			case 0:
				str = "Square";
				goto IL_8E;
			case 1:
			case 2:
			case 5:
				IL_29:
				if (sides == 42)
				{
					str = "Everythingon";
					goto IL_8E;
				}
				if (sides != 128)
				{
					str = "Polygon";
					goto IL_8E;
				}
				str = "Circle";
				goto IL_8E;
			case 3:
				str = "Triangle";
				goto IL_8E;
			case 4:
				str = "Diamond";
				goto IL_8E;
			case 6:
				str = "Hexagon";
				goto IL_8E;
			}
			goto IL_29;
			IL_8E:
			Texture2D icon = EditorGUIUtility.IconContent("Sprite Icon").image as Texture2D;
			DoCreateSpritePolygon doCreateSpritePolygon = ScriptableObject.CreateInstance<DoCreateSpritePolygon>();
			doCreateSpritePolygon.sides = sides;
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, doCreateSpritePolygon, str + ".png", icon, null);
		}

		internal static string SetLineEndings(string content, LineEndingsMode lineEndingsMode)
		{
			string replacement;
			switch (lineEndingsMode)
			{
			case LineEndingsMode.OSNative:
				if (Application.platform == RuntimePlatform.WindowsEditor)
				{
					replacement = "\r\n";
				}
				else
				{
					replacement = "\n";
				}
				break;
			case LineEndingsMode.Unix:
				replacement = "\n";
				break;
			case LineEndingsMode.Windows:
				replacement = "\r\n";
				break;
			default:
				replacement = "\n";
				break;
			}
			content = Regex.Replace(content, "\\r\\n?|\\n", replacement);
			return content;
		}

		internal static UnityEngine.Object CreateScriptAssetWithContent(string pathName, string templateContent)
		{
			templateContent = ProjectWindowUtil.SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
			string fullPath = Path.GetFullPath(pathName);
			UTF8Encoding encoding = new UTF8Encoding(true);
			File.WriteAllText(fullPath, templateContent, encoding);
			AssetDatabase.ImportAsset(pathName);
			return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
		}

		internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
		{
			string text = File.ReadAllText(resourceFile);
			text = text.Replace("#NOTRIM#", "");
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
			text = text.Replace("#NAME#", fileNameWithoutExtension);
			string text2 = fileNameWithoutExtension.Replace(" ", "");
			text = text.Replace("#SCRIPTNAME#", text2);
			if (char.IsUpper(text2, 0))
			{
				text2 = char.ToLower(text2[0]) + text2.Substring(1);
				text = text.Replace("#SCRIPTNAME_LOWER#", text2);
			}
			else
			{
				text2 = "my" + char.ToUpper(text2[0]) + text2.Substring(1);
				text = text.Replace("#SCRIPTNAME_LOWER#", text2);
			}
			return ProjectWindowUtil.CreateScriptAssetWithContent(pathName, text);
		}

		public static void StartNameEditingIfProjectWindowExists(int instanceID, EndNameEditAction endAction, string pathName, Texture2D icon, string resourceFile)
		{
			ProjectBrowser projectBrowserIfExists = ProjectWindowUtil.GetProjectBrowserIfExists();
			if (projectBrowserIfExists)
			{
				projectBrowserIfExists.Focus();
				projectBrowserIfExists.BeginPreimportedNameEditing(instanceID, endAction, pathName, icon, resourceFile);
				projectBrowserIfExists.Repaint();
			}
			else
			{
				if (!pathName.StartsWith("assets/", StringComparison.CurrentCultureIgnoreCase))
				{
					pathName = "Assets/" + pathName;
				}
				ProjectWindowUtil.EndNameEditAction(endAction, instanceID, pathName, resourceFile);
				Selection.activeObject = EditorUtility.InstanceIDToObject(instanceID);
			}
		}

		private static ProjectBrowser GetProjectBrowserIfExists()
		{
			return ProjectBrowser.s_LastInteractedProjectBrowser;
		}

		internal static void FrameObjectInProjectWindow(int instanceID)
		{
			ProjectBrowser projectBrowserIfExists = ProjectWindowUtil.GetProjectBrowserIfExists();
			if (projectBrowserIfExists)
			{
				projectBrowserIfExists.FrameObject(instanceID, false);
			}
		}

		internal static bool IsFavoritesItem(int instanceID)
		{
			return instanceID >= ProjectWindowUtil.k_FavoritesStartInstanceID;
		}

		internal static void StartDrag(int draggedInstanceID, List<int> selectedInstanceIDs)
		{
			DragAndDrop.PrepareStartDrag();
			string title = "";
			if (ProjectWindowUtil.IsFavoritesItem(draggedInstanceID))
			{
				DragAndDrop.SetGenericData(ProjectWindowUtil.k_DraggingFavoriteGenericData, draggedInstanceID);
			}
			else
			{
				bool flag = ProjectWindowUtil.IsFolder(draggedInstanceID);
				DragAndDrop.objectReferences = ProjectWindowUtil.GetDragAndDropObjects(draggedInstanceID, selectedInstanceIDs);
				DragAndDrop.SetGenericData(ProjectWindowUtil.k_IsFolderGenericData, (!flag) ? "" : "isFolder");
				string[] dragAndDropPaths = ProjectWindowUtil.GetDragAndDropPaths(draggedInstanceID, selectedInstanceIDs);
				if (dragAndDropPaths.Length > 0)
				{
					DragAndDrop.paths = dragAndDropPaths;
				}
				if (DragAndDrop.objectReferences.Length > 1)
				{
					title = "<Multiple>";
				}
				else
				{
					title = ObjectNames.GetDragAndDropTitle(InternalEditorUtility.GetObjectFromInstanceID(draggedInstanceID));
				}
			}
			DragAndDrop.StartDrag(title);
		}

		internal static UnityEngine.Object[] GetDragAndDropObjects(int draggedInstanceID, List<int> selectedInstanceIDs)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>(selectedInstanceIDs.Count);
			if (selectedInstanceIDs.Contains(draggedInstanceID))
			{
				for (int i = 0; i < selectedInstanceIDs.Count; i++)
				{
					UnityEngine.Object objectFromInstanceID = InternalEditorUtility.GetObjectFromInstanceID(selectedInstanceIDs[i]);
					if (objectFromInstanceID != null)
					{
						list.Add(objectFromInstanceID);
					}
				}
			}
			else
			{
				UnityEngine.Object objectFromInstanceID2 = InternalEditorUtility.GetObjectFromInstanceID(draggedInstanceID);
				if (objectFromInstanceID2 != null)
				{
					list.Add(objectFromInstanceID2);
				}
			}
			return list.ToArray();
		}

		internal static string[] GetDragAndDropPaths(int draggedInstanceID, List<int> selectedInstanceIDs)
		{
			List<string> list = new List<string>();
			foreach (int current in selectedInstanceIDs)
			{
				if (AssetDatabase.IsMainAsset(current))
				{
					string assetPath = AssetDatabase.GetAssetPath(current);
					list.Add(assetPath);
				}
			}
			string assetPath2 = AssetDatabase.GetAssetPath(draggedInstanceID);
			string[] result;
			if (!string.IsNullOrEmpty(assetPath2))
			{
				if (list.Contains(assetPath2))
				{
					result = list.ToArray();
				}
				else
				{
					result = new string[]
					{
						assetPath2
					};
				}
			}
			else
			{
				result = new string[0];
			}
			return result;
		}

		public static int[] GetAncestors(int instanceID)
		{
			List<int> list = new List<int>();
			int mainAssetInstanceID = AssetDatabase.GetMainAssetInstanceID(AssetDatabase.GetAssetPath(instanceID));
			bool flag = mainAssetInstanceID != instanceID;
			if (flag)
			{
				list.Add(mainAssetInstanceID);
			}
			string containingFolder = ProjectWindowUtil.GetContainingFolder(AssetDatabase.GetAssetPath(mainAssetInstanceID));
			while (!string.IsNullOrEmpty(containingFolder))
			{
				int mainAssetInstanceID2 = AssetDatabase.GetMainAssetInstanceID(containingFolder);
				list.Add(mainAssetInstanceID2);
				containingFolder = ProjectWindowUtil.GetContainingFolder(AssetDatabase.GetAssetPath(mainAssetInstanceID2));
			}
			return list.ToArray();
		}

		public static bool IsFolder(int instanceID)
		{
			return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(instanceID));
		}

		public static string GetContainingFolder(string path)
		{
			string result;
			if (string.IsNullOrEmpty(path))
			{
				result = null;
			}
			else
			{
				path = path.Trim(new char[]
				{
					'/'
				});
				int num = path.LastIndexOf("/", StringComparison.Ordinal);
				if (num != -1)
				{
					result = path.Substring(0, num);
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		public static string[] GetBaseFolders(string[] folders)
		{
			string[] result;
			if (folders.Length <= 1)
			{
				result = folders;
			}
			else
			{
				List<string> list = new List<string>();
				List<string> list2 = new List<string>(folders);
				for (int i = 0; i < list2.Count; i++)
				{
					list2[i] = list2[i].Trim(new char[]
					{
						'/'
					});
				}
				list2.Sort();
				for (int j = 0; j < list2.Count; j++)
				{
					if (!list2[j].EndsWith("/"))
					{
						list2[j] += "/";
					}
				}
				string text = list2[0];
				list.Add(text);
				for (int k = 1; k < list2.Count; k++)
				{
					if (list2[k].IndexOf(text, StringComparison.Ordinal) != 0)
					{
						list.Add(list2[k]);
						text = list2[k];
					}
				}
				for (int l = 0; l < list.Count; l++)
				{
					list[l] = list[l].Trim(new char[]
					{
						'/'
					});
				}
				result = list.ToArray();
			}
			return result;
		}

		internal static void DuplicateSelectedAssets()
		{
			AssetDatabase.Refresh();
			List<UnityEngine.Object> list = (from o in Selection.objects
			where o is AnimationClip && AssetDatabase.Contains(o)
			select o).ToList<UnityEngine.Object>();
			IEnumerable<UnityEngine.Object> source;
			if (list.Count > 0)
			{
				source = ProjectWindowUtil.DuplicateAnimationClips(list.Cast<AnimationClip>()).Cast<UnityEngine.Object>();
			}
			else
			{
				IEnumerable<UnityEngine.Object> source2 = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets).Except(list);
				source = ProjectWindowUtil.DuplicateAssets(from o in source2
				select o.GetInstanceID());
			}
			Selection.objects = source.ToArray<UnityEngine.Object>();
		}

		internal static bool DeleteAssets(List<int> instanceIDs, bool askIfSure)
		{
			bool result;
			if (instanceIDs.Count == 0)
			{
				result = true;
			}
			else
			{
				bool flag = instanceIDs.IndexOf(ProjectBrowserColumnOneTreeViewDataSource.GetAssetsFolderInstanceID()) >= 0;
				if (flag)
				{
					string title = "Cannot Delete";
					EditorUtility.DisplayDialog(title, "Deleting the 'Assets' folder is not allowed", "Ok");
					result = false;
				}
				else
				{
					List<string> list = ProjectWindowUtil.GetMainPathsOfAssets(instanceIDs).ToList<string>();
					if (list.Count == 0)
					{
						result = false;
					}
					else
					{
						if (askIfSure)
						{
							string text = "Delete selected asset";
							if (list.Count > 1)
							{
								text += "s";
							}
							text += "?";
							int num = 3;
							string text2 = "";
							int num2 = 0;
							while (num2 < list.Count && num2 < num)
							{
								text2 = text2 + "   " + list[num2] + "\n";
								num2++;
							}
							if (list.Count > num)
							{
								text2 += "   ...\n";
							}
							text2 += "\nYou cannot undo this action.";
							if (!EditorUtility.DisplayDialog(text, text2, "Delete", "Cancel"))
							{
								result = false;
								return result;
							}
						}
						bool flag2 = true;
						AssetDatabase.StartAssetEditing();
						foreach (string current in list)
						{
							if (!AssetDatabase.MoveAssetToTrash(current))
							{
								flag2 = false;
							}
						}
						AssetDatabase.StopAssetEditing();
						result = flag2;
					}
				}
			}
			return result;
		}

		internal static IEnumerable<AnimationClip> DuplicateAnimationClips(IEnumerable<AnimationClip> clips)
		{
			AssetDatabase.Refresh();
			List<string> list = new List<string>();
			foreach (AnimationClip current in clips)
			{
				if (current != null)
				{
					string path = AssetDatabase.GetAssetPath(current);
					path = Path.Combine(Path.GetDirectoryName(path), current.name) + ".anim";
					string text = AssetDatabase.GenerateUniqueAssetPath(path);
					AnimationClip animationClip = new AnimationClip();
					EditorUtility.CopySerialized(current, animationClip);
					AssetDatabase.CreateAsset(animationClip, text);
					list.Add(text);
				}
			}
			AssetDatabase.Refresh();
			return from s in list
			select AssetDatabase.LoadMainAssetAtPath(s) as AnimationClip;
		}

		internal static IEnumerable<UnityEngine.Object> DuplicateAssets(IEnumerable<string> assets)
		{
			AssetDatabase.Refresh();
			List<string> list = new List<string>();
			foreach (string current in assets)
			{
				string text = AssetDatabase.GenerateUniqueAssetPath(current);
				if (text.Length != 0 && AssetDatabase.CopyAsset(current, text))
				{
					list.Add(text);
				}
			}
			AssetDatabase.Refresh();
			IEnumerable<string> arg_87_0 = list;
			if (ProjectWindowUtil.<>f__mg$cache0 == null)
			{
				ProjectWindowUtil.<>f__mg$cache0 = new Func<string, UnityEngine.Object>(AssetDatabase.LoadMainAssetAtPath);
			}
			return arg_87_0.Select(ProjectWindowUtil.<>f__mg$cache0);
		}

		internal static IEnumerable<UnityEngine.Object> DuplicateAssets(IEnumerable<int> instanceIDs)
		{
			return ProjectWindowUtil.DuplicateAssets(ProjectWindowUtil.GetMainPathsOfAssets(instanceIDs));
		}

		[DebuggerHidden]
		internal static IEnumerable<string> GetMainPathsOfAssets(IEnumerable<int> instanceIDs)
		{
			ProjectWindowUtil.<GetMainPathsOfAssets>c__Iterator0 <GetMainPathsOfAssets>c__Iterator = new ProjectWindowUtil.<GetMainPathsOfAssets>c__Iterator0();
			<GetMainPathsOfAssets>c__Iterator.instanceIDs = instanceIDs;
			ProjectWindowUtil.<GetMainPathsOfAssets>c__Iterator0 expr_0E = <GetMainPathsOfAssets>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}
	}
}
