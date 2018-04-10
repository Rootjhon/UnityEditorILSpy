using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(CharacterJoint))]
	internal class CharacterJointEditor : JointEditor<CharacterJoint>
	{
		protected override void DoAngularLimitHandles(CharacterJoint joint)
		{
			base.DoAngularLimitHandles(joint);
			base.angularLimitHandle.xMotion = ConfigurableJointMotion.Limited;
			base.angularLimitHandle.yMotion = ConfigurableJointMotion.Limited;
			base.angularLimitHandle.zMotion = ConfigurableJointMotion.Limited;
			SoftJointLimit softJointLimit = joint.lowTwistLimit;
			base.angularLimitHandle.xMin = softJointLimit.limit;
			softJointLimit = joint.highTwistLimit;
			base.angularLimitHandle.xMax = softJointLimit.limit;
			softJointLimit = joint.swing1Limit;
			base.angularLimitHandle.yMax = softJointLimit.limit;
			base.angularLimitHandle.yMin = -softJointLimit.limit;
			softJointLimit = joint.swing2Limit;
			base.angularLimitHandle.zMax = softJointLimit.limit;
			base.angularLimitHandle.zMin = -softJointLimit.limit;
			EditorGUI.BeginChangeCheck();
			base.angularLimitHandle.radius = JointEditor<CharacterJoint>.GetAngularLimitHandleSize(Vector3.zero);
			base.angularLimitHandle.DrawHandle();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(joint, JointEditor<CharacterJoint>.Styles.editAngularLimitsUndoMessage);
				softJointLimit = joint.lowTwistLimit;
				softJointLimit.limit = base.angularLimitHandle.xMin;
				joint.lowTwistLimit = softJointLimit;
				softJointLimit = joint.highTwistLimit;
				softJointLimit.limit = base.angularLimitHandle.xMax;
				joint.highTwistLimit = softJointLimit;
				softJointLimit = joint.swing1Limit;
				softJointLimit.limit = ((base.angularLimitHandle.yMax != softJointLimit.limit) ? base.angularLimitHandle.yMax : (-base.angularLimitHandle.yMin));
				joint.swing1Limit = softJointLimit;
				softJointLimit = joint.swing2Limit;
				softJointLimit.limit = ((base.angularLimitHandle.zMax != softJointLimit.limit) ? base.angularLimitHandle.zMax : (-base.angularLimitHandle.zMin));
				joint.swing2Limit = softJointLimit;
			}
		}
	}
}
