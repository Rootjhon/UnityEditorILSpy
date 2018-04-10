using System;
using System.Collections.Generic;
using UnityEditor.Experimental.U2D;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.U2D
{
	internal class SpriteDataSingleMode : SpriteDataBase
	{
		public override SpriteAlignment alignment
		{
			get;
			set;
		}

		public override Vector4 border
		{
			get;
			set;
		}

		public override string name
		{
			get;
			set;
		}

		public override List<Vector2[]> outline
		{
			get;
			set;
		}

		public override List<Vector2[]> physicsShape
		{
			get;
			set;
		}

		public override Vector2 pivot
		{
			get;
			set;
		}

		public override Rect rect
		{
			get;
			set;
		}

		public override float tessellationDetail
		{
			get;
			set;
		}

		public void Apply(SerializedObject so)
		{
			so.FindProperty("m_Alignment").intValue = (int)this.alignment;
			so.FindProperty("m_SpriteBorder").vector4Value = this.border;
			so.FindProperty("m_SpritePivot").vector2Value = this.pivot;
			so.FindProperty("m_SpriteTessellationDetail").floatValue = this.tessellationDetail;
			SerializedProperty serializedProperty = so.FindProperty("m_SpriteSheet.m_Outline");
			if (this.outline != null)
			{
				SpriteDataSingleMode.ApplyOutlineChanges(serializedProperty, this.outline);
			}
			else
			{
				serializedProperty.ClearArray();
			}
			SerializedProperty serializedProperty2 = so.FindProperty("m_SpriteSheet.m_PhysicsShape");
			if (this.physicsShape != null)
			{
				SpriteDataSingleMode.ApplyOutlineChanges(serializedProperty2, this.physicsShape);
			}
			else
			{
				serializedProperty2.ClearArray();
			}
		}

		public void Load(SerializedObject so)
		{
			TextureImporter textureImporter = so.targetObject as TextureImporter;
			this.name = textureImporter.name;
			this.alignment = (SpriteAlignment)so.FindProperty("m_Alignment").intValue;
			this.border = textureImporter.spriteBorder;
			this.pivot = SpriteEditorUtility.GetPivotValue(this.alignment, textureImporter.spritePivot);
			this.tessellationDetail = so.FindProperty("m_SpriteTessellationDetail").floatValue;
			SerializedProperty outlineSP = so.FindProperty("m_SpriteSheet.m_Outline");
			this.outline = SpriteDataSingleMode.AcquireOutline(outlineSP);
			SerializedProperty outlineSP2 = so.FindProperty("m_SpriteSheet.m_PhysicsShape");
			this.physicsShape = SpriteDataSingleMode.AcquireOutline(outlineSP2);
			Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(textureImporter.assetPath);
			this.rect = new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height);
		}

		protected static List<Vector2[]> AcquireOutline(SerializedProperty outlineSP)
		{
			List<Vector2[]> list = new List<Vector2[]>();
			for (int i = 0; i < outlineSP.arraySize; i++)
			{
				SerializedProperty arrayElementAtIndex = outlineSP.GetArrayElementAtIndex(i);
				Vector2[] array = new Vector2[arrayElementAtIndex.arraySize];
				for (int j = 0; j < arrayElementAtIndex.arraySize; j++)
				{
					array[j] = arrayElementAtIndex.GetArrayElementAtIndex(j).vector2Value;
				}
				list.Add(array);
			}
			return list;
		}

		protected static void ApplyOutlineChanges(SerializedProperty outlineSP, List<Vector2[]> outline)
		{
			outlineSP.ClearArray();
			for (int i = 0; i < outline.Count; i++)
			{
				outlineSP.InsertArrayElementAtIndex(i);
				Vector2[] array = outline[i];
				SerializedProperty arrayElementAtIndex = outlineSP.GetArrayElementAtIndex(i);
				arrayElementAtIndex.ClearArray();
				for (int j = 0; j < array.Length; j++)
				{
					arrayElementAtIndex.InsertArrayElementAtIndex(j);
					arrayElementAtIndex.GetArrayElementAtIndex(j).vector2Value = array[j];
				}
			}
		}
	}
}
