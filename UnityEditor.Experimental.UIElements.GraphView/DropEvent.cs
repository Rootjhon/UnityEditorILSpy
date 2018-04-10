using System;
using System.Collections.Generic;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal delegate void DropEvent(IMGUIEvent evt, List<ISelectable> selection, IDropTarget dropTarget);
}
