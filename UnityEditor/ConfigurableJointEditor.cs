using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(ConfigurableJoint))]
	internal class ConfigurableJointEditor : JointEditor<ConfigurableJoint>
	{
		protected override void GetActors(ConfigurableJoint joint, out Rigidbody dynamicActor, out Rigidbody connectedActor, out int jointFrameActorIndex, out bool rightHandedLimit)
		{
			base.GetActors(joint, out dynamicActor, out connectedActor, out jointFrameActorIndex, out rightHandedLimit);
			if (joint.swapBodies)
			{
				jointFrameActorIndex = 0;
				rightHandedLimit = true;
			}
		}

		protected override void DoAngularLimitHandles(ConfigurableJoint joint)
		{
			base.DoAngularLimitHandles(joint);
			base.angularLimitHandle.xMotion = joint.angularXMotion;
			base.angularLimitHandle.yMotion = joint.angularYMotion;
			base.angularLimitHandle.zMotion = joint.angularZMotion;
			SoftJointLimit softJointLimit = joint.lowAngularXLimit;
			base.angularLimitHandle.xMin = softJointLimit.limit;
			softJointLimit = joint.highAngularXLimit;
			base.angularLimitHandle.xMax = softJointLimit.limit;
			softJointLimit = joint.angularYLimit;
			base.angularLimitHandle.yMax = softJointLimit.limit;
			base.angularLimitHandle.yMin = -softJointLimit.limit;
			softJointLimit = joint.angularZLimit;
			base.angularLimitHandle.zMax = softJointLimit.limit;
			base.angularLimitHandle.zMin = -softJointLimit.limit;
			EditorGUI.BeginChangeCheck();
			base.angularLimitHandle.radius = JointEditor<ConfigurableJoint>.GetAngularLimitHandleSize(Vector3.zero);
			base.angularLimitHandle.DrawHandle();
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(joint, JointEditor<ConfigurableJoint>.Styles.editAngularLimitsUndoMessage);
				softJointLimit = joint.lowAngularXLimit;
				softJointLimit.limit = base.angularLimitHandle.xMin;
				joint.lowAngularXLimit = softJointLimit;
				softJointLimit = joint.highAngularXLimit;
				softJointLimit.limit = base.angularLimitHandle.xMax;
				joint.highAngularXLimit = softJointLimit;
				softJointLimit = joint.angularYLimit;
				softJointLimit.limit = ((base.angularLimitHandle.yMax != softJointLimit.limit) ? base.angularLimitHandle.yMax : (-base.angularLimitHandle.yMin));
				joint.angularYLimit = softJointLimit;
				softJointLimit = joint.angularZLimit;
				softJointLimit.limit = ((base.angularLimitHandle.zMax != softJointLimit.limit) ? base.angularLimitHandle.zMax : (-base.angularLimitHandle.zMin));
				joint.angularZLimit = softJointLimit;
			}
		}
	}
}
