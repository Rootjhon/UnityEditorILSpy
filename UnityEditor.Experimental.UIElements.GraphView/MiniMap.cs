using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class MiniMap : GraphElement
	{
		private float m_PreviousContainerWidth = -1f;

		private float m_PreviousContainerHeight = -1f;

		private readonly Label m_Label;

		private Dragger m_Dragger;

		private readonly Color m_ViewportColor = new Color(1f, 1f, 0f, 0.35f);

		private readonly Color m_SelectedChildrenColor = new Color(1f, 1f, 1f, 0.5f);

		private Rect m_ViewportRect;

		private Rect m_ContentRect;

		private Rect m_ContentRectLocal;

		private int titleBarOffset
		{
			get
			{
				return (int)base.style.paddingTop;
			}
		}

		public MiniMap()
		{
			base.clippingOptions = VisualElement.ClippingOptions.NoClipping;
			this.m_Label = new Label("Floating Minimap");
			base.Add(this.m_Label);
			base.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.ShowContextualMenu), Capture.NoCapture);
			base.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
		}

		protected void ShowContextualMenu(MouseUpEvent e)
		{
			if (e.button == 1)
			{
				MiniMapPresenter presenter = base.GetPresenter<MiniMapPresenter>();
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent((!presenter.anchored) ? "Anchor" : "Make floating"), false, delegate(object contentView)
				{
					MiniMapPresenter presenter2 = base.GetPresenter<MiniMapPresenter>();
					presenter2.anchored = !presenter2.anchored;
				}, this);
				genericMenu.DropDown(new Rect(e.mousePosition.x, e.mousePosition.y, 0f, 0f));
				e.StopPropagation();
			}
		}

		public override void OnDataChanged()
		{
			base.OnDataChanged();
			this.AdjustAnchoring();
			this.Resize();
		}

		private void AdjustAnchoring()
		{
			MiniMapPresenter presenter = base.GetPresenter<MiniMapPresenter>();
			if (!(presenter == null))
			{
				if (presenter.anchored)
				{
					presenter.capabilities &= ~Capabilities.Movable;
					base.ResetPositionProperties();
					base.AddToClassList("anchored");
				}
				else
				{
					if (this.m_Dragger == null)
					{
						this.m_Dragger = new Dragger
						{
							clampToParentEdges = true
						};
						this.AddManipulator(this.m_Dragger);
					}
					base.presenter.capabilities |= Capabilities.Movable;
					base.RemoveFromClassList("anchored");
				}
			}
		}

		private void Resize()
		{
			if (base.parent != null)
			{
				MiniMapPresenter presenter = base.GetPresenter<MiniMapPresenter>();
				base.style.width = presenter.maxWidth;
				base.style.height = presenter.maxHeight;
				if (base.style.positionLeft + base.style.width > base.parent.layout.x + base.parent.layout.width)
				{
					Rect position = presenter.position;
					position.x -= base.style.positionLeft + base.style.width - (base.parent.layout.x + base.parent.layout.width);
					presenter.position = position;
				}
				if (base.style.positionTop + base.style.height > base.parent.layout.y + base.parent.layout.height)
				{
					Rect position2 = presenter.position;
					position2.y -= base.style.positionTop + base.style.height - (base.parent.layout.y + base.parent.layout.height);
					presenter.position = position2;
				}
				Rect position3 = presenter.position;
				position3.width = base.style.width;
				position3.height = base.style.height;
				position3.x = Mathf.Max(base.parent.layout.x, position3.x);
				position3.y = Mathf.Max(base.parent.layout.y, position3.y);
				presenter.position = position3;
				if (!presenter.anchored)
				{
					base.layout = presenter.position;
				}
			}
		}

		private static void ChangeToMiniMapCoords(ref Rect rect, float factor, Vector3 translation)
		{
			rect.width *= factor;
			rect.height *= factor;
			rect.x *= factor;
			rect.y *= factor;
			rect.x += translation.x;
			rect.y += translation.y;
		}

		private void CalculateRects(GraphView gView)
		{
			this.m_ContentRect = gView.CalculateRectToFitAll();
			this.m_ContentRectLocal = this.m_ContentRect;
			Matrix4x4 inverse = gView.contentViewContainer.worldTransform.inverse;
			Vector4 column = inverse.GetColumn(3);
			Vector2 vector = new Vector2(inverse.m00, inverse.m11);
			this.m_ViewportRect = base.parent.layout;
			this.m_ViewportRect.x = this.m_ViewportRect.x + column.x;
			this.m_ViewportRect.y = this.m_ViewportRect.y + column.y;
			this.m_ViewportRect.width = this.m_ViewportRect.width * vector.x;
			this.m_ViewportRect.height = this.m_ViewportRect.height * vector.y;
			this.m_Label.text = "MiniMap v: " + string.Format("{0:0}", this.m_ViewportRect.width) + "x" + string.Format("{0:0}", this.m_ViewportRect.height);
			Rect rect = RectUtils.Encompass(this.m_ContentRect, this.m_ViewportRect);
			float factor = base.layout.width / rect.width;
			MiniMap.ChangeToMiniMapCoords(ref rect, factor, Vector3.zero);
			Vector3 translation = new Vector3(base.layout.x - rect.x, base.layout.y + (float)this.titleBarOffset - rect.y);
			MiniMap.ChangeToMiniMapCoords(ref this.m_ViewportRect, factor, translation);
			MiniMap.ChangeToMiniMapCoords(ref this.m_ContentRect, factor, translation);
			if (rect.height > base.layout.height - (float)this.titleBarOffset)
			{
				float num = (base.layout.height - (float)this.titleBarOffset) / rect.height;
				float num2 = (base.layout.width - rect.width * num) / 2f;
				float num3 = base.layout.y + (float)this.titleBarOffset - (rect.y + translation.y) * num;
				this.m_ContentRect.width = this.m_ContentRect.width * num;
				this.m_ContentRect.height = this.m_ContentRect.height * num;
				this.m_ContentRect.y = this.m_ContentRect.y * num;
				this.m_ContentRect.x = this.m_ContentRect.x + num2;
				this.m_ContentRect.y = this.m_ContentRect.y + num3;
				this.m_ViewportRect.width = this.m_ViewportRect.width * num;
				this.m_ViewportRect.height = this.m_ViewportRect.height * num;
				this.m_ViewportRect.y = this.m_ViewportRect.y * num;
				this.m_ViewportRect.x = this.m_ViewportRect.x + num2;
				this.m_ViewportRect.y = this.m_ViewportRect.y + num3;
			}
		}

		private Rect CalculateElementRect(GraphElement elem)
		{
			GraphElementPresenter presenter = elem.GetPresenter<GraphElementPresenter>();
			Rect result;
			if ((presenter.capabilities & Capabilities.Floating) != (Capabilities)0 || presenter is EdgePresenter)
			{
				result = new Rect(0f, 0f, 0f, 0f);
			}
			else
			{
				Rect localBound = elem.localBound;
				localBound.x = this.m_ContentRect.x + (localBound.x - this.m_ContentRectLocal.x) * this.m_ContentRect.width / this.m_ContentRectLocal.width;
				localBound.y = this.m_ContentRect.y + (localBound.y - this.m_ContentRectLocal.y) * this.m_ContentRect.height / this.m_ContentRectLocal.height;
				localBound.width *= this.m_ContentRect.width / this.m_ContentRectLocal.width;
				localBound.height *= this.m_ContentRect.height / this.m_ContentRectLocal.height;
				float num = base.layout.xMin + 2f;
				float num2 = base.layout.xMax - 2f;
				float num3 = base.layout.yMax - 2f;
				if (localBound.x < num)
				{
					if (localBound.x < num - localBound.width)
					{
						result = new Rect(0f, 0f, 0f, 0f);
						return result;
					}
					localBound.width -= num - localBound.x;
					localBound.x = num;
				}
				if (localBound.x + localBound.width >= num2)
				{
					if (localBound.x >= num2)
					{
						result = new Rect(0f, 0f, 0f, 0f);
						return result;
					}
					localBound.width -= localBound.x + localBound.width - num2;
				}
				if (localBound.y < base.layout.yMin + (float)this.titleBarOffset)
				{
					if (localBound.y < base.layout.yMin + (float)this.titleBarOffset - localBound.height)
					{
						result = new Rect(0f, 0f, 0f, 0f);
						return result;
					}
					localBound.height -= base.layout.yMin + (float)this.titleBarOffset - localBound.y;
					localBound.y = base.layout.yMin + (float)this.titleBarOffset;
				}
				if (localBound.y + localBound.height >= num3)
				{
					if (localBound.y >= num3)
					{
						result = new Rect(0f, 0f, 0f, 0f);
						return result;
					}
					localBound.height -= localBound.y + localBound.height - num3;
				}
				result = localBound;
			}
			return result;
		}

		public override void DoRepaint()
		{
			GraphView firstAncestorOfType = base.GetFirstAncestorOfType<GraphView>();
			Matrix4x4 matrix = firstAncestorOfType.viewTransform.matrix;
			Vector2 vector = new Vector2(matrix.m00, matrix.m11);
			float num = base.parent.layout.width / vector.x;
			float num2 = base.parent.layout.height / vector.y;
			if (Mathf.Abs(num - this.m_PreviousContainerWidth) > Mathf.Epsilon || Mathf.Abs(num2 - this.m_PreviousContainerHeight) > Mathf.Epsilon)
			{
				this.m_PreviousContainerWidth = num;
				this.m_PreviousContainerHeight = num2;
				this.Resize();
			}
			this.CalculateRects(firstAncestorOfType);
			base.DoRepaint();
			Color color = Handles.color;
			firstAncestorOfType.graphElements.ForEach(delegate(GraphElement elem)
			{
				Rect rect = this.CalculateElementRect(elem);
				Handles.color = elem.elementTypeColor;
				Handles.DrawSolidRectangleWithOutline(rect, elem.elementTypeColor, elem.elementTypeColor);
				GraphElementPresenter presenter = elem.GetPresenter<GraphElementPresenter>();
				if (presenter != null && presenter.selected)
				{
					this.DrawRectangleOutline(rect, this.m_SelectedChildrenColor);
				}
			});
			this.DrawRectangleOutline(this.m_ViewportRect, this.m_ViewportColor);
			Handles.color = color;
		}

		private void DrawRectangleOutline(Rect rect, Color color)
		{
			Color color2 = Handles.color;
			Handles.color = color;
			Handles.DrawPolyLine(new Vector3[]
			{
				new Vector3(rect.x, rect.y, 0f),
				new Vector3(rect.x + rect.width, rect.y, 0f),
				new Vector3(rect.x + rect.width, rect.y + rect.height, 0f),
				new Vector3(rect.x, rect.y + rect.height, 0f),
				new Vector3(rect.x, rect.y, 0f)
			});
			Handles.color = color2;
		}

		protected void OnMouseDown(MouseDownEvent e)
		{
			MiniMap.<OnMouseDown>c__AnonStorey0 <OnMouseDown>c__AnonStorey = new MiniMap.<OnMouseDown>c__AnonStorey0();
			<OnMouseDown>c__AnonStorey.e = e;
			<OnMouseDown>c__AnonStorey.$this = this;
			<OnMouseDown>c__AnonStorey.gView = base.GetFirstAncestorOfType<GraphView>();
			this.CalculateRects(<OnMouseDown>c__AnonStorey.gView);
			<OnMouseDown>c__AnonStorey.mousePosition = <OnMouseDown>c__AnonStorey.e.localMousePosition;
			MiniMap.<OnMouseDown>c__AnonStorey0 expr_44_cp_0 = <OnMouseDown>c__AnonStorey;
			expr_44_cp_0.mousePosition.x = expr_44_cp_0.mousePosition.x + base.layout.x;
			MiniMap.<OnMouseDown>c__AnonStorey0 expr_64_cp_0 = <OnMouseDown>c__AnonStorey;
			expr_64_cp_0.mousePosition.y = expr_64_cp_0.mousePosition.y + base.layout.y;
			<OnMouseDown>c__AnonStorey.gView.graphElements.ForEach(delegate(GraphElement child)
			{
				if (child != null)
				{
					ISelectable firstOfType = child.GetFirstOfType<ISelectable>();
					if (firstOfType != null && firstOfType.IsSelectable())
					{
						if (<OnMouseDown>c__AnonStorey.$this.CalculateElementRect(child).Contains(<OnMouseDown>c__AnonStorey.mousePosition))
						{
							<OnMouseDown>c__AnonStorey.gView.ClearSelection();
							<OnMouseDown>c__AnonStorey.gView.AddToSelection(firstOfType);
							<OnMouseDown>c__AnonStorey.gView.FrameSelection();
							<OnMouseDown>c__AnonStorey.e.StopPropagation();
						}
					}
				}
			});
		}
	}
}
