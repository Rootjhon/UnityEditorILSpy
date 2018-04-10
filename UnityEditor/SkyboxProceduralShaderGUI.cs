using System;
using UnityEngine;

namespace UnityEditor
{
	internal class SkyboxProceduralShaderGUI : ShaderGUI
	{
		private enum SunDiskMode
		{
			None,
			Simple,
			HighQuality
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
		{
			materialEditor.SetDefaultGUIWidths();
			MaterialProperty materialProperty = ShaderGUI.FindProperty("_SunDisk", props);
			SkyboxProceduralShaderGUI.SunDiskMode sunDiskMode = (SkyboxProceduralShaderGUI.SunDiskMode)materialProperty.floatValue;
			for (int i = 0; i < props.Length; i++)
			{
				if ((props[i].flags & (MaterialProperty.PropFlags.HideInInspector | MaterialProperty.PropFlags.PerRendererData)) == MaterialProperty.PropFlags.None)
				{
					if (!(props[i].name == "_SunSizeConvergence") || sunDiskMode == SkyboxProceduralShaderGUI.SunDiskMode.HighQuality)
					{
						float propertyHeight = materialEditor.GetPropertyHeight(props[i], props[i].displayName);
						Rect controlRect = EditorGUILayout.GetControlRect(true, propertyHeight, EditorStyles.layerMaskField, new GUILayoutOption[0]);
						materialEditor.ShaderProperty(controlRect, props[i], props[i].displayName);
					}
				}
			}
		}
	}
}
