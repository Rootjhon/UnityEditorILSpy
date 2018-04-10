using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class Edge : GraphElement
	{
		private const float k_EndPointRadius = 4f;

		private const float k_InterceptWidth = 3f;

		private const string k_EdgeWidthProperty = "edge-width";

		private const string k_SelectedEdgeColorProperty = "selected-edge-color";

		private const string k_EdgeColorProperty = "edge-color";

		private GraphView m_GraphView;

		private NodeAnchorPresenter m_OutputPresenter;

		private NodeAnchorPresenter m_InputPresenter;

		private NodeAnchor m_LeftAnchor;

		private NodeAnchor m_RightAnchor;

		private EdgeControl m_EdgeControl;

		private StyleValue<int> m_EdgeWidth;

		private StyleValue<Color> m_SelectedColor;

		private StyleValue<Color> m_DefaultColor;

		public EdgeControl edgeControl
		{
			get
			{
				if (this.m_EdgeControl == null)
				{
					this.m_EdgeControl = this.CreateEdgeControl();
				}
				return this.m_EdgeControl;
			}
		}

		public int edgeWidth
		{
			get
			{
				return this.m_EdgeWidth.GetSpecifiedValueOrDefault(2);
			}
		}

		public Color selectedColor
		{
			get
			{
				return this.m_SelectedColor.GetSpecifiedValueOrDefault(new Color(0.9411765f, 0.9411765f, 0.9411765f));
			}
		}

		public Color defaultColor
		{
			get
			{
				return this.m_DefaultColor.GetSpecifiedValueOrDefault(new Color(0.572549045f, 0.572549045f, 0.572549045f));
			}
		}

		protected Vector3[] PointsAndTangents
		{
			get
			{
				return this.edgeControl.controlPoints;
			}
		}

		public Edge()
		{
			base.clippingOptions = VisualElement.ClippingOptions.NoClipping;
			base.ClearClassList();
			base.AddToClassList("edge");
			base.Add(this.edgeControl);
		}

		public override bool Overlaps(Rect rectangle)
		{
			return this.UpdateEdgeControl() && this.edgeControl.Overlaps(rectangle);
		}

		public override bool ContainsPoint(Vector2 localPoint)
		{
			return this.UpdateEdgeControl() && this.edgeControl.ContainsPoint(localPoint);
		}

		protected bool UpdateEdgeControl()
		{
			EdgePresenter presenter = base.GetPresenter<EdgePresenter>();
			NodeAnchorPresenter output = presenter.output;
			NodeAnchorPresenter input = presenter.input;
			bool result;
			if (output == null && input == null)
			{
				result = false;
			}
			else
			{
				Vector2 zero = Vector2.zero;
				Vector2 zero2 = Vector2.zero;
				this.GetFromToPoints(presenter, output, input, ref zero, ref zero2);
				this.edgeControl.from = zero;
				this.edgeControl.to = zero2;
				this.edgeControl.orientation = ((!(output != null)) ? input.orientation : output.orientation);
				result = true;
			}
			return result;
		}

		public override void DoRepaint()
		{
			this.DrawEdge();
		}

		protected void GetFromToPoints(EdgePresenter edgePresenter, NodeAnchorPresenter outputPresenter, NodeAnchorPresenter inputPresenter, ref Vector2 from, ref Vector2 to)
		{
			if (!(outputPresenter == null) || !(inputPresenter == null))
			{
				if (this.m_GraphView == null)
				{
					this.m_GraphView = base.GetFirstOfType<GraphView>();
				}
				if (outputPresenter != null)
				{
					if (this.m_OutputPresenter != outputPresenter)
					{
						this.m_LeftAnchor = (from e in this.m_GraphView.Query(null, null)
						where e.direction == Direction.Output && e.GetPresenter<NodeAnchorPresenter>() == outputPresenter
						select e).First();
						this.m_OutputPresenter = outputPresenter;
					}
					if (this.m_LeftAnchor != null)
					{
						from = this.m_LeftAnchor.GetGlobalCenter();
						from = base.worldTransform.inverse.MultiplyPoint3x4(from);
					}
				}
				else
				{
					from = base.worldTransform.inverse.MultiplyPoint3x4(new Vector3(edgePresenter.candidatePosition.x, edgePresenter.candidatePosition.y));
				}
				if (inputPresenter != null)
				{
					if (this.m_InputPresenter != inputPresenter)
					{
						this.m_RightAnchor = (from e in this.m_GraphView.Query(null, null)
						where e.direction == Direction.Input && e.GetPresenter<NodeAnchorPresenter>() == inputPresenter
						select e).First();
						this.m_InputPresenter = inputPresenter;
					}
					if (this.m_RightAnchor != null)
					{
						to = this.m_RightAnchor.GetGlobalCenter();
						to = base.worldTransform.inverse.MultiplyPoint3x4(to);
					}
				}
				else
				{
					to = base.worldTransform.inverse.MultiplyPoint3x4(new Vector3(edgePresenter.candidatePosition.x, edgePresenter.candidatePosition.y));
				}
			}
		}

		public override void OnStyleResolved(ICustomStyle styles)
		{
			base.OnStyleResolved(styles);
			styles.ApplyCustomProperty("edge-width", ref this.m_EdgeWidth);
			styles.ApplyCustomProperty("selected-edge-color", ref this.m_SelectedColor);
			styles.ApplyCustomProperty("edge-color", ref this.m_DefaultColor);
		}

		protected virtual void DrawEdge()
		{
			if (this.UpdateEdgeControl())
			{
				EdgePresenter presenter = base.GetPresenter<EdgePresenter>();
				Color color = (!presenter.selected) ? this.defaultColor : this.selectedColor;
				this.edgeControl.edgeColor = color;
				this.edgeControl.startCapColor = color;
				this.edgeControl.endCapColor = ((!(presenter.input == null)) ? color : this.edgeControl.startCapColor);
			}
		}

		protected virtual EdgeControl CreateEdgeControl()
		{
			return new EdgeControl
			{
				capRadius = 4f,
				interceptWidth = 3f
			};
		}
	}
}
