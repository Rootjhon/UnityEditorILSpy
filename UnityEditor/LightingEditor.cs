using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor
{
	[CustomEditor(typeof(RenderSettings))]
	internal class LightingEditor : Editor
	{
		internal static class Styles
		{
			public static readonly GUIContent env_top;

			public static readonly GUIContent env_skybox_mat;

			public static readonly GUIContent env_skybox_sun;

			public static readonly GUIContent env_amb_top;

			public static readonly GUIContent env_amb_src;

			public static readonly GUIContent env_amb_int;

			public static readonly GUIContent env_refl_top;

			public static readonly GUIContent env_refl_src;

			public static readonly GUIContent env_refl_res;

			public static readonly GUIContent env_refl_cmp;

			public static readonly GUIContent env_refl_int;

			public static readonly GUIContent env_refl_bnc;

			public static readonly GUIContent skyboxWarning;

			public static readonly GUIContent createLight;

			public static readonly GUIContent ambientUp;

			public static readonly GUIContent ambientMid;

			public static readonly GUIContent ambientDown;

			public static readonly GUIContent ambient;

			public static readonly GUIContent customReflection;

			public static readonly GUIContent AmbientLightingMode;

			public static readonly GUIContent[] kFullAmbientSource;

			public static readonly GUIContent[] AmbientLightingModes;

			public static readonly int[] kFullAmbientSourceValues;

			static Styles()
			{
				LightingEditor.Styles.env_top = EditorGUIUtility.TextContent("Environment");
				LightingEditor.Styles.env_skybox_mat = EditorGUIUtility.TextContent("Skybox Material|Specifies the material that is used to simulate the sky or other distant background in the Scene.");
				LightingEditor.Styles.env_skybox_sun = EditorGUIUtility.TextContent("Sun Source|Specifies the directional light that is used to indicate the direction of the sun when a procedural skybox is used. If set to None, the brightest directional light in the Scene is used to represent the sun.");
				LightingEditor.Styles.env_amb_top = EditorGUIUtility.TextContent("Environment Lighting");
				LightingEditor.Styles.env_amb_src = EditorGUIUtility.TextContent("Source|Specifies whether to use a skybox, gradient, or color for ambient light contributed to the Scene.");
				LightingEditor.Styles.env_amb_int = EditorGUIUtility.TextContent("Intensity Multiplier|Controls the brightness of the skybox lighting in the Scene.");
				LightingEditor.Styles.env_refl_top = EditorGUIUtility.TextContent("Environment Reflections");
				LightingEditor.Styles.env_refl_src = EditorGUIUtility.TextContent("Source|Specifies whether to use the skybox or a custom cube map for reflection effects in the Scene.");
				LightingEditor.Styles.env_refl_res = EditorGUIUtility.TextContent("Resolution|Controls the resolution for the cube map assigned to the skybox material for reflection effects in the Scene.");
				LightingEditor.Styles.env_refl_cmp = EditorGUIUtility.TextContent("Compression|Controls how Unity compresses the reflection cube maps. Options are Auto, Compressed, and Uncompressed. Auto compresses the cube maps if the compression format is suitable.");
				LightingEditor.Styles.env_refl_int = EditorGUIUtility.TextContent("Intensity Multiplier|Controls how much the skybox or custom cubemap affects reflections in the Scene. A value of 1 produces physically correct results.");
				LightingEditor.Styles.env_refl_bnc = EditorGUIUtility.TextContent("Bounces|Controls how many times a reflection includes other reflections. A value of 1 results in the Scene being rendered once so mirrored reflections will be black. A value of 2 results in mirrored reflections being visible in the Scene.");
				LightingEditor.Styles.skyboxWarning = EditorGUIUtility.TextContent("Shader of this material does not support skybox rendering.");
				LightingEditor.Styles.createLight = EditorGUIUtility.TextContent("Create Light");
				LightingEditor.Styles.ambientUp = EditorGUIUtility.TextContent("Sky Color|Controls the color of light emitted from the sky in the Scene.");
				LightingEditor.Styles.ambientMid = EditorGUIUtility.TextContent("Equator Color|Controls the color of light emitted from the sides of the Scene.");
				LightingEditor.Styles.ambientDown = EditorGUIUtility.TextContent("Ground Color|Controls the color of light emitted from the ground of the Scene.");
				LightingEditor.Styles.ambient = EditorGUIUtility.TextContent("Ambient Color|Controls the color of the ambient light contributed to the Scene.");
				LightingEditor.Styles.customReflection = EditorGUIUtility.TextContent("Cubemap|Specifies the custom cube map used for reflection effects in the Scene.");
				LightingEditor.Styles.AmbientLightingMode = EditorGUIUtility.TextContent("Ambient Mode|Specifies the Global Illumination mode that should be used for handling ambient light in the Scene. Options are Realtime or Baked. This property is not editable unless both Realtime Global Illumination and Baked Global Illumination are enabled for the scene.");
				LightingEditor.Styles.kFullAmbientSource = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Skybox"),
					EditorGUIUtility.TextContent("Gradient"),
					EditorGUIUtility.TextContent("Color")
				};
				LightingEditor.Styles.AmbientLightingModes = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Realtime"),
					EditorGUIUtility.TextContent("Baked")
				};
				LightingEditor.Styles.kFullAmbientSourceValues = new int[]
				{
					0,
					1,
					3
				};
			}
		}

		protected SerializedProperty m_Sun;

		protected SerializedProperty m_AmbientSource;

		protected SerializedProperty m_AmbientSkyColor;

		protected SerializedProperty m_AmbientEquatorColor;

		protected SerializedProperty m_AmbientGroundColor;

		protected SerializedProperty m_AmbientIntensity;

		protected SerializedProperty m_AmbientLightingMode;

		protected SerializedProperty m_ReflectionIntensity;

		protected SerializedProperty m_ReflectionBounces;

		protected SerializedProperty m_SkyboxMaterial;

		protected SerializedProperty m_DefaultReflectionMode;

		protected SerializedProperty m_DefaultReflectionResolution;

		protected SerializedProperty m_CustomReflection;

		protected SerializedProperty m_ReflectionCompression;

		protected SerializedObject m_LightmapSettings;

		private bool m_bShowEnvironment;

		private const string kShowEnvironment = "ShowEnvironment";

		public virtual void OnEnable()
		{
			this.m_Sun = base.serializedObject.FindProperty("m_Sun");
			this.m_AmbientSource = base.serializedObject.FindProperty("m_AmbientMode");
			this.m_AmbientSkyColor = base.serializedObject.FindProperty("m_AmbientSkyColor");
			this.m_AmbientEquatorColor = base.serializedObject.FindProperty("m_AmbientEquatorColor");
			this.m_AmbientGroundColor = base.serializedObject.FindProperty("m_AmbientGroundColor");
			this.m_AmbientIntensity = base.serializedObject.FindProperty("m_AmbientIntensity");
			this.m_ReflectionIntensity = base.serializedObject.FindProperty("m_ReflectionIntensity");
			this.m_ReflectionBounces = base.serializedObject.FindProperty("m_ReflectionBounces");
			this.m_SkyboxMaterial = base.serializedObject.FindProperty("m_SkyboxMaterial");
			this.m_DefaultReflectionMode = base.serializedObject.FindProperty("m_DefaultReflectionMode");
			this.m_DefaultReflectionResolution = base.serializedObject.FindProperty("m_DefaultReflectionResolution");
			this.m_CustomReflection = base.serializedObject.FindProperty("m_CustomReflection");
			this.m_LightmapSettings = new SerializedObject(LightmapEditorSettings.GetLightmapSettings());
			this.m_ReflectionCompression = this.m_LightmapSettings.FindProperty("m_LightmapEditorSettings.m_ReflectionCompression");
			this.m_AmbientLightingMode = this.m_LightmapSettings.FindProperty("m_GISettings.m_EnvironmentLightingMode");
			this.m_bShowEnvironment = SessionState.GetBool("ShowEnvironment", true);
		}

		public virtual void OnDisable()
		{
			SessionState.SetBool("ShowEnvironment", this.m_bShowEnvironment);
		}

		private void DrawGUI()
		{
			Material material = this.m_SkyboxMaterial.objectReferenceValue as Material;
			this.m_bShowEnvironment = EditorGUILayout.FoldoutTitlebar(this.m_bShowEnvironment, LightingEditor.Styles.env_top, true);
			if (this.m_bShowEnvironment)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(this.m_SkyboxMaterial, LightingEditor.Styles.env_skybox_mat, new GUILayoutOption[0]);
				if (material && !EditorMaterialUtility.IsBackgroundMaterial(material))
				{
					EditorGUILayout.HelpBox(LightingEditor.Styles.skyboxWarning.text, MessageType.Warning);
				}
				EditorGUILayout.PropertyField(this.m_Sun, LightingEditor.Styles.env_skybox_sun, new GUILayoutOption[0]);
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(LightingEditor.Styles.env_amb_top, new GUILayoutOption[0]);
				EditorGUI.indentLevel++;
				EditorGUILayout.IntPopup(this.m_AmbientSource, LightingEditor.Styles.kFullAmbientSource, LightingEditor.Styles.kFullAmbientSourceValues, LightingEditor.Styles.env_amb_src, new GUILayoutOption[0]);
				AmbientMode intValue = (AmbientMode)this.m_AmbientSource.intValue;
				if (intValue != AmbientMode.Trilight)
				{
					if (intValue != AmbientMode.Flat)
					{
						if (intValue == AmbientMode.Skybox)
						{
							if (material == null)
							{
								EditorGUI.BeginChangeCheck();
								Color colorValue = EditorGUILayout.ColorField(LightingEditor.Styles.ambient, this.m_AmbientSkyColor.colorValue, true, false, true, ColorPicker.defaultHDRConfig, new GUILayoutOption[0]);
								if (EditorGUI.EndChangeCheck())
								{
									this.m_AmbientSkyColor.colorValue = colorValue;
								}
							}
							else
							{
								EditorGUILayout.Slider(this.m_AmbientIntensity, 0f, 8f, LightingEditor.Styles.env_amb_int, new GUILayoutOption[0]);
							}
						}
					}
					else
					{
						EditorGUI.BeginChangeCheck();
						Color colorValue2 = EditorGUILayout.ColorField(LightingEditor.Styles.ambient, this.m_AmbientSkyColor.colorValue, true, false, true, ColorPicker.defaultHDRConfig, new GUILayoutOption[0]);
						if (EditorGUI.EndChangeCheck())
						{
							this.m_AmbientSkyColor.colorValue = colorValue2;
						}
					}
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					Color colorValue3 = EditorGUILayout.ColorField(LightingEditor.Styles.ambientUp, this.m_AmbientSkyColor.colorValue, true, false, true, ColorPicker.defaultHDRConfig, new GUILayoutOption[0]);
					Color colorValue4 = EditorGUILayout.ColorField(LightingEditor.Styles.ambientMid, this.m_AmbientEquatorColor.colorValue, true, false, true, ColorPicker.defaultHDRConfig, new GUILayoutOption[0]);
					Color colorValue5 = EditorGUILayout.ColorField(LightingEditor.Styles.ambientDown, this.m_AmbientGroundColor.colorValue, true, false, true, ColorPicker.defaultHDRConfig, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						this.m_AmbientSkyColor.colorValue = colorValue3;
						this.m_AmbientEquatorColor.colorValue = colorValue4;
						this.m_AmbientGroundColor.colorValue = colorValue5;
					}
				}
				if (LightModeUtil.Get().IsAnyGIEnabled())
				{
					int selectedValue;
					bool ambientLightingMode = LightModeUtil.Get().GetAmbientLightingMode(out selectedValue);
					using (new EditorGUI.DisabledScope(!ambientLightingMode))
					{
						int[] optionValues = new int[]
						{
							0,
							1
						};
						if (ambientLightingMode)
						{
							this.m_AmbientLightingMode.intValue = EditorGUILayout.IntPopup(LightingEditor.Styles.AmbientLightingMode, this.m_AmbientLightingMode.intValue, LightingEditor.Styles.AmbientLightingModes, optionValues, new GUILayoutOption[0]);
						}
						else
						{
							EditorGUILayout.IntPopup(LightingEditor.Styles.AmbientLightingMode, selectedValue, LightingEditor.Styles.AmbientLightingModes, optionValues, new GUILayoutOption[0]);
						}
					}
				}
				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(LightingEditor.Styles.env_refl_top, new GUILayoutOption[0]);
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(this.m_DefaultReflectionMode, LightingEditor.Styles.env_refl_src, new GUILayoutOption[0]);
				DefaultReflectionMode intValue2 = (DefaultReflectionMode)this.m_DefaultReflectionMode.intValue;
				if (intValue2 != DefaultReflectionMode.FromSkybox)
				{
					if (intValue2 == DefaultReflectionMode.Custom)
					{
						EditorGUILayout.PropertyField(this.m_CustomReflection, LightingEditor.Styles.customReflection, new GUILayoutOption[0]);
					}
				}
				else
				{
					int[] optionValues2 = null;
					GUIContent[] displayedOptions = null;
					ReflectionProbeEditor.GetResolutionArray(ref optionValues2, ref displayedOptions);
					EditorGUILayout.IntPopup(this.m_DefaultReflectionResolution, displayedOptions, optionValues2, LightingEditor.Styles.env_refl_res, new GUILayoutOption[]
					{
						GUILayout.MinWidth(40f)
					});
				}
				EditorGUILayout.PropertyField(this.m_ReflectionCompression, LightingEditor.Styles.env_refl_cmp, new GUILayoutOption[0]);
				EditorGUILayout.Slider(this.m_ReflectionIntensity, 0f, 1f, LightingEditor.Styles.env_refl_int, new GUILayoutOption[0]);
				EditorGUILayout.IntSlider(this.m_ReflectionBounces, 1, 5, LightingEditor.Styles.env_refl_bnc, new GUILayoutOption[0]);
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
			}
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			this.m_LightmapSettings.Update();
			this.DrawGUI();
			base.serializedObject.ApplyModifiedProperties();
			this.m_LightmapSettings.ApplyModifiedProperties();
		}
	}
}
