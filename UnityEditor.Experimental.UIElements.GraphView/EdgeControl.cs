using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class EdgeControl : VisualElement
	{
		private Orientation m_Orientation;

		private LineType m_LineType;

		private Color m_EdgeColor;

		private Color m_StartCapColor;

		private Color m_EndCapColor;

		private float m_CapRadius = 5f;

		private int m_EdgeWidth = 2;

		private float m_InterceptWidth = 5f;

		private Vector2 m_From;

		private Vector2 m_To;

		private bool m_TangentsDirty;

		private Vector3[] m_ControlPoints;

		private Vector3[] m_RenderPoints;

		public Orientation orientation
		{
			get
			{
				return this.m_Orientation;
			}
			set
			{
				if (this.m_Orientation != value)
				{
					this.m_Orientation = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public LineType lineType
		{
			get
			{
				return this.m_LineType;
			}
			set
			{
				if (this.m_LineType != value)
				{
					this.m_LineType = value;
					this.PointsChanged();
				}
			}
		}

		public Color edgeColor
		{
			get
			{
				return this.m_EdgeColor;
			}
			set
			{
				if (!(this.m_EdgeColor == value))
				{
					this.m_EdgeColor = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public Color startCapColor
		{
			get
			{
				return this.m_StartCapColor;
			}
			set
			{
				if (!(this.m_StartCapColor == value))
				{
					this.m_StartCapColor = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public Color endCapColor
		{
			get
			{
				return this.m_EndCapColor;
			}
			set
			{
				if (!(this.m_EndCapColor == value))
				{
					this.m_EndCapColor = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public float capRadius
		{
			get
			{
				return this.m_CapRadius;
			}
			set
			{
				if (this.m_CapRadius != value)
				{
					this.m_CapRadius = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public int edgeWidth
		{
			get
			{
				return this.m_EdgeWidth;
			}
			set
			{
				if (this.m_EdgeWidth != value)
				{
					this.m_EdgeWidth = value;
					base.Dirty(ChangeType.Repaint);
				}
			}
		}

		public float interceptWidth
		{
			get
			{
				return this.m_InterceptWidth;
			}
			set
			{
				this.m_InterceptWidth = value;
			}
		}

		public Vector2 from
		{
			get
			{
				return this.m_From;
			}
			set
			{
				if (this.m_From != value)
				{
					this.m_From = value;
					this.PointsChanged();
				}
			}
		}

		public Vector2 to
		{
			get
			{
				return this.m_To;
			}
			set
			{
				if (this.m_To != value)
				{
					this.m_To = value;
					this.PointsChanged();
				}
			}
		}

		public Vector3[] controlPoints
		{
			get
			{
				if (this.m_TangentsDirty || this.m_ControlPoints == null)
				{
					this.CacheLineData();
					this.m_TangentsDirty = false;
				}
				return this.m_ControlPoints;
			}
		}

		public Vector3[] renderPoints
		{
			get
			{
				if (this.m_TangentsDirty || this.m_RenderPoints == null)
				{
					this.CacheLineData();
					this.m_TangentsDirty = false;
				}
				return this.m_RenderPoints;
			}
		}

		protected virtual void DrawEdge()
		{
			Vector3[] controlPoints = this.controlPoints;
			switch (this.lineType)
			{
			case LineType.Bezier:
				Handles.DrawBezier(controlPoints[0], controlPoints[3], controlPoints[1], controlPoints[2], this.edgeColor, null, (float)this.edgeWidth);
				break;
			case LineType.PolyLine:
			case LineType.StraightLine:
				Handles.color = this.edgeColor;
				Handles.DrawAAPolyLine((float)this.edgeWidth, this.renderPoints);
				break;
			default:
				throw new ArgumentOutOfRangeException("Unsupported LineType: " + this.lineType);
			}
		}

		protected virtual void DrawEndpoint(Vector2 pos)
		{
			Handles.DrawSolidDisc(pos, new Vector3(0f, 0f, -1f), this.capRadius);
		}

		public override void DoRepaint()
		{
			Color color = Handles.color;
			this.DrawEdge();
			Handles.color = this.startCapColor;
			this.DrawEndpoint(this.from);
			Handles.color = this.endCapColor;
			this.DrawEndpoint(this.to);
			Handles.color = color;
		}

		public override bool ContainsPoint(Vector2 localPoint)
		{
			bool result;
			if (Vector2.Distance(this.from, localPoint) <= 2f * this.capRadius || Vector2.Distance(this.to, localPoint) <= 2f * this.capRadius)
			{
				result = false;
			}
			else
			{
				Vector3[] renderPoints = this.renderPoints;
				float num = float.PositiveInfinity;
				for (int i = 0; i < renderPoints.Length; i++)
				{
					Vector3 a = renderPoints[i];
					float b = Vector3.Distance(a, localPoint);
					num = Mathf.Min(num, b);
					if (num < this.interceptWidth)
					{
						result = true;
						return result;
					}
				}
				result = false;
			}
			return result;
		}

		public override bool Overlaps(Rect rect)
		{
			Vector3[] renderPoints = this.renderPoints;
			bool result;
			for (int i = 0; i < renderPoints.Length - 1; i++)
			{
				Vector2 p = new Vector2(renderPoints[i].x, renderPoints[i].y);
				Vector2 p2 = new Vector2(renderPoints[i + 1].x, renderPoints[i + 1].y);
				if (RectUtils.IntersectsSegment(rect, p, p2))
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		private void PointsChanged()
		{
			this.m_TangentsDirty = true;
			base.layout = new Rect(Vector2.Min(this.m_To, this.m_From), new Vector2(Mathf.Abs(this.m_From.x - this.m_To.x), Mathf.Abs(this.m_From.y - this.m_To.y)));
			base.Dirty(ChangeType.Repaint);
		}

		private void CacheLineData()
		{
			Vector2 vector = this.to;
			Vector2 vector2 = this.from;
			if (this.orientation == Orientation.Horizontal && this.from.x < this.to.x)
			{
				vector = this.from;
				vector2 = this.to;
			}
			if (this.lineType == LineType.StraightLine)
			{
				if (this.m_ControlPoints == null || this.m_ControlPoints.Length != 2)
				{
					this.m_ControlPoints = new Vector3[2];
				}
				this.m_ControlPoints[0] = vector;
				this.m_ControlPoints[1] = vector2;
				this.m_RenderPoints = this.m_ControlPoints;
			}
			else
			{
				if (this.m_ControlPoints == null || this.m_ControlPoints.Length != 4)
				{
					this.m_ControlPoints = new Vector3[4];
				}
				this.m_ControlPoints[0] = vector;
				this.m_ControlPoints[3] = vector2;
				switch (this.lineType)
				{
				case LineType.Bezier:
				{
					float num = 0.5f;
					float num2 = 1f - num;
					float num3 = 0f;
					float d = Mathf.Clamp01(((vector - vector2).magnitude - 10f) / 50f);
					if (this.orientation == Orientation.Horizontal)
					{
						this.m_ControlPoints[1] = vector + new Vector2((vector2.x - vector.x) * num + 30f, num3) * d;
						this.m_ControlPoints[2] = vector2 + new Vector2((vector2.x - vector.x) * -num2 - 30f, -num3) * d;
					}
					else
					{
						float num4 = vector.y - vector2.y + 100f;
						num4 = Mathf.Min((vector - vector2).magnitude, num4);
						if (num4 < 0f)
						{
							num4 = -num4;
						}
						this.m_ControlPoints[1] = vector + new Vector2(0f, num4 * -0.5f);
						this.m_ControlPoints[2] = vector2 + new Vector2(0f, num4 * 0.5f);
					}
					this.m_RenderPoints = Handles.MakeBezierPoints(this.m_ControlPoints[0], this.m_ControlPoints[3], this.m_ControlPoints[1], this.m_ControlPoints[2], 20);
					break;
				}
				case LineType.PolyLine:
					if (this.orientation == Orientation.Horizontal)
					{
						this.m_ControlPoints[2] = new Vector2((vector.x + vector2.x) / 2f, vector2.y);
						this.m_ControlPoints[1] = new Vector2((vector.x + vector2.x) / 2f, vector.y);
					}
					else
					{
						this.m_ControlPoints[2] = new Vector2(vector2.x, (vector.y + vector2.y) / 2f);
						this.m_ControlPoints[1] = new Vector2(vector.x, (vector.y + vector2.y) / 2f);
					}
					this.m_RenderPoints = this.m_ControlPoints;
					break;
				case LineType.StraightLine:
					break;
				default:
					throw new ArgumentOutOfRangeException("Unsupported LineType: " + this.lineType);
				}
			}
		}
	}
}
