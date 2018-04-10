using System;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class OutputNodeAnchorPresenter : NodeAnchorPresenter
	{
		public override Direction direction
		{
			get
			{
				return Direction.Output;
			}
		}
	}
}
