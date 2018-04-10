using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(Tile))]
	internal class TileEditor : Editor
	{
		private static class Styles
		{
			public static readonly GUIContent invalidMatrixLabel = EditorGUIUtility.TextContent("Invalid Matrix|No valid Position / Rotation / Scale components available for this matrix");

			public static readonly GUIContent resetMatrixLabel = EditorGUIUtility.TextContent("Reset Matrix");

			public static readonly GUIContent previewLabel = EditorGUIUtility.TextContent("Preview|Preview of tile with attributes set");

			public static readonly GUIContent positionLabel = EditorGUIUtility.TextContent("Position");

			public static readonly GUIContent rotationLabel = EditorGUIUtility.TextContent("Rotation");

			public static readonly GUIContent scaleLabel = EditorGUIUtility.TextContent("Scale");
		}

		private const float k_PreviewWidth = 32f;

		private const float k_PreviewHeight = 32f;

		private SerializedProperty m_Color;

		private SerializedProperty m_ColliderType;

		private SerializedProperty m_Sprite;

		private Tile tile
		{
			get
			{
				return base.target as Tile;
			}
		}

		public void OnEnable()
		{
			this.m_Color = base.serializedObject.FindProperty("m_Color");
			this.m_ColliderType = base.serializedObject.FindProperty("m_ColliderType");
			this.m_Sprite = base.serializedObject.FindProperty("m_Sprite");
		}

		public override void OnInspectorGUI()
		{
			TileEditor.DoTilePreview(this.tile.sprite, this.tile.color, Matrix4x4.identity);
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_Sprite, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Color, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_ColliderType, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}

		public static void DoTilePreview(Sprite sprite, Color color, Matrix4x4 transform)
		{
			if (!(sprite == null))
			{
				Rect totalPosition = EditorGUILayout.GetControlRect(false, 32f, new GUILayoutOption[0]);
				totalPosition = EditorGUI.PrefixLabel(totalPosition, new GUIContent(TileEditor.Styles.previewLabel));
				Rect position = new Rect(totalPosition.xMin, totalPosition.yMin, 32f, 32f);
				Rect position2 = new Rect(totalPosition.xMin - 1f, totalPosition.yMin - 1f, 34f, 34f);
				if (Event.current.type == EventType.Repaint)
				{
					EditorStyles.textField.Draw(position2, false, false, false, false);
				}
				Texture2D image = SpriteUtility.RenderStaticPreview(sprite, color, 32, 32, transform);
				EditorGUI.DrawTextureTransparent(position, image, ScaleMode.StretchToFill);
			}
		}

		public static Matrix4x4 TransformMatrixOnGUI(Matrix4x4 matrix)
		{
			Matrix4x4 result = matrix;
			if (matrix.ValidTRS())
			{
				EditorGUI.BeginChangeCheck();
				Vector3 vector = TileEditor.Round(matrix.GetColumn(3), 3);
				Vector3 vector2 = TileEditor.Round(matrix.rotation.eulerAngles, 3);
				Vector3 vector3 = TileEditor.Round(matrix.lossyScale, 3);
				vector = EditorGUILayout.Vector3Field(TileEditor.Styles.positionLabel, vector, new GUILayoutOption[0]);
				vector2 = EditorGUILayout.Vector3Field(TileEditor.Styles.rotationLabel, vector2, new GUILayoutOption[0]);
				vector3 = EditorGUILayout.Vector3Field(TileEditor.Styles.scaleLabel, vector3, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck() && vector3.x != 0f && vector3.y != 0f && vector3.z != 0f)
				{
					result = Matrix4x4.TRS(vector, Quaternion.Euler(vector2), vector3);
				}
			}
			else
			{
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUILayout.Label(TileEditor.Styles.invalidMatrixLabel, new GUILayoutOption[0]);
				if (GUILayout.Button(TileEditor.Styles.resetMatrixLabel, new GUILayoutOption[0]))
				{
					result = Matrix4x4.identity;
				}
				GUILayout.EndVertical();
			}
			return result;
		}

		private static Vector3 Round(Vector3 value, int digits)
		{
			float num = Mathf.Pow(10f, (float)digits);
			return new Vector3(Mathf.Round(value.x * num) / num, Mathf.Round(value.y * num) / num, Mathf.Round(value.z * num) / num);
		}

		public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
		{
			return SpriteUtility.RenderStaticPreview(this.tile.sprite, this.tile.color, width, height);
		}
	}
}
