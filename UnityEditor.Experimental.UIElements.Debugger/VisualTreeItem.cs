using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class VisualTreeItem : TreeViewItem
	{
		public readonly VisualElement elt;

		public uint controlId
		{
			get
			{
				return this.elt.controlid;
			}
		}

		public VisualTreeItem(VisualElement elt, int depth) : base((int)elt.controlid, depth, VisualTreeItem.GetDisplayName(elt))
		{
			this.elt = elt;
		}

		private static string GetDisplayName(VisualElement elt)
		{
			return elt.GetType().Name + " " + elt.name;
		}
	}
}
