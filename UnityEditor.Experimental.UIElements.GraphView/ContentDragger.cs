using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class ContentDragger : MouseManipulator
	{
		private Vector2 m_Start;

		private bool m_Active;

		public Vector2 panSpeed
		{
			get;
			set;
		}

		public bool clampToParentEdges
		{
			get;
			set;
		}

		public ContentDragger()
		{
			this.m_Active = false;
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse,
				modifiers = EventModifiers.Alt
			});
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.MiddleMouse
			});
			this.panSpeed = new Vector2(1f, 1f);
			this.clampToParentEdges = false;
		}

		protected Rect CalculatePosition(float x, float y, float width, float height)
		{
			Rect result = new Rect(x, y, width, height);
			if (this.clampToParentEdges)
			{
				if (result.x < base.target.parent.layout.xMin)
				{
					result.x = base.target.parent.layout.xMin;
				}
				else if (result.xMax > base.target.parent.layout.xMax)
				{
					result.x = base.target.parent.layout.xMax - result.width;
				}
				if (result.y < base.target.parent.layout.yMin)
				{
					result.y = base.target.parent.layout.yMin;
				}
				else if (result.yMax > base.target.parent.layout.yMax)
				{
					result.y = base.target.parent.layout.yMax - result.height;
				}
				result.width = width;
				result.height = height;
			}
			return result;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (!(base.target is GraphView))
			{
				throw new InvalidOperationException("Manipulator can only be added to a GraphView");
			}
			base.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
			base.target.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.UnregisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
			base.target.UnregisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
		}

		protected void OnMouseDown(MouseDownEvent e)
		{
			if (base.CanStartManipulation(e))
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					this.m_Start = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, e.localMousePosition);
					this.m_Active = true;
					base.target.TakeCapture();
					e.StopPropagation();
				}
			}
		}

		protected void OnMouseMove(MouseMoveEvent e)
		{
			if (this.m_Active && base.target.HasCapture())
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					Vector2 v = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, e.localMousePosition) - this.m_Start;
					Vector3 scale = graphView.contentViewContainer.transform.scale;
					graphView.viewTransform.position += Vector3.Scale(v, scale);
					e.StopPropagation();
				}
			}
		}

		protected void OnMouseUp(MouseUpEvent e)
		{
			if (this.m_Active && base.CanStopManipulation(e))
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					Vector3 position = graphView.contentViewContainer.transform.position;
					Vector3 scale = graphView.contentViewContainer.transform.scale;
					graphView.UpdateViewTransform(position, scale);
					this.m_Active = false;
					base.target.ReleaseCapture();
					e.StopPropagation();
				}
			}
		}
	}
}
