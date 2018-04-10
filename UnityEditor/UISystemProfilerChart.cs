using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class UISystemProfilerChart : ProfilerChart
	{
		private EventMarker[] m_Markers;

		private string[] m_MarkerNames;

		public bool showMarkers = true;

		public UISystemProfilerChart(Chart.ChartType type, float dataScale, int seriesCount) : base(ProfilerArea.UIDetails, type, dataScale, seriesCount)
		{
		}

		public void Update(int firstFrame, int historyLength)
		{
			int uISystemEventMarkersCount = ProfilerDriver.GetUISystemEventMarkersCount(firstFrame, historyLength);
			if (uISystemEventMarkersCount != 0)
			{
				this.m_Markers = new EventMarker[uISystemEventMarkersCount];
				this.m_MarkerNames = new string[uISystemEventMarkersCount];
				ProfilerDriver.GetUISystemEventMarkersBatch(firstFrame, historyLength, this.m_Markers, this.m_MarkerNames);
			}
		}

		public override int DoChartGUI(int currentFrame, ProfilerArea currentArea)
		{
			int result = base.DoChartGUI(currentFrame, currentArea);
			if (this.m_Markers != null && this.showMarkers)
			{
				Rect lastRect = GUILayoutUtility.GetLastRect();
				lastRect.xMin += 180f;
				for (int i = 0; i < this.m_Markers.Length; i++)
				{
					EventMarker eventMarker = this.m_Markers[i];
					Color color = ProfilerColors.currentColors[(int)(checked((IntPtr)(unchecked((ulong)this.m_Series.Length % (ulong)((long)ProfilerColors.currentColors.Length)))))];
					Chart.DrawVerticalLine(eventMarker.frame, this.m_Data, lastRect, color.AlphaMultiplied(0.3f), color.AlphaMultiplied(0.4f), 1f);
				}
				this.DrawMarkerLabels(this.m_Data, lastRect, this.m_Markers, this.m_MarkerNames);
			}
			return result;
		}

		private void DrawMarkerLabels(ChartViewData cdata, Rect r, EventMarker[] markers, string[] markerNames)
		{
			Color contentColor = GUI.contentColor;
			Vector2 dataDomain = cdata.GetDataDomain();
			int num = (int)(dataDomain.y - dataDomain.x);
			float num2 = r.width / (float)num;
			int num3 = (int)(r.height / 12f);
			if (num3 != 0)
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				for (int i = 0; i < markers.Length; i++)
				{
					int num4 = markers[i].frame;
					int num5;
					if (!dictionary.TryGetValue(markers[i].nameOffset, out num5) || num5 != num4 - 1 || num5 < cdata.chartDomainOffset)
					{
						num4 -= cdata.chartDomainOffset;
						if (num4 >= 0)
						{
							float num6 = r.x + num2 * (float)num4;
							Color a = ProfilerColors.currentColors[(int)(checked((IntPtr)(unchecked((ulong)this.m_Series.Length % (ulong)((long)ProfilerColors.currentColors.Length)))))];
							GUI.contentColor = (a + Color.white) * 0.5f;
							Chart.DoLabel(num6 + -1f, r.y + r.height - (float)((i % num3 + 1) * 12), markerNames[i], 0f);
						}
					}
					dictionary[markers[i].nameOffset] = markers[i].frame;
				}
			}
			GUI.contentColor = contentColor;
		}

		protected override Rect DoSeriesList(Rect position, int chartControlID, Chart.ChartType chartType, ChartViewData cdata)
		{
			Rect rect = base.DoSeriesList(position, chartControlID, chartType, cdata);
			GUIContent label = EditorGUIUtility.TempContent("Markers");
			Color color = ProfilerColors.currentColors[cdata.numSeries % ProfilerColors.currentColors.Length];
			base.DoSeriesToggle(rect, label, ref this.showMarkers, color, cdata);
			rect.y += rect.height;
			return rect;
		}
	}
}
