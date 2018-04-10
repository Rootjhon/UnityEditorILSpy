using System;
using UnityEditor.PackageManager.Requests;

namespace UnityEditor.PackageManager
{
	public static class Client
	{
		public static ListRequest List()
		{
			long operationId;
			NativeClient.StatusCode initialStatus = NativeClient.List(out operationId);
			return new ListRequest(operationId, initialStatus);
		}

		public static AddRequest Add(string packageIdOrName)
		{
			long operationId;
			NativeClient.StatusCode initialStatus = NativeClient.Add(out operationId, packageIdOrName);
			return new AddRequest(operationId, initialStatus);
		}

		public static RemoveRequest Remove(string packageIdOrName)
		{
			long operationId;
			NativeClient.StatusCode initialStatus = NativeClient.Remove(out operationId, packageIdOrName);
			return new RemoveRequest(operationId, initialStatus, packageIdOrName);
		}

		public static SearchRequest Search(string packageIdOrName)
		{
			long operationId;
			NativeClient.StatusCode initialStatus = NativeClient.Search(out operationId, packageIdOrName);
			return new SearchRequest(operationId, initialStatus, packageIdOrName);
		}

		public static ResetToEditorDefaultsRequest ResetToEditorDefaults()
		{
			long operationId;
			NativeClient.StatusCode initialStatus = NativeClient.ResetToEditorDefaults(out operationId);
			return new ResetToEditorDefaultsRequest(operationId, initialStatus);
		}
	}
}
