using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class EdgeConnector : MouseManipulator
	{
	}
	internal class EdgeConnector<TEdgePresenter> : EdgeConnector where TEdgePresenter : EdgePresenter
	{
		private List<NodeAnchorPresenter> m_CompatibleAnchors;

		private TEdgePresenter m_EdgePresenterCandidate;

		private GraphViewPresenter m_GraphViewPresenter;

		private GraphView m_GraphView;

		private bool m_Active;

		private static NodeAdapter s_nodeAdapter = new NodeAdapter();

		private readonly IEdgeConnectorListener m_Listener;

		public EdgeConnector(IEdgeConnectorListener listener)
		{
			this.m_Listener = listener;
			this.m_Active = false;
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse
			});
		}

		protected override void RegisterCallbacksOnTarget()
		{
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
				NodeAnchor nodeAnchor = e.target as NodeAnchor;
				if (nodeAnchor != null)
				{
					NodeAnchorPresenter presenter = nodeAnchor.GetPresenter<NodeAnchorPresenter>();
					this.m_GraphView = nodeAnchor.GetFirstAncestorOfType<GraphView>();
					if (!(presenter == null) && this.m_GraphView != null)
					{
						this.m_GraphViewPresenter = this.m_GraphView.presenter;
						if (!(this.m_GraphViewPresenter == null))
						{
							this.m_Active = true;
							base.target.TakeCapture();
							this.m_CompatibleAnchors = this.m_GraphViewPresenter.GetCompatibleAnchors(presenter, EdgeConnector<TEdgePresenter>.s_nodeAdapter);
							foreach (NodeAnchorPresenter current in this.m_CompatibleAnchors)
							{
								current.highlight = true;
							}
							this.m_EdgePresenterCandidate = ScriptableObject.CreateInstance<TEdgePresenter>();
							this.m_EdgePresenterCandidate.position = new Rect(0f, 0f, 1f, 1f);
							bool flag = presenter.direction == Direction.Output;
							if (flag)
							{
								this.m_EdgePresenterCandidate.output = nodeAnchor.GetPresenter<NodeAnchorPresenter>();
								this.m_EdgePresenterCandidate.input = null;
							}
							else
							{
								this.m_EdgePresenterCandidate.output = null;
								this.m_EdgePresenterCandidate.input = nodeAnchor.GetPresenter<NodeAnchorPresenter>();
							}
							this.m_EdgePresenterCandidate.candidate = true;
							this.m_EdgePresenterCandidate.candidatePosition = e.mousePosition;
							this.m_GraphViewPresenter.AddTempElement(this.m_EdgePresenterCandidate);
							e.StopPropagation();
						}
					}
				}
			}
		}

		protected void OnMouseMove(MouseMoveEvent e)
		{
			if (this.m_Active)
			{
				this.m_EdgePresenterCandidate.candidatePosition = e.mousePosition;
				e.StopPropagation();
			}
		}

		protected void OnMouseUp(MouseUpEvent e)
		{
			if (this.m_Active)
			{
				if (base.CanStopManipulation(e))
				{
					NodeAnchorPresenter nodeAnchorPresenter = null;
					if (this.m_GraphView != null)
					{
						using (List<NodeAnchorPresenter>.Enumerator enumerator = this.m_CompatibleAnchors.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								NodeAnchorPresenter compatibleAnchor = enumerator.Current;
								compatibleAnchor.highlight = false;
								NodeAnchor nodeAnchor = (from el in this.m_GraphView.Query(null, null)
								where el.GetPresenter<NodeAnchorPresenter>() == compatibleAnchor
								select el).First();
								if (nodeAnchor != null && nodeAnchor.worldBound.Contains(e.mousePosition))
								{
									nodeAnchorPresenter = compatibleAnchor;
								}
							}
						}
					}
					if (nodeAnchorPresenter == null && this.m_Listener != null)
					{
						this.m_Listener.OnDropOutsideAnchor(this.m_EdgePresenterCandidate, e.mousePosition);
					}
					this.m_GraphViewPresenter.RemoveTempElement(this.m_EdgePresenterCandidate);
					if (this.m_EdgePresenterCandidate != null && this.m_GraphViewPresenter != null)
					{
						if (nodeAnchorPresenter != null)
						{
							if (this.m_EdgePresenterCandidate.output == null)
							{
								this.m_EdgePresenterCandidate.output = nodeAnchorPresenter;
							}
							else
							{
								this.m_EdgePresenterCandidate.input = nodeAnchorPresenter;
							}
							this.m_EdgePresenterCandidate.output.Connect(this.m_EdgePresenterCandidate);
							this.m_EdgePresenterCandidate.input.Connect(this.m_EdgePresenterCandidate);
							this.m_GraphViewPresenter.AddElement(this.m_EdgePresenterCandidate);
						}
						this.m_EdgePresenterCandidate.candidate = false;
					}
					this.m_EdgePresenterCandidate = (TEdgePresenter)((object)null);
					this.m_GraphViewPresenter = null;
					this.m_Active = false;
					e.StopPropagation();
				}
			}
			base.target.ReleaseCapture();
		}
	}
}
