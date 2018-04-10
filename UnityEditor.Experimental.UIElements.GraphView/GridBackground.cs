using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class GridBackground : VisualElement
	{
		private const string k_SpacingProperty = "spacing";

		private const string k_ThickLinesProperty = "thick-lines";

		private const string k_LineColorProperty = "line-color";

		private const string k_ThickLineColorProperty = "thick-line-color";

		private const string k_GridBackgroundColorProperty = "grid-background-color";

		private StyleValue<float> m_Spacing;

		private StyleValue<int> m_ThickLines;

		private StyleValue<Color> m_LineColor;

		private StyleValue<Color> m_ThickLineColor;

		private StyleValue<Color> m_GridBackgroundColor;

		private VisualElement m_Container;

		private float spacing
		{
			get
			{
				return this.m_Spacing.GetSpecifiedValueOrDefault(50f);
			}
		}

		private int thickLines
		{
			get
			{
				return this.m_ThickLines.GetSpecifiedValueOrDefault(10);
			}
		}

		private Color lineColor
		{
			get
			{
				return this.m_LineColor.GetSpecifiedValueOrDefault(new Color(0f, 0f, 0f, 0.18f));
			}
		}

		private Color thickLineColor
		{
			get
			{
				return this.m_ThickLineColor.GetSpecifiedValueOrDefault(new Color(0f, 0f, 0f, 0.38f));
			}
		}

		private Color gridBackgroundColor
		{
			get
			{
				return this.m_GridBackgroundColor.GetSpecifiedValueOrDefault(new Color(0.17f, 0.17f, 0.17f, 1f));
			}
		}

		public GridBackground()
		{
			base.pickingMode = PickingMode.Ignore;
			this.StretchToParentSize();
		}

		private Vector3 Clip(Rect clipRect, Vector3 _in)
		{
			if (_in.x < clipRect.xMin)
			{
				_in.x = clipRect.xMin;
			}
			if (_in.x > clipRect.xMax)
			{
				_in.x = clipRect.xMax;
			}
			if (_in.y < clipRect.yMin)
			{
				_in.y = clipRect.yMin;
			}
			if (_in.y > clipRect.yMax)
			{
				_in.y = clipRect.yMax;
			}
			return _in;
		}

		public override void OnStyleResolved(ICustomStyle elementStyle)
		{
			base.OnStyleResolved(elementStyle);
			elementStyle.ApplyCustomProperty("spacing", ref this.m_Spacing);
			elementStyle.ApplyCustomProperty("thick-lines", ref this.m_ThickLines);
			elementStyle.ApplyCustomProperty("thick-line-color", ref this.m_ThickLineColor);
			elementStyle.ApplyCustomProperty("line-color", ref this.m_LineColor);
			elementStyle.ApplyCustomProperty("grid-background-color", ref this.m_GridBackgroundColor);
		}

		public override void DoRepaint()
		{
			VisualElement parent = base.parent;
			GraphView graphView = parent as GraphView;
			if (graphView == null)
			{
				throw new InvalidOperationException("GridBackground can only be added to a GraphView");
			}
			this.m_Container = graphView.contentViewContainer;
			Rect layout = graphView.layout;
			layout.x = 0f;
			layout.y = 0f;
			Vector3 vector = new Vector3(this.m_Container.transform.matrix.GetColumn(0).magnitude, this.m_Container.transform.matrix.GetColumn(1).magnitude, this.m_Container.transform.matrix.GetColumn(2).magnitude);
			Vector4 column = this.m_Container.transform.matrix.GetColumn(3);
			Rect layout2 = this.m_Container.layout;
			HandleUtility.ApplyWireMaterial();
			GL.Begin(7);
			GL.Color(this.gridBackgroundColor);
			GL.Vertex(new Vector3(layout.x, layout.y));
			GL.Vertex(new Vector3(layout.xMax, layout.y));
			GL.Vertex(new Vector3(layout.xMax, layout.yMax));
			GL.Vertex(new Vector3(layout.x, layout.yMax));
			GL.End();
			Vector3 vector2 = new Vector3(layout.x, layout.y, 0f);
			Vector3 vector3 = new Vector3(layout.x, layout.height, 0f);
			Matrix4x4 matrix4x = Matrix4x4.TRS(column, Quaternion.identity, Vector3.one);
			vector2 = matrix4x.MultiplyPoint(vector2);
			vector3 = matrix4x.MultiplyPoint(vector3);
			vector2.x += layout2.x * vector.x;
			vector2.y += layout2.y * vector.y;
			vector3.x += layout2.x * vector.x;
			vector3.y += layout2.y * vector.y;
			Handles.DrawWireDisc(vector2, new Vector3(0f, 0f, -1f), 6f);
			float x = vector2.x;
			float y = vector2.y;
			vector2.x = vector2.x % (this.spacing * vector.x) - this.spacing * vector.x;
			vector3.x = vector2.x;
			vector2.y = layout.y;
			vector3.y = layout.y + layout.height;
			while (vector2.x < layout.width)
			{
				vector2.x += this.spacing * vector.x;
				vector3.x += this.spacing * vector.x;
				GL.Begin(1);
				GL.Color(this.lineColor);
				GL.Vertex(this.Clip(layout, vector2));
				GL.Vertex(this.Clip(layout, vector3));
				GL.End();
			}
			float num = this.spacing * (float)this.thickLines;
			vector2.x = (vector3.x = x % (num * vector.x) - num * vector.x);
			while (vector2.x < layout.width + num)
			{
				GL.Begin(1);
				GL.Color(this.thickLineColor);
				GL.Vertex(this.Clip(layout, vector2));
				GL.Vertex(this.Clip(layout, vector3));
				GL.End();
				vector2.x += this.spacing * vector.x * (float)this.thickLines;
				vector3.x += this.spacing * vector.x * (float)this.thickLines;
			}
			vector2 = new Vector3(layout.x, layout.y, 0f);
			vector3 = new Vector3(layout.x + layout.width, layout.y, 0f);
			vector2.x += layout2.x * vector.x;
			vector2.y += layout2.y * vector.y;
			vector3.x += layout2.x * vector.x;
			vector3.y += layout2.y * vector.y;
			vector2 = matrix4x.MultiplyPoint(vector2);
			vector3 = matrix4x.MultiplyPoint(vector3);
			vector2.y = (vector3.y = vector2.y % (this.spacing * vector.y) - this.spacing * vector.y);
			vector2.x = layout.x;
			vector3.x = layout.width;
			while (vector2.y < layout.height)
			{
				vector2.y += this.spacing * vector.y;
				vector3.y += this.spacing * vector.y;
				GL.Begin(1);
				GL.Color(this.lineColor);
				GL.Vertex(this.Clip(layout, vector2));
				GL.Vertex(this.Clip(layout, vector3));
				GL.End();
			}
			num = this.spacing * (float)this.thickLines;
			vector2.y = (vector3.y = y % (num * vector.y) - num * vector.y);
			while (vector2.y < layout.height + num)
			{
				GL.Begin(1);
				GL.Color(this.thickLineColor);
				GL.Vertex(this.Clip(layout, vector2));
				GL.Vertex(this.Clip(layout, vector3));
				GL.End();
				vector2.y += this.spacing * vector.y * (float)this.thickLines;
				vector3.y += this.spacing * vector.y * (float)this.thickLines;
			}
		}
	}
}
