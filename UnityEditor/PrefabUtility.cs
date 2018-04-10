using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.SceneManagement;
using UnityEditor.Utils;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace UnityEditor
{
	public sealed class PrefabUtility
	{
		public delegate void PrefabInstanceUpdated(GameObject instance);

		public static PrefabUtility.PrefabInstanceUpdated prefabInstanceUpdated;

		private const string kMaterialExtension = ".mat";

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern UnityEngine.Object GetPrefabParent(UnityEngine.Object source);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern UnityEngine.Object GetPrefabObject(UnityEngine.Object targetObject);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern PropertyModification[] GetPropertyModifications(UnityEngine.Object targetPrefab);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetPropertyModifications(UnityEngine.Object targetPrefab, PropertyModification[] modifications);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern UnityEngine.Object InstantiateAttachedAsset(UnityEngine.Object targetObject);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void RecordPrefabInstancePropertyModifications(UnityEngine.Object targetObject);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void MergeAllPrefabInstances(UnityEngine.Object targetObject);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void DisconnectPrefabInstance(UnityEngine.Object targetObject);

		public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object target)
		{
			return PrefabUtility.InternalInstantiatePrefab(target, EditorSceneManager.GetTargetSceneForNewGameObjects());
		}

		public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object target, Scene destinationScene)
		{
			return PrefabUtility.InternalInstantiatePrefab(target, destinationScene);
		}

		private static UnityEngine.Object InternalInstantiatePrefab(UnityEngine.Object target, Scene destinationScene)
		{
			return PrefabUtility.INTERNAL_CALL_InternalInstantiatePrefab(target, ref destinationScene);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern UnityEngine.Object INTERNAL_CALL_InternalInstantiatePrefab(UnityEngine.Object target, ref Scene destinationScene);

		public static UnityEngine.Object CreateEmptyPrefab(string path)
		{
			UnityEngine.Object result;
			if (!Paths.IsValidAssetPathWithErrorLogging(path, ".prefab"))
			{
				result = null;
			}
			else
			{
				result = PrefabUtility.Internal_CreateEmptyPrefab(path);
			}
			return result;
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern UnityEngine.Object Internal_CreateEmptyPrefab(string path);

		[ExcludeFromDocs]
		public static GameObject CreatePrefab(string path, GameObject go)
		{
			ReplacePrefabOptions options = ReplacePrefabOptions.Default;
			return PrefabUtility.CreatePrefab(path, go, options);
		}

		public static GameObject CreatePrefab(string path, GameObject go, [DefaultValue("ReplacePrefabOptions.Default")] ReplacePrefabOptions options)
		{
			GameObject result;
			if (!Paths.IsValidAssetPathWithErrorLogging(path, ".prefab"))
			{
				result = null;
			}
			else
			{
				result = PrefabUtility.Internal_CreatePrefab(path, go, options);
			}
			return result;
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern GameObject Internal_CreatePrefab(string path, GameObject go, [DefaultValue("ReplacePrefabOptions.Default")] ReplacePrefabOptions options);

		[ExcludeFromDocs]
		private static GameObject Internal_CreatePrefab(string path, GameObject go)
		{
			ReplacePrefabOptions options = ReplacePrefabOptions.Default;
			return PrefabUtility.Internal_CreatePrefab(path, go, options);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject ReplacePrefab(GameObject go, UnityEngine.Object targetPrefab, [DefaultValue("ReplacePrefabOptions.Default")] ReplacePrefabOptions options);

		[ExcludeFromDocs]
		public static GameObject ReplacePrefab(GameObject go, UnityEngine.Object targetPrefab)
		{
			ReplacePrefabOptions options = ReplacePrefabOptions.Default;
			return PrefabUtility.ReplacePrefab(go, targetPrefab, options);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject ConnectGameObjectToPrefab(GameObject go, GameObject sourcePrefab);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject FindRootGameObjectWithSameParentPrefab(GameObject target);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject FindValidUploadPrefabInstanceRoot(GameObject target);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool ReconnectToLastPrefab(GameObject go);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool ResetToPrefabState(UnityEngine.Object obj);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsComponentAddedToPrefabInstance(UnityEngine.Object source);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool RevertPrefabInstance(GameObject go);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern PrefabType GetPrefabType(UnityEngine.Object target);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern GameObject FindPrefabRoot(GameObject source);

		private static void Internal_CallPrefabInstanceUpdated(GameObject instance)
		{
			if (PrefabUtility.prefabInstanceUpdated != null)
			{
				PrefabUtility.prefabInstanceUpdated(instance);
			}
		}

		[RequiredByNativeCode]
		internal static void ExtractSelectedObjectsFromPrefab()
		{
			HashSet<string> hashSet = new HashSet<string>();
			string text = null;
			UnityEngine.Object[] objects = Selection.objects;
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				string assetPath = AssetDatabase.GetAssetPath(@object);
				if (text == null)
				{
					text = EditorUtility.SaveFolderPanel("Select Materials Folder", FileUtil.DeleteLastPathNameComponent(assetPath), "");
					if (string.IsNullOrEmpty(text))
					{
						return;
					}
					text = FileUtil.GetProjectRelativePath(text);
				}
				string str = (!(@object is Material)) ? string.Empty : ".mat";
				string text2 = FileUtil.CombinePaths(new string[]
				{
					text,
					@object.name
				}) + str;
				text2 = AssetDatabase.GenerateUniqueAssetPath(text2);
				string value = AssetDatabase.ExtractAsset(@object, text2);
				if (string.IsNullOrEmpty(value))
				{
					hashSet.Add(assetPath);
				}
			}
			foreach (string current in hashSet)
			{
				AssetDatabase.WriteImportSettingsIfDirty(current);
				AssetDatabase.ImportAsset(current, ImportAssetOptions.ForceUpdate);
			}
		}

		internal static void ExtractMaterialsFromAsset(UnityEngine.Object[] targets, string destinationPath)
		{
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < targets.Length; i++)
			{
				UnityEngine.Object @object = targets[i];
				ModelImporter modelImporter = @object as ModelImporter;
				IEnumerable<UnityEngine.Object> enumerable = from x in AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath)
				where x.GetType() == typeof(Material)
				select x;
				foreach (UnityEngine.Object current in enumerable)
				{
					string text = FileUtil.CombinePaths(new string[]
					{
						destinationPath,
						current.name
					}) + ".mat";
					text = AssetDatabase.GenerateUniqueAssetPath(text);
					string value = AssetDatabase.ExtractAsset(current, text);
					if (string.IsNullOrEmpty(value))
					{
						hashSet.Add(modelImporter.assetPath);
					}
				}
			}
			foreach (string current2 in hashSet)
			{
				AssetDatabase.WriteImportSettingsIfDirty(current2);
				AssetDatabase.ImportAsset(current2, ImportAssetOptions.ForceUpdate);
			}
		}

		private static void GetObjectListFromHierarchy(List<UnityEngine.Object> hierarchy, GameObject gameObject)
		{
			Transform transform = null;
			List<Component> list = new List<Component>();
			hierarchy.Add(gameObject);
			gameObject.GetComponents<Component>(list);
			foreach (Component current in list)
			{
				if (current is Transform)
				{
					transform = (current as Transform);
				}
				else
				{
					hierarchy.Add(current);
				}
			}
			if (transform != null)
			{
				int childCount = transform.childCount;
				for (int i = 0; i < childCount; i++)
				{
					PrefabUtility.GetObjectListFromHierarchy(hierarchy, transform.GetChild(i).gameObject);
				}
			}
		}

		private static void RegisterNewObjects(List<UnityEngine.Object> newHierarchy, List<UnityEngine.Object> hierarchy, string actionName)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			foreach (UnityEngine.Object current in newHierarchy)
			{
				bool flag = false;
				foreach (UnityEngine.Object current2 in hierarchy)
				{
					if (current2.GetInstanceID() == current.GetInstanceID())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					list.Add(current);
				}
			}
			HashSet<Type> hashSet = new HashSet<Type>
			{
				typeof(Transform)
			};
			bool flag2 = false;
			while (list.Count > 0 && !flag2)
			{
				flag2 = true;
				for (int i = 0; i < list.Count; i++)
				{
					UnityEngine.Object @object = list[i];
					object[] customAttributes = @object.GetType().GetCustomAttributes(typeof(RequireComponent), true);
					bool flag3 = true;
					object[] array = customAttributes;
					for (int j = 0; j < array.Length; j++)
					{
						RequireComponent requireComponent = (RequireComponent)array[j];
						if ((requireComponent.m_Type0 != null && !hashSet.Contains(requireComponent.m_Type0)) || (requireComponent.m_Type1 != null && !hashSet.Contains(requireComponent.m_Type1)) || (requireComponent.m_Type2 != null && !hashSet.Contains(requireComponent.m_Type2)))
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						Undo.RegisterCreatedObjectUndo(@object, actionName);
						hashSet.Add(@object.GetType());
						list.RemoveAt(i);
						i--;
						flag2 = false;
					}
				}
			}
			foreach (UnityEngine.Object current3 in list)
			{
				Undo.RegisterCreatedObjectUndo(current3, actionName);
			}
		}

		internal static void RevertPrefabInstanceWithUndo(GameObject target)
		{
			string text = "Revert Prefab Instance";
			PrefabType prefabType = PrefabUtility.GetPrefabType(target);
			bool flag = prefabType == PrefabType.DisconnectedModelPrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance;
			GameObject gameObject;
			if (flag)
			{
				gameObject = PrefabUtility.FindRootGameObjectWithSameParentPrefab(target);
			}
			else
			{
				gameObject = PrefabUtility.FindValidUploadPrefabInstanceRoot(target);
			}
			List<UnityEngine.Object> hierarchy = new List<UnityEngine.Object>();
			PrefabUtility.GetObjectListFromHierarchy(hierarchy, gameObject);
			Undo.RegisterFullObjectHierarchyUndo(gameObject, text);
			if (flag)
			{
				PrefabUtility.ReconnectToLastPrefab(gameObject);
				Undo.RegisterCreatedObjectUndo(PrefabUtility.GetPrefabObject(gameObject), text);
			}
			PrefabUtility.RevertPrefabInstance(gameObject);
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			PrefabUtility.GetObjectListFromHierarchy(list, PrefabUtility.FindPrefabRoot(gameObject));
			PrefabUtility.RegisterNewObjects(list, hierarchy, text);
		}

		internal static void ReplacePrefabWithUndo(GameObject target)
		{
			string text = "Apply instance to prefab";
			UnityEngine.Object prefabParent = PrefabUtility.GetPrefabParent(target);
			GameObject gameObject = PrefabUtility.FindValidUploadPrefabInstanceRoot(target);
			Undo.RegisterFullObjectHierarchyUndo(prefabParent, text);
			Undo.RegisterFullObjectHierarchyUndo(gameObject, text);
			Undo.RegisterCreatedObjectUndo(gameObject, text);
			List<UnityEngine.Object> hierarchy = new List<UnityEngine.Object>();
			PrefabUtility.GetObjectListFromHierarchy(hierarchy, prefabParent as GameObject);
			PrefabUtility.ReplacePrefab(gameObject, prefabParent, ReplacePrefabOptions.ConnectToPrefab);
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			PrefabUtility.GetObjectListFromHierarchy(list, prefabParent as GameObject);
			PrefabUtility.RegisterNewObjects(list, hierarchy, text);
		}
	}
}
