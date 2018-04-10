using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(MeshCollider))]
	internal class MeshColliderEditor : Collider3DEditorBase
	{
		private static class Texts
		{
			public static GUIContent isTriggerText = new GUIContent("Is Trigger", "Is this collider a trigger? Triggers are only supported on convex colliders.");

			public static GUIContent convextText = new GUIContent("Convex", "Is this collider convex?");

			public static GUIContent skinWidthText = new GUIContent("Skin Width", "How far out to inflate the mesh when building collision mesh.");

			public static GUIContent cookingOptionsText = new GUIContent("Cooking Options", "Options affecting the result of the mesh processing by the physics engine.");
		}

		private SerializedProperty m_Mesh;

		private SerializedProperty m_Convex;

		private SerializedProperty m_CookingOptions;

		private SerializedProperty m_SkinWidth;

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_Mesh = base.serializedObject.FindProperty("m_Mesh");
			this.m_Convex = base.serializedObject.FindProperty("m_Convex");
			this.m_CookingOptions = base.serializedObject.FindProperty("m_CookingOptions");
			this.m_SkinWidth = base.serializedObject.FindProperty("m_SkinWidth");
		}

		private MeshColliderCookingOptions GetCookingOptions()
		{
			return (MeshColliderCookingOptions)this.m_CookingOptions.intValue;
		}

		private void SetCookingOptions(MeshColliderCookingOptions cookingOptions)
		{
			this.m_CookingOptions.intValue = (int)cookingOptions;
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(this.m_Convex, MeshColliderEditor.Texts.convextText, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck() && !this.m_Convex.boolValue)
			{
				this.m_IsTrigger.boolValue = false;
				this.SetCookingOptions(this.GetCookingOptions() & ~MeshColliderCookingOptions.InflateConvexMesh);
			}
			EditorGUI.indentLevel++;
			using (new EditorGUI.DisabledScope(!this.m_Convex.boolValue))
			{
				EditorGUILayout.PropertyField(this.m_IsTrigger, MeshColliderEditor.Texts.isTriggerText, new GUILayoutOption[0]);
			}
			EditorGUI.indentLevel--;
			if (this.m_Convex.boolValue && (this.GetCookingOptions() & MeshColliderCookingOptions.InflateConvexMesh) != MeshColliderCookingOptions.None)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(this.m_SkinWidth, MeshColliderEditor.Texts.skinWidthText, new GUILayoutOption[0]);
				EditorGUI.indentLevel--;
			}
			this.SetCookingOptions((MeshColliderCookingOptions)EditorGUILayout.EnumFlagsField(MeshColliderEditor.Texts.cookingOptionsText, this.GetCookingOptions(), new GUILayoutOption[0]));
			EditorGUILayout.PropertyField(this.m_Material, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Mesh, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
