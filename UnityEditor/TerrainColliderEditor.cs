using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(TerrainCollider))]
	internal class TerrainColliderEditor : Collider3DEditorBase
	{
		private SerializedProperty m_TerrainData;

		private SerializedProperty m_EnableTreeColliders;

		protected GUIContent terrainContent = EditorGUIUtility.TextContent("Terrain Data|The TerrainData asset that stores heightmaps, terrain textures, detail meshes and trees.");

		protected GUIContent treeColliderContent = EditorGUIUtility.TextContent("Enable Tree Colliders|When selected, Tree Colliders will be enabled.");

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_TerrainData = base.serializedObject.FindProperty("m_TerrainData");
			this.m_EnableTreeColliders = base.serializedObject.FindProperty("m_EnableTreeColliders");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_Material, this.materialContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_TerrainData, this.terrainContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_EnableTreeColliders, this.treeColliderContent, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
