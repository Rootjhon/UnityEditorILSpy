using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class Resizer : VisualElement
	{
		private Vector2 m_Start;

		private Rect m_StartPos;

		private Vector2 m_MinimumSize;

		private GUIStyle m_StyleWidget;

		private GUIStyle m_StyleLabel;

		private GUIContent m_LabelText = new GUIContent();

		private readonly Rect k_WidgetTextOffset = new Rect(0f, 0f, 5f, 5f);

		private bool m_Active;

		public MouseButton activateButton
		{
			get;
			set;
		}

		private Texture image
		{
			get;
			set;
		}

		public Resizer() : this(new Vector2(30f, 30f))
		{
		}

		public Resizer(Vector2 minimumSize)
		{
			this.m_MinimumSize = minimumSize;
			base.style.positionType = PositionType.Absolute;
			base.style.positionTop = float.NaN;
			base.style.positionLeft = float.NaN;
			base.style.positionBottom = 0f;
			base.style.positionRight = 0f;
			base.style.paddingLeft = 10f;
			base.style.paddingTop = 14f;
			base.style.width = 20f;
			base.style.height = 20f;
			this.m_Active = false;
			base.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
			base.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
		}

		private void OnMouseDown(MouseDownEvent e)
		{
			GraphElement graphElement = base.parent as GraphElement;
			if (graphElement != null)
			{
				GraphElementPresenter presenter = graphElement.presenter;
				if (!(presenter == null))
				{
					if ((presenter.capabilities & Capabilities.Resizable) == Capabilities.Resizable)
					{
						if (e.button == (int)this.activateButton)
						{
							this.m_Start = this.ChangeCoordinatesTo(base.parent, e.localMousePosition);
							this.m_StartPos = base.parent.layout;
							if (base.parent.style.positionType != PositionType.Manual)
							{
								Debug.LogWarning("Attempting to resize an object with a non manual position");
							}
							this.m_Active = true;
							this.TakeCapture();
							e.StopPropagation();
						}
					}
				}
			}
		}

		private void OnMouseUp(MouseUpEvent e)
		{
			GraphElement graphElement = base.parent as GraphElement;
			if (graphElement != null)
			{
				GraphElementPresenter presenter = graphElement.presenter;
				if (!(presenter == null))
				{
					if ((presenter.capabilities & Capabilities.Resizable) == Capabilities.Resizable)
					{
						if (this.m_Active)
						{
							if (e.button == (int)this.activateButton && this.m_Active)
							{
								this.m_Active = false;
								this.ReleaseCapture();
								e.StopPropagation();
							}
						}
					}
				}
			}
		}

		private void OnMouseMove(MouseMoveEvent e)
		{
			GraphElement graphElement = base.parent as GraphElement;
			if (graphElement != null)
			{
				GraphElementPresenter presenter = graphElement.presenter;
				if (!(presenter == null))
				{
					if ((presenter.capabilities & Capabilities.Resizable) == Capabilities.Resizable)
					{
						if (this.m_Active && base.parent.style.positionType == PositionType.Manual)
						{
							Vector2 vector = this.ChangeCoordinatesTo(base.parent, e.localMousePosition) - this.m_Start;
							Vector2 vector2 = new Vector2(this.m_StartPos.width + vector.x, this.m_StartPos.height + vector.y);
							if (vector2.x < this.m_MinimumSize.x)
							{
								vector2.x = this.m_MinimumSize.x;
							}
							if (vector2.y < this.m_MinimumSize.y)
							{
								vector2.y = this.m_MinimumSize.y;
							}
							presenter.position = new Rect(presenter.position.x, presenter.position.y, vector2.x, vector2.y);
							this.m_LabelText.text = string.Format("{0:0}", base.parent.layout.width) + "x" + string.Format("{0:0}", base.parent.layout.height);
							e.StopPropagation();
						}
					}
				}
			}
		}

		public override void DoRepaint()
		{
			if (this.m_StyleWidget == null)
			{
				this.m_StyleWidget = new GUIStyle("WindowBottomResize")
				{
					fixedHeight = 0f
				};
				this.image = this.m_StyleWidget.normal.background;
			}
			if (this.image == null)
			{
				Debug.LogWarning("null texture passed to GUI.DrawTexture");
			}
			else
			{
				GUI.DrawTexture(base.contentRect, this.image, ScaleMode.ScaleAndCrop, true, 0f, GUI.color, 0f, 0f);
				if (this.m_StyleLabel == null)
				{
					this.m_StyleLabel = new GUIStyle("Label");
				}
				if (this.m_Active)
				{
					Rect rect = this.k_WidgetTextOffset;
					Rect position = new Rect(base.layout.max.x + rect.width, base.layout.max.y + rect.height, 200f, 20f);
					this.m_StyleLabel.Draw(position, this.m_LabelText, false, false, false, false);
				}
			}
		}
	}
}
