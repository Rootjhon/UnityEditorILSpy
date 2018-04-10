using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.PackageManager.Requests
{
	[Serializable]
	public sealed class SearchRequest : Request<UnityEditor.PackageManager.PackageInfo[]>
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

		private SearchRequest()
		{
		}

		internal SearchRequest(long operationId, NativeClient.StatusCode initialStatus, string packageIdOrName) : base(operationId, initialStatus)
		{
			this.m_PackageIdOrName = packageIdOrName;
		}

		protected override UnityEditor.PackageManager.PackageInfo[] GetResult()
		{
			return (from p in NativeClient.GetSearchOperationData(base.Id)
			select p).ToArray<UnityEditor.PackageManager.PackageInfo>();
		}
	}
}
