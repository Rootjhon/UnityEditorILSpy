using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class ProfilerChart : Chart
	{
		private const string kPrefCharts = "ProfilerChart";

		private static readonly GUIContent performanceWarning = new GUIContent("", EditorGUIUtility.LoadIcon("console.warnicon.sml"), "Collecting GPU Profiler data might have overhead. Close graph if you don't need its data");

		private bool m_Active;

		public ProfilerArea m_Area;

		public Chart.ChartType m_Type;

		public float m_DataScale;

		public ChartViewData m_Data;

		public ChartSeriesViewData[] m_Series;

		private static string[] s_LocalizedChartNames = null;

		public bool active
		{
			get
			{
				return this.m_Active;
			}
			set
			{
				if (this.m_Active != value)
				{
					this.m_Active = value;
					this.ApplyActiveState();
					this.SaveActiveState();
				}
			}
		}

		public ProfilerChart(ProfilerArea area, Chart.ChartType type, float dataScale, int seriesCount)
		{
			base.labelRange = new Vector2(Mathf.Epsilon, float.PositiveInfinity);
			this.m_Area = area;
			this.m_Type = type;
			this.m_DataScale = dataScale;
			this.m_Data = new ChartViewData();
			this.m_Series = new ChartSeriesViewData[seriesCount];
			this.m_Active = this.ReadActiveState();
			this.ApplyActiveState();
		}

		private string GetLocalizedChartName()
		{
			if (ProfilerChart.s_LocalizedChartNames == null)
			{
				ProfilerChart.s_LocalizedChartNames = new string[]
				{
					LocalizationDatabase.GetLocalizedString("CPU Usage|Graph out the various CPU areas"),
					LocalizationDatabase.GetLocalizedString("GPU Usage|Graph out the various GPU areas"),
					LocalizationDatabase.GetLocalizedString("Rendering"),
					LocalizationDatabase.GetLocalizedString("Memory|Graph out the various memory usage areas"),
					LocalizationDatabase.GetLocalizedString("Audio"),
					LocalizationDatabase.GetLocalizedString("Video"),
					LocalizationDatabase.GetLocalizedString("Physics"),
					LocalizationDatabase.GetLocalizedString("Physics (2D)"),
					LocalizationDatabase.GetLocalizedString("Network Messages"),
					LocalizationDatabase.GetLocalizedString("Network Operations"),
					LocalizationDatabase.GetLocalizedString("UI"),
					LocalizationDatabase.GetLocalizedString("UI Details"),
					LocalizationDatabase.GetLocalizedString("Global Illumination|Graph of the Precomputed Realtime Global Illumination system resource usage.")
				};
			}
			return ProfilerChart.s_LocalizedChartNames[(int)this.m_Area];
		}

		protected override void DoLegendGUI(Rect position, Chart.ChartType type, ChartViewData cdata, EventType evtType, bool active)
		{
			Rect position2 = position;
			position2.xMin = position2.xMax - (float)ProfilerChart.performanceWarning.image.width;
			position2.yMin = position2.yMax - (float)ProfilerChart.performanceWarning.image.height;
			base.DoLegendGUI(position, type, cdata, evtType, active);
			if (this.m_Area == ProfilerArea.GPU)
			{
				GUI.Label(position2, ProfilerChart.performanceWarning);
			}
		}

		public virtual int DoChartGUI(int currentFrame, ProfilerArea currentArea)
		{
			if (Event.current.type == EventType.Repaint)
			{
				string[] array = new string[this.m_Series.Length];
				for (int i = 0; i < this.m_Series.Length; i++)
				{
					string propertyName = (!this.m_Data.hasOverlay) ? this.m_Series[i].name : ("Selected" + this.m_Series[i].name);
					int statisticsIdentifier = ProfilerDriver.GetStatisticsIdentifier(propertyName);
					array[i] = ProfilerDriver.GetFormattedStatisticsValue(currentFrame, statisticsIdentifier);
				}
				this.m_Data.AssignSelectedLabels(array);
			}
			if (base.legendHeaderLabel == null)
			{
				string icon = string.Format("Profiler.{0}", Enum.GetName(typeof(ProfilerArea), this.m_Area));
				base.legendHeaderLabel = EditorGUIUtility.TextContentWithIcon(this.GetLocalizedChartName(), icon);
			}
			return base.DoGUI(this.m_Type, currentFrame, this.m_Data, currentArea == this.m_Area);
		}

		public void LoadAndBindSettings()
		{
			base.LoadAndBindSettings("ProfilerChart" + this.m_Area, this.m_Data);
		}

		private void ApplyActiveState()
		{
			if (this.m_Area == ProfilerArea.GPU)
			{
				ProfilerDriver.profileGPU = this.active;
			}
		}

		private bool ReadActiveState()
		{
			bool @bool;
			if (this.m_Area == ProfilerArea.GPU)
			{
				@bool = SessionState.GetBool("ProfilerChart" + this.m_Area, false);
			}
			else
			{
				@bool = EditorPrefs.GetBool("ProfilerChart" + this.m_Area, true);
			}
			return @bool;
		}

		private void SaveActiveState()
		{
			if (this.m_Area == ProfilerArea.GPU)
			{
				SessionState.SetBool("ProfilerChart" + this.m_Area, this.m_Active);
			}
			else
			{
				EditorPrefs.SetBool("ProfilerChart" + this.m_Area, this.m_Active);
			}
		}
	}
}
