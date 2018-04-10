using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	[Serializable]
	internal class TickHandler
	{
		[SerializeField]
		private float[] m_TickModulos = new float[0];

		[SerializeField]
		private float[] m_TickStrengths = new float[0];

		[SerializeField]
		private int m_SmallestTick = 0;

		[SerializeField]
		private int m_BiggestTick = -1;

		[SerializeField]
		private float m_MinValue = 0f;

		[SerializeField]
		private float m_MaxValue = 1f;

		[SerializeField]
		private float m_PixelRange = 1f;

		public int tickLevels
		{
			get
			{
				return this.m_BiggestTick - this.m_SmallestTick + 1;
			}
		}

		public void SetTickModulos(float[] tickModulos)
		{
			this.m_TickModulos = tickModulos;
		}

		public List<float> GetTickModulosForFrameRate(float frameRate)
		{
			List<float> result;
			if (frameRate > 1.07374182E+09f || frameRate != Mathf.Round(frameRate))
			{
				List<float> list = new List<float>
				{
					1f / frameRate,
					5f / frameRate,
					10f / frameRate,
					50f / frameRate,
					100f / frameRate,
					500f / frameRate,
					1000f / frameRate,
					5000f / frameRate,
					10000f / frameRate,
					50000f / frameRate,
					100000f / frameRate,
					500000f / frameRate
				};
				result = list;
			}
			else
			{
				List<int> list2 = new List<int>();
				int num = 1;
				while ((float)num < frameRate)
				{
					if ((double)Math.Abs((float)num - frameRate) < 1E-05)
					{
						break;
					}
					int num2 = Mathf.RoundToInt(frameRate / (float)num);
					if (num2 % 60 == 0)
					{
						num *= 2;
						list2.Add(num);
					}
					else if (num2 % 30 == 0)
					{
						num *= 3;
						list2.Add(num);
					}
					else if (num2 % 20 == 0)
					{
						num *= 2;
						list2.Add(num);
					}
					else if (num2 % 10 == 0)
					{
						num *= 2;
						list2.Add(num);
					}
					else if (num2 % 5 == 0)
					{
						num *= 5;
						list2.Add(num);
					}
					else if (num2 % 2 == 0)
					{
						num *= 2;
						list2.Add(num);
					}
					else if (num2 % 3 == 0)
					{
						num *= 3;
						list2.Add(num);
					}
					else
					{
						num = Mathf.RoundToInt(frameRate);
					}
				}
				List<float> list = new List<float>(13 + list2.Count);
				for (int i = 0; i < list2.Count; i++)
				{
					list.Add(1f / (float)list2[list2.Count - i - 1]);
				}
				list.Add(1f);
				list.Add(5f);
				list.Add(10f);
				list.Add(30f);
				list.Add(60f);
				list.Add(300f);
				list.Add(600f);
				list.Add(1800f);
				list.Add(3600f);
				list.Add(21600f);
				list.Add(86400f);
				list.Add(604800f);
				list.Add(1209600f);
				result = list;
			}
			return result;
		}

		public void SetTickModulosForFrameRate(float frameRate)
		{
			List<float> tickModulosForFrameRate = this.GetTickModulosForFrameRate(frameRate);
			this.SetTickModulos(tickModulosForFrameRate.ToArray());
		}

		public void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
		{
			this.m_MinValue = minValue;
			this.m_MaxValue = maxValue;
			this.m_PixelRange = maxPixel - minPixel;
		}

		public float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
		{
			float[] result;
			if (level < 0)
			{
				result = new float[0];
			}
			else
			{
				int num = Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_TickModulos.Length - 1);
				List<float> list = new List<float>();
				int num2 = Mathf.FloorToInt(this.m_MinValue / this.m_TickModulos[num]);
				int num3 = Mathf.CeilToInt(this.m_MaxValue / this.m_TickModulos[num]);
				for (int i = num2; i <= num3; i++)
				{
					if (!excludeTicksFromHigherlevels || num >= this.m_BiggestTick || i % Mathf.RoundToInt(this.m_TickModulos[num + 1] / this.m_TickModulos[num]) != 0)
					{
						list.Add((float)i * this.m_TickModulos[num]);
					}
				}
				result = list.ToArray();
			}
			return result;
		}

		public float GetStrengthOfLevel(int level)
		{
			return this.m_TickStrengths[this.m_SmallestTick + level];
		}

		public float GetPeriodOfLevel(int level)
		{
			return this.m_TickModulos[Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_TickModulos.Length - 1)];
		}

		public int GetLevelWithMinSeparation(float pixelSeparation)
		{
			int result;
			for (int i = 0; i < this.m_TickModulos.Length; i++)
			{
				float num = this.m_TickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue);
				if (num >= pixelSeparation)
				{
					result = i - this.m_SmallestTick;
					return result;
				}
			}
			result = -1;
			return result;
		}

		public void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing, bool sqrt)
		{
			this.m_TickStrengths = new float[this.m_TickModulos.Length];
			this.m_SmallestTick = 0;
			this.m_BiggestTick = this.m_TickModulos.Length - 1;
			for (int i = this.m_TickModulos.Length - 1; i >= 0; i--)
			{
				float num = this.m_TickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue);
				this.m_TickStrengths[i] = (num - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);
				if (this.m_TickStrengths[i] >= 1f)
				{
					this.m_BiggestTick = i;
				}
				if (num <= tickMinSpacing)
				{
					this.m_SmallestTick = i;
					break;
				}
			}
			for (int j = this.m_SmallestTick; j <= this.m_BiggestTick; j++)
			{
				this.m_TickStrengths[j] = Mathf.Clamp01(this.m_TickStrengths[j]);
				if (sqrt)
				{
					this.m_TickStrengths[j] = Mathf.Sqrt(this.m_TickStrengths[j]);
				}
			}
		}
	}
}
