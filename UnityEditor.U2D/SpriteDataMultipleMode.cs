using System;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.U2D
{
	internal class SpriteDataMultipleMode : SpriteDataSingleMode
	{
		public void Load(SerializedProperty sp)
		{
			this.rect = sp.FindPropertyRelative("m_Rect").rectValue;
			this.border = sp.FindPropertyRelative("m_Border").vector4Value;
			this.name = sp.FindPropertyRelative("m_Name").stringValue;
			this.alignment = (SpriteAlignment)sp.FindPropertyRelative("m_Alignment").intValue;
			this.pivot = SpriteEditorUtility.GetPivotValue(this.alignment, sp.FindPropertyRelative("m_Pivot").vector2Value);
			this.tessellationDetail = sp.FindPropertyRelative("m_TessellationDetail").floatValue;
			SerializedProperty outlineSP = sp.FindPropertyRelative("m_Outline");
			this.outline = SpriteDataSingleMode.AcquireOutline(outlineSP);
			outlineSP = sp.FindPropertyRelative("m_PhysicsShape");
			this.physicsShape = SpriteDataSingleMode.AcquireOutline(outlineSP);
		}

		public void Apply(SerializedProperty sp)
		{
			sp.FindPropertyRelative("m_Rect").rectValue = this.rect;
			sp.FindPropertyRelative("m_Border").vector4Value = this.border;
			sp.FindPropertyRelative("m_Name").stringValue = this.name;
			sp.FindPropertyRelative("m_Alignment").intValue = (int)this.alignment;
			sp.FindPropertyRelative("m_Pivot").vector2Value = this.pivot;
			sp.FindPropertyRelative("m_TessellationDetail").floatValue = this.tessellationDetail;
			SerializedProperty serializedProperty = sp.FindPropertyRelative("m_Outline");
			serializedProperty.ClearArray();
			if (this.outline != null)
			{
				SpriteDataSingleMode.ApplyOutlineChanges(serializedProperty, this.outline);
			}
			SerializedProperty serializedProperty2 = sp.FindPropertyRelative("m_PhysicsShape");
			serializedProperty2.ClearArray();
			if (this.physicsShape != null)
			{
				SpriteDataSingleMode.ApplyOutlineChanges(serializedProperty2, this.physicsShape);
			}
		}
	}
}
