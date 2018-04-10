using System;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class InputNodeAnchorPresenter : NodeAnchorPresenter
	{
		public override Direction direction
		{
			get
			{
				return Direction.Input;
			}
		}
	}
}
