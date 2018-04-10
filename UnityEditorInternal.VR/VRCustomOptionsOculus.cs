using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditorInternal.VR
{
	internal class VRCustomOptionsOculus : VRCustomOptions
	{
		private static GUIContent s_SharedDepthBufferLabel = EditorGUIUtility.TextContent("Shared Depth Buffer|Enable depth buffer submission to allow for overlay depth occlusion, etc.");

		private static GUIContent s_DashSupportLabel = EditorGUIUtility.TextContent("Dash Support|If enabled, pressing the home button brings up Dash, otherwise it brings up the older universal menu.");

		private SerializedProperty m_SharedDepthBuffer;

		private SerializedProperty m_DashSupport;

		public override void Initialize(SerializedObject settings)
		{
			this.Initialize(settings, "oculus");
		}

		public override void Initialize(SerializedObject settings, string propertyName)
		{
			base.Initialize(settings, propertyName);
			this.m_SharedDepthBuffer = base.FindPropertyAssert("sharedDepthBuffer");
			this.m_DashSupport = base.FindPropertyAssert("dashSupport");
		}

		public override Rect Draw(Rect rect)
		{
			rect.y += EditorGUIUtility.standardVerticalSpacing;
			rect.height = EditorGUIUtility.singleLineHeight;
			GUIContent label = EditorGUI.BeginProperty(rect, VRCustomOptionsOculus.s_SharedDepthBufferLabel, this.m_SharedDepthBuffer);
			EditorGUI.BeginChangeCheck();
			bool boolValue = EditorGUI.Toggle(rect, label, this.m_SharedDepthBuffer.boolValue);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_SharedDepthBuffer.boolValue = boolValue;
			}
			EditorGUI.EndProperty();
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			rect.height = EditorGUIUtility.singleLineHeight;
			label = EditorGUI.BeginProperty(rect, VRCustomOptionsOculus.s_DashSupportLabel, this.m_DashSupport);
			EditorGUI.BeginChangeCheck();
			boolValue = EditorGUI.Toggle(rect, label, this.m_DashSupport.boolValue);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_DashSupport.boolValue = boolValue;
			}
			EditorGUI.EndProperty();
			rect.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return rect;
		}

		public override float GetHeight()
		{
			return EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing * 3f;
		}
	}
}
