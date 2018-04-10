using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal struct ManipulatorActivationFilter
	{
		public MouseButton button;

		public EventModifiers modifiers;

		public bool Matches(IMouseEvent evt)
		{
			return this.button == (MouseButton)evt.button && this.HasModifiers(evt);
		}

		private bool HasModifiers(IMouseEvent evt)
		{
			return ((this.modifiers & EventModifiers.Alt) == EventModifiers.None || evt.altKey) && ((this.modifiers & EventModifiers.Alt) != EventModifiers.None || !evt.altKey) && ((this.modifiers & EventModifiers.Control) == EventModifiers.None || evt.ctrlKey) && ((this.modifiers & EventModifiers.Control) != EventModifiers.None || !evt.ctrlKey) && ((this.modifiers & EventModifiers.Shift) == EventModifiers.None || evt.shiftKey) && ((this.modifiers & EventModifiers.Shift) != EventModifiers.None || !evt.shiftKey) && ((this.modifiers & EventModifiers.Command) == EventModifiers.None || evt.commandKey) && ((this.modifiers & EventModifiers.Command) != EventModifiers.None || !evt.commandKey);
		}
	}
}
