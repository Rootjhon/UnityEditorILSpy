using System;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class ChartViewData
	{
		public ChartSeriesViewData[] series
		{
			get;
			private set;
		}

		public ChartSeriesViewData[] overlays
		{
			get;
			private set;
		}

		public int[] order
		{
			get;
			private set;
		}

		public float[] grid
		{
			get;
			private set;
		}

		public string[] gridLabels
		{
			get;
			private set;
		}

		public string[] selectedLabels
		{
			get;
			private set;
		}

		public int firstSelectableFrame
		{
			get;
			private set;
		}

		public bool hasOverlay
		{
			get;
			set;
		}

		public float maxValue
		{
			get;
			set;
		}

		public int numSeries
		{
			get;
			private set;
		}

		public int chartDomainOffset
		{
			get;
			private set;
		}

		public void Assign(ChartSeriesViewData[] series, int firstFrame, int firstSelectableFrame)
		{
			this.series = series;
			this.chartDomainOffset = firstFrame;
			this.firstSelectableFrame = firstSelectableFrame;
			this.numSeries = series.Length;
			if (this.order == null || this.order.Length != this.numSeries)
			{
				this.order = new int[this.numSeries];
				int i = 0;
				int num = this.order.Length;
				while (i < num)
				{
					this.order[i] = this.order.Length - 1 - i;
					i++;
				}
			}
			if (this.overlays == null || this.overlays.Length != this.numSeries)
			{
				this.overlays = new ChartSeriesViewData[this.numSeries];
			}
		}

		public void AssignSelectedLabels(string[] selectedLabels)
		{
			this.selectedLabels = selectedLabels;
		}

		public void SetGrid(float[] grid, string[] labels)
		{
			this.grid = grid;
			this.gridLabels = labels;
		}

		public Vector2 GetDataDomain()
		{
			Vector2 result;
			if (this.series == null || this.numSeries == 0 || this.series[0].numDataPoints == 0)
			{
				result = Vector2.zero;
			}
			else
			{
				Vector2 vector = Vector2.one * this.series[0].xValues[0];
				for (int i = 0; i < this.numSeries; i++)
				{
					if (this.series[i].numDataPoints != 0)
					{
						vector.x = Mathf.Min(vector.x, this.series[i].xValues[0]);
						vector.y = Mathf.Max(vector.y, this.series[i].xValues[this.series[i].numDataPoints - 1]);
					}
				}
				result = vector;
			}
			return result;
		}
	}
}
