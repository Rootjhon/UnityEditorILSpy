using System;
using UnityEditor.AnimatedValues;
using UnityEditor.Build;
using UnityEditorInternal.VR;
using UnityEngine.Events;

namespace UnityEditor
{
	internal class SkyboxPanoramicShaderGUI : ShaderGUI
	{
		private readonly AnimBool m_ShowLatLongLayout = new AnimBool();

		private readonly AnimBool m_ShowMirrorOnBack = new AnimBool();

		private readonly AnimBool m_Show3DControl = new AnimBool();

		private bool m_Initialized = false;

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
		{
			if (!this.m_Initialized)
			{
				this.m_ShowLatLongLayout.valueChanged.AddListener(new UnityAction(materialEditor.Repaint));
				this.m_ShowMirrorOnBack.valueChanged.AddListener(new UnityAction(materialEditor.Repaint));
				this.m_Show3DControl.valueChanged.AddListener(new UnityAction(materialEditor.Repaint));
				this.m_Initialized = true;
			}
			float labelWidth = EditorGUIUtility.labelWidth;
			materialEditor.SetDefaultGUIWidths();
			this.ShowProp(materialEditor, ShaderGUI.FindProperty("_Tint", props));
			this.ShowProp(materialEditor, ShaderGUI.FindProperty("_Exposure", props));
			this.ShowProp(materialEditor, ShaderGUI.FindProperty("_Rotation", props));
			this.ShowProp(materialEditor, ShaderGUI.FindProperty("_MainTex", props));
			EditorGUIUtility.labelWidth = labelWidth;
			this.m_ShowLatLongLayout.target = (this.ShowProp(materialEditor, ShaderGUI.FindProperty("_Mapping", props)) == 1f);
			if (EditorGUILayout.BeginFadeGroup(this.m_ShowLatLongLayout.faded))
			{
				this.m_ShowMirrorOnBack.target = (this.ShowProp(materialEditor, ShaderGUI.FindProperty("_ImageType", props)) == 1f);
				if (EditorGUILayout.BeginFadeGroup(this.m_ShowMirrorOnBack.faded))
				{
					EditorGUI.indentLevel++;
					this.ShowProp(materialEditor, ShaderGUI.FindProperty("_MirrorOnBack", props));
					EditorGUI.indentLevel--;
				}
				EditorGUILayout.EndFadeGroup();
				this.m_Show3DControl.value = false;
				BuildPlatform[] buildPlatforms = BuildPlatforms.instance.buildPlatforms;
				for (int i = 0; i < buildPlatforms.Length; i++)
				{
					BuildPlatform buildPlatform = buildPlatforms[i];
					if (VREditor.GetVREnabledOnTargetGroup(buildPlatform.targetGroup))
					{
						this.m_Show3DControl.value = true;
						break;
					}
				}
				if (EditorGUILayout.BeginFadeGroup(this.m_Show3DControl.faded))
				{
					this.ShowProp(materialEditor, ShaderGUI.FindProperty("_Layout", props));
				}
				EditorGUILayout.EndFadeGroup();
			}
			EditorGUILayout.EndFadeGroup();
			materialEditor.PropertiesDefaultGUI(new MaterialProperty[0]);
		}

		private float ShowProp(MaterialEditor materialEditor, MaterialProperty prop)
		{
			materialEditor.ShaderProperty(prop, prop.displayName);
			return prop.floatValue;
		}
	}
}
