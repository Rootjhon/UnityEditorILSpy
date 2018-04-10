using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.UIElements
{
	internal static class TooltipExtension
	{
		[RequiredByNativeCode]
		internal static void SetTooltip(float mouseX, float mouseY)
		{
			GUIView mouseOverView = GUIView.mouseOverView;
			if (mouseOverView != null && mouseOverView.visualTree != null && mouseOverView.visualTree.panel != null)
			{
				IPanel panel = mouseOverView.visualTree.panel;
				VisualElement visualElement = panel.Pick(new Vector2(mouseX, mouseY) - mouseOverView.screenPosition.position);
				if (visualElement != null)
				{
					TooltipEvent pooled = EventBase<TooltipEvent>.GetPooled();
					pooled.target = visualElement;
					pooled.tooltip = null;
					pooled.rect = Rect.zero;
					mouseOverView.visualTree.panel.dispatcher.DispatchEvent(pooled, panel);
					if (!string.IsNullOrEmpty(pooled.tooltip))
					{
						Rect rect = pooled.rect;
						rect.position += mouseOverView.screenPosition.position;
						GUIStyle.SetMouseTooltip(pooled.tooltip, rect);
					}
				}
			}
		}

		internal static void AddTooltip(this VisualElement e, string tooltip)
		{
			if (string.IsNullOrEmpty(tooltip))
			{
				e.RemoveTooltip();
			}
			else
			{
				TooltipElement tooltipElement = e.Query().Children<TooltipElement>(null, null);
				if (tooltipElement == null)
				{
					tooltipElement = new TooltipElement();
				}
				tooltipElement.style.positionType = PositionType.Absolute;
				IStyle arg_85_0 = tooltipElement.style;
				StyleValue<float> styleValue = 0f;
				tooltipElement.style.positionBottom = styleValue;
				styleValue = styleValue;
				tooltipElement.style.positionTop = styleValue;
				styleValue = styleValue;
				tooltipElement.style.positionRight = styleValue;
				arg_85_0.positionLeft = styleValue;
				tooltipElement.tooltip = tooltip;
				e.Add(tooltipElement);
			}
		}

		internal static void RemoveTooltip(this VisualElement e)
		{
			TooltipElement tooltipElement = e.Query().Children<TooltipElement>(null, null);
			if (tooltipElement != null)
			{
				e.Remove(tooltipElement);
			}
		}
	}
}
