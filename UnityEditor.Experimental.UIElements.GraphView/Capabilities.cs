using System;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Flags]
	internal enum Capabilities
	{
		Normal = 1,
		Selectable = 2,
		DoesNotCollapse = 4,
		Floating = 8,
		Resizable = 16,
		Movable = 32,
		Deletable = 64,
		Droppable = 128
	}
}
