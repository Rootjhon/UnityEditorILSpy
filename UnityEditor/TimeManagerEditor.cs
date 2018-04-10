using System;
using UnityEngine;

namespace UnityEditor
{
	[CustomEditor(typeof(TimeManager))]
	internal class TimeManagerEditor : Editor
	{
		private SerializedProperty m_FixedTimestepProperty;

		private SerializedProperty m_MaxAllowedTimestepProperty;

		private SerializedProperty m_TimeScaleProperty;

		private SerializedProperty m_MaxParticleTimestepProperty;

		private GUIContent m_FixedTimestepLabel;

		private GUIContent m_MaxAllowedTimestepLabel;

		private GUIContent m_TimeScaleLabel;

		private GUIContent m_MaxParticleTimestepLabel;

		public void OnEnable()
		{
			this.m_FixedTimestepProperty = base.serializedObject.FindProperty("Fixed Timestep");
			this.m_MaxAllowedTimestepProperty = base.serializedObject.FindProperty("Maximum Allowed Timestep");
			this.m_TimeScaleProperty = base.serializedObject.FindProperty("m_TimeScale");
			this.m_MaxParticleTimestepProperty = base.serializedObject.FindProperty("Maximum Particle Timestep");
			this.m_FixedTimestepLabel = EditorGUIUtility.TextContent("Fixed Timestep|A framerate-independent interval that dictates when physics calculations and FixedUpdate() events are performed.");
			this.m_MaxAllowedTimestepLabel = EditorGUIUtility.TextContent("Maximum Allowed Timestep|A framerate-independent interval that caps the worst case scenario when framerate is low. Physics calculations and FixedUpdate() events will not be performed for longer time than specified.");
			this.m_TimeScaleLabel = EditorGUIUtility.TextContent("Time Scale|The speed at which time progresses. Change this value to simulate bullet-time effects. A value of 1 means real-time. A value of .5 means half speed; a value of 2 is double speed.");
			this.m_MaxParticleTimestepLabel = EditorGUIUtility.TextContent("Maximum Particle Timestep|The maximum time that should be allowed to process particles for a frame.");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_FixedTimestepProperty, this.m_FixedTimestepLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_MaxAllowedTimestepProperty, this.m_MaxAllowedTimestepLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_TimeScaleProperty, this.m_TimeScaleLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_MaxParticleTimestepProperty, this.m_MaxParticleTimestepLabel, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
