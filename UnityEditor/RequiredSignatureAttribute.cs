using System;

namespace UnityEditor
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	internal sealed class RequiredSignatureAttribute : Attribute
	{
	}
}
