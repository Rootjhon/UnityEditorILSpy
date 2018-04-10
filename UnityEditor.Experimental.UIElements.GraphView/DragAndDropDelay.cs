using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class DragAndDropDelay
	{
		private const float k_StartDragTreshold = 4f;

		private Vector2 mouseDownPosition
		{
			get;
			set;
		}

		public void Init(Vector2 mousePosition)
		{
			this.mouseDownPosition = mousePosition;
		}

		public bool CanStartDrag(Vector2 mousePosition)
		{
			return Vector2.Distance(this.mouseDownPosition, mousePosition) > 4f;
		}
	}
}
