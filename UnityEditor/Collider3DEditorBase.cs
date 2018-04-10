using System;
using UnityEngine;

namespace UnityEditor
{
	internal class Collider3DEditorBase : ColliderEditorBase
	{
		protected SerializedProperty m_Material;

		protected SerializedProperty m_IsTrigger;

		protected GUIContent materialContent = EditorGUIUtility.TextContent("Material|Reference to the Physic Material that determines how this Collider interacts with others.");

		protected GUIContent triggerContent = EditorGUIUtility.TextContent("Is Trigger|If enabled, this Collider is used for triggering events and is ignored by the physics engine.");

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_Material = base.serializedObject.FindProperty("m_Material");
			this.m_IsTrigger = base.serializedObject.FindProperty("m_IsTrigger");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_IsTrigger, this.triggerContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Material, this.materialContent, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
