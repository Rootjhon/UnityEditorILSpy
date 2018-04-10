using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal interface ISelectable
	{
		bool IsSelectable();

		bool Overlaps(Rect rectangle);

		void Select(GraphView selectionContainer, bool additive);

		void Unselect(GraphView selectionContainer);

		bool IsSelected(GraphView selectionContainer);
	}
}
