using System;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[CustomEditor(typeof(PhysicsManager))]
	internal class PhysicsManagerInspector : ProjectSettingsBaseEditor
	{
		private static class Styles
		{
			public static readonly GUIContent interCollisionPropertiesLabel = EditorGUIUtility.TextContent("Cloth Inter-Collision");

			public static readonly GUIContent interCollisionDistanceLabel = EditorGUIUtility.TextContent("Distance");

			public static readonly GUIContent interCollisionStiffnessLabel = EditorGUIUtility.TextContent("Stiffness");
		}

		private Vector2 scrollPos;

		private bool show = true;

		private bool GetValue(int layerA, int layerB)
		{
			return !Physics.GetIgnoreLayerCollision(layerA, layerB);
		}

		private void SetValue(int layerA, int layerB, bool val)
		{
			Physics.IgnoreLayerCollision(layerA, layerB, !val);
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			Editor.DrawPropertiesExcluding(base.serializedObject, new string[]
			{
				"m_ClothInterCollisionDistance",
				"m_ClothInterCollisionStiffness",
				"m_ClothInterCollisionSettingsToggle"
			});
			LayerMatrixGUI.DoGUI("Layer Collision Matrix", ref this.show, ref this.scrollPos, new LayerMatrixGUI.GetValueFunc(this.GetValue), new LayerMatrixGUI.SetValueFunc(this.SetValue));
			EditorGUI.BeginChangeCheck();
			bool interCollisionSettingsToggle = EditorGUILayout.Toggle(PhysicsManagerInspector.Styles.interCollisionPropertiesLabel, Physics.interCollisionSettingsToggle, new GUILayoutOption[0]);
			bool flag = EditorGUI.EndChangeCheck();
			if (flag)
			{
				Physics.interCollisionSettingsToggle = interCollisionSettingsToggle;
			}
			if (Physics.interCollisionSettingsToggle)
			{
				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				float num = EditorGUILayout.FloatField(PhysicsManagerInspector.Styles.interCollisionDistanceLabel, Physics.interCollisionDistance, new GUILayoutOption[0]);
				bool flag2 = EditorGUI.EndChangeCheck();
				if (flag2)
				{
					if (num < 0f)
					{
						num = 0f;
					}
					Physics.interCollisionDistance = num;
				}
				EditorGUI.BeginChangeCheck();
				float num2 = EditorGUILayout.FloatField(PhysicsManagerInspector.Styles.interCollisionStiffnessLabel, Physics.interCollisionStiffness, new GUILayoutOption[0]);
				bool flag3 = EditorGUI.EndChangeCheck();
				if (flag3)
				{
					if (num2 < 0f)
					{
						num2 = 0f;
					}
					Physics.interCollisionStiffness = num2;
				}
				EditorGUI.indentLevel--;
			}
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
