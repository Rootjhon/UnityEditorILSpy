using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class SelectionDropper : Manipulator
	{
		private readonly DragAndDropDelay m_DragAndDropDelay;

		private bool m_Active;

		private bool m_AddedByMouseDown;

		private bool m_Dragging;

		public IDropTarget prevDropTarget;

		public event DropEvent OnDrop
		{
			add
			{
				DropEvent dropEvent = this.OnDrop;
				DropEvent dropEvent2;
				do
				{
					dropEvent2 = dropEvent;
					dropEvent = Interlocked.CompareExchange<DropEvent>(ref this.OnDrop, (DropEvent)Delegate.Combine(dropEvent2, value), dropEvent);
				}
				while (dropEvent != dropEvent2);
			}
			remove
			{
				DropEvent dropEvent = this.OnDrop;
				DropEvent dropEvent2;
				do
				{
					dropEvent2 = dropEvent;
					dropEvent = Interlocked.CompareExchange<DropEvent>(ref this.OnDrop, (DropEvent)Delegate.Remove(dropEvent2, value), dropEvent);
				}
				while (dropEvent != dropEvent2);
			}
		}

		public Vector2 panSpeed
		{
			get;
			set;
		}

		public MouseButton activateButton
		{
			get;
			set;
		}

		public bool clampToParentEdges
		{
			get;
			set;
		}

		private GraphElement selectedElement
		{
			get;
			set;
		}

		private ISelection selectionContainer
		{
			get;
			set;
		}

		public SelectionDropper(DropEvent handler)
		{
			this.m_Active = true;
			this.OnDrop += handler;
			this.m_DragAndDropDelay = new DragAndDropDelay();
			this.activateButton = MouseButton.LeftMouse;
			this.panSpeed = new Vector2(1f, 1f);
		}

		protected override void RegisterCallbacksOnTarget()
		{
			base.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
			base.target.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
			base.target.RegisterCallback<IMGUIEvent>(new EventCallback<IMGUIEvent>(this.OnIMGUIEvent), Capture.NoCapture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.UnregisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
			base.target.UnregisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
			base.target.UnregisterCallback<IMGUIEvent>(new EventCallback<IMGUIEvent>(this.OnIMGUIEvent), Capture.NoCapture);
		}

		protected void OnMouseDown(MouseDownEvent e)
		{
			this.m_Active = false;
			this.m_Dragging = false;
			this.m_AddedByMouseDown = false;
			if (base.target != null)
			{
				this.selectionContainer = base.target.GetFirstAncestorOfType<ISelection>();
				if (this.selectionContainer != null)
				{
					this.selectedElement = base.target.GetFirstOfType<GraphElement>();
					if (this.selectedElement != null)
					{
						if (!this.selectionContainer.selection.Contains(this.selectedElement))
						{
							if (!e.ctrlKey)
							{
								this.selectionContainer.ClearSelection();
							}
							this.selectionContainer.AddToSelection(this.selectedElement);
							this.m_AddedByMouseDown = true;
						}
						if (e.button == (int)this.activateButton)
						{
							GraphElementPresenter presenter = this.selectedElement.presenter;
							if (!(presenter != null) || (presenter.capabilities & Capabilities.Droppable) == Capabilities.Droppable)
							{
								this.m_DragAndDropDelay.Init(e.localMousePosition);
								this.m_Active = true;
								base.target.TakeCapture();
								e.StopPropagation();
							}
						}
					}
				}
			}
		}

		protected void OnMouseMove(MouseMoveEvent e)
		{
			if (this.m_Active && !this.m_Dragging && this.selectionContainer != null)
			{
				List<ISelectable> list = this.selectionContainer.selection.ToList<ISelectable>();
				if (list.Count > 0)
				{
					bool flag = false;
					GraphElement graphElement = list[0] as GraphElement;
					if (graphElement != null)
					{
						GraphElementPresenter presenter = graphElement.presenter;
						if (presenter != null)
						{
							flag = ((presenter.capabilities & Capabilities.Droppable) == Capabilities.Droppable);
						}
					}
					if (flag && this.m_DragAndDropDelay.CanStartDrag(e.localMousePosition))
					{
						DragAndDrop.PrepareStartDrag();
						DragAndDrop.objectReferences = new UnityEngine.Object[0];
						DragAndDrop.SetGenericData("DragSelection", list);
						this.m_Dragging = true;
						DragAndDrop.StartDrag("");
						DragAndDrop.visualMode = ((!e.ctrlKey) ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Copy);
					}
					e.StopPropagation();
				}
			}
		}

		protected void OnMouseUp(MouseUpEvent e)
		{
			if (this.m_Active && this.selectionContainer != null)
			{
				if (e.button == (int)this.activateButton)
				{
					if (!e.ctrlKey)
					{
						this.selectionContainer.ClearSelection();
						this.selectionContainer.AddToSelection(this.selectedElement);
					}
					else if (!this.m_AddedByMouseDown && !this.m_Dragging)
					{
						this.selectionContainer.RemoveFromSelection(this.selectedElement);
					}
					base.target.ReleaseCapture();
					e.StopPropagation();
				}
				this.m_Active = false;
				this.m_AddedByMouseDown = false;
				this.m_Dragging = false;
			}
		}

		protected void OnIMGUIEvent(IMGUIEvent e)
		{
			if (this.m_Active && this.selectionContainer != null)
			{
				List<ISelectable> list = this.selectionContainer.selection.ToList<ISelectable>();
				Event imguiEvent = e.imguiEvent;
				EventType type = imguiEvent.type;
				if (type != EventType.DragUpdated)
				{
					if (type != EventType.DragExited)
					{
						if (type == EventType.DragPerform)
						{
							if (this.m_Active && imguiEvent.button == (int)this.activateButton && list.Count > 0)
							{
								if (list.Count > 0)
								{
									if (this.OnDrop != null)
									{
										VisualElement visualElement = base.target.panel.Pick(base.target.LocalToWorld(imguiEvent.mousePosition));
										IDropTarget dropTarget = (visualElement == null) ? null : visualElement.GetFirstAncestorOfType<IDropTarget>();
										this.OnDrop(e, list, dropTarget);
									}
									DragAndDrop.visualMode = DragAndDropVisualMode.None;
									DragAndDrop.SetGenericData("DragSelection", null);
								}
							}
							this.prevDropTarget = null;
							this.m_Active = false;
							base.target.ReleaseCapture();
						}
					}
					else
					{
						if (this.OnDrop != null && this.prevDropTarget != null)
						{
							this.OnDrop(e, list, this.prevDropTarget);
						}
						DragAndDrop.visualMode = DragAndDropVisualMode.None;
						DragAndDrop.SetGenericData("DragSelection", null);
						this.prevDropTarget = null;
						this.m_Active = false;
						base.target.ReleaseCapture();
					}
				}
				else if (base.target.HasCapture() && imguiEvent.button == (int)this.activateButton && list.Count > 0)
				{
					this.selectedElement = null;
					if (this.OnDrop != null)
					{
						VisualElement visualElement2 = base.target.panel.Pick(base.target.LocalToWorld(imguiEvent.mousePosition));
						IDropTarget dropTarget2 = (visualElement2 == null) ? null : visualElement2.GetFirstAncestorOfType<IDropTarget>();
						if (this.prevDropTarget != dropTarget2 && this.prevDropTarget != null)
						{
							IMGUIEvent pooled = IMGUIEvent.GetPooled(e.imguiEvent);
							pooled.imguiEvent.type = EventType.DragExited;
							this.OnDrop(pooled, list, this.prevDropTarget);
							EventBase<IMGUIEvent>.ReleasePooled(pooled);
						}
						this.OnDrop(e, list, dropTarget2);
						this.prevDropTarget = dropTarget2;
					}
					DragAndDrop.visualMode = ((!imguiEvent.control) ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Copy);
				}
			}
		}
	}
}
