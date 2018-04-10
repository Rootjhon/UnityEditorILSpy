using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Scripting;

namespace UnityEditor
{
	internal class RetainedMode : AssetPostprocessor
	{
		private static HashSet<UnityEngine.Object> s_TmpDirtySet;

		[CompilerGenerated]
		private static Action<IMGUIContainer> <>f__mg$cache0;

		[CompilerGenerated]
		private static Action<IMGUIContainer> <>f__mg$cache1;

		static RetainedMode()
		{
			RetainedMode.s_TmpDirtySet = new HashSet<UnityEngine.Object>();
			if (RetainedMode.<>f__mg$cache0 == null)
			{
				RetainedMode.<>f__mg$cache0 = new Action<IMGUIContainer>(RetainedMode.OnBeginContainer);
			}
			UIElementsUtility.s_BeginContainerCallback = RetainedMode.<>f__mg$cache0;
			if (RetainedMode.<>f__mg$cache1 == null)
			{
				RetainedMode.<>f__mg$cache1 = new Action<IMGUIContainer>(RetainedMode.OnEndContainer);
			}
			UIElementsUtility.s_EndContainerCallback = RetainedMode.<>f__mg$cache1;
		}

		private static void OnBeginContainer(IMGUIContainer c)
		{
			HandleUtility.BeginHandles();
		}

		private static void OnEndContainer(IMGUIContainer c)
		{
			HandleUtility.EndHandles();
		}

		[RequiredByNativeCode]
		private static void UpdateSchedulers()
		{
			try
			{
				RetainedMode.UpdateSchedulersInternal(RetainedMode.s_TmpDirtySet);
			}
			finally
			{
				RetainedMode.s_TmpDirtySet.Clear();
			}
		}

		private static void UpdateSchedulersInternal(HashSet<UnityEngine.Object> tmpDirtySet)
		{
			DataWatchService.sharedInstance.PollNativeData();
			Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
			while (panelsIterator.MoveNext())
			{
				KeyValuePair<int, Panel> current = panelsIterator.Current;
				Panel value = current.Value;
				if (value.contextType == ContextType.Editor)
				{
					IScheduler scheduler = value.scheduler;
					value.timerEventScheduler.UpdateScheduledEvents();
					if (value.visualTree.IsDirty(ChangeType.Repaint))
					{
						GUIView gUIView = value.ownerObject as GUIView;
						if (gUIView != null)
						{
							gUIView.Repaint();
						}
					}
				}
			}
		}

		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < importedAssets.Length; i++)
			{
				string text = importedAssets[i];
				if (text.EndsWith("uss"))
				{
					flag2 = true;
					RetainedMode.FlagStyleSheetChange();
				}
				else if (text.EndsWith("uxml"))
				{
					flag = true;
					UIElementsViewImporter.logger.FinishImport();
					StyleSheetCache.ClearCaches();
				}
				if (flag && flag2)
				{
					break;
				}
			}
		}

		public static void FlagStyleSheetChange()
		{
			StyleSheetCache.ClearCaches();
			Dictionary<int, Panel>.Enumerator panelsIterator = UIElementsUtility.GetPanelsIterator();
			while (panelsIterator.MoveNext())
			{
				KeyValuePair<int, Panel> current = panelsIterator.Current;
				Panel value = current.Value;
				if (value.contextType == ContextType.Editor)
				{
					value.styleContext.DirtyStyleSheets();
					value.visualTree.Dirty(ChangeType.Styles);
					GUIView gUIView = value.ownerObject as GUIView;
					if (gUIView != null)
					{
						gUIView.Repaint();
					}
				}
			}
		}
	}
}
