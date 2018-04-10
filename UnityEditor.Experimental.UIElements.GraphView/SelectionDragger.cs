using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class SelectionDragger : Dragger
	{
		private bool m_AddedOnMouseDown;

		private bool m_Dragged;

		private GraphElement selectedElement
		{
			get;
			set;
		}

		private GraphElement clickedElement
		{
			get;
			set;
		}

		public SelectionDragger()
		{
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse
			});
			base.panSpeed = new Vector2(1f, 1f);
			base.clampToParentEdges = false;
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

		protected new void OnMouseDown(MouseDownEvent e)
		{
			GraphView graphView = base.target as GraphView;
			if (graphView != null)
			{
				this.selectedElement = null;
				this.clickedElement = (e.target as GraphElement);
				if (this.clickedElement == null)
				{
					VisualElement visualElement = e.target as VisualElement;
					this.clickedElement = visualElement.GetFirstAncestorOfType<GraphElement>();
					if (this.clickedElement == null)
					{
						return;
					}
				}
				if (!graphView.selection.Contains(this.clickedElement))
				{
					if (!e.ctrlKey)
					{
						graphView.ClearSelection();
					}
					graphView.AddToSelection(this.clickedElement);
					this.m_AddedOnMouseDown = true;
				}
				if (base.CanStartManipulation(e))
				{
					this.selectedElement = this.clickedElement;
					GraphElementPresenter presenter = this.selectedElement.presenter;
					if (!(presenter != null) || (this.selectedElement.presenter.capabilities & Capabilities.Movable) == Capabilities.Movable)
					{
						this.m_Active = true;
						base.target.TakeCapture();
					}
				}
			}
		}

		protected new void OnMouseMove(MouseMoveEvent e)
		{
			if (this.m_Active)
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					foreach (ISelectable current in graphView.selection)
					{
						GraphElement graphElement = current as GraphElement;
						if (graphElement != null && !(graphElement.presenter == null))
						{
							if ((graphElement.presenter.capabilities & Capabilities.Movable) == Capabilities.Movable)
							{
								this.m_Dragged = true;
								Matrix4x4 worldTransform = graphElement.worldTransform;
								Vector3 vector = new Vector3(worldTransform.m00, worldTransform.m11, worldTransform.m22);
								graphElement.SetPosition(base.CalculatePosition(graphElement.layout.x + e.mouseDelta.x * base.panSpeed.x / vector.x, graphElement.layout.y + e.mouseDelta.y * base.panSpeed.y / vector.y, graphElement.layout.width, graphElement.layout.height));
							}
						}
					}
					this.selectedElement = null;
					e.StopPropagation();
				}
			}
		}

		protected new void OnMouseUp(MouseUpEvent e)
		{
			GraphView graphView = base.target as GraphView;
			if (graphView != null)
			{
				if (this.clickedElement != null && !this.m_Dragged)
				{
					if (graphView.selection.Contains(this.clickedElement))
					{
						if (e.ctrlKey)
						{
							if (!this.m_AddedOnMouseDown)
							{
								graphView.RemoveFromSelection(this.clickedElement);
							}
						}
						else
						{
							graphView.ClearSelection();
							graphView.AddToSelection(this.clickedElement);
						}
					}
				}
				if (this.m_Active && base.CanStopManipulation(e))
				{
					if (this.selectedElement == null)
					{
						foreach (ISelectable current in graphView.selection)
						{
							GraphElement graphElement = current as GraphElement;
							if (graphElement != null && !(graphElement.presenter == null))
							{
								GraphElementPresenter presenter = graphElement.presenter;
								if ((graphElement.presenter.capabilities & Capabilities.Movable) == Capabilities.Movable)
								{
									presenter.position = graphElement.layout;
									presenter.CommitChanges();
								}
							}
						}
					}
					base.target.ReleaseCapture();
					e.StopPropagation();
				}
				this.m_AddedOnMouseDown = false;
				this.m_Dragged = false;
				this.m_Active = false;
			}
		}
	}
}
