using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(TilemapRenderer))]
	internal class TilemapRendererEditor : RendererEditorBase
	{
		private static class Styles
		{
			public static readonly GUIContent materialLabel = EditorGUIUtility.TextContent("Material");
		}

		private SerializedProperty m_Material;

		private SerializedProperty m_SortOrder;

		private SerializedProperty m_MaskInteraction;

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_Material = base.serializedObject.FindProperty("m_Materials.Array");
			this.m_SortOrder = base.serializedObject.FindProperty("m_SortOrder");
			this.m_MaskInteraction = base.serializedObject.FindProperty("m_MaskInteraction");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_Material.GetArrayElementAtIndex(0), TilemapRendererEditor.Styles.materialLabel, true, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_SortOrder, new GUILayoutOption[0]);
			base.RenderSortingLayerFields();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_MaskInteraction, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
