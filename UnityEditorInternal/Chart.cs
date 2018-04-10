using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class Chart
	{
		public delegate void ChangedEventHandler(Chart sender);

		internal enum ChartType
		{
			StackedFill,
			Line
		}

		private static class Styles
		{
			public static readonly GUIStyle background = "OL Box";

			public static readonly GUIStyle legendHeaderLabel = EditorStyles.label;

			public static readonly GUIStyle legendBackground = "ProfilerLeftPane";

			public static readonly GUIStyle rightPane = "ProfilerRightPane";

			public static readonly GUIStyle seriesLabel = "ProfilerPaneSubLabel";

			public static readonly GUIStyle seriesDragHandle = "RL DragHandle";

			public static readonly GUIStyle closeButton = "WinBtnClose";

			public static readonly GUIStyle whiteLabel = "ProfilerBadge";

			public static readonly GUIStyle selectedLabel = "ProfilerSelectedLabel";

			public static readonly float labelDropShadowOpacity = 0.3f;

			public static readonly float labelLerpToWhiteAmount = 0.5f;

			public static readonly Color selectedFrameColor1 = new Color(1f, 1f, 1f, 0.6f);

			public static readonly Color selectedFrameColor2 = new Color(1f, 1f, 1f, 0.7f);
		}

		private struct LabelLayoutData
		{
			public Rect position;

			public float desiredYPosition;
		}

		private static int s_ChartHash = "Charts".GetHashCode();

		public const float kSideWidth = 180f;

		private const int kDistFromTopToFirstLabel = 38;

		private const int kLabelHeight = 11;

		private const int kCloseButtonSize = 13;

		private const float kLabelOffset = 5f;

		private const float kWarningLabelHeightOffset = 43f;

		private const float kChartMinHeight = 130f;

		private const float k_LineWidth = 2f;

		private const int k_LabelLayoutMaxIterations = 5;

		private Vector3[] m_LineDrawingPoints;

		private float[] m_StackedSampleSums;

		private static readonly Color s_OverlayBackgroundDimFactor = new Color(0.9f, 0.9f, 0.9f, 0.4f);

		private string m_ChartSettingsName;

		private int m_chartControlID;

		private int m_DragItemIndex = -1;

		private Vector2 m_DragDownPos;

		private int[] m_OldChartOrder;

		public string m_NotSupportedWarning = null;

		private readonly List<Chart.LabelLayoutData> m_LabelData = new List<Chart.LabelLayoutData>(16);

		private readonly List<int> m_LabelOrder = new List<int>(16);

		private readonly List<int> m_MostOverlappingLabels = new List<int>(16);

		private readonly List<int> m_OverlappingLabels = new List<int>(16);

		private readonly List<float> m_SelectedFrameValues = new List<float>(16);

		public event Chart.ChangedEventHandler closed
		{
			add
			{
				Chart.ChangedEventHandler changedEventHandler = this.closed;
				Chart.ChangedEventHandler changedEventHandler2;
				do
				{
					changedEventHandler2 = changedEventHandler;
					changedEventHandler = Interlocked.CompareExchange<Chart.ChangedEventHandler>(ref this.closed, (Chart.ChangedEventHandler)Delegate.Combine(changedEventHandler2, value), changedEventHandler);
				}
				while (changedEventHandler != changedEventHandler2);
			}
			remove
			{
				Chart.ChangedEventHandler changedEventHandler = this.closed;
				Chart.ChangedEventHandler changedEventHandler2;
				do
				{
					changedEventHandler2 = changedEventHandler;
					changedEventHandler = Interlocked.CompareExchange<Chart.ChangedEventHandler>(ref this.closed, (Chart.ChangedEventHandler)Delegate.Remove(changedEventHandler2, value), changedEventHandler);
				}
				while (changedEventHandler != changedEventHandler2);
			}
		}

		public event Chart.ChangedEventHandler selected
		{
			add
			{
				Chart.ChangedEventHandler changedEventHandler = this.selected;
				Chart.ChangedEventHandler changedEventHandler2;
				do
				{
					changedEventHandler2 = changedEventHandler;
					changedEventHandler = Interlocked.CompareExchange<Chart.ChangedEventHandler>(ref this.selected, (Chart.ChangedEventHandler)Delegate.Combine(changedEventHandler2, value), changedEventHandler);
				}
				while (changedEventHandler != changedEventHandler2);
			}
			remove
			{
				Chart.ChangedEventHandler changedEventHandler = this.selected;
				Chart.ChangedEventHandler changedEventHandler2;
				do
				{
					changedEventHandler2 = changedEventHandler;
					changedEventHandler = Interlocked.CompareExchange<Chart.ChangedEventHandler>(ref this.selected, (Chart.ChangedEventHandler)Delegate.Remove(changedEventHandler2, value), changedEventHandler);
				}
				while (changedEventHandler != changedEventHandler2);
			}
		}

		public GUIContent legendHeaderLabel
		{
			get;
			set;
		}

		public Vector2 labelRange
		{
			get;
			set;
		}

		public Chart()
		{
			this.labelRange = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
		}

		public void LoadAndBindSettings(string chartSettingsName, ChartViewData cdata)
		{
			this.m_ChartSettingsName = chartSettingsName;
			this.LoadChartsSettings(cdata);
		}

		private int MoveSelectedFrame(int selectedFrame, ChartViewData cdata, int direction)
		{
			Vector2 dataDomain = cdata.GetDataDomain();
			int num = (int)(dataDomain.y - dataDomain.x);
			int num2 = selectedFrame + direction;
			int result;
			if (num2 < cdata.firstSelectableFrame || num2 > cdata.chartDomainOffset + num)
			{
				result = selectedFrame;
			}
			else
			{
				result = num2;
			}
			return result;
		}

		private int DoFrameSelectionDrag(float x, Rect r, ChartViewData cdata, int len)
		{
			int num = Mathf.RoundToInt((x - r.x) / r.width * (float)len - 0.5f);
			GUI.changed = true;
			return Mathf.Clamp(num + cdata.chartDomainOffset, cdata.firstSelectableFrame, cdata.chartDomainOffset + len);
		}

		private int HandleFrameSelectionEvents(int selectedFrame, int chartControlID, Rect chartFrame, ChartViewData cdata)
		{
			Event current = Event.current;
			switch (current.type)
			{
			case EventType.MouseDown:
				if (chartFrame.Contains(current.mousePosition))
				{
					GUIUtility.keyboardControl = chartControlID;
					GUIUtility.hotControl = chartControlID;
					Vector2 dataDomain = cdata.GetDataDomain();
					int len = (int)(dataDomain.y - dataDomain.x);
					selectedFrame = this.DoFrameSelectionDrag(current.mousePosition.x, chartFrame, cdata, len);
					current.Use();
				}
				break;
			case EventType.MouseUp:
				if (GUIUtility.hotControl == chartControlID)
				{
					GUIUtility.hotControl = 0;
					current.Use();
				}
				break;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == chartControlID)
				{
					Vector2 dataDomain2 = cdata.GetDataDomain();
					int len2 = (int)(dataDomain2.y - dataDomain2.x);
					selectedFrame = this.DoFrameSelectionDrag(current.mousePosition.x, chartFrame, cdata, len2);
					current.Use();
				}
				break;
			case EventType.KeyDown:
				if (GUIUtility.keyboardControl == chartControlID && selectedFrame >= 0)
				{
					if (current.keyCode == KeyCode.LeftArrow)
					{
						selectedFrame = this.MoveSelectedFrame(selectedFrame, cdata, -1);
						current.Use();
					}
					else if (current.keyCode == KeyCode.RightArrow)
					{
						selectedFrame = this.MoveSelectedFrame(selectedFrame, cdata, 1);
						current.Use();
					}
				}
				break;
			}
			return selectedFrame;
		}

		public void OnLostFocus()
		{
			if (GUIUtility.hotControl == this.m_chartControlID)
			{
				GUIUtility.hotControl = 0;
			}
			this.m_chartControlID = 0;
		}

		protected virtual void DoLegendGUI(Rect position, Chart.ChartType type, ChartViewData cdata, EventType evtType, bool active)
		{
			if (Event.current.type == EventType.Repaint)
			{
				Chart.Styles.legendBackground.Draw(position, GUIContent.none, false, false, active, false);
			}
			Rect rect = position;
			GUIContent content = this.legendHeaderLabel ?? GUIContent.none;
			rect.height = Chart.Styles.legendHeaderLabel.CalcSize(content).y;
			GUI.Label(rect, content, Chart.Styles.legendHeaderLabel);
			position.yMin += rect.height;
			position.xMin += 5f;
			position.xMax -= 5f;
			this.DoSeriesList(position, this.m_chartControlID, type, cdata);
			Rect position2 = rect;
			position2.xMax -= (float)Chart.Styles.legendHeaderLabel.padding.right;
			position2.xMin = position2.xMax - 13f;
			position2.yMin += (float)Chart.Styles.legendHeaderLabel.padding.top;
			position2.yMax = position2.yMin + 13f;
			if (GUI.Button(position2, GUIContent.none, Chart.Styles.closeButton) && this.closed != null)
			{
				this.closed(this);
			}
		}

		public int DoGUI(Chart.ChartType type, int selectedFrame, ChartViewData cdata, bool active)
		{
			int result;
			if (cdata == null)
			{
				result = selectedFrame;
			}
			else
			{
				this.m_chartControlID = GUIUtility.GetControlID(Chart.s_ChartHash, FocusType.Keyboard);
				GUILayoutOption gUILayoutOption = GUILayout.MinHeight(Math.Max(5f + (float)((cdata.numSeries + 1) * 11) + 38f, 130f));
				Rect rect = GUILayoutUtility.GetRect(GUIContent.none, Chart.Styles.background, new GUILayoutOption[]
				{
					gUILayoutOption
				});
				Rect rect2 = rect;
				rect2.x += 180f;
				rect2.width -= 180f;
				Event current = Event.current;
				EventType typeForControl = current.GetTypeForControl(this.m_chartControlID);
				if (typeForControl == EventType.MouseDown && rect.Contains(current.mousePosition) && this.selected != null)
				{
					this.selected(this);
				}
				if (this.m_DragItemIndex == -1)
				{
					selectedFrame = this.HandleFrameSelectionEvents(selectedFrame, this.m_chartControlID, rect2, cdata);
				}
				Rect position = rect2;
				position.x -= 180f;
				position.width = 180f;
				this.DoLegendGUI(position, type, cdata, typeForControl, active);
				if (current.type == EventType.Repaint)
				{
					Chart.Styles.rightPane.Draw(rect2, false, false, active, false);
					if (this.m_NotSupportedWarning == null)
					{
						rect2.height -= 1f;
						if (type == Chart.ChartType.StackedFill)
						{
							this.DrawChartStacked(selectedFrame, cdata, rect2);
						}
						else
						{
							this.DrawChartLine(selectedFrame, cdata, rect2);
						}
					}
					else
					{
						Rect position2 = rect2;
						position2.x += 59.4f;
						position2.y += 43f;
						GUI.Label(position2, this.m_NotSupportedWarning, EditorStyles.boldLabel);
					}
				}
				result = selectedFrame;
			}
			return result;
		}

		private void DrawSelectedFrame(int selectedFrame, ChartViewData cdata, Rect r)
		{
			if (cdata.firstSelectableFrame != -1 && selectedFrame - cdata.firstSelectableFrame >= 0)
			{
				Chart.DrawVerticalLine(selectedFrame, cdata, r, Chart.Styles.selectedFrameColor1, Chart.Styles.selectedFrameColor2, 1f);
			}
		}

		internal static void DrawVerticalLine(int frame, ChartViewData cdata, Rect r, Color color1, Color color2, float widthFactor)
		{
			if (Event.current.type == EventType.Repaint)
			{
				frame -= cdata.chartDomainOffset;
				if (frame >= 0)
				{
					Vector2 dataDomain = cdata.GetDataDomain();
					float num = dataDomain.y - dataDomain.x;
					HandleUtility.ApplyWireMaterial();
					GL.Begin(7);
					GL.Color(color1);
					GL.Vertex3(r.x + r.width / num * (float)frame, r.y + 1f, 0f);
					GL.Vertex3(r.x + r.width / num * (float)frame + r.width / num, r.y + 1f, 0f);
					GL.Color(color2);
					GL.Vertex3(r.x + r.width / num * (float)frame + r.width / num, r.yMax, 0f);
					GL.Vertex3(r.x + r.width / num * (float)frame, r.yMax, 0f);
					GL.End();
				}
			}
		}

		private void DrawMaxValueScale(ChartViewData cdata, Rect r)
		{
			Handles.Label(new Vector3(r.x + r.width / 2f - 20f, r.yMin + 2f, 0f), "Scale: " + cdata.maxValue);
		}

		private void DrawChartLine(int selectedFrame, ChartViewData cdata, Rect r)
		{
			for (int i = 0; i < cdata.numSeries; i++)
			{
				this.DrawChartItemLine(r, cdata, i);
			}
			if (cdata.maxValue > 0f)
			{
				this.DrawMaxValueScale(cdata, r);
			}
			this.DrawSelectedFrame(selectedFrame, cdata, r);
			this.DrawLabels(r, cdata, selectedFrame, Chart.ChartType.Line);
		}

		private void DrawChartStacked(int selectedFrame, ChartViewData cdata, Rect r)
		{
			HandleUtility.ApplyWireMaterial();
			Vector2 dataDomain = cdata.GetDataDomain();
			int num = (int)(dataDomain.y - dataDomain.x);
			if (num > 0)
			{
				if (this.m_StackedSampleSums == null || this.m_StackedSampleSums.Length < num)
				{
					this.m_StackedSampleSums = new float[num];
				}
				for (int i = 0; i < num; i++)
				{
					this.m_StackedSampleSums[i] = 0f;
				}
				for (int j = 0; j < cdata.numSeries; j++)
				{
					if (cdata.hasOverlay)
					{
						this.DrawChartItemStackedOverlay(r, j, cdata, this.m_StackedSampleSums);
					}
					this.DrawChartItemStacked(r, j, cdata, this.m_StackedSampleSums);
				}
				this.DrawSelectedFrame(selectedFrame, cdata, r);
				this.DrawGridStacked(r, cdata);
				this.DrawLabels(r, cdata, selectedFrame, Chart.ChartType.StackedFill);
				if (cdata.hasOverlay)
				{
					string text = ProfilerDriver.selectedPropertyPath;
					if (text.Length > 0)
					{
						int num2 = text.LastIndexOf('/');
						if (num2 != -1)
						{
							text = text.Substring(num2 + 1);
						}
						GUIContent content = EditorGUIUtility.TempContent("Selected: " + text);
						Vector2 vector = EditorStyles.whiteBoldLabel.CalcSize(content);
						EditorGUI.DropShadowLabel(new Rect(r.x + r.width - vector.x - 3f, r.y + 3f, vector.x, vector.y), content, Chart.Styles.selectedLabel);
					}
				}
			}
		}

		internal static void DoLabel(float x, float y, string text, float alignment)
		{
			if (!string.IsNullOrEmpty(text))
			{
				GUIContent content = EditorGUIUtility.TempContent(text);
				Vector2 vector = Chart.Styles.whiteLabel.CalcSize(content);
				Rect position = new Rect(x + vector.x * alignment, y, vector.x, vector.y);
				EditorGUI.DoDropShadowLabel(position, content, Chart.Styles.whiteLabel, Chart.Styles.labelDropShadowOpacity);
			}
		}

		private void DrawGridStacked(Rect r, ChartViewData cdata)
		{
			if (Event.current.type == EventType.Repaint && cdata.grid != null && cdata.gridLabels != null)
			{
				GL.Begin(1);
				GL.Color(new Color(1f, 1f, 1f, 0.2f));
				float num = (cdata.series[0].rangeAxis.sqrMagnitude != 0f) ? (1f / (cdata.series[0].rangeAxis.y - cdata.series[0].rangeAxis.x) * r.height) : 0f;
				float num2 = r.y + r.height;
				for (int i = 0; i < cdata.grid.Length; i++)
				{
					float num3 = num2 - (cdata.grid[i] - cdata.series[0].rangeAxis.x) * num;
					if (num3 > r.y)
					{
						GL.Vertex3(r.x + 80f, num3, 0f);
						GL.Vertex3(r.x + r.width, num3, 0f);
					}
				}
				GL.End();
				for (int j = 0; j < cdata.grid.Length; j++)
				{
					float num4 = num2 - (cdata.grid[j] - cdata.series[0].rangeAxis.x) * num;
					if (num4 > r.y)
					{
						Chart.DoLabel(r.x + 5f, num4 - 8f, cdata.gridLabels[j], 0f);
					}
				}
			}
		}

		private void DrawLabels(Rect chartPosition, ChartViewData data, int selectedFrame, Chart.ChartType chartType)
		{
			if (data.selectedLabels != null && Event.current.type == EventType.Repaint)
			{
				Vector2 dataDomain = data.GetDataDomain();
				if (selectedFrame >= data.firstSelectableFrame && selectedFrame <= data.chartDomainOffset + (int)(dataDomain.y - dataDomain.x) && dataDomain.y - dataDomain.x != 0f)
				{
					int num = selectedFrame - data.chartDomainOffset;
					this.m_LabelOrder.Clear();
					this.m_LabelOrder.AddRange(data.order);
					this.m_SelectedFrameValues.Clear();
					float num2 = 0f;
					bool flag = chartType == Chart.ChartType.StackedFill;
					int num3 = 0;
					int i = 0;
					int numSeries = data.numSeries;
					while (i < numSeries)
					{
						float num4 = data.series[i].yValues[num];
						this.m_SelectedFrameValues.Add(num4);
						if (data.series[i].enabled)
						{
							num2 += num4;
							num3++;
						}
						i++;
					}
					if (num3 != 0)
					{
						this.m_LabelData.Clear();
						float num5 = chartPosition.x + chartPosition.width * (((float)num + 0.5f) / (dataDomain.y - dataDomain.x));
						float num6 = 0f;
						num3 = 0;
						int j = 0;
						int numSeries2 = data.numSeries;
						while (j < numSeries2)
						{
							Chart.LabelLayoutData item = default(Chart.LabelLayoutData);
							float num7 = this.m_SelectedFrameValues[j];
							if (data.series[j].enabled && num7 >= this.labelRange.x && num7 <= this.labelRange.y)
							{
								Vector2 rangeAxis = data.series[j].rangeAxis;
								float num8 = (rangeAxis.sqrMagnitude != 0f) ? (rangeAxis.y - rangeAxis.x) : 1f;
								if (flag)
								{
									float num9 = 0f;
									for (int k = 0; k < numSeries2; k++)
									{
										int num10 = data.order[k];
										if (num10 < j && data.series[num10].enabled)
										{
											num9 += data.series[num10].yValues[num];
										}
									}
									num7 = num2 - num9 - 0.5f * num7;
								}
								Vector2 position = new Vector2(num5 + 0.5f, chartPosition.y + chartPosition.height * (1f - (num7 - rangeAxis.x) / num8));
								Vector2 size = Chart.Styles.whiteLabel.CalcSize(EditorGUIUtility.TempContent(data.selectedLabels[j]));
								position.y -= 0.5f * size.y;
								position.y = Mathf.Clamp(position.y, chartPosition.yMin, chartPosition.yMax - size.y);
								item.position = new Rect(position, size);
								item.desiredYPosition = item.position.center.y;
								num3++;
							}
							this.m_LabelData.Add(item);
							num6 = Mathf.Max(num6, item.position.width);
							j++;
						}
						if (num3 != 0)
						{
							if (!flag)
							{
								this.m_LabelOrder.Sort(new Comparison<int>(this.SortLineLabelIndices));
							}
							if (num5 > chartPosition.x + chartPosition.width - num6)
							{
								int l = 0;
								int numSeries3 = data.numSeries;
								while (l < numSeries3)
								{
									Chart.LabelLayoutData value = this.m_LabelData[l];
									value.position.x = value.position.x - value.position.width;
									this.m_LabelData[l] = value;
									l++;
								}
							}
							else if (num5 > chartPosition.x + num6)
							{
								int num11 = 0;
								int m = 0;
								int numSeries4 = data.numSeries;
								while (m < numSeries4)
								{
									int index = this.m_LabelOrder[m];
									if (this.m_LabelData[index].position.size.sqrMagnitude != 0f)
									{
										if ((num11 & 1) == 0)
										{
											Chart.LabelLayoutData value2 = this.m_LabelData[index];
											value2.position.x = value2.position.x - (value2.position.width + 1f);
											this.m_LabelData[index] = value2;
										}
										num11++;
									}
									m++;
								}
							}
							for (int n = 0; n < 5; n++)
							{
								this.m_MostOverlappingLabels.Clear();
								int num12 = 0;
								int numSeries5 = data.numSeries;
								while (num12 < numSeries5)
								{
									this.m_OverlappingLabels.Clear();
									this.m_OverlappingLabels.Add(num12);
									if (this.m_LabelData[num12].position.size.sqrMagnitude != 0f)
									{
										for (int num13 = 0; num13 < numSeries5; num13++)
										{
											if (this.m_LabelData[num13].position.size.sqrMagnitude != 0f)
											{
												if (num12 != num13 && this.m_LabelData[num12].position.Overlaps(this.m_LabelData[num13].position))
												{
													this.m_OverlappingLabels.Add(num13);
												}
											}
										}
										if (this.m_OverlappingLabels.Count > this.m_MostOverlappingLabels.Count)
										{
											this.m_MostOverlappingLabels.Clear();
											this.m_MostOverlappingLabels.AddRange(this.m_OverlappingLabels);
										}
									}
									num12++;
								}
								if (this.m_MostOverlappingLabels.Count == 1)
								{
									break;
								}
								float num15;
								float num14 = this.GetGeometricCenter(this.m_MostOverlappingLabels, this.m_LabelData, out num15);
								bool flag2 = true;
								while (flag2)
								{
									flag2 = false;
									float y = num14 - 0.5f * num15;
									float y2 = num14 + 0.5f * num15;
									int num16 = 0;
									int numSeries6 = data.numSeries;
									while (num16 < numSeries6)
									{
										if (!this.m_MostOverlappingLabels.Contains(num16))
										{
											Rect position2 = this.m_LabelData[num16].position;
											if (position2.size.sqrMagnitude != 0f)
											{
												float x = (position2.xMax >= num5) ? position2.xMin : position2.xMax;
												if (position2.Contains(new Vector2(x, y)) || position2.Contains(new Vector2(x, y2)))
												{
													this.m_MostOverlappingLabels.Add(num16);
													flag2 = true;
												}
											}
										}
										num16++;
									}
									this.GetGeometricCenter(this.m_MostOverlappingLabels, this.m_LabelData, out num15);
									if (num14 - 0.5f * num15 < chartPosition.yMin)
									{
										num14 = chartPosition.yMin + 0.5f * num15;
									}
									else if (num14 + 0.5f * num15 > chartPosition.yMax)
									{
										num14 = chartPosition.yMax - 0.5f * num15;
									}
								}
								this.m_MostOverlappingLabels.Sort(new Comparison<int>(this.SortOverlappingRectIndices));
								float num17 = 0f;
								int num18 = 0;
								int count = this.m_MostOverlappingLabels.Count;
								while (num18 < count)
								{
									int index2 = this.m_MostOverlappingLabels[num18];
									Chart.LabelLayoutData value3 = this.m_LabelData[index2];
									value3.position.y = num14 - num15 * 0.5f + num17;
									this.m_LabelData[index2] = value3;
									num17 += value3.position.height;
									num18++;
								}
							}
							Color contentColor = GUI.contentColor;
							for (int num19 = 0; num19 < data.numSeries; num19++)
							{
								int num20 = this.m_LabelOrder[num19];
								if (this.m_LabelData[num20].position.size.sqrMagnitude != 0f)
								{
									GUI.contentColor = Color.Lerp(data.series[num20].color, Color.white, Chart.Styles.labelLerpToWhiteAmount);
									EditorGUI.DoDropShadowLabel(this.m_LabelData[num20].position, EditorGUIUtility.TempContent(data.selectedLabels[num20]), Chart.Styles.whiteLabel, Chart.Styles.labelDropShadowOpacity);
								}
							}
							GUI.contentColor = contentColor;
						}
					}
				}
			}
		}

		private int SortLineLabelIndices(int index1, int index2)
		{
			return -this.m_LabelData[index1].desiredYPosition.CompareTo(this.m_LabelData[index2].desiredYPosition);
		}

		private int SortOverlappingRectIndices(int index1, int index2)
		{
			return -this.m_LabelOrder.IndexOf(index1).CompareTo(this.m_LabelOrder.IndexOf(index2));
		}

		private float GetGeometricCenter(List<int> overlappingRects, List<Chart.LabelLayoutData> labelData, out float totalHeight)
		{
			float num = 0f;
			totalHeight = 0f;
			int i = 0;
			int count = overlappingRects.Count;
			while (i < count)
			{
				int index = overlappingRects[i];
				num += labelData[index].desiredYPosition;
				totalHeight += labelData[index].position.height;
				i++;
			}
			return num / (float)overlappingRects.Count;
		}

		private void DrawChartItemLine(Rect r, ChartViewData cdata, int index)
		{
			ChartSeriesViewData chartSeriesViewData = cdata.series[index];
			if (chartSeriesViewData.enabled)
			{
				if (this.m_LineDrawingPoints == null || chartSeriesViewData.numDataPoints > this.m_LineDrawingPoints.Length)
				{
					this.m_LineDrawingPoints = new Vector3[chartSeriesViewData.numDataPoints];
				}
				Vector2 dataDomain = cdata.GetDataDomain();
				float num = dataDomain.y - dataDomain.x;
				if (num > 0f)
				{
					float num2 = 1f / num * r.width;
					float num3 = (cdata.series[index].rangeAxis.sqrMagnitude != 0f) ? (1f / (cdata.series[index].rangeAxis.y - cdata.series[index].rangeAxis.x) * r.height) : 0f;
					float num4 = r.y + r.height;
					for (int i = 0; i < chartSeriesViewData.numDataPoints; i++)
					{
						this.m_LineDrawingPoints[i].Set((chartSeriesViewData.xValues[i] - dataDomain.x) * num2 + r.x, num4 - (chartSeriesViewData.yValues[i] - chartSeriesViewData.rangeAxis.x) * num3, 0f);
					}
					using (new Handles.DrawingScope(cdata.series[index].color))
					{
						Handles.DrawAAPolyLine(2f, chartSeriesViewData.numDataPoints, this.m_LineDrawingPoints);
					}
				}
			}
		}

		private void DrawChartItemStacked(Rect r, int index, ChartViewData cdata, float[] stackedSampleSums)
		{
			Vector2 dataDomain = cdata.GetDataDomain();
			int num = (int)(dataDomain.y - dataDomain.x);
			float num2 = r.width / (float)num;
			index = cdata.order[index];
			if (cdata.series[index].enabled)
			{
				Color color = cdata.series[index].color;
				if (cdata.hasOverlay)
				{
					color *= Chart.s_OverlayBackgroundDimFactor;
				}
				GL.Begin(5);
				float num3 = r.x + num2 * 0.5f;
				float num4 = (cdata.series[0].rangeAxis.sqrMagnitude != 0f) ? (1f / (cdata.series[0].rangeAxis.y - cdata.series[0].rangeAxis.x) * r.height) : 0f;
				float num5 = r.y + r.height;
				int i = 0;
				while (i < num)
				{
					float num6 = num5 - stackedSampleSums[i];
					float num7 = cdata.series[index].yValues[i];
					if (num7 != -1f)
					{
						float num8 = (num7 - cdata.series[0].rangeAxis.x) * num4;
						if (num6 - num8 < r.yMin)
						{
							num8 = num6 - r.yMin;
						}
						GL.Color(color);
						GL.Vertex3(num3, num6 - num8, 0f);
						GL.Vertex3(num3, num6, 0f);
						stackedSampleSums[i] += num8;
					}
					i++;
					num3 += num2;
				}
				GL.End();
			}
		}

		private void DrawChartItemStackedOverlay(Rect r, int index, ChartViewData cdata, float[] stackedSampleSums)
		{
			Vector2 dataDomain = cdata.GetDataDomain();
			int num = (int)(dataDomain.y - dataDomain.x);
			float num2 = r.width / (float)num;
			int num3 = cdata.order[index];
			if (cdata.series[num3].enabled)
			{
				Color color = cdata.series[num3].color;
				GL.Begin(5);
				float num4 = r.x + num2 * 0.5f;
				float num5 = (cdata.series[0].rangeAxis.sqrMagnitude != 0f) ? (1f / (cdata.series[0].rangeAxis.y - cdata.series[0].rangeAxis.x) * r.height) : 0f;
				float num6 = r.y + r.height;
				int i = 0;
				while (i < num)
				{
					float num7 = num6 - stackedSampleSums[i];
					float num8 = cdata.overlays[num3].yValues[i];
					if (num8 != -1f)
					{
						float num9 = (num8 - cdata.series[0].rangeAxis.x) * num5;
						GL.Color(color);
						GL.Vertex3(num4, num7 - num9, 0f);
						GL.Vertex3(num4, num7, 0f);
					}
					i++;
					num4 += num2;
				}
				GL.End();
			}
		}

		protected virtual Rect DoSeriesList(Rect position, int chartControlID, Chart.ChartType chartType, ChartViewData cdata)
		{
			Rect rect = position;
			Event current = Event.current;
			EventType typeForControl = current.GetTypeForControl(chartControlID);
			Vector2 mousePosition = current.mousePosition;
			if (this.m_DragItemIndex != -1)
			{
				if (typeForControl != EventType.MouseUp)
				{
					if (typeForControl == EventType.KeyDown)
					{
						if (current.keyCode == KeyCode.Escape)
						{
							GUIUtility.hotControl = 0;
							Array.Copy(this.m_OldChartOrder, cdata.order, this.m_OldChartOrder.Length);
							this.m_DragItemIndex = -1;
							current.Use();
						}
					}
				}
				else if (GUIUtility.hotControl == chartControlID)
				{
					GUIUtility.hotControl = 0;
					this.m_DragItemIndex = -1;
					current.Use();
				}
			}
			for (int i = cdata.numSeries - 1; i >= 0; i--)
			{
				int num = cdata.order[i];
				GUIContent gUIContent = EditorGUIUtility.TempContent(cdata.series[num].name);
				rect.height = Chart.Styles.seriesLabel.CalcHeight(gUIContent, rect.width);
				Rect rect2 = rect;
				if (i == this.m_DragItemIndex)
				{
					rect2.y = mousePosition.y - this.m_DragDownPos.y;
				}
				if (chartType == Chart.ChartType.StackedFill)
				{
					Rect position2 = rect2;
					position2.xMin = position2.xMax - rect.height;
					switch (typeForControl)
					{
					case EventType.MouseDown:
						if (position2.Contains(mousePosition))
						{
							this.m_DragItemIndex = i;
							this.m_DragDownPos = mousePosition;
							this.m_DragDownPos.x = this.m_DragDownPos.x - rect.x;
							this.m_DragDownPos.y = this.m_DragDownPos.y - rect.y;
							this.m_OldChartOrder = new int[cdata.numSeries];
							Array.Copy(cdata.order, this.m_OldChartOrder, this.m_OldChartOrder.Length);
							GUIUtility.hotControl = chartControlID;
							current.Use();
						}
						break;
					case EventType.MouseUp:
						if (this.m_DragItemIndex == i)
						{
							current.Use();
						}
						this.m_DragItemIndex = -1;
						break;
					case EventType.MouseDrag:
						if (i == this.m_DragItemIndex)
						{
							bool flag = mousePosition.y > rect.yMax;
							bool flag2 = mousePosition.y < rect.yMin;
							if (flag || flag2)
							{
								int num2 = cdata.order[i];
								int num3 = (!flag2) ? Mathf.Max(0, i - 1) : Mathf.Min(cdata.numSeries - 1, i + 1);
								cdata.order[i] = cdata.order[num3];
								cdata.order[num3] = num2;
								this.m_DragItemIndex = num3;
								this.SaveChartsSettingsOrder(cdata);
							}
							current.Use();
						}
						break;
					case EventType.Repaint:
						Chart.Styles.seriesDragHandle.Draw(position2, false, false, false, false);
						break;
					}
				}
				this.DoSeriesToggle(rect2, gUIContent, ref cdata.series[num].enabled, cdata.series[num].color, cdata);
				rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			}
			return rect;
		}

		protected void DoSeriesToggle(Rect position, GUIContent label, ref bool enabled, Color color, ChartViewData cdata)
		{
			Color backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = ((!enabled) ? Color.black : color);
			EditorGUI.BeginChangeCheck();
			enabled = GUI.Toggle(position, enabled, label, Chart.Styles.seriesLabel);
			if (EditorGUI.EndChangeCheck())
			{
				this.SaveChartsSettingsEnabled(cdata);
			}
			GUI.backgroundColor = backgroundColor;
		}

		private void LoadChartsSettings(ChartViewData cdata)
		{
			if (!string.IsNullOrEmpty(this.m_ChartSettingsName))
			{
				string @string = EditorPrefs.GetString(this.m_ChartSettingsName + "Order");
				if (!string.IsNullOrEmpty(@string))
				{
					try
					{
						string[] array = @string.Split(new char[]
						{
							','
						});
						if (array.Length == cdata.numSeries)
						{
							for (int i = 0; i < cdata.numSeries; i++)
							{
								cdata.order[i] = int.Parse(array[i]);
							}
						}
					}
					catch (FormatException)
					{
					}
				}
				@string = EditorPrefs.GetString(this.m_ChartSettingsName + "Visible");
				for (int j = 0; j < cdata.numSeries; j++)
				{
					if (j < @string.Length && @string[j] == '0')
					{
						cdata.series[j].enabled = false;
					}
				}
			}
		}

		private void SaveChartsSettingsOrder(ChartViewData cdata)
		{
			if (!string.IsNullOrEmpty(this.m_ChartSettingsName))
			{
				string text = string.Empty;
				for (int i = 0; i < cdata.numSeries; i++)
				{
					if (text.Length != 0)
					{
						text += ",";
					}
					text += cdata.order[i];
				}
				EditorPrefs.SetString(this.m_ChartSettingsName + "Order", text);
			}
		}

		protected void SaveChartsSettingsEnabled(ChartViewData cdata)
		{
			string text = string.Empty;
			for (int i = 0; i < cdata.numSeries; i++)
			{
				text += ((!cdata.series[i].enabled) ? '0' : '1');
			}
			EditorPrefs.SetString(this.m_ChartSettingsName + "Visible", text);
		}
	}
}
