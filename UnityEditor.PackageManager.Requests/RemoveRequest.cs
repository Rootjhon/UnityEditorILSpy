using System;
using UnityEngine;

namespace UnityEditor.PackageManager.Requests
{
	[Serializable]
	public sealed class RemoveRequest : Request
	{
		[SerializeField]
		private string m_PackageIdOrName;

		public string PackageIdOrName
		{
			get
			{
				return this.m_PackageIdOrName;
			}
		}

		private RemoveRequest()
		{
		}

		internal RemoveRequest(long operationId, NativeClient.StatusCode initialStatus, string packageName) : base(operationId, initialStatus)
		{
			this.m_PackageIdOrName = packageName;
		}
	}
}
