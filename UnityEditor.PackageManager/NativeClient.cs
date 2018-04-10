using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

namespace UnityEditor.PackageManager
{
	internal sealed class NativeClient
	{
		public enum StatusCode : uint
		{
			InQueue,
			InProgress,
			Done,
			Error,
			NotFound
		}

		public static Dictionary<string, OutdatedPackage> GetOutdatedOperationData(long operationId)
		{
			string[] array;
			OutdatedPackage[] outdatedOperationData = NativeClient.GetOutdatedOperationData(operationId, out array);
			Dictionary<string, OutdatedPackage> dictionary = new Dictionary<string, OutdatedPackage>(array.Length);
			for (int i = 0; i < array.Length; i++)
			{
				dictionary[array[i]] = outdatedOperationData[i];
			}
			return dictionary;
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode List(out long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode Add(out long operationId, string packageId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode Remove(out long operationId, string packageId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode Search(out long operationId, string packageName);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode Outdated(out long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode ResetToEditorDefaults(out long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern NativeClient.StatusCode GetOperationStatus(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern Error GetOperationError(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern OperationStatus GetListOperationData(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern UpmPackageInfo GetAddOperationData(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern string GetRemoveOperationData(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern UpmPackageInfo[] GetSearchOperationData(long operationId);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern OutdatedPackage[] GetOutdatedOperationData(long operationId, out string[] names);
	}
}
