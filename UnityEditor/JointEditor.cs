using System;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal abstract class JointEditor<T> : Editor where T : Joint
	{
		protected static class Styles
		{
			public static readonly GUIContent editAngularLimitsButton;

			public static readonly string editAngularLimitsUndoMessage;

			static Styles()
			{
				JointEditor<T>.Styles.editAngularLimitsButton = new GUIContent(EditorGUIUtility.IconContent("JointAngularLimits"));
				JointEditor<T>.Styles.editAngularLimitsUndoMessage = EditorGUIUtility.TextContent("Change Joint Angular Limits").text;
				JointEditor<T>.Styles.editAngularLimitsButton.tooltip = EditorGUIUtility.TextContent("Edit joint angular limits.").text;
			}
		}

		private JointAngularLimitHandle m_AngularLimitHandle = new JointAngularLimitHandle();

		protected JointAngularLimitHandle angularLimitHandle
		{
			get
			{
				return this.m_AngularLimitHandle;
			}
		}

		protected bool editingAngularLimits
		{
			get
			{
				return EditMode.editMode == EditMode.SceneViewEditMode.JointAngularLimits && EditMode.IsOwner(this);
			}
		}

		protected static float GetAngularLimitHandleSize(Vector3 position)
		{
			return HandleUtility.GetHandleSize(position);
		}

		public override void OnInspectorGUI()
		{
			this.DoInspectorEditButtons();
			base.OnInspectorGUI();
		}

		protected void DoInspectorEditButtons()
		{
			T t = (T)((object)base.target);
			EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.JointAngularLimits, "Edit Joint Angular Limits", JointEditor<T>.Styles.editAngularLimitsButton, this);
		}

		internal override Bounds GetWorldBoundsOfTarget(UnityEngine.Object targetObject)
		{
			Bounds worldBoundsOfTarget = base.GetWorldBoundsOfTarget(targetObject);
			worldBoundsOfTarget.Encapsulate(this.GetAngularLimitHandleMatrix((T)((object)targetObject)).MultiplyPoint3x4(Vector3.zero));
			return worldBoundsOfTarget;
		}

		protected virtual void OnSceneGUI()
		{
			if (this.editingAngularLimits)
			{
				T joint = (T)((object)base.target);
				EditorGUI.BeginChangeCheck();
				using (new Handles.DrawingScope(this.GetAngularLimitHandleMatrix(joint)))
				{
					this.DoAngularLimitHandles(joint);
				}
				if (EditorGUI.EndChangeCheck())
				{
					Rigidbody component = joint.GetComponent<Rigidbody>();
					if (component.isKinematic && joint.connectedBody != null)
					{
						joint.connectedBody.WakeUp();
					}
					else
					{
						component.WakeUp();
					}
				}
			}
		}

		protected virtual void GetActors(T joint, out Rigidbody dynamicActor, out Rigidbody connectedActor, out int jointFrameActorIndex, out bool rightHandedLimit)
		{
			jointFrameActorIndex = 1;
			rightHandedLimit = false;
			dynamicActor = joint.GetComponent<Rigidbody>();
			connectedActor = joint.connectedBody;
			if (dynamicActor.isKinematic && connectedActor != null && !connectedActor.isKinematic)
			{
				Rigidbody rigidbody = connectedActor;
				connectedActor = dynamicActor;
				dynamicActor = rigidbody;
			}
		}

		private Matrix4x4 GetAngularLimitHandleMatrix(T joint)
		{
			Rigidbody rigidbody;
			Rigidbody rigidbody2;
			int actorIndex;
			bool flag;
			this.GetActors(joint, out rigidbody, out rigidbody2, out actorIndex, out flag);
			Quaternion lhs = (!(rigidbody2 == null)) ? rigidbody2.transform.rotation : Quaternion.identity;
			Matrix4x4 actorLocalPose = joint.GetActorLocalPose(actorIndex);
			Quaternion rhs = Quaternion.LookRotation(actorLocalPose.MultiplyVector(Vector3.forward), actorLocalPose.MultiplyVector((!flag) ? Vector3.up : Vector3.down));
			Vector3 vector = joint.anchor;
			if (rigidbody != null)
			{
				vector = rigidbody.transform.TransformPoint(vector);
			}
			return Matrix4x4.TRS(vector, lhs * rhs, Vector3.one);
		}

		protected virtual void DoAngularLimitHandles(T joint)
		{
		}
	}
}
