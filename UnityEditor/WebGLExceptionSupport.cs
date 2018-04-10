using System;

namespace UnityEditor
{
	public enum WebGLExceptionSupport
	{
		None,
		ExplicitlyThrownExceptionsOnly,
		FullWithoutStacktrace,
		FullWithStacktrace
	}
}
