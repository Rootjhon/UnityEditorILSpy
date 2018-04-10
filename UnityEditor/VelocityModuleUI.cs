using System;
using UnityEngine;

namespace UnityEditor
{
	internal class VelocityModuleUI : ModuleUI
	{
		private class Texts
		{
			public GUIContent x = EditorGUIUtility.TextContent("X");

			public GUIContent y = EditorGUIUtility.TextContent("Y");

			public GUIContent z = EditorGUIUtility.TextContent("Z");

			public GUIContent space = EditorGUIUtility.TextContent("Space|Specifies if the velocity values are in local space (rotated with the transform) or world space.");

			public GUIContent speedMultiplier = EditorGUIUtility.TextContent("Speed Modifier|Multiply the particle speed by this value");

			public string[] spaces = new string[]
			{
				"Local",
				"World"
			};
		}

		private SerializedMinMaxCurve m_X;

		private SerializedMinMaxCurve m_Y;

		private SerializedMinMaxCurve m_Z;

		private SerializedProperty m_InWorldSpace;

		private SerializedMinMaxCurve m_SpeedModifier;

		private static VelocityModuleUI.Texts s_Texts;

		public VelocityModuleUI(ParticleSystemUI owner, SerializedObject o, string displayName) : base(owner, o, "VelocityModule", displayName)
		{
			this.m_ToolTip = "Controls the velocity of each particle during its lifetime.";
		}

		protected override void Init()
		{
			if (this.m_X == null)
			{
				if (VelocityModuleUI.s_Texts == null)
				{
					VelocityModuleUI.s_Texts = new VelocityModuleUI.Texts();
				}
				this.m_X = new SerializedMinMaxCurve(this, VelocityModuleUI.s_Texts.x, "x", ModuleUI.kUseSignedRange);
				this.m_Y = new SerializedMinMaxCurve(this, VelocityModuleUI.s_Texts.y, "y", ModuleUI.kUseSignedRange);
				this.m_Z = new SerializedMinMaxCurve(this, VelocityModuleUI.s_Texts.z, "z", ModuleUI.kUseSignedRange);
				this.m_InWorldSpace = base.GetProperty("inWorldSpace");
				this.m_SpeedModifier = new SerializedMinMaxCurve(this, VelocityModuleUI.s_Texts.speedMultiplier, "speedModifier", ModuleUI.kUseSignedRange);
			}
		}

		public override void OnInspectorGUI(InitialModuleUI initial)
		{
			base.GUITripleMinMaxCurve(GUIContent.none, VelocityModuleUI.s_Texts.x, this.m_X, VelocityModuleUI.s_Texts.y, this.m_Y, VelocityModuleUI.s_Texts.z, this.m_Z, null, new GUILayoutOption[0]);
			ModuleUI.GUIBoolAsPopup(VelocityModuleUI.s_Texts.space, this.m_InWorldSpace, VelocityModuleUI.s_Texts.spaces, new GUILayoutOption[0]);
			ModuleUI.GUIMinMaxCurve(VelocityModuleUI.s_Texts.speedMultiplier, this.m_SpeedModifier, new GUILayoutOption[0]);
		}

		public override void UpdateCullingSupportedString(ref string text)
		{
			this.Init();
			string empty = string.Empty;
			if (!this.m_X.SupportsProcedural(ref empty))
			{
				text = text + "\nVelocity over Lifetime module curve X: " + empty;
			}
			empty = string.Empty;
			if (!this.m_Y.SupportsProcedural(ref empty))
			{
				text = text + "\nVelocity over Lifetime module curve Y: " + empty;
			}
			empty = string.Empty;
			if (!this.m_Z.SupportsProcedural(ref empty))
			{
				text = text + "\nVelocity over Lifetime module curve Z: " + empty;
			}
			empty = string.Empty;
			if (this.m_SpeedModifier.state != MinMaxCurveState.k_Scalar || this.m_SpeedModifier.maxConstant != 1f)
			{
				text += "\nVelocity over Lifetime module curve Speed Multiplier is being used";
			}
		}
	}
}
