using System;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	internal class TooltipElement : VisualElement
	{
		public string tooltip
		{
			get;
			set;
		}

		public TooltipElement()
		{
			base.RegisterCallback<TooltipEvent>(new EventCallback<TooltipEvent>(this.OnTooltip), Capture.NoCapture);
		}

		private void OnTooltip(TooltipEvent e)
		{
			e.tooltip = this.tooltip;
			e.rect = base.worldBound;
			e.StopPropagation();
		}
	}
}
