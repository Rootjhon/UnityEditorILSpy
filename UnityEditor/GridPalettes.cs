using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class GridPalettes : ScriptableSingleton<GridPalettes>
	{
		public class AssetProcessor : AssetPostprocessor
		{
			public override int GetPostprocessOrder()
			{
				return 1;
			}

			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
			{
				if (!GridPaintingState.savingPalette)
				{
					GridPalettes.CleanCache();
				}
			}
		}

		private static bool s_RefreshCache;

		[SerializeField]
		private List<GameObject> m_PalettesCache;

		public static List<GameObject> palettes
		{
			get
			{
				if (ScriptableSingleton<GridPalettes>.instance.m_PalettesCache == null || GridPalettes.s_RefreshCache)
				{
					ScriptableSingleton<GridPalettes>.instance.RefreshPalettesCache();
					GridPalettes.s_RefreshCache = false;
				}
				return ScriptableSingleton<GridPalettes>.instance.m_PalettesCache;
			}
		}

		private void RefreshPalettesCache()
		{
			if (ScriptableSingleton<GridPalettes>.instance.m_PalettesCache == null)
			{
				ScriptableSingleton<GridPalettes>.instance.m_PalettesCache = new List<GameObject>();
			}
			string[] array = AssetDatabase.FindAssets("t:GridPalette");
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string guid = array2[i];
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				GridPalette gridPalette = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GridPalette)) as GridPalette;
				if (gridPalette != null)
				{
					string assetPath2 = AssetDatabase.GetAssetPath(gridPalette);
					GameObject gameObject = AssetDatabase.LoadMainAssetAtPath(assetPath2) as GameObject;
					if (gameObject != null)
					{
						this.m_PalettesCache.Add(gameObject);
					}
				}
			}
			this.m_PalettesCache.Sort((GameObject x, GameObject y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase));
		}

		internal static void CleanCache()
		{
			ScriptableSingleton<GridPalettes>.instance.m_PalettesCache = null;
		}
	}
}
