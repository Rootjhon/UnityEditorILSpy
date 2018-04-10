using System;
using System.IO;
using UnityEngine;
using UnityEngine.StyleSheets;

namespace UnityEditor.StyleSheets
{
	internal class StyleSheetResourceUtil
	{
		internal static UnityEngine.Object LoadResource(string pathName, Type type)
		{
			return StyleSheetResourceUtil.LoadResource(pathName, type, GUIUtility.pixelsPerPoint > 1f);
		}

		internal static UnityEngine.Object LoadResource(string pathName, Type type, bool lookForRetinaAssets)
		{
			UnityEngine.Object @object = null;
			string text = string.Empty;
			lookForRetinaAssets &= (type == typeof(Texture2D));
			if (lookForRetinaAssets)
			{
				string extension = Path.GetExtension(pathName);
				string path = Path.GetFileNameWithoutExtension(pathName) + "@2x" + extension;
				text = Path.Combine(Path.GetDirectoryName(pathName), path);
				lookForRetinaAssets = !string.IsNullOrEmpty(text);
			}
			if (lookForRetinaAssets)
			{
				@object = EditorGUIUtility.Load(text);
			}
			if (@object == null)
			{
				@object = EditorGUIUtility.Load(pathName);
			}
			if (@object == null && lookForRetinaAssets)
			{
				@object = Resources.Load(text, type);
			}
			if (@object == null)
			{
				@object = Resources.Load(pathName, type);
			}
			if (@object == null && lookForRetinaAssets)
			{
				@object = AssetDatabase.LoadMainAssetAtPath(text);
			}
			if (@object == null)
			{
				@object = AssetDatabase.LoadMainAssetAtPath(pathName);
			}
			if (@object != null)
			{
				string assetPath = AssetDatabase.GetAssetPath(@object);
				if (type != typeof(StyleSheet) && !assetPath.StartsWith("Library"))
				{
					StyleSheetAssetPostprocessor.AddReferencedAssetPath(assetPath);
				}
			}
			return @object;
		}
	}
}
