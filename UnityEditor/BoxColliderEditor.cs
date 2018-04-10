using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(BoxCollider))]
	internal class BoxColliderEditor : PrimitiveCollider3DEditor
	{
		private SerializedProperty m_Center;

		private SerializedProperty m_Size;

		private readonly BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();

		protected GUIContent centerContent = EditorGUIUtility.TextContent("Center|The position of the Collider in the object’s local space.");

		protected GUIContent sizeContent = EditorGUIUtility.TextContent("Size|The size of the Collider in the X, Y, Z directions.");

		protected override PrimitiveBoundsHandle boundsHandle
		{
			get
			{
				return this.m_BoundsHandle;
			}
		}

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_Center = base.serializedObject.FindProperty("m_Center");
			this.m_Size = base.serializedObject.FindProperty("m_Size");
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			base.InspectorEditButtonGUI();
			EditorGUILayout.PropertyField(this.m_IsTrigger, this.triggerContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Material, this.materialContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Center, this.centerContent, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Size, this.sizeContent, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}

		protected override void CopyColliderPropertiesToHandle()
		{
			BoxCollider boxCollider = (BoxCollider)base.target;
			this.m_BoundsHandle.center = base.TransformColliderCenterToHandleSpace(boxCollider.transform, boxCollider.center);
			this.m_BoundsHandle.size = Vector3.Scale(boxCollider.size, boxCollider.transform.lossyScale);
		}

		protected override void CopyHandlePropertiesToCollider()
		{
			BoxCollider boxCollider = (BoxCollider)base.target;
			boxCollider.center = base.TransformHandleCenterToColliderSpace(boxCollider.transform, this.m_BoundsHandle.center);
			Vector3 size = Vector3.Scale(this.m_BoundsHandle.size, base.InvertScaleVector(boxCollider.transform.lossyScale));
			size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
			boxCollider.size = size;
		}
	}
}
