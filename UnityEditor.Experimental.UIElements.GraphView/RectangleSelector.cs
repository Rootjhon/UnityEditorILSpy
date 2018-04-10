using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class RectangleSelector : MouseManipulator
	{
		private class RectangleSelect : VisualElement
		{
			public Vector2 start
			{
				get;
				set;
			}

			public Vector2 end
			{
				get;
				set;
			}

			public override void DoRepaint()
			{
				VisualElement parent = base.parent;
				Vector2 a = this.start;
				Vector2 a2 = this.end;
				if (!(this.start == this.end))
				{
					a += parent.layout.position;
					a2 += parent.layout.position;
					Rect rect = new Rect
					{
						min = new Vector2(Math.Min(a.x, a2.x), Math.Min(a.y, a2.y)),
						max = new Vector2(Math.Max(a.x, a2.x), Math.Max(a.y, a2.y))
					};
					Color col = new Color(1f, 0.6f, 0f, 1f);
					float segmentsLength = 5f;
					Vector3[] array = new Vector3[]
					{
						new Vector3(rect.xMin, rect.yMin, 0f),
						new Vector3(rect.xMax, rect.yMin, 0f),
						new Vector3(rect.xMax, rect.yMax, 0f),
						new Vector3(rect.xMin, rect.yMax, 0f)
					};
					this.DrawDottedLine(array[0], array[1], segmentsLength, col);
					this.DrawDottedLine(array[1], array[2], segmentsLength, col);
					this.DrawDottedLine(array[2], array[3], segmentsLength, col);
					this.DrawDottedLine(array[3], array[0], segmentsLength, col);
					string text = string.Concat(new string[]
					{
						"(",
						string.Format("{0:0}", this.start.x),
						", ",
						string.Format("{0:0}", this.start.y),
						")"
					});
					GUI.skin.label.Draw(new Rect(a.x, a.y - 18f, 200f, 20f), new GUIContent(text), 0);
					text = string.Concat(new string[]
					{
						"(",
						string.Format("{0:0}", this.end.x),
						", ",
						string.Format("{0:0}", this.end.y),
						")"
					});
					GUI.skin.label.Draw(new Rect(a2.x - 80f, a2.y + 5f, 200f, 20f), new GUIContent(text), 0);
				}
			}

			private void DrawDottedLine(Vector3 p1, Vector3 p2, float segmentsLength, Color col)
			{
				HandleUtility.ApplyWireMaterial();
				GL.Begin(1);
				GL.Color(col);
				float num = Vector3.Distance(p1, p2);
				int num2 = Mathf.CeilToInt(num / segmentsLength);
				for (int i = 0; i < num2; i += 2)
				{
					GL.Vertex(Vector3.Lerp(p1, p2, (float)i * segmentsLength / num));
					GL.Vertex(Vector3.Lerp(p1, p2, (float)(i + 1) * segmentsLength / num));
				}
				GL.End();
			}
		}

		private readonly RectangleSelector.RectangleSelect m_Rectangle;

		private bool m_Active;

		public RectangleSelector()
		{
			base.activators.Add(new ManipulatorActivationFilter
			{
				button = MouseButton.LeftMouse
			});
			this.m_Rectangle = new RectangleSelector.RectangleSelect();
			this.m_Rectangle.style.positionType = PositionType.Absolute;
			this.m_Rectangle.style.positionTop = 0f;
			this.m_Rectangle.style.positionLeft = 0f;
			this.m_Rectangle.style.positionBottom = 0f;
			this.m_Rectangle.style.positionRight = 0f;
			this.m_Active = false;
		}

		public Rect ComputeAxisAlignedBound(Rect position, Matrix4x4 transform)
		{
			Vector3 vector = transform.MultiplyPoint3x4(position.min);
			Vector3 vector2 = transform.MultiplyPoint3x4(position.max);
			return Rect.MinMaxRect(Math.Min(vector.x, vector2.x), Math.Min(vector.y, vector2.y), Math.Max(vector.x, vector2.x), Math.Max(vector.y, vector2.y));
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (!(base.target is GraphView))
			{
				throw new InvalidOperationException("Manipulator can only be added to a GraphView");
			}
			base.target.RegisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.RegisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
			base.target.RegisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<MouseDownEvent>(new EventCallback<MouseDownEvent>(this.OnMouseDown), Capture.NoCapture);
			base.target.UnregisterCallback<MouseUpEvent>(new EventCallback<MouseUpEvent>(this.OnMouseUp), Capture.NoCapture);
			base.target.UnregisterCallback<MouseMoveEvent>(new EventCallback<MouseMoveEvent>(this.OnMouseMove), Capture.NoCapture);
		}

		private void OnMouseDown(MouseDownEvent e)
		{
			if (e.target == base.target)
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					if (base.CanStartManipulation(e))
					{
						if (!e.ctrlKey)
						{
							graphView.ClearSelection();
						}
						graphView.Add(this.m_Rectangle);
						this.m_Rectangle.start = e.localMousePosition;
						this.m_Rectangle.end = this.m_Rectangle.start;
						this.m_Active = true;
						base.target.TakeCapture();
						e.StopPropagation();
					}
				}
			}
		}

		private void OnMouseUp(MouseUpEvent e)
		{
			if (this.m_Active)
			{
				GraphView graphView = base.target as GraphView;
				if (graphView != null)
				{
					if (base.CanStopManipulation(e))
					{
						graphView.Remove(this.m_Rectangle);
						this.m_Rectangle.end = e.localMousePosition;
						Rect selectionRect = new Rect
						{
							min = new Vector2(Math.Min(this.m_Rectangle.start.x, this.m_Rectangle.end.x), Math.Min(this.m_Rectangle.start.y, this.m_Rectangle.end.y)),
							max = new Vector2(Math.Max(this.m_Rectangle.start.x, this.m_Rectangle.end.x), Math.Max(this.m_Rectangle.start.y, this.m_Rectangle.end.y))
						};
						selectionRect = this.ComputeAxisAlignedBound(selectionRect, graphView.viewTransform.matrix.inverse);
						List<ISelectable> selection = graphView.selection;
						List<ISelectable> newSelection = new List<ISelectable>();
						graphView.graphElements.ForEach(delegate(GraphElement child)
						{
							Matrix4x4 inverse = child.transform.matrix.inverse;
							Rect rectangle = new Rect(inverse.MultiplyPoint3x4(selectionRect.position), inverse.MultiplyPoint3x4(selectionRect.size));
							if (child.IsSelectable() && child.Overlaps(rectangle))
							{
								newSelection.Add(child);
							}
						});
						foreach (ISelectable current in newSelection)
						{
							if (selection.Contains(current))
							{
								if (e.ctrlKey)
								{
									graphView.RemoveFromSelection(current);
								}
							}
							else
							{
								graphView.AddToSelection(current);
							}
						}
						this.m_Active = false;
						base.target.ReleaseCapture();
						e.StopPropagation();
					}
				}
			}
		}

		private void OnMouseMove(MouseMoveEvent e)
		{
			if (this.m_Active)
			{
				this.m_Rectangle.end = e.localMousePosition;
				e.StopPropagation();
			}
		}
	}
}
