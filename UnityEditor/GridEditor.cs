using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(Grid))]
	internal class GridEditor : Editor
	{
		private SerializedProperty m_CellSize;

		private SerializedProperty m_CellGap;

		private SerializedProperty m_CellSwizzle;

		private void OnEnable()
		{
			this.m_CellSize = base.serializedObject.FindProperty("m_CellSize");
			this.m_CellGap = base.serializedObject.FindProperty("m_CellGap");
			this.m_CellSwizzle = base.serializedObject.FindProperty("m_CellSwizzle");
			SceneViewGridManager.FlushCachedGridProxy();
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_CellSize, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_CellGap, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_CellSwizzle, new GUILayoutOption[0]);
			if (base.serializedObject.ApplyModifiedProperties())
			{
				SceneViewGridManager.FlushCachedGridProxy();
			}
		}
	}
}
