using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.IMGUI.Controls
{
	internal abstract class TreeViewDragging : ITreeViewDragging
	{
		protected class DropData
		{
			public int[] expandedArrayBeforeDrag;

			public int lastControlID = -1;

			public int dropTargetControlID = -1;

			public int rowMarkerControlID = -1;

			public double expandItemBeginTimer;

			public Vector2 expandItemBeginPosition;
		}

		public enum DropPosition
		{
			Upon,
			Below,
			Above
		}

		protected TreeViewController m_TreeView;

		protected TreeViewDragging.DropData m_DropData = new TreeViewDragging.DropData();

		private const double k_DropExpandTimeout = 0.7;

		public bool drawRowMarkerAbove
		{
			get;
			set;
		}

		public TreeViewDragging(TreeViewController treeView)
		{
			this.m_TreeView = treeView;
		}

		public virtual void OnInitialize()
		{
		}

		public int GetDropTargetControlID()
		{
			return this.m_DropData.dropTargetControlID;
		}

		public int GetRowMarkerControlID()
		{
			return this.m_DropData.rowMarkerControlID;
		}

		public virtual bool CanStartDrag(TreeViewItem targetItem, List<int> draggedItemIDs, Vector2 mouseDownPosition)
		{
			return true;
		}

		public abstract void StartDrag(TreeViewItem draggedItem, List<int> draggedItemIDs);

		public abstract DragAndDropVisualMode DoDrag(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPosition);

		protected float GetDropBetweenHalfHeight(TreeViewItem item, Rect itemRect)
		{
			return (!this.m_TreeView.data.CanBeParent(item)) ? (itemRect.height * 0.5f) : this.m_TreeView.gui.halfDropBetweenHeight;
		}

		protected bool TryGetDropPosition(TreeViewItem item, Rect itemRect, int row, out TreeViewDragging.DropPosition dropPosition)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			bool result;
			if (itemRect.Contains(mousePosition))
			{
				float dropBetweenHalfHeight = this.GetDropBetweenHalfHeight(item, itemRect);
				if (mousePosition.y >= itemRect.yMax - dropBetweenHalfHeight)
				{
					dropPosition = TreeViewDragging.DropPosition.Below;
				}
				else if (mousePosition.y <= itemRect.yMin + dropBetweenHalfHeight)
				{
					dropPosition = TreeViewDragging.DropPosition.Above;
				}
				else
				{
					dropPosition = TreeViewDragging.DropPosition.Upon;
				}
				result = true;
			}
			else
			{
				float height = this.m_TreeView.gui.halfDropBetweenHeight;
				int num = row + 1;
				if (num < this.m_TreeView.data.rowCount)
				{
					Rect rowRect = this.m_TreeView.gui.GetRowRect(num, itemRect.width);
					bool flag = this.m_TreeView.data.CanBeParent(this.m_TreeView.data.GetItem(num));
					if (flag)
					{
						height = this.m_TreeView.gui.halfDropBetweenHeight;
					}
					else
					{
						height = rowRect.height * 0.5f;
					}
				}
				Rect rect = itemRect;
				rect.y = itemRect.yMax;
				rect.height = height;
				if (rect.Contains(mousePosition))
				{
					dropPosition = TreeViewDragging.DropPosition.Below;
					result = true;
				}
				else
				{
					if (row == 0)
					{
						Rect rect2 = itemRect;
						rect2.yMin -= this.m_TreeView.gui.halfDropBetweenHeight;
						rect2.height = this.m_TreeView.gui.halfDropBetweenHeight;
						if (rect2.Contains(mousePosition))
						{
							dropPosition = TreeViewDragging.DropPosition.Above;
							result = true;
							return result;
						}
					}
					dropPosition = TreeViewDragging.DropPosition.Below;
					result = false;
				}
			}
			return result;
		}

		public virtual bool DragElement(TreeViewItem targetItem, Rect targetItemRect, int row)
		{
			bool flag = Event.current.type == EventType.DragPerform;
			bool result;
			TreeViewDragging.DropPosition dropPosition;
			if (targetItem == null)
			{
				if (this.m_DropData != null)
				{
					this.m_DropData.dropTargetControlID = 0;
					this.m_DropData.rowMarkerControlID = 0;
				}
				DragAndDrop.visualMode = this.DoDrag(null, null, flag, TreeViewDragging.DropPosition.Below);
				if (DragAndDrop.visualMode != DragAndDropVisualMode.None && flag)
				{
					this.FinalizeDragPerformed(true);
				}
				result = false;
			}
			else if (!this.TryGetDropPosition(targetItem, targetItemRect, row, out dropPosition))
			{
				result = false;
			}
			else
			{
				TreeViewItem treeViewItem = null;
				switch (dropPosition)
				{
				case TreeViewDragging.DropPosition.Upon:
					treeViewItem = targetItem;
					break;
				case TreeViewDragging.DropPosition.Below:
					if (this.m_TreeView.data.IsExpanded(targetItem) && targetItem.hasChildren)
					{
						treeViewItem = targetItem;
						targetItem = targetItem.children[0];
						dropPosition = TreeViewDragging.DropPosition.Above;
					}
					else
					{
						treeViewItem = targetItem.parent;
					}
					break;
				case TreeViewDragging.DropPosition.Above:
					treeViewItem = targetItem.parent;
					break;
				}
				DragAndDropVisualMode dragAndDropVisualMode = DragAndDropVisualMode.None;
				if (flag)
				{
					if (dropPosition == TreeViewDragging.DropPosition.Upon)
					{
						dragAndDropVisualMode = this.DoDrag(targetItem, targetItem, true, dropPosition);
					}
					if (dragAndDropVisualMode == DragAndDropVisualMode.None && treeViewItem != null)
					{
						dragAndDropVisualMode = this.DoDrag(treeViewItem, targetItem, true, dropPosition);
					}
					if (dragAndDropVisualMode != DragAndDropVisualMode.None)
					{
						this.FinalizeDragPerformed(false);
					}
					else
					{
						this.DragCleanup(true);
						this.m_TreeView.NotifyListenersThatDragEnded(null, false);
					}
				}
				else
				{
					if (this.m_DropData == null)
					{
						this.m_DropData = new TreeViewDragging.DropData();
					}
					this.m_DropData.dropTargetControlID = 0;
					this.m_DropData.rowMarkerControlID = 0;
					int itemControlID = TreeViewController.GetItemControlID(targetItem);
					this.HandleAutoExpansion(itemControlID, targetItem, targetItemRect);
					if (dropPosition == TreeViewDragging.DropPosition.Upon)
					{
						dragAndDropVisualMode = this.DoDrag(targetItem, targetItem, false, dropPosition);
					}
					if (dragAndDropVisualMode != DragAndDropVisualMode.None)
					{
						this.m_DropData.dropTargetControlID = itemControlID;
						DragAndDrop.visualMode = dragAndDropVisualMode;
					}
					else if (targetItem != null && treeViewItem != null)
					{
						dragAndDropVisualMode = this.DoDrag(treeViewItem, targetItem, false, dropPosition);
						if (dragAndDropVisualMode != DragAndDropVisualMode.None)
						{
							this.drawRowMarkerAbove = (dropPosition == TreeViewDragging.DropPosition.Above);
							this.m_DropData.rowMarkerControlID = itemControlID;
							DragAndDrop.visualMode = dragAndDropVisualMode;
						}
					}
				}
				Event.current.Use();
				result = true;
			}
			return result;
		}

		private void FinalizeDragPerformed(bool revertExpanded)
		{
			this.DragCleanup(revertExpanded);
			DragAndDrop.AcceptDrag();
			List<UnityEngine.Object> list = new List<UnityEngine.Object>(DragAndDrop.objectReferences);
			bool draggedItemsFromOwnTreeView = true;
			if (list.Count > 0 && list[0] != null && TreeViewUtility.FindItemInList<TreeViewItem>(list[0].GetInstanceID(), this.m_TreeView.data.GetRows()) == null)
			{
				draggedItemsFromOwnTreeView = false;
			}
			int[] array = new int[list.Count];
			for (int i = 0; i < list.Count; i++)
			{
				if (!(list[i] == null))
				{
					array[i] = list[i].GetInstanceID();
				}
			}
			this.m_TreeView.NotifyListenersThatDragEnded(array, draggedItemsFromOwnTreeView);
		}

		protected virtual void HandleAutoExpansion(int itemControlID, TreeViewItem targetItem, Rect targetItemRect)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			float contentIndent = this.m_TreeView.gui.GetContentIndent(targetItem);
			float dropBetweenHalfHeight = this.GetDropBetweenHalfHeight(targetItem, targetItemRect);
			Rect rect = new Rect(targetItemRect.x + contentIndent, targetItemRect.y + dropBetweenHalfHeight, targetItemRect.width - contentIndent, targetItemRect.height - dropBetweenHalfHeight * 2f);
			bool flag = rect.Contains(mousePosition);
			if (itemControlID != this.m_DropData.lastControlID || !flag || this.m_DropData.expandItemBeginPosition != mousePosition)
			{
				this.m_DropData.lastControlID = itemControlID;
				this.m_DropData.expandItemBeginTimer = (double)Time.realtimeSinceStartup;
				this.m_DropData.expandItemBeginPosition = mousePosition;
			}
			bool flag2 = (double)Time.realtimeSinceStartup - this.m_DropData.expandItemBeginTimer > 0.7;
			bool flag3 = flag && flag2;
			if (targetItem != null && flag3 && targetItem.hasChildren && !this.m_TreeView.data.IsExpanded(targetItem))
			{
				if (this.m_DropData.expandedArrayBeforeDrag == null)
				{
					List<int> currentExpanded = this.GetCurrentExpanded();
					this.m_DropData.expandedArrayBeforeDrag = currentExpanded.ToArray();
				}
				this.m_TreeView.data.SetExpanded(targetItem, true);
				this.m_DropData.expandItemBeginTimer = (double)Time.realtimeSinceStartup;
				this.m_DropData.lastControlID = 0;
			}
		}

		public virtual void DragCleanup(bool revertExpanded)
		{
			if (this.m_DropData != null)
			{
				if (this.m_DropData.expandedArrayBeforeDrag != null && revertExpanded)
				{
					this.RestoreExpanded(new List<int>(this.m_DropData.expandedArrayBeforeDrag));
				}
				this.m_DropData = new TreeViewDragging.DropData();
			}
		}

		public List<int> GetCurrentExpanded()
		{
			IList<TreeViewItem> rows = this.m_TreeView.data.GetRows();
			return (from item in rows
			where this.m_TreeView.data.IsExpanded(item)
			select item.id).ToList<int>();
		}

		public void RestoreExpanded(List<int> ids)
		{
			IList<TreeViewItem> rows = this.m_TreeView.data.GetRows();
			foreach (TreeViewItem current in rows)
			{
				this.m_TreeView.data.SetExpanded(current, ids.Contains(current.id));
			}
		}

		internal static int GetInsertionIndex(TreeViewItem parentItem, TreeViewItem targetItem, TreeViewDragging.DropPosition dropPosition)
		{
			int result;
			if (parentItem == null)
			{
				result = -1;
			}
			else
			{
				int num;
				if (parentItem == targetItem)
				{
					num = -1;
				}
				else
				{
					int num2 = parentItem.children.IndexOf(targetItem);
					if (num2 >= 0)
					{
						if (dropPosition == TreeViewDragging.DropPosition.Below)
						{
							num = num2 + 1;
						}
						else
						{
							num = num2;
						}
					}
					else
					{
						Debug.LogError("Did not find targetItem,; should be a child of parentItem");
						num = -1;
					}
				}
				result = num;
			}
			return result;
		}
	}
}
