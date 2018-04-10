using System;

namespace UnityEditor.PackageManager.Requests
{
	[Serializable]
	public sealed class ResetToEditorDefaultsRequest : Request
	{
		private ResetToEditorDefaultsRequest()
		{
		}

		internal ResetToEditorDefaultsRequest(long operationId, NativeClient.StatusCode initialStatus) : base(operationId, initialStatus)
		{
		}
	}
}
