using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.PackageManager.Requests
{
	[Serializable]
	public sealed class ListRequest : Request<PackageCollection>
	{
		private ListRequest()
		{
		}

		internal ListRequest(long operationId, NativeClient.StatusCode initialStatus) : base(operationId, initialStatus)
		{
		}

		protected override PackageCollection GetResult()
		{
			OperationStatus listOperationData = NativeClient.GetListOperationData(base.Id);
			IEnumerable<UnityEditor.PackageManager.PackageInfo> packages = from p in listOperationData.packageList
			select p;
			return new PackageCollection(packages);
		}
	}
}
