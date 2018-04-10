using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	internal sealed class ModuleMetadata
	{
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string[] GetModuleNames();

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string[] GetModuleDependencies(string moduleName);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool GetModuleStrippable(string moduleName);

		public static UnityType[] GetModuleTypes(string moduleName)
		{
			uint[] moduleTypeIndices = ModuleMetadata.GetModuleTypeIndices(moduleName);
			return (from index in moduleTypeIndices
			select UnityType.GetTypeByRuntimeTypeIndex(index)).ToArray<UnityType>();
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern ModuleIncludeSetting GetModuleIncludeSettingForModule(string module);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern void SetModuleIncludeSettingForModule(string module, ModuleIncludeSetting setting);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern ModuleIncludeSetting GetModuleIncludeSettingForObject(UnityEngine.Object o);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern uint[] GetModuleTypeIndices(string moduleName);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string GetICallModule(string icall);
	}
}
