using System;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(TilemapCollider2D))]
	internal class TilemapCollider2DEditor : Collider2DEditorBase
	{
		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			base.OnInspectorGUI();
			base.serializedObject.ApplyModifiedProperties();
			base.FinalizeInspectorGUI();
		}
	}
}
