using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace UnityEditorInternal.VR
{
	public sealed class VREditor
	{
		private static Dictionary<BuildTargetGroup, bool> dirtyDeviceLists = new Dictionary<BuildTargetGroup, bool>();

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern VRDeviceInfoEditor[] GetAllVRDeviceInfo(BuildTargetGroup targetGroup);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern VRDeviceInfoEditor[] GetAllVRDeviceInfoByTarget(BuildTarget target);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool GetVREnabledOnTargetGroup(BuildTargetGroup targetGroup);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetVREnabledOnTargetGroup(BuildTargetGroup targetGroup, bool value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string[] GetVREnabledDevicesOnTargetGroup(BuildTargetGroup targetGroup);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string[] GetVREnabledDevicesOnTarget(BuildTarget target);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetVREnabledDevicesOnTargetGroup(BuildTargetGroup targetGroup, string[] devices);

		public static bool IsDeviceListDirty(BuildTargetGroup targetGroup)
		{
			return VREditor.dirtyDeviceLists.ContainsKey(targetGroup) && VREditor.dirtyDeviceLists[targetGroup];
		}

		private static void SetDeviceListDirty(BuildTargetGroup targetGroup)
		{
			if (VREditor.dirtyDeviceLists.ContainsKey(targetGroup))
			{
				VREditor.dirtyDeviceLists[targetGroup] = true;
			}
			else
			{
				VREditor.dirtyDeviceLists.Add(targetGroup, true);
			}
		}

		public static void ClearDeviceListDirty(BuildTargetGroup targetGroup)
		{
			if (VREditor.dirtyDeviceLists.ContainsKey(targetGroup))
			{
				VREditor.dirtyDeviceLists[targetGroup] = false;
			}
		}

		public static VRDeviceInfoEditor[] GetEnabledVRDeviceInfo(BuildTargetGroup targetGroup)
		{
			string[] enabledVRDevices = VREditor.GetVREnabledDevicesOnTargetGroup(targetGroup);
			return (from d in VREditor.GetAllVRDeviceInfo(targetGroup)
			where enabledVRDevices.Contains(d.deviceNameKey)
			select d).ToArray<VRDeviceInfoEditor>();
		}

		public static VRDeviceInfoEditor[] GetEnabledVRDeviceInfo(BuildTarget target)
		{
			string[] enabledVRDevices = VREditor.GetVREnabledDevicesOnTarget(target);
			return (from d in VREditor.GetAllVRDeviceInfoByTarget(target)
			where enabledVRDevices.Contains(d.deviceNameKey)
			select d).ToArray<VRDeviceInfoEditor>();
		}

		public static bool IsVRDeviceEnabledForBuildTarget(BuildTarget target, string deviceName)
		{
			string[] vREnabledDevicesOnTarget = VREditor.GetVREnabledDevicesOnTarget(target);
			string[] array = vREnabledDevicesOnTarget;
			bool result;
			for (int i = 0; i < array.Length; i++)
			{
				string a = array[i];
				if (a == deviceName)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		public static string[] GetAvailableVirtualRealitySDKs(BuildTargetGroup targetGroup)
		{
			VRDeviceInfoEditor[] allVRDeviceInfo = VREditor.GetAllVRDeviceInfo(targetGroup);
			string[] array = new string[allVRDeviceInfo.Length];
			for (int i = 0; i < allVRDeviceInfo.Length; i++)
			{
				array[i] = allVRDeviceInfo[i].deviceNameKey;
			}
			return array;
		}

		public static string[] GetVirtualRealitySDKs(BuildTargetGroup targetGroup)
		{
			return VREditor.GetVREnabledDevicesOnTargetGroup(targetGroup);
		}

		public static void SetVirtualRealitySDKs(BuildTargetGroup targetGroup, string[] sdks)
		{
			VREditor.SetVREnabledDevicesOnTargetGroup(targetGroup, sdks);
			VREditor.SetDeviceListDirty(targetGroup);
		}
	}
}
