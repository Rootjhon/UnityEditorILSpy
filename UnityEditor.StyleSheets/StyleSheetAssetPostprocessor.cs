using System;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace UnityEditor.StyleSheets
{
	internal class StyleSheetAssetPostprocessor : AssetPostprocessor
	{
		private static HashSet<string> s_StyleSheetReferencedAssetPaths;

		public static void ClearReferencedAssets()
		{
			StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths = null;
		}

		public static void AddReferencedAssetPath(string assetPath)
		{
			if (StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths == null)
			{
				StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths = new HashSet<string>();
			}
			StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths.Add(assetPath);
		}

		private static void ProcessAssetPath(string assetPath)
		{
			if (StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths != null && StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths.Contains(assetPath))
			{
				StyleContext.ClearStyleCache();
				Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
				while (panelsIterator.MoveNext())
				{
					KeyValuePair<int, Panel> current = panelsIterator.Current;
					Panel value = current.Value;
					value.visualTree.Dirty(ChangeType.Styles | ChangeType.Repaint);
				}
			}
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (StyleSheetAssetPostprocessor.s_StyleSheetReferencedAssetPaths != null)
			{
				for (int i = 0; i < deletedAssets.Length; i++)
				{
					string assetPath = deletedAssets[i];
					StyleSheetAssetPostprocessor.ProcessAssetPath(assetPath);
				}
				for (int j = 0; j < movedAssets.Length; j++)
				{
					StyleSheetAssetPostprocessor.ProcessAssetPath(movedAssets[j]);
					StyleSheetAssetPostprocessor.ProcessAssetPath(movedFromAssetPaths[j]);
				}
			}
		}
	}
}
