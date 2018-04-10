using System;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class MouseManipulator : Manipulator
	{
		private ManipulatorActivationFilter m_CurrentActivator;

		public List<ManipulatorActivationFilter> activators
		{
			get;
			private set;
		}

		protected MouseManipulator()
		{
			this.activators = new List<ManipulatorActivationFilter>();
		}

		protected bool CanStartManipulation(IMouseEvent e)
		{
			bool result;
			foreach (ManipulatorActivationFilter current in this.activators)
			{
				if (current.Matches(e))
				{
					this.m_CurrentActivator = current;
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		protected bool CanStopManipulation(IMouseEvent e)
		{
			return e.button == (int)this.m_CurrentActivator.button;
		}
	}
}
