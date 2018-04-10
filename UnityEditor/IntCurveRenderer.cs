using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class IntCurveRenderer : NormalCurveRenderer
	{
		private const float kSegmentWindowResolution = 1000f;

		private const int kMaximumSampleCount = 1000;

		private const float kStepHelperOffset = 1E-06f;

		public IntCurveRenderer(AnimationCurve curve) : base(curve)
		{
		}

		public override float ClampedValue(float value)
		{
			return Mathf.Floor(value + 0.5f);
		}

		public override float EvaluateCurveSlow(float time)
		{
			return this.ClampedValue(base.GetCurve().Evaluate(time));
		}

		protected override int GetSegmentResolution(float minTime, float maxTime, float keyTime, float nextKeyTime)
		{
			float num = maxTime - minTime;
			float num2 = nextKeyTime - keyTime;
			int value = Mathf.RoundToInt(1000f * (num2 / num));
			return Mathf.Clamp(value, 1, 1000);
		}

		protected override void AddPoint(ref List<Vector3> points, ref float lastTime, float sampleTime, ref float lastValue, float sampleValue)
		{
			if (lastValue != sampleValue)
			{
				points.Add(new Vector3(lastTime + 1E-06f, sampleValue));
			}
			points.Add(new Vector3(sampleTime, sampleValue));
			lastTime = sampleTime;
			lastValue = sampleValue;
		}
	}
}
