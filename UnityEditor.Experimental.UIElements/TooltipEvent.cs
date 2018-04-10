using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	internal class TooltipEvent : EventBase<TooltipEvent>, IPropagatableEvent
	{
		public string tooltip
		{
			get;
			set;
		}

		public Rect rect
		{
			get;
			set;
		}
	}
}
