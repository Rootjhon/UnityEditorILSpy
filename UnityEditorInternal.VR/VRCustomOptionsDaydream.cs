using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal.VR
{
	internal class VRCustomOptionsDaydream : VRCustomOptionsGoogleVR
	{
		private const int kDisabled = 0;

		private const int kSupported = 1;

		private const int kRequired = 2;

		private const int kThreeDoFHeadTracking = 0;

		private const int kSixDoFHeadTracking = 1;

		private const float s_Indent = 10f;

		private static GUIContent s_ForegroundIconLabel = EditorGUIUtility.TextContent("Foreground Icon|Icon should be a Texture with dimensions of 512px by 512px and a 1:1 aspect ratio.");

		private static GUIContent s_BackgroundIconLabel = EditorGUIUtility.TextContent("Background Icon|Icon should be a Texture with dimensions of 512px by 512px and a 1:1 aspect ratio.");

		private static GUIContent s_SustainedPerformanceModeLabel = EditorGUIUtility.TextContent("Sustained Performance|Sustained Performance mode is intended to provide a consistent level of performance for a prolonged amount of time");

		private static GUIContent s_EnableVideoLayer = EditorGUIUtility.TextContent("Video Surface|Enable the use of the video surface integrated with Daydream asynchronous reprojection.");

		private static GUIContent s_UseProtectedVideoMemoryLabel = EditorGUIUtility.TextContent("Protected Memory|Enable the use of DRM protection. Only usable if all content is DRM Protected.");

		private static GUIContent s_MinimumTargetHeadTracking = EditorGUIUtility.TextContent("Positional Head Tracking|Requested head tracking support of target devices to run the application on.");

		private static GUIContent[] s_TargetHeadTrackingOptions = new GUIContent[]
		{
			EditorGUIUtility.TextContent("Disabled|Will run on any device and provides no head tracking."),
			EditorGUIUtility.TextContent("Supported|Will run on any device and will provide head tracking on devices that support head tracking."),
			EditorGUIUtility.TextContent("Required|Will only run on devices with full 6 DoF head tracking support.")
		};

		private SerializedProperty m_DaydreamIcon;

		private SerializedProperty m_DaydreamIconBackground;

		private SerializedProperty m_DaydreamUseSustainedPerformanceMode;

		private SerializedProperty m_DaydreamEnableVideoLayer;

		private SerializedProperty m_DaydreamUseProtectedMemory;

		private SerializedProperty m_MinimumSupportedHeadTracking;

		private SerializedProperty m_MaximumSupportedHeadTracking;

		public override void Initialize(SerializedObject settings)
		{
			this.Initialize(settings, "daydream");
		}

		public override void Initialize(SerializedObject settings, string propertyName)
		{
			base.Initialize(settings, propertyName);
			this.m_DaydreamIcon = base.FindPropertyAssert("daydreamIconForeground");
			this.m_DaydreamIconBackground = base.FindPropertyAssert("daydreamIconBackground");
			this.m_DaydreamUseSustainedPerformanceMode = base.FindPropertyAssert("useSustainedPerformanceMode");
			this.m_DaydreamEnableVideoLayer = base.FindPropertyAssert("enableVideoLayer");
			this.m_DaydreamUseProtectedMemory = base.FindPropertyAssert("useProtectedVideoMemory");
			this.m_MinimumSupportedHeadTracking = base.FindPropertyAssert("minimumSupportedHeadTracking");
			this.m_MaximumSupportedHeadTracking = base.FindPropertyAssert("maximumSupportedHeadTracking");
		}

		private Rect DrawTextureUI(Rect rect, GUIContent propLabel, SerializedProperty prop)
		{
			rect.height = 64f;
			GUIContent label = EditorGUI.BeginProperty(rect, propLabel, prop);
			EditorGUI.BeginChangeCheck();
			Texture2D objectReferenceValue = EditorGUI.ObjectField(rect, label, (Texture2D)prop.objectReferenceValue, typeof(Texture2D), false) as Texture2D;
			if (EditorGUI.EndChangeCheck())
			{
				prop.objectReferenceValue = objectReferenceValue;
			}
			EditorGUI.EndProperty();
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return rect;
		}

		private int GetHeadTrackingValue()
		{
			int result = 0;
			if (this.m_MinimumSupportedHeadTracking.intValue == 0 && this.m_MaximumSupportedHeadTracking.intValue == 0)
			{
				result = 0;
			}
			else if (this.m_MinimumSupportedHeadTracking.intValue == 0 && this.m_MaximumSupportedHeadTracking.intValue == 1)
			{
				result = 1;
			}
			else if (this.m_MinimumSupportedHeadTracking.intValue == 1 && this.m_MaximumSupportedHeadTracking.intValue == 1)
			{
				result = 2;
			}
			return result;
		}

		private void SetHeadTrackingValue(int headTrackingValue)
		{
			if (headTrackingValue != 0)
			{
				if (headTrackingValue != 1)
				{
					if (headTrackingValue == 2)
					{
						this.m_MinimumSupportedHeadTracking.intValue = 1;
						this.m_MaximumSupportedHeadTracking.intValue = 1;
					}
				}
				else
				{
					this.m_MinimumSupportedHeadTracking.intValue = 0;
					this.m_MaximumSupportedHeadTracking.intValue = 1;
				}
			}
			else
			{
				this.m_MinimumSupportedHeadTracking.intValue = 0;
				this.m_MaximumSupportedHeadTracking.intValue = 0;
			}
		}

		public override Rect Draw(Rect rect)
		{
			rect = base.Draw(rect);
			rect = this.DrawTextureUI(rect, VRCustomOptionsDaydream.s_ForegroundIconLabel, this.m_DaydreamIcon);
			rect = this.DrawTextureUI(rect, VRCustomOptionsDaydream.s_BackgroundIconLabel, this.m_DaydreamIconBackground);
			rect.height = EditorGUIUtility.singleLineHeight;
			GUIContent label = EditorGUI.BeginProperty(rect, VRCustomOptionsDaydream.s_SustainedPerformanceModeLabel, this.m_DaydreamUseSustainedPerformanceMode);
			EditorGUI.BeginChangeCheck();
			bool flag = EditorGUI.Toggle(rect, label, this.m_DaydreamUseSustainedPerformanceMode.boolValue);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_DaydreamUseSustainedPerformanceMode.boolValue = flag;
			}
			EditorGUI.EndProperty();
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			rect.height = EditorGUIUtility.singleLineHeight;
			label = EditorGUI.BeginProperty(rect, VRCustomOptionsDaydream.s_EnableVideoLayer, this.m_DaydreamEnableVideoLayer);
			EditorGUI.BeginChangeCheck();
			flag = EditorGUI.Toggle(rect, label, this.m_DaydreamEnableVideoLayer.boolValue);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_DaydreamEnableVideoLayer.boolValue = flag;
				if (!flag)
				{
					this.m_DaydreamUseProtectedMemory.boolValue = false;
				}
			}
			EditorGUI.EndProperty();
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			if (this.m_DaydreamEnableVideoLayer.boolValue)
			{
				rect.x += 10f;
				rect.width -= 10f;
				rect.height = EditorGUIUtility.singleLineHeight;
				label = EditorGUI.BeginProperty(rect, VRCustomOptionsDaydream.s_UseProtectedVideoMemoryLabel, this.m_DaydreamUseProtectedMemory);
				EditorGUI.BeginChangeCheck();
				flag = EditorGUI.Toggle(rect, label, this.m_DaydreamUseProtectedMemory.boolValue);
				if (EditorGUI.EndChangeCheck())
				{
					this.m_DaydreamUseProtectedMemory.boolValue = flag;
				}
				EditorGUI.EndProperty();
				rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
				rect.x -= 10f;
				rect.width += 10f;
			}
			rect.y += EditorGUIUtility.standardVerticalSpacing;
			rect.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.BeginChangeCheck();
			int headTrackingValue = EditorGUI.Popup(rect, VRCustomOptionsDaydream.s_MinimumTargetHeadTracking, this.GetHeadTrackingValue(), VRCustomOptionsDaydream.s_TargetHeadTrackingOptions);
			if (EditorGUI.EndChangeCheck())
			{
				this.SetHeadTrackingValue(headTrackingValue);
			}
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return rect;
		}

		public override float GetHeight()
		{
			float num = 5f;
			float num2 = 2f;
			float num3 = 4f;
			if (this.m_DaydreamEnableVideoLayer.boolValue)
			{
				num += 1f;
				num3 += 1f;
			}
			return base.GetHeight() + EditorGUIUtility.singleLineHeight * num + 64f * num2 + EditorGUIUtility.standardVerticalSpacing * num3;
		}
	}
}
