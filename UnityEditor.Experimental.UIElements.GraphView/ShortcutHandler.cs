using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class ShortcutHandler : Manipulator
	{
		private readonly Dictionary<Event, ShortcutDelegate> m_Dictionary;

		public ShortcutHandler(Dictionary<Event, ShortcutDelegate> dictionary)
		{
			this.m_Dictionary = dictionary;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			base.target.RegisterCallback<KeyDownEvent>(new EventCallback<KeyDownEvent>(this.OnKeyDown), Capture.NoCapture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<KeyDownEvent>(new EventCallback<KeyDownEvent>(this.OnKeyDown), Capture.NoCapture);
		}

		private void OnKeyDown(KeyDownEvent evt)
		{
			if (this.m_Dictionary.ContainsKey(evt.imguiEvent))
			{
				EventPropagation eventPropagation = this.m_Dictionary[evt.imguiEvent]();
				if (eventPropagation == EventPropagation.Stop)
				{
					evt.StopPropagation();
				}
			}
		}
	}
}
