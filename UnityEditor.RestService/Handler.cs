using System;
using UnityEngine.Scripting;

namespace UnityEditor.RestService
{
	[RequiredByNativeCode]
	internal abstract class Handler
	{
		protected abstract void InvokeGet(Request request, string payload, Response writeResponse);

		protected abstract void InvokePost(Request request, string payload, Response writeResponse);

		protected abstract void InvokeDelete(Request request, string payload, Response writeResponse);
	}
}
