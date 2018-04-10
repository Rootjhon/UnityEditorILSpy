using System;
using UnityEngine;

namespace UnityEditor
{
	internal class BoolCurveRenderer : NormalCurveRenderer
	{
		public BoolCurveRenderer(AnimationCurve curve) : base(curve)
		{
		}

		public override float ClampedValue(float value)
		{
			return (value == 0f) ? 0f : 1f;
		}

		public override float EvaluateCurveSlow(float time)
		{
			return this.ClampedValue(base.GetCurve().Evaluate(time));
		}
	}
}
