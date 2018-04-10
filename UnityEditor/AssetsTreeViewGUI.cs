using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.VersionControl;
using UnityEditorInternal.VersionControl;
using UnityEngine;

namespace UnityEditor
{
	internal class AssetsTreeViewGUI : TreeViewGUI
	{
		internal delegate void OnAssetIconDrawDelegate(Rect iconRect, string guid);

		internal delegate bool OnAssetLabelDrawDelegate(Rect drawRect, string guid);

		private static bool s_VCEnabled;

		private const float k_IconOverlayPadding = 7f;

		private static IDictionary<int, string> s_GUIDCache;

		internal static event AssetsTreeViewGUI.OnAssetIconDrawDelegate postAssetIconDrawCallback
		{
			add
			{
				AssetsTreeViewGUI.OnAssetIconDrawDelegate onAssetIconDrawDelegate = AssetsTreeViewGUI.postAssetIconDrawCallback;
				AssetsTreeViewGUI.OnAssetIconDrawDelegate onAssetIconDrawDelegate2;
				do
				{
					onAssetIconDrawDelegate2 = onAssetIconDrawDelegate;
					onAssetIconDrawDelegate = Interlocked.CompareExchange<AssetsTreeViewGUI.OnAssetIconDrawDelegate>(ref AssetsTreeViewGUI.postAssetIconDrawCallback, (AssetsTreeViewGUI.OnAssetIconDrawDelegate)Delegate.Combine(onAssetIconDrawDelegate2, value), onAssetIconDrawDelegate);
				}
				while (onAssetIconDrawDelegate != onAssetIconDrawDelegate2);
			}
			remove
			{
				AssetsTreeViewGUI.OnAssetIconDrawDelegate onAssetIconDrawDelegate = AssetsTreeViewGUI.postAssetIconDrawCallback;
				AssetsTreeViewGUI.OnAssetIconDrawDelegate onAssetIconDrawDelegate2;
				do
				{
					onAssetIconDrawDelegate2 = onAssetIconDrawDelegate;
					onAssetIconDrawDelegate = Interlocked.CompareExchange<AssetsTreeViewGUI.OnAssetIconDrawDelegate>(ref AssetsTreeViewGUI.postAssetIconDrawCallback, (AssetsTreeViewGUI.OnAssetIconDrawDelegate)Delegate.Remove(onAssetIconDrawDelegate2, value), onAssetIconDrawDelegate);
				}
				while (onAssetIconDrawDelegate != onAssetIconDrawDelegate2);
			}
		}

		internal static event AssetsTreeViewGUI.OnAssetLabelDrawDelegate postAssetLabelDrawCallback
		{
			add
			{
				AssetsTreeViewGUI.OnAssetLabelDrawDelegate onAssetLabelDrawDelegate = AssetsTreeViewGUI.postAssetLabelDrawCallback;
				AssetsTreeViewGUI.OnAssetLabelDrawDelegate onAssetLabelDrawDelegate2;
				do
				{
					onAssetLabelDrawDelegate2 = onAssetLabelDrawDelegate;
					onAssetLabelDrawDelegate = Interlocked.CompareExchange<AssetsTreeViewGUI.OnAssetLabelDrawDelegate>(ref AssetsTreeViewGUI.postAssetLabelDrawCallback, (AssetsTreeViewGUI.OnAssetLabelDrawDelegate)Delegate.Combine(onAssetLabelDrawDelegate2, value), onAssetLabelDrawDelegate);
				}
				while (onAssetLabelDrawDelegate != onAssetLabelDrawDelegate2);
			}
			remove
			{
				AssetsTreeViewGUI.OnAssetLabelDrawDelegate onAssetLabelDrawDelegate = AssetsTreeViewGUI.postAssetLabelDrawCallback;
				AssetsTreeViewGUI.OnAssetLabelDrawDelegate onAssetLabelDrawDelegate2;
				do
				{
					onAssetLabelDrawDelegate2 = onAssetLabelDrawDelegate;
					onAssetLabelDrawDelegate = Interlocked.CompareExchange<AssetsTreeViewGUI.OnAssetLabelDrawDelegate>(ref AssetsTreeViewGUI.postAssetLabelDrawCallback, (AssetsTreeViewGUI.OnAssetLabelDrawDelegate)Delegate.Remove(onAssetLabelDrawDelegate2, value), onAssetLabelDrawDelegate);
				}
				while (onAssetLabelDrawDelegate != onAssetLabelDrawDelegate2);
			}
		}

		public AssetsTreeViewGUI(TreeViewController treeView) : base(treeView)
		{
			base.iconOverlayGUI = (Action<TreeViewItem, Rect>)Delegate.Combine(base.iconOverlayGUI, new Action<TreeViewItem, Rect>(this.OnIconOverlayGUI));
			base.labelOverlayGUI = (Action<TreeViewItem, Rect>)Delegate.Combine(base.labelOverlayGUI, new Action<TreeViewItem, Rect>(this.OnLabelOverlayGUI));
			this.k_TopRowMargin = 4f;
		}

		public override void BeginRowGUI()
		{
			AssetsTreeViewGUI.s_VCEnabled = Provider.isActive;
			float num = (!AssetsTreeViewGUI.s_VCEnabled) ? 0f : 7f;
			base.iconRightPadding = num;
			base.iconLeftPadding = num;
			base.BeginRowGUI();
		}

