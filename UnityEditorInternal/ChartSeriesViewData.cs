using System;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class ChartSeriesViewData
	{
		public bool enabled;

		public string name
		{
			get;
			private set;
		}

		public Color color
		{
			get;
			private set;
		}

		public float[] xValues
		{
			get;
			private set;
		}

		public float[] yValues
		{
			get;
			private set;
		}

		public Vector2 rangeAxis
		{
			get;
			set;
		}

		public int numDataPoints
		{
			get;
			private set;
		}

		public ChartSeriesViewData(string name, int numDataPoints, Color color)
		{
			this.name = name;
			this.color = color;
			this.numDataPoints = numDataPoints;
			this.xValues = new float[numDataPoints];
			this.yValues = new float[numDataPoints];
			this.enabled = true;
		}
	}
}
