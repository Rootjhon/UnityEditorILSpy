using System;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(HingeJoint2D))]
	internal class HingeJoint2DEditor : AnchoredJoint2DEditor
	{
		protected new static class Styles
		{
			public static readonly GUIContent editAngularLimitsButton;

			public static readonly string editAngularLimitsUndoMessage;

			public static readonly Color handleColor;

			public static readonly float handleRadius;

			static Styles()
			{
				HingeJoint2DEditor.Styles.editAngularLimitsButton = new GUIContent(EditorGUIUtility.IconContent("JointAngularLimits"));
				HingeJoint2DEditor.Styles.editAngularLimitsUndoMessage = EditorGUIUtility.TextContent("Change Joint Angular Limits").text;
				HingeJoint2DEditor.Styles.handleColor = new Color(0f, 1f, 0f, 0.7f);
				HingeJoint2DEditor.Styles.handleRadius = 0.8f;
				HingeJoint2DEditor.Styles.editAngularLimitsButton.tooltip = EditorGUIUtility.TextContent("Edit joint angular limits.").text;
			}
		}

		private JointAngularLimitHandle m_AngularLimitHandle = new JointAngularLimitHandle();

		private static readonly Quaternion s_RightHandedHandleOrientationOffset = Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(90f, Vector3.up);

		private static readonly Quaternion s_LeftHandedHandleOrientationOffset = Quaternion.AngleAxis(180f, Vector3.forward) * Quaternion.AngleAxis(90f, Vector3.up);

		public new void OnEnable()
		{
			base.OnEnable();
			this.m_AngularLimitHandle.xHandleColor = Color.white;
			this.m_AngularLimitHandle.yHandleColor = Color.clear;
			this.m_AngularLimitHandle.zHandleColor = Color.clear;
			this.m_AngularLimitHandle.yMotion = ConfigurableJointMotion.Locked;
			this.m_AngularLimitHandle.zMotion = ConfigurableJointMotion.Locked;
			this.m_AngularLimitHandle.xRange = new Vector2(-1000000f, 1000000f);
		}

		public override void OnInspectorGUI()
		{
			HingeJoint2D hingeJoint2D = (HingeJoint2D)base.target;
			EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.JointAngularLimits, "Edit Joint Angular Limits", HingeJoint2DEditor.Styles.editAngularLimitsButton, this);
			base.OnInspectorGUI();
		}

		internal override Bounds GetWorldBoundsOfTarget(UnityEngine.Object targetObject)
		{
			Bounds worldBoundsOfTarget = base.GetWorldBoundsOfTarget(targetObject);
			HingeJoint2D hingeJoint2D = (HingeJoint2D)targetObject;
			worldBoundsOfTarget.Encapsulate(Joint2DEditor.TransformPoint(hingeJoint2D.transform, hingeJoint2D.anchor));
			return worldBoundsOfTarget;
		}

		private void NonEditableHandleDrawFunction(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
		{
		}

		public new void OnSceneGUI()
		{
			HingeJoint2D hingeJoint2D = (HingeJoint2D)base.target;
			if (hingeJoint2D.enabled)
			{
				this.m_AngularLimitHandle.xMotion = ((!hingeJoint2D.useLimits) ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited);
				JointAngleLimits2D limits = hingeJoint2D.limits;
				this.m_AngularLimitHandle.xMin = limits.min;
				this.m_AngularLimitHandle.xMax = limits.max;
				bool flag = EditMode.editMode == EditMode.SceneViewEditMode.JointAngularLimits && EditMode.IsOwner(this);
				if (flag)
				{
					this.m_AngularLimitHandle.angleHandleDrawFunction = null;
				}
				else
				{
					this.m_AngularLimitHandle.angleHandleDrawFunction = new Handles.CapFunction(this.NonEditableHandleDrawFunction);
				}
				Rigidbody2D rigidbody2D = hingeJoint2D.attachedRigidbody;
				Vector3 point = Vector3.right;
				Vector2 v = hingeJoint2D.anchor;
				Rigidbody2D rigidbody2D2 = hingeJoint2D.connectedBody;
				Quaternion rhs = HingeJoint2DEditor.s_RightHandedHandleOrientationOffset;
				if (rigidbody2D.bodyType != RigidbodyType2D.Dynamic && hingeJoint2D.connectedBody != null && hingeJoint2D.connectedBody.bodyType == RigidbodyType2D.Dynamic)
				{
					rigidbody2D = hingeJoint2D.connectedBody;
					point = Vector3.left;
					v = hingeJoint2D.connectedAnchor;
					rigidbody2D2 = hingeJoint2D.attachedRigidbody;
					rhs = HingeJoint2DEditor.s_LeftHandedHandleOrientationOffset;
				}
				Vector3 vector = Joint2DEditor.TransformPoint(rigidbody2D.transform, v);
				Quaternion q = ((!(rigidbody2D2 == null)) ? Quaternion.LookRotation(Vector3.forward, rigidbody2D2.transform.rotation * Vector3.up) : Quaternion.identity) * rhs;
				Vector3 point2 = vector + Quaternion.LookRotation(Vector3.forward, rigidbody2D.transform.rotation * Vector3.up) * point;
				Matrix4x4 matrix = Matrix4x4.TRS(vector, q, Vector3.one);
				EditorGUI.BeginChangeCheck();
				using (new Handles.DrawingScope(HingeJoint2DEditor.Styles.handleColor, matrix))
				{
					float num = HandleUtility.GetHandleSize(Vector3.zero) * HingeJoint2DEditor.Styles.handleRadius;
					this.m_AngularLimitHandle.radius = num;
					Handles.DrawLine(Vector3.zero, matrix.inverse.MultiplyPoint3x4(point2).normalized * num);
					this.m_AngularLimitHandle.DrawHandle();
				}
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(hingeJoint2D, HingeJoint2DEditor.Styles.editAngularLimitsUndoMessage);
					limits = hingeJoint2D.limits;
					limits.min = this.m_AngularLimitHandle.xMin;
					limits.max = this.m_AngularLimitHandle.xMax;
					hingeJoint2D.limits = limits;
					rigidbody2D.WakeUp();
				}
				base.OnSceneGUI();
			}
		}
	}
}
