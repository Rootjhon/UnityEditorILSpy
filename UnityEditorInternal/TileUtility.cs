using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditorInternal
{
	internal class TileUtility
	{
		[MenuItem("Assets/Create/Tile", priority = 357)]
		public static void CreateNewTile()
		{
			string message = string.Format("Save tile'{0}':", "tile");
			string text = EditorUtility.SaveFilePanelInProject("Save tile", "New Tile", "asset", message, ProjectWindowUtil.GetActiveFolderPath());
			if (!string.IsNullOrEmpty(text))
			{
				AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Tile>(), text);
			}
		}
	}
}
