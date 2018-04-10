using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(HingeJoint))]
	internal class HingeJointEditor : JointEditor<HingeJoint>
	{
		private static readonly GUIContent s_WarningMessage = EditorGUIUtility.TextContent("Min and max limits must be within the range [-180, 180].");

		private SerializedProperty m_MinLimit;

		private SerializedProperty m_MaxLimit;

		private void OnEnable()
		{
			base.angularLimitHandle.yMotion = ConfigurableJointMotion.Locked;
			base.angularLimitHandle.zMotion = ConfigurableJointMotion.Locked;
			base.angularLimitHandle.yHandleColor = Color.clear;
			base.angularLimitHandle.zHandleColor = Color.clear;
			base.angularLimitHandle.xRange = new Vector2(-3.40282326E+38f, 3.40282326E+38f);
			this.m_MinLimit = base.serializedObject.FindProperty("m_Limits.min");
			this.m_MaxLimit = base.serializedObject.FindProperty("m_Limits.max");
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			float floatValue = this.m_MinLimit.floatValue;
			float floatValue2 = this.m_MaxLimit.floatValue;
			if (floatValue < -180f || floatValue > 180f || floatValue2 < -180f || floatValue2 > 180f)
			{
				EditorGUILayout.HelpBox(HingeJointEditor.s_WarningMessage.text, MessageType.Warning);
			}
		}

		protected override void GetActors(HingeJoint joint, out Rigidbody dynamicActor, out Rigidbody connectedActor, out int jointFrameActorIndex, out bool rightHandedLimit)
		{
			base.GetActors(joint, out dynamicActor, out connectedActor, out jointFrameActorIndex, out rightHandedLimit);
			rightHandedLimit = true;
		}

		protected override void DoAngularLimitHandles(HingeJoint joint)
		{
			base.DoAngularLimitHandles(joint);
			base.angularLimitHandle.xMotion = ((!joint.useLimits) ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited);
			JointLimits limits = joint.limits;
			base.angularLimitHandle.xMin = limits.min;
			base.angularLimitHandle.xMax = limits.max;
			EditorGUI.BeginChangeCheck();
			base.angularLimitHandle.radius = JointEditor<HingeJoint>.GetAngularLimitHandleSize(Vector3.zero);
			base.angularLimitHandle.DrawHandle();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(joint, JointEditor<HingeJoint>.Styles.editAngularLimitsUndoMessage);
				limits = joint.limits;
				limits.min = base.angularLimitHandle.xMin;
				limits.max = base.angularLimitHandle.xMax;
				joint.limits = limits;
			}
		}
	}
}
