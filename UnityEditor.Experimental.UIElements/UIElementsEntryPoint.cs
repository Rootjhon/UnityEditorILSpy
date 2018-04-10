using System;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	public static class UIElementsEntryPoint
	{
		public static VisualElement GetRootVisualContainer(this EditorWindow window)
		{
			return window.rootVisualContainer;
		}
	}
}
