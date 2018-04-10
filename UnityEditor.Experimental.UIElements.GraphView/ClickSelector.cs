using System;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class ClickSelector : MouseManipulator
	{
		public ClickSelector()
		{
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse
			});
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.RightMouse
			});
		}

		protected override void RegisterCallbacksOnTarget()
		{
			base.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.Capture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.Capture);
		}

		protected void OnMouseDown(MouseDownEvent e)
		{
			if (e.currentTarget is ISelectable)
			{
				if (base.CanStartManipulation(e))
				{
					GraphElement graphElement = e.currentTarget as GraphElement;
					if (graphElement != null)
					{
						VisualElement parent = graphElement.shadow.parent;
						while (parent != null && !(parent is GraphView))
						{
							parent = parent.shadow.parent;
						}
						GraphView selectionContainer = parent as GraphView;
						if (!graphElement.IsSelected(selectionContainer))
						{
							graphElement.Select(selectionContainer, e.shiftKey || e.ctrlKey);
						}
					}
				}
			}
		}
	}
}
