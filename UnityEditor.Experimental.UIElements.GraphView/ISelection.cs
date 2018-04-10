using System;
using System.Collections.Generic;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal interface ISelection
	{
		List<ISelectable> selection
		{
			get;
		}

		void AddToSelection(ISelectable selectable);

		void RemoveFromSelection(ISelectable selectable);

		void ClearSelection();
	}
}
