using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEditor.IMGUI.Controls
{
	public class JointAngularLimitHandle
	{
		private enum ArcType
		{
			Solid,
			Wire
		}

		private static readonly Matrix4x4 s_XHandleOffset = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90f, Vector3.forward), Vector3.one);

		private static readonly Matrix4x4 s_ZHandleOffset = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(90f, Vector3.left), Vector3.one);

		private static readonly float s_LockedColorAmount = 0.5f;

		private static readonly Color s_LockedColor = new Color(0.5f, 0.5f, 0.5f, 0f);

		private List<KeyValuePair<Action, float>> m_HandleFunctionDistances = new List<KeyValuePair<Action, float>>(6);

		private ArcHandle m_XMinHandle;

		private ArcHandle m_XMaxHandle;

		private ArcHandle m_YMinHandle;

		private ArcHandle m_YMaxHandle;

		private ArcHandle m_ZMinHandle;

		private ArcHandle m_ZMaxHandle;

		private Matrix4x4 m_SecondaryAxesMatrix;

		private bool m_XHandleColorInitialized = false;

		private bool m_YHandleColorInitialized = false;

		private bool m_ZHandleColorInitialized = false;

		[CompilerGenerated]
		private static Comparison<KeyValuePair<Action, float>> <>f__mg$cache0;

		public float xMin
		{
			get
			{
				ConfigurableJointMotion xMotion = this.xMotion;
				float result;
				if (xMotion != ConfigurableJointMotion.Free)
				{
					if (xMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_XMinHandle.angle, this.xRange.x, this.m_XMaxHandle.angle);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.xRange.x;
				}
				return result;
			}
			set
			{
				this.m_XMinHandle.angle = value;
			}
		}

		public float xMax
		{
			get
			{
				ConfigurableJointMotion xMotion = this.xMotion;
				float result;
				if (xMotion != ConfigurableJointMotion.Free)
				{
					if (xMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_XMaxHandle.angle, this.m_XMinHandle.angle, this.xRange.y);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.xRange.y;
				}
				return result;
			}
			set
			{
				this.m_XMaxHandle.angle = value;
			}
		}

		public float yMin
		{
			get
			{
				ConfigurableJointMotion yMotion = this.yMotion;
				float result;
				if (yMotion != ConfigurableJointMotion.Free)
				{
					if (yMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_YMinHandle.angle, this.yRange.x, this.m_YMaxHandle.angle);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.yRange.x;
				}
				return result;
			}
			set
			{
				this.m_YMinHandle.angle = value;
			}
		}

		public float yMax
		{
			get
			{
				ConfigurableJointMotion yMotion = this.yMotion;
				float result;
				if (yMotion != ConfigurableJointMotion.Free)
				{
					if (yMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_YMaxHandle.angle, this.m_YMinHandle.angle, this.yRange.y);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.yRange.y;
				}
				return result;
			}
			set
			{
				this.m_YMaxHandle.angle = value;
			}
		}

		public float zMin
		{
			get
			{
				ConfigurableJointMotion zMotion = this.zMotion;
				float result;
				if (zMotion != ConfigurableJointMotion.Free)
				{
					if (zMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_ZMinHandle.angle, this.zRange.x, this.m_ZMaxHandle.angle);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.zRange.x;
				}
				return result;
			}
			set
			{
				this.m_ZMinHandle.angle = value;
			}
		}

		public float zMax
		{
			get
			{
				ConfigurableJointMotion zMotion = this.zMotion;
				float result;
				if (zMotion != ConfigurableJointMotion.Free)
				{
					if (zMotion != ConfigurableJointMotion.Locked)
					{
						result = Mathf.Clamp(this.m_ZMaxHandle.angle, this.m_ZMinHandle.angle, this.zRange.y);
					}
					else
					{
						result = 0f;
					}
				}
				else
				{
					result = this.zRange.y;
				}
				return result;
			}
			set
			{
				this.m_ZMaxHandle.angle = value;
			}
		}

		public Vector2 xRange
		{
			get;
			set;
		}

		public Vector2 yRange
		{
			get;
			set;
		}

		public Vector2 zRange
		{
			get;
			set;
		}

		public ConfigurableJointMotion xMotion
		{
			get;
			set;
		}

		public ConfigurableJointMotion yMotion
		{
			get;
			set;
		}

		public ConfigurableJointMotion zMotion
		{
			get;
			set;
		}

		public Color xHandleColor
		{
			get
			{
				if (!this.m_XHandleColorInitialized)
				{
					this.xHandleColor = Handles.xAxisColor;
				}
				return this.m_XMinHandle.angleHandleColor;
			}
			set
			{
				this.m_XMinHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_XMaxHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_XHandleColorInitialized = true;
			}
		}

		public Color yHandleColor
		{
			get
			{
				if (!this.m_YHandleColorInitialized)
				{
					this.yHandleColor = Handles.yAxisColor;
				}
				return this.m_YMinHandle.angleHandleColor;
			}
			set
			{
				this.m_YMinHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_YMaxHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_YHandleColorInitialized = true;
			}
		}

		public Color zHandleColor
		{
			get
			{
				if (!this.m_ZHandleColorInitialized)
				{
					this.zHandleColor = Handles.zAxisColor;
				}
				return this.m_ZMinHandle.angleHandleColor;
			}
			set
			{
				this.m_ZMinHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_ZMaxHandle.SetColorWithoutRadiusHandle(value, this.fillAlpha);
				this.m_ZHandleColorInitialized = true;
			}
		}

		public float radius
		{
			get
			{
				return this.m_XMinHandle.radius;
			}
			set
			{
				this.m_XMinHandle.radius = value;
				this.m_XMaxHandle.radius = value;
				this.m_YMinHandle.radius = value;
				this.m_YMaxHandle.radius = value;
				this.m_ZMinHandle.radius = value;
				this.m_ZMaxHandle.radius = value;
			}
		}

		public float fillAlpha
		{
			get;
			set;
		}

		public float wireframeAlpha
		{
			get;
			set;
		}

		public Handles.CapFunction angleHandleDrawFunction
		{
			get
			{
				return this.m_XMinHandle.angleHandleDrawFunction;
			}
			set
			{
				this.m_XMinHandle.angleHandleDrawFunction = value;
				this.m_XMaxHandle.angleHandleDrawFunction = value;
				this.m_YMinHandle.angleHandleDrawFunction = value;
				this.m_YMaxHandle.angleHandleDrawFunction = value;
				this.m_ZMinHandle.angleHandleDrawFunction = value;
				this.m_ZMaxHandle.angleHandleDrawFunction = value;
			}
		}

		public Handles.SizeFunction angleHandleSizeFunction
		{
			get
			{
				return this.m_XMinHandle.angleHandleSizeFunction;
			}
			set
			{
				this.m_XMinHandle.angleHandleSizeFunction = value;
				this.m_XMaxHandle.angleHandleSizeFunction = value;
				this.m_YMinHandle.angleHandleSizeFunction = value;
				this.m_YMaxHandle.angleHandleSizeFunction = value;
				this.m_ZMinHandle.angleHandleSizeFunction = value;
				this.m_ZMaxHandle.angleHandleSizeFunction = value;
			}
		}

		public JointAngularLimitHandle()
		{
			this.m_XMinHandle = new ArcHandle();
			this.m_XMaxHandle = new ArcHandle();
			this.m_YMinHandle = new ArcHandle();
			this.m_YMaxHandle = new ArcHandle();
			this.m_ZMinHandle = new ArcHandle();
			this.m_ZMaxHandle = new ArcHandle();
			ConfigurableJointMotion configurableJointMotion = ConfigurableJointMotion.Limited;
			this.zMotion = configurableJointMotion;
			configurableJointMotion = configurableJointMotion;
			this.yMotion = configurableJointMotion;
			this.xMotion = configurableJointMotion;
			this.radius = 1f;
			this.fillAlpha = 0.1f;
			this.wireframeAlpha = 1f;
			Vector2 vector = new Vector2(-180f, 180f);
			this.zRange = vector;
			vector = vector;
			this.yRange = vector;
			this.xRange = vector;
		}

		private static float GetSortingDistance(ArcHandle handle)
		{
			Vector3 a = Handles.matrix.MultiplyPoint3x4(Quaternion.AngleAxis(handle.angle, Vector3.up) * Vector3.forward * handle.radius);
			Vector3 rhs = a - Camera.current.transform.position;
			if (Camera.current.orthographic)
			{
				Vector3 forward = Camera.current.transform.forward;
				rhs = forward * Vector3.Dot(forward, rhs);
			}
			return rhs.sqrMagnitude;
		}

		private static int CompareHandleFunctionsByDistance(KeyValuePair<Action, float> func1, KeyValuePair<Action, float> func2)
		{
			return func2.Value.CompareTo(func1.Value);
		}

		public void DrawHandle()
		{
			this.m_SecondaryAxesMatrix = Handles.matrix;
			this.xHandleColor = this.xHandleColor;
			this.yHandleColor = this.yHandleColor;
			this.zHandleColor = this.zHandleColor;
			ArcHandle arg_49_0 = this.m_XMinHandle;
			Color clear = Color.clear;
			this.m_XMinHandle.wireframeColor = clear;
			arg_49_0.fillColor = clear;
			ArcHandle arg_67_0 = this.m_XMaxHandle;
			clear = Color.clear;
			this.m_XMaxHandle.wireframeColor = clear;
			arg_67_0.fillColor = clear;
			ArcHandle arg_85_0 = this.m_YMinHandle;
			clear = Color.clear;
			this.m_YMinHandle.wireframeColor = clear;
			arg_85_0.fillColor = clear;
			ArcHandle arg_A3_0 = this.m_YMaxHandle;
			clear = Color.clear;
			this.m_YMaxHandle.wireframeColor = clear;
			arg_A3_0.fillColor = clear;
			ArcHandle arg_C1_0 = this.m_ZMinHandle;
			clear = Color.clear;
			this.m_ZMinHandle.wireframeColor = clear;
			arg_C1_0.fillColor = clear;
			ArcHandle arg_DF_0 = this.m_ZMaxHandle;
			clear = Color.clear;
			this.m_ZMaxHandle.wireframeColor = clear;
			arg_DF_0.fillColor = clear;
			Color b = new Color(1f, 1f, 1f, this.fillAlpha);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			ConfigurableJointMotion xMotion = this.xMotion;
			if (xMotion != ConfigurableJointMotion.Free)
			{
				if (xMotion != ConfigurableJointMotion.Limited)
				{
					if (xMotion == ConfigurableJointMotion.Locked)
					{
						using (new Handles.DrawingScope(Handles.color * Color.Lerp(this.xHandleColor, JointAngularLimitHandle.s_LockedColor, JointAngularLimitHandle.s_LockedColorAmount)))
						{
							Handles.DrawWireDisc(Vector3.zero, Vector3.right, this.radius);
						}
					}
				}
				else
				{
					flag = true;
					this.m_SecondaryAxesMatrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis((this.xMin + this.xMax) * 0.5f, Vector3.left), Vector3.one);
					if (this.yMotion == ConfigurableJointMotion.Limited)
					{
						this.DrawMultiaxialFillShape();
					}
					else
					{
						using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_XHandleOffset))
						{
							this.DrawArc(this.m_XMinHandle, this.m_XMaxHandle, this.xHandleColor * b, JointAngularLimitHandle.ArcType.Solid);
						}
					}
				}
			}
			else
			{
				using (new Handles.DrawingScope(Handles.color * this.xHandleColor))
				{
					Handles.DrawWireDisc(Vector3.zero, Vector3.right, this.radius);
					Handles.color *= b;
					Handles.DrawSolidDisc(Vector3.zero, Vector3.right, this.radius);
				}
			}
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix))
			{
				ConfigurableJointMotion yMotion = this.yMotion;
				if (yMotion != ConfigurableJointMotion.Free)
				{
					if (yMotion != ConfigurableJointMotion.Limited)
					{
						if (yMotion == ConfigurableJointMotion.Locked)
						{
							using (new Handles.DrawingScope(Handles.color * Color.Lerp(this.yHandleColor, JointAngularLimitHandle.s_LockedColor, JointAngularLimitHandle.s_LockedColorAmount)))
							{
								Handles.DrawWireDisc(Vector3.zero, Vector3.up, this.radius);
							}
						}
					}
					else
					{
						flag2 = true;
						if (this.xMotion != ConfigurableJointMotion.Limited)
						{
							this.DrawArc(this.m_YMinHandle, this.m_YMaxHandle, this.yHandleColor * b, JointAngularLimitHandle.ArcType.Solid);
						}
					}
				}
				else
				{
					using (new Handles.DrawingScope(Handles.color * this.yHandleColor))
					{
						Handles.DrawWireDisc(Vector3.zero, Vector3.up, this.radius);
						Handles.color *= b;
						Handles.DrawSolidDisc(Vector3.zero, Vector3.up, this.radius);
					}
				}
				ConfigurableJointMotion zMotion = this.zMotion;
				if (zMotion != ConfigurableJointMotion.Free)
				{
					if (zMotion != ConfigurableJointMotion.Limited)
					{
						if (zMotion == ConfigurableJointMotion.Locked)
						{
							using (new Handles.DrawingScope(Handles.color * Color.Lerp(this.zHandleColor, JointAngularLimitHandle.s_LockedColor, JointAngularLimitHandle.s_LockedColorAmount)))
							{
								Handles.DrawWireDisc(Vector3.zero, Vector3.forward, this.radius);
							}
						}
					}
					else
					{
						using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_ZHandleOffset))
						{
							this.DrawArc(this.m_ZMinHandle, this.m_ZMaxHandle, this.zHandleColor * b, JointAngularLimitHandle.ArcType.Solid);
						}
						flag3 = true;
					}
				}
				else
				{
					using (new Handles.DrawingScope(Handles.color * this.zHandleColor))
					{
						Handles.DrawWireDisc(Vector3.zero, Vector3.forward, this.radius);
						Handles.color *= b;
						Handles.DrawSolidDisc(Vector3.zero, Vector3.forward, this.radius);
					}
				}
			}
			this.m_HandleFunctionDistances.Clear();
			this.m_XMinHandle.GetControlIDs();
			this.m_XMaxHandle.GetControlIDs();
			this.m_YMinHandle.GetControlIDs();
			this.m_YMaxHandle.GetControlIDs();
			this.m_ZMinHandle.GetControlIDs();
			this.m_ZMaxHandle.GetControlIDs();
			if (flag)
			{
				using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_XHandleOffset))
				{
					this.DrawArc(this.m_XMinHandle, this.m_XMaxHandle, this.xHandleColor, JointAngularLimitHandle.ArcType.Wire);
					this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawXMinHandle), JointAngularLimitHandle.GetSortingDistance(this.m_XMinHandle)));
					this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawXMaxHandle), JointAngularLimitHandle.GetSortingDistance(this.m_XMaxHandle)));
				}
			}
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix))
			{
				if (flag2)
				{
					this.DrawArc(this.m_YMinHandle, this.m_YMaxHandle, this.yHandleColor, JointAngularLimitHandle.ArcType.Wire);
					this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawYMinHandle), JointAngularLimitHandle.GetSortingDistance(this.m_YMinHandle)));
					this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawYMaxHandle), JointAngularLimitHandle.GetSortingDistance(this.m_YMaxHandle)));
				}
				if (flag3)
				{
					using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_ZHandleOffset))
					{
						this.DrawArc(this.m_ZMinHandle, this.m_ZMaxHandle, this.zHandleColor, JointAngularLimitHandle.ArcType.Wire);
						this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawZMinHandle), JointAngularLimitHandle.GetSortingDistance(this.m_ZMinHandle)));
						this.m_HandleFunctionDistances.Add(new KeyValuePair<Action, float>(new Action(this.DrawZMaxHandle), JointAngularLimitHandle.GetSortingDistance(this.m_ZMaxHandle)));
					}
				}
			}
			List<KeyValuePair<Action, float>> arg_72E_0 = this.m_HandleFunctionDistances;
			if (JointAngularLimitHandle.<>f__mg$cache0 == null)
			{
				JointAngularLimitHandle.<>f__mg$cache0 = new Comparison<KeyValuePair<Action, float>>(JointAngularLimitHandle.CompareHandleFunctionsByDistance);
			}
			arg_72E_0.Sort(JointAngularLimitHandle.<>f__mg$cache0);
			foreach (KeyValuePair<Action, float> current in this.m_HandleFunctionDistances)
			{
				current.Key();
			}
		}

		private void DrawArc(ArcHandle minHandle, ArcHandle maxHandle, Color color, JointAngularLimitHandle.ArcType arcType)
		{
			float num = maxHandle.angle - minHandle.angle;
			Vector3 from = Quaternion.AngleAxis(minHandle.angle, Vector3.up) * Vector3.forward;
			using (new Handles.DrawingScope(Handles.color * color))
			{
				if (arcType == JointAngularLimitHandle.ArcType.Solid)
				{
					int i = 0;
					int num2 = (int)Mathf.Abs(num) / 360;
					while (i < num2)
					{
						Handles.DrawSolidArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, this.radius);
						i++;
					}
					Handles.DrawSolidArc(Vector3.zero, Vector3.up, from, num % 360f, this.radius);
				}
				else
				{
					int j = 0;
					int num3 = (int)Mathf.Abs(num) / 360;
					while (j < num3)
					{
						Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.forward, 360f, this.radius);
						j++;
					}
					Handles.DrawWireArc(Vector3.zero, Vector3.up, from, num % 360f, this.radius);
				}
			}
		}

		private void DrawMultiaxialFillShape()
		{
			Quaternion quaternion = Quaternion.AngleAxis(this.xMin, Vector3.left);
			Quaternion quaternion2 = Quaternion.AngleAxis(this.xMax, Vector3.left);
			Quaternion rhs = Quaternion.AngleAxis(this.yMin, Vector3.up);
			Quaternion rhs2 = Quaternion.AngleAxis(this.yMax, Vector3.up);
			Color b = new Color(1f, 1f, 1f, this.fillAlpha);
			using (new Handles.DrawingScope(Handles.color * (this.yHandleColor * b)))
			{
				float angle = this.yMax - this.yMin;
				Vector3 from = quaternion2 * rhs2 * Vector3.forward;
				Handles.DrawSolidArc(Vector3.zero, quaternion2 * Vector3.down, from, angle, this.radius);
				from = quaternion * rhs2 * Vector3.forward;
				Handles.DrawSolidArc(Vector3.zero, quaternion * Vector3.down, from, angle, this.radius);
			}
			using (new Handles.DrawingScope(Handles.color * (this.xHandleColor * b)))
			{
				float angle2 = this.xMax - this.xMin;
				Vector3 from = quaternion2 * rhs2 * Vector3.forward;
				Handles.DrawSolidArc(Vector3.zero, Vector3.right, from, angle2, this.radius);
				from = quaternion2 * rhs * Vector3.forward;
				Handles.DrawSolidArc(Vector3.zero, Vector3.right, from, angle2, this.radius);
			}
		}

		private void DrawXMinHandle()
		{
			using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_XHandleOffset))
			{
				this.m_XMinHandle.DrawHandle();
				this.m_XMinHandle.angle = Mathf.Clamp(this.m_XMinHandle.angle, this.xRange.x, this.m_XMaxHandle.angle);
			}
		}

		private void DrawXMaxHandle()
		{
			using (new Handles.DrawingScope(Handles.matrix * JointAngularLimitHandle.s_XHandleOffset))
			{
				this.m_XMaxHandle.DrawHandle();
				this.m_XMaxHandle.angle = Mathf.Clamp(this.m_XMaxHandle.angle, this.m_XMinHandle.angle, this.xRange.y);
			}
		}

		private void DrawYMinHandle()
		{
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix))
			{
				this.m_YMinHandle.DrawHandle();
				this.m_YMinHandle.angle = Mathf.Clamp(this.m_YMinHandle.angle, this.yRange.x, this.m_YMaxHandle.angle);
			}
		}

		private void DrawYMaxHandle()
		{
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix))
			{
				this.m_YMaxHandle.DrawHandle();
				this.m_YMaxHandle.angle = Mathf.Clamp(this.m_YMaxHandle.angle, this.m_YMinHandle.angle, this.yRange.y);
			}
		}

		private void DrawZMinHandle()
		{
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix * JointAngularLimitHandle.s_ZHandleOffset))
			{
				this.m_ZMinHandle.DrawHandle();
				this.m_ZMinHandle.angle = Mathf.Clamp(this.m_ZMinHandle.angle, this.zRange.x, this.m_ZMaxHandle.angle);
			}
		}

		private void DrawZMaxHandle()
		{
			using (new Handles.DrawingScope(this.m_SecondaryAxesMatrix * JointAngularLimitHandle.s_ZHandleOffset))
			{
				this.m_ZMaxHandle.DrawHandle();
				this.m_ZMaxHandle.angle = Mathf.Clamp(this.m_ZMaxHandle.angle, this.m_ZMinHandle.angle, this.zRange.y);
			}
		}
	}
}
