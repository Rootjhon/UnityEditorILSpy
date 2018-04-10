using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class PanelDebug : BasePanelDebug
	{
		public class RepaintData
		{
			public readonly Color color;

			public readonly Rect contentRect;

			public readonly uint controlId;

			public RepaintData(uint controlId, Rect contentRect, Color color)
			{
				this.contentRect = contentRect;
				this.color = color;
				this.controlId = controlId;
			}
		}

		internal uint highlightedElement;

		private List<PanelDebug.RepaintData> m_RepaintDatas = new List<PanelDebug.RepaintData>();

		internal override bool RecordRepaint(VisualElement visualElement)
		{
			bool result;
			if (!base.enabled)
			{
				result = false;
			}
			else
			{
				this.m_RepaintDatas.Add(new PanelDebug.RepaintData(visualElement.controlid, visualElement.worldBound, Color.HSVToRGB(visualElement.controlid * 11u % 32u / 32f, 0.6f, 1f)));
				result = true;
			}
			return result;
		}

		internal override bool EndRepaint()
		{
			bool result;
			if (!base.enabled)
			{
				result = false;
			}
			else
			{
				PanelDebug.RepaintData repaintData = null;
				foreach (PanelDebug.RepaintData current in this.m_RepaintDatas)
				{
					Color c = current.color;
					if (this.highlightedElement != 0u)
					{
						if (this.highlightedElement == current.controlId)
						{
							repaintData = current;
							continue;
						}
						c = Color.gray;
					}
					PickingData.DrawRect(current.contentRect, c);
				}
				this.m_RepaintDatas.Clear();
				if (repaintData != null)
				{
					PickingData.DrawRect(repaintData.contentRect, repaintData.color);
				}
				result = true;
			}
			return result;
		}
	}
}
