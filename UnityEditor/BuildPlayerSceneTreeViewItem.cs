using System;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor
{
	internal class BuildPlayerSceneTreeViewItem : TreeViewItem
	{
		private const string kAssetsFolder = "Assets/";

		private const string kSceneExtension = ".unity";

		public static int kInvalidCounter = -1;

		public bool active;

		public int counter;

		public string fullName;

		public GUID guid;

		public BuildPlayerSceneTreeViewItem(int id, int depth, string path, bool state) : base(id, depth)
		{
			this.active = state;
			this.counter = BuildPlayerSceneTreeViewItem.kInvalidCounter;
			this.guid = new GUID(AssetDatabase.AssetPathToGUID(path));
			this.fullName = "";
			this.displayName = path;
			this.UpdateName();
		}

		public void UpdateName()
		{
			string a = AssetDatabase.GUIDToAssetPath(this.guid.ToString());
			if (a != this.fullName)
			{
				this.fullName = a;
				this.displayName = this.fullName;
				if (this.displayName.StartsWith("Assets/"))
				{
					this.displayName = this.displayName.Remove(0, "Assets/".Length);
				}
				int num = this.displayName.LastIndexOf(".unity");
				if (num > 0)
				{
					this.displayName = this.displayName.Substring(0, num);
				}
			}
		}
	}
}
