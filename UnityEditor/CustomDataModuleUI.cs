using System;
using UnityEngine;

namespace UnityEditor
{
	internal class CustomDataModuleUI : ModuleUI
	{
		private enum Mode
		{
			Disabled,
			Vector,
			Color
		}

		private class Texts
		{
			public GUIContent mode = EditorGUIUtility.TextContent("Mode|Select the type of data to populate this stream with.");

			public GUIContent vectorComponentCount = EditorGUIUtility.TextContent("Number of Components|How many of the components (XYZW) to fill.");

			public GUIContent[] modes = new GUIContent[]
			{
				EditorGUIUtility.TextContent("Disabled"),
				EditorGUIUtility.TextContent("Vector"),
				EditorGUIUtility.TextContent("Color")
			};
		}

		private const int k_NumCustomDataStreams = 2;

		private const int k_NumChannelsPerStream = 4;

		private SerializedProperty[] m_Modes = new SerializedProperty[2];

		private SerializedProperty[] m_VectorComponentCount = new SerializedProperty[2];

		private SerializedMinMaxCurve[,] m_Vectors = new SerializedMinMaxCurve[2, 4];

		private SerializedMinMaxGradient[] m_Colors = new SerializedMinMaxGradient[2];

		private SerializedProperty[,] m_VectorLabels = new SerializedProperty[2, 4];

		private SerializedProperty[] m_ColorLabels = new SerializedProperty[2];

		private static CustomDataModuleUI.Texts s_Texts;

		public CustomDataModuleUI(ParticleSystemUI owner, SerializedObject o, string displayName) : base(owner, o, "CustomDataModule", displayName)
		{
			this.m_ToolTip = "Configure custom data to be read in scripts or shaders. Use GetCustomParticleData from script, or send to shaders using the Custom Vertex Streams.";
		}

		protected override void Init()
		{
			if (this.m_Modes[0] == null)
			{
				if (CustomDataModuleUI.s_Texts == null)
				{
					CustomDataModuleUI.s_Texts = new CustomDataModuleUI.Texts();
				}
				for (int i = 0; i < 2; i++)
				{
					this.m_Modes[i] = base.GetProperty("mode" + i);
					this.m_VectorComponentCount[i] = base.GetProperty("vectorComponentCount" + i);
					this.m_Colors[i] = new SerializedMinMaxGradient(this, "color" + i);
					this.m_ColorLabels[i] = base.GetProperty("colorLabel" + i);
					for (int j = 0; j < 4; j++)
					{
						this.m_Vectors[i, j] = new SerializedMinMaxCurve(this, GUIContent.none, string.Concat(new object[]
						{
							"vector",
							i,
							"_",
							j
						}), ModuleUI.kUseSignedRange);
						this.m_VectorLabels[i, j] = base.GetProperty(string.Concat(new object[]
						{
							"vectorLabel",
							i,
							"_",
							j
						}));
					}
				}
			}
		}

		public override void OnInspectorGUI(InitialModuleUI initial)
		{
			for (int i = 0; i < 2; i++)
			{
				GUILayout.BeginVertical("Custom" + (i + 1), GUI.skin.window, new GUILayoutOption[0]);
				CustomDataModuleUI.Mode mode = (CustomDataModuleUI.Mode)ModuleUI.GUIPopup(CustomDataModuleUI.s_Texts.mode, this.m_Modes[i], CustomDataModuleUI.s_Texts.modes, new GUILayoutOption[0]);
				if (mode == CustomDataModuleUI.Mode.Vector)
				{
					int num = Mathf.Min(ModuleUI.GUIInt(CustomDataModuleUI.s_Texts.vectorComponentCount, this.m_VectorComponentCount[i], new GUILayoutOption[0]), 4);
					for (int j = 0; j < num; j++)
					{
						ModuleUI.GUIMinMaxCurve(this.m_VectorLabels[i, j], this.m_Vectors[i, j], new GUILayoutOption[0]);
					}
				}
				else if (mode == CustomDataModuleUI.Mode.Color)
				{
					base.GUIMinMaxGradient(this.m_ColorLabels[i], this.m_Colors[i], true, new GUILayoutOption[0]);
				}
				GUILayout.EndVertical();
			}
		}
	}
}