		protected CreateAssetUtility GetCreateAssetUtility()
		{
			return ((TreeViewStateWithAssetUtility)this.m_TreeView.state).createAssetUtility;
		}

		protected virtual bool IsCreatingNewAsset(int instanceID)
		{
			return this.GetCreateAssetUtility().IsCreatingNewAsset() && this.IsRenaming(instanceID);
		}

		protected override void ClearRenameAndNewItemState()
		{
			this.GetCreateAssetUtility().Clear();
			base.ClearRenameAndNewItemState();
		}

		protected override void RenameEnded()
		{
			string name = (!string.IsNullOrEmpty(base.GetRenameOverlay().name)) ? base.GetRenameOverlay().name : base.GetRenameOverlay().originalName;
			int userData = base.GetRenameOverlay().userData;
			bool flag = this.GetCreateAssetUtility().IsCreatingNewAsset();
			bool userAcceptedRename = base.GetRenameOverlay().userAcceptedRename;
			if (userAcceptedRename)
			{
				if (flag)
				{
					this.GetCreateAssetUtility().EndNewAssetCreation(name);
				}
				else
				{
					ObjectNames.SetNameSmartWithInstanceID(userData, name);
				}
			}
		}

		protected override void SyncFakeItem()
		{
			if (!this.m_TreeView.data.HasFakeItem() && this.GetCreateAssetUtility().IsCreatingNewAsset())
			{
				int mainAssetInstanceID = AssetDatabase.GetMainAssetInstanceID(this.GetCreateAssetUtility().folder);
				this.m_TreeView.data.InsertFakeItem(this.GetCreateAssetUtility().instanceID, mainAssetInstanceID, this.GetCreateAssetUtility().originalName, this.GetCreateAssetUtility().icon);
			}
			if (this.m_TreeView.data.HasFakeItem() && !this.GetCreateAssetUtility().IsCreatingNewAsset())
			{
				this.m_TreeView.data.RemoveFakeItem();
			}
		}

		public virtual void BeginCreateNewAsset(int instanceID, EndNameEditAction endAction, string pathName, Texture2D icon, string resourceFile)
		{
			this.ClearRenameAndNewItemState();
			if (this.GetCreateAssetUtility().BeginNewAssetCreation(instanceID, endAction, pathName, icon, resourceFile))
			{
				this.SyncFakeItem();
				if (!base.GetRenameOverlay().BeginRename(this.GetCreateAssetUtility().originalName, instanceID, 0f))
				{
					Debug.LogError("Rename not started (when creating new asset)");
				}
			}
		}

		protected override Texture GetIconForItem(TreeViewItem item)
		{
			Texture result;
			if (item == null)
			{
				result = null;
			}
			else
			{
				Texture texture = null;
				if (this.IsCreatingNewAsset(item.id))
				{
					texture = this.GetCreateAssetUtility().icon;
				}
				if (texture == null)
				{
					texture = item.icon;
				}
				if (texture == null && item.id != 0)
				{
					string assetPath = AssetDatabase.GetAssetPath(item.id);
					texture = AssetDatabase.GetCachedIcon(assetPath);
				}
				result = texture;
			}
			return result;
		}

		private void OnIconOverlayGUI(TreeViewItem item, Rect overlayRect)
		{
			if (AssetsTreeViewGUI.postAssetIconDrawCallback != null && AssetDatabase.IsMainAsset(item.id))
			{
				string gUIDForInstanceID = AssetsTreeViewGUI.GetGUIDForInstanceID(item.id);
				AssetsTreeViewGUI.postAssetIconDrawCallback(overlayRect, gUIDForInstanceID);
			}
			if (AssetsTreeViewGUI.s_VCEnabled && AssetDatabase.IsMainAsset(item.id))
			{
				string gUIDForInstanceID2 = AssetsTreeViewGUI.GetGUIDForInstanceID(item.id);
				ProjectHooks.OnProjectWindowItem(gUIDForInstanceID2, overlayRect);
			}
		}

		private void OnLabelOverlayGUI(TreeViewItem item, Rect labelRect)
		{
			if (AssetsTreeViewGUI.postAssetLabelDrawCallback != null && AssetDatabase.IsMainAsset(item.id))
			{
				string gUIDForInstanceID = AssetsTreeViewGUI.GetGUIDForInstanceID(item.id);
				AssetsTreeViewGUI.postAssetLabelDrawCallback(labelRect, gUIDForInstanceID);
			}
		}

		private static string GetGUIDForInstanceID(int instanceID)
		{
			if (AssetsTreeViewGUI.s_GUIDCache == null)
			{
				AssetsTreeViewGUI.s_GUIDCache = new Dictionary<int, string>();
			}
			string text = null;
			if (!AssetsTreeViewGUI.s_GUIDCache.TryGetValue(instanceID, out text))
			{
				string assetPath = AssetDatabase.GetAssetPath(instanceID);
				text = AssetDatabase.AssetPathToGUID(assetPath);
				AssetsTreeViewGUI.s_GUIDCache.Add(instanceID, text);
			}
			return text;
		}

		static AssetsTreeViewGUI()
		{
			// Note: this type is marked as 'beforefieldinit'.
			AssetsTreeViewGUI.postAssetIconDrawCallback = null;
			AssetsTreeViewGUI.postAssetLabelDrawCallback = null;
			AssetsTreeViewGUI.s_GUIDCache = null;
		}
	}
}
