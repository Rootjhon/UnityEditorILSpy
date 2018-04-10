using System;
using UnityEngine;

namespace UnityEditor.Collaboration
{
	internal class CollabProjectHook
	{
		public static void OnProjectWindowIconOverlay(Rect iconRect, string guid, bool isListMode)
		{
			CollabProjectHook.DrawProjectBrowserIconOverlay(iconRect, guid, isListMode);
		}

		public static void OnProjectBrowserNavPanelIconOverlay(Rect iconRect, string guid)
		{
			CollabProjectHook.DrawProjectBrowserIconOverlay(iconRect, guid, true);
		}

		private static void DrawProjectBrowserIconOverlay(Rect iconRect, string guid, bool isListMode)
		{
			if (Collab.instance.IsCollabEnabledForCurrentProject())
			{
				Collab.CollabStates assetState = CollabProjectHook.GetAssetState(guid);
				Overlay.DrawOverlays(assetState, iconRect, isListMode);
			}
		}

		public static Collab.CollabStates GetAssetState(string assetGuid)
		{
			Collab.CollabStates result;
			if (!Collab.instance.IsCollabEnabledForCurrentProject())
			{
				result = Collab.CollabStates.kCollabNone;
			}
			else
			{
				Collab.CollabStates assetState = Collab.instance.GetAssetState(assetGuid);
				result = assetState;
			}
			return result;
		}
	}
}
