using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class RectUtils
	{
		public static bool IntersectsSegment(Rect rect, Vector2 p1, Vector2 p2)
		{
			float num = Mathf.Min(p1.x, p2.x);
			float num2 = Mathf.Max(p1.x, p2.x);
			if (num2 > rect.xMax)
			{
				num2 = rect.xMax;
			}
			if (num < rect.xMin)
			{
				num = rect.xMin;
			}
			bool result;
			if (num > num2)
			{
				result = false;
			}
			else
			{
				float num3 = Mathf.Min(p1.y, p2.y);
				float num4 = Mathf.Max(p1.y, p2.y);
				float num5 = p2.x - p1.x;
				if (Mathf.Abs(num5) > 1.401298E-45f)
				{
					float num6 = (p2.y - p1.y) / num5;
					float num7 = p1.y - num6 * p1.x;
					num3 = num6 * num + num7;
					num4 = num6 * num2 + num7;
				}
				if (num3 > num4)
				{
					float num8 = num4;
					num4 = num3;
					num3 = num8;
				}
				if (num4 > rect.yMax)
				{
					num4 = rect.yMax;
				}
				if (num3 < rect.yMin)
				{
					num3 = rect.yMin;
				}
				result = (num3 <= num4);
			}
			return result;
		}

		public static Rect Encompass(Rect a, Rect b)
		{
			return new Rect
			{
				xMin = Math.Min(a.xMin, b.xMin),
				yMin = Math.Min(a.yMin, b.yMin),
				xMax = Math.Max(a.xMax, b.xMax),
				yMax = Math.Max(a.yMax, b.yMax)
			};
		}
	}
}
