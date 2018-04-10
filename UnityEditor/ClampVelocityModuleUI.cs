using System;
using UnityEngine;

namespace UnityEditor
{
	internal class ClampVelocityModuleUI : ModuleUI
	{
		private class Texts
		{
			public GUIContent x = EditorGUIUtility.TextContent("X");

			public GUIContent y = EditorGUIUtility.TextContent("Y");

			public GUIContent z = EditorGUIUtility.TextContent("Z");

			public GUIContent dampen = EditorGUIUtility.TextContent("Dampen|Controls how much the velocity that exceeds the velocity limit should be dampened. A value of 0.5 will dampen the exceeding velocity by 50%.");

			public GUIContent magnitude = EditorGUIUtility.TextContent("Speed|The speed limit of particles over the particle lifetime.");

			public GUIContent separateAxes = EditorGUIUtility.TextContent("Separate Axes|If enabled, you can control the velocity limit separately for each axis.");

			public GUIContent space = EditorGUIUtility.TextContent("Space|Specifies if the velocity values are in local space (rotated with the transform) or world space.");

			public string[] spaces = new string[]
			{
				"Local",
				"World"
			};

			public GUIContent drag = EditorGUIUtility.TextContent("Drag|Control the amount of drag applied to each particle during its lifetime.");

			public GUIContent multiplyDragByParticleSize = EditorGUIUtility.TextContent("Multiply by Size|Adjust the drag based on the size of the particles.");

			public GUIContent multiplyDragByParticleVelocity = EditorGUIUtility.TextContent("Multiply by Velocity|Adjust the drag based on the velocity of the particles.");
		}

		private SerializedMinMaxCurve m_X;

		private SerializedMinMaxCurve m_Y;

		private SerializedMinMaxCurve m_Z;

		private SerializedMinMaxCurve m_Magnitude;

		private SerializedProperty m_SeparateAxes;

		private SerializedProperty m_InWorldSpace;

		private SerializedProperty m_Dampen;

		private SerializedMinMaxCurve m_Drag;

		private SerializedProperty m_MultiplyDragByParticleSize;

		private SerializedProperty m_MultiplyDragByParticleVelocity;

		private static ClampVelocityModuleUI.Texts s_Texts;

		public ClampVelocityModuleUI(ParticleSystemUI owner, SerializedObject o, string displayName) : base(owner, o, "ClampVelocityModule", displayName)
		{
			this.m_ToolTip = "Controls the velocity limit and damping of each particle during its lifetime.";
		}

		protected override void Init()
		{
			if (this.m_X == null)
			{
				if (ClampVelocityModuleUI.s_Texts == null)
				{
					ClampVelocityModuleUI.s_Texts = new ClampVelocityModuleUI.Texts();
				}
				this.m_X = new SerializedMinMaxCurve(this, ClampVelocityModuleUI.s_Texts.x, "x");
				this.m_Y = new SerializedMinMaxCurve(this, ClampVelocityModuleUI.s_Texts.y, "y");
				this.m_Z = new SerializedMinMaxCurve(this, ClampVelocityModuleUI.s_Texts.z, "z");
				this.m_Magnitude = new SerializedMinMaxCurve(this, ClampVelocityModuleUI.s_Texts.magnitude, "magnitude");
				this.m_SeparateAxes = base.GetProperty("separateAxis");
				this.m_InWorldSpace = base.GetProperty("inWorldSpace");
				this.m_Dampen = base.GetProperty("dampen");
				this.m_Drag = new SerializedMinMaxCurve(this, ClampVelocityModuleUI.s_Texts.drag, "drag");
				this.m_MultiplyDragByParticleSize = base.GetProperty("multiplyDragByParticleSize");
				this.m_MultiplyDragByParticleVelocity = base.GetProperty("multiplyDragByParticleVelocity");
			}
		}

		public override void OnInspectorGUI(InitialModuleUI initial)
		{
			EditorGUI.BeginChangeCheck();
			bool flag = ModuleUI.GUIToggle(ClampVelocityModuleUI.s_Texts.separateAxes, this.m_SeparateAxes, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				if (flag)
				{
					this.m_Magnitude.RemoveCurveFromEditor();
				}
				else
				{
					this.m_X.RemoveCurveFromEditor();
					this.m_Y.RemoveCurveFromEditor();
					this.m_Z.RemoveCurveFromEditor();
				}
			}
			if (!this.m_X.stateHasMultipleDifferentValues)
			{
				this.m_Y.SetMinMaxState(this.m_X.state, flag);
				this.m_Z.SetMinMaxState(this.m_X.state, flag);
			}
			if (flag)
			{
				base.GUITripleMinMaxCurve(GUIContent.none, ClampVelocityModuleUI.s_Texts.x, this.m_X, ClampVelocityModuleUI.s_Texts.y, this.m_Y, ClampVelocityModuleUI.s_Texts.z, this.m_Z, null, new GUILayoutOption[0]);
				EditorGUI.indentLevel++;
				ModuleUI.GUIBoolAsPopup(ClampVelocityModuleUI.s_Texts.space, this.m_InWorldSpace, ClampVelocityModuleUI.s_Texts.spaces, new GUILayoutOption[0]);
				EditorGUI.indentLevel--;
			}
			else
			{
				ModuleUI.GUIMinMaxCurve(ClampVelocityModuleUI.s_Texts.magnitude, this.m_Magnitude, new GUILayoutOption[0]);
			}
			EditorGUI.indentLevel++;
			ModuleUI.GUIFloat(ClampVelocityModuleUI.s_Texts.dampen, this.m_Dampen, new GUILayoutOption[0]);
			EditorGUI.indentLevel--;
			ModuleUI.GUIMinMaxCurve(ClampVelocityModuleUI.s_Texts.drag, this.m_Drag, new GUILayoutOption[0]);
			EditorGUI.indentLevel++;
			ModuleUI.GUIToggle(ClampVelocityModuleUI.s_Texts.multiplyDragByParticleSize, this.m_MultiplyDragByParticleSize, new GUILayoutOption[0]);
			ModuleUI.GUIToggle(ClampVelocityModuleUI.s_Texts.multiplyDragByParticleVelocity, this.m_MultiplyDragByParticleVelocity, new GUILayoutOption[0]);
			EditorGUI.indentLevel--;
		}

		public override void UpdateCullingSupportedString(ref string text)
		{
			text += "\nLimit Velocity over Lifetime module is enabled.";
		}
	}
}
