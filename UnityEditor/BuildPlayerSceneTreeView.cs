using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor
{
	internal class BuildPlayerSceneTreeView : TreeView
	{
		public BuildPlayerSceneTreeView(TreeViewState state) : base(state)
		{
			base.showBorder = true;
			EditorBuildSettings.sceneListChanged += new Action(this.HandleExternalSceneListChange);
		}

		internal void UnsubscribeListChange()
		{
			EditorBuildSettings.sceneListChanged -= new Action(this.HandleExternalSceneListChange);
		}

		private void HandleExternalSceneListChange()
		{
			base.Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			TreeViewItem treeViewItem = new TreeViewItem(-1, -1);
			treeViewItem.children = new List<TreeViewItem>();
			List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
			foreach (EditorBuildSettingsScene current in list)
			{
				BuildPlayerSceneTreeViewItem child = new BuildPlayerSceneTreeViewItem(current.guid.GetHashCode(), 0, current.path, current.enabled);
				treeViewItem.AddChild(child);
			}
			return treeViewItem;
		}

		protected override bool CanBeParent(TreeViewItem item)
		{
			return false;
		}

		protected override void BeforeRowsGUI()
		{
			int num = 0;
			foreach (TreeViewItem current in base.rootItem.children)
			{
				BuildPlayerSceneTreeViewItem buildPlayerSceneTreeViewItem = current as BuildPlayerSceneTreeViewItem;
				if (buildPlayerSceneTreeViewItem != null)
				{
					buildPlayerSceneTreeViewItem.UpdateName();
				}
				if (buildPlayerSceneTreeViewItem.active)
				{
					buildPlayerSceneTreeViewItem.counter = num;
					num++;
				}
				else
				{
					buildPlayerSceneTreeViewItem.counter = BuildPlayerSceneTreeViewItem.kInvalidCounter;
				}
			}
			base.BeforeRowsGUI();
		}

		protected override void RowGUI(TreeView.RowGUIArgs args)
		{
			BuildPlayerSceneTreeViewItem buildPlayerSceneTreeViewItem = args.item as BuildPlayerSceneTreeViewItem;
			if (buildPlayerSceneTreeViewItem != null)
			{
				bool flag = !buildPlayerSceneTreeViewItem.guid.Empty() && File.Exists(buildPlayerSceneTreeViewItem.fullName);
				using (new EditorGUI.DisabledScope(!flag))
				{
					bool flag2 = buildPlayerSceneTreeViewItem.active;
					if (!flag)
					{
						flag2 = false;
					}
					flag2 = GUI.Toggle(new Rect(args.rowRect.x, args.rowRect.y, 16f, 16f), flag2, "");
					if (flag2 != buildPlayerSceneTreeViewItem.active)
					{
						if (base.GetSelection().Contains(buildPlayerSceneTreeViewItem.id))
						{
							IList<int> selection = base.GetSelection();
							foreach (int current in selection)
							{
								BuildPlayerSceneTreeViewItem buildPlayerSceneTreeViewItem2 = base.FindItem(current, base.rootItem) as BuildPlayerSceneTreeViewItem;
								buildPlayerSceneTreeViewItem2.active = flag2;
							}
						}
						else
						{
							buildPlayerSceneTreeViewItem.active = flag2;
						}
						EditorBuildSettings.scenes = this.GetSceneList();
					}
					base.RowGUI(args);
					if (buildPlayerSceneTreeViewItem.counter != BuildPlayerSceneTreeViewItem.kInvalidCounter)
					{
						TreeView.DefaultGUI.LabelRightAligned(args.rowRect, "" + buildPlayerSceneTreeViewItem.counter, args.selected, args.focused);
					}
					else if (buildPlayerSceneTreeViewItem.displayName == string.Empty || !flag)
					{
						TreeView.DefaultGUI.LabelRightAligned(args.rowRect, "Deleted", args.selected, args.focused);
					}
				}
			}
			else
			{
				base.RowGUI(args);
			}
		}

		protected override DragAndDropVisualMode HandleDragAndDrop(TreeView.DragAndDropArgs args)
		{
			DragAndDropVisualMode result = DragAndDropVisualMode.None;
			List<int> list = DragAndDrop.GetGenericData("BuildPlayerSceneTreeViewItem") as List<int>;
			if (list != null && list.Count > 0)
			{
				result = DragAndDropVisualMode.Move;
				if (args.performDrop)
				{
					int num = this.FindDropAtIndex(args);
					List<TreeViewItem> list2 = new List<TreeViewItem>();
					int num2 = 0;
					foreach (TreeViewItem current in base.rootItem.children)
					{
						if (num2 == num)
						{
							foreach (int current2 in list)
							{
								list2.Add(base.FindItem(current2, base.rootItem));
							}
						}
						num2++;
						if (!list.Contains(current.id))
						{
							list2.Add(current);
						}
					}
					if (list2.Count < base.rootItem.children.Count)
					{
						foreach (int current3 in list)
						{
							list2.Add(base.FindItem(current3, base.rootItem));
						}
					}
					base.rootItem.children = list2;
					EditorBuildSettings.scenes = this.GetSceneList();
					this.ReloadAndSelect(list);
					base.Repaint();
				}
			}
			else if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
			{
				result = DragAndDropVisualMode.Copy;
				if (args.performDrop)
				{
					List<EditorBuildSettingsScene> list3 = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
					List<EditorBuildSettingsScene> list4 = new List<EditorBuildSettingsScene>();
					List<int> list5 = new List<int>();
					string[] paths = DragAndDrop.paths;
					for (int i = 0; i < paths.Length; i++)
					{
						string text = paths[i];
						if (AssetDatabase.GetMainAssetTypeAtPath(text) == typeof(SceneAsset))
						{
							GUID gUID = new GUID(AssetDatabase.AssetPathToGUID(text));
							list5.Add(gUID.GetHashCode());
							bool flag = true;
							foreach (EditorBuildSettingsScene current4 in list3)
							{
								if (current4.path == text)
								{
									flag = false;
									break;
								}
							}
							if (flag)
							{
								list4.Add(new EditorBuildSettingsScene(text, true));
							}
						}
					}
					int index = this.FindDropAtIndex(args);
					list3.InsertRange(index, list4);
					EditorBuildSettings.scenes = list3.ToArray();
					this.ReloadAndSelect(list5);
					base.Repaint();
				}
			}
			return result;
		}

		private void ReloadAndSelect(IList<int> hashCodes)
		{
			base.Reload();
			base.SetSelection(hashCodes, TreeViewSelectionOptions.RevealAndFrame);
			this.SelectionChanged(hashCodes);
		}

		protected override void DoubleClickedItem(int id)
		{
			BuildPlayerSceneTreeViewItem buildPlayerSceneTreeViewItem = base.FindItem(id, base.rootItem) as BuildPlayerSceneTreeViewItem;
			int instanceIDFromGUID = AssetDatabase.GetInstanceIDFromGUID(AssetDatabase.AssetPathToGUID(buildPlayerSceneTreeViewItem.fullName));
			EditorGUIUtility.PingObject(instanceIDFromGUID);
		}

		protected int FindDropAtIndex(TreeView.DragAndDropArgs args)
		{
			int num = args.insertAtIndex;
			if (num < 0 || num > base.rootItem.children.Count)
			{
				num = base.rootItem.children.Count;
			}
			return num;
		}

		protected override bool CanStartDrag(TreeView.CanStartDragArgs args)
		{
			return true;
		}

		protected override void SetupDragAndDrop(TreeView.SetupDragAndDropArgs args)
		{
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.paths = null;
			DragAndDrop.objectReferences = new UnityEngine.Object[0];
			DragAndDrop.SetGenericData("BuildPlayerSceneTreeViewItem", new List<int>(args.draggedItemIDs));
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			DragAndDrop.StartDrag("BuildPlayerSceneTreeView");
		}

		protected override void KeyEvent()
		{
			if ((Event.current.keyCode == KeyCode.Delete || Event.current.keyCode == KeyCode.Backspace) && base.GetSelection().Count > 0)
			{
				this.RemoveSelection();
			}
		}

		protected override void ContextClicked()
		{
			if (base.GetSelection().Count > 0)
			{
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Remove Selection"), false, new GenericMenu.MenuFunction(this.RemoveSelection));
				genericMenu.ShowAsContext();
			}
		}

		protected void RemoveSelection()
		{
			foreach (int current in base.GetSelection())
			{
				base.rootItem.children.Remove(base.FindItem(current, base.rootItem));
			}
			EditorBuildSettings.scenes = this.GetSceneList();
			base.Reload();
			base.Repaint();
		}

		public EditorBuildSettingsScene[] GetSceneList()
		{
			EditorBuildSettingsScene[] array = new EditorBuildSettingsScene[base.rootItem.children.Count];
			for (int i = 0; i < base.rootItem.children.Count; i++)
			{
				BuildPlayerSceneTreeViewItem buildPlayerSceneTreeViewItem = base.rootItem.children[i] as BuildPlayerSceneTreeViewItem;
				array[i] = new EditorBuildSettingsScene(buildPlayerSceneTreeViewItem.fullName, buildPlayerSceneTreeViewItem.active);
			}
			return array;
		}
	}
}
