using System;

namespace UnityEditor
{
	public enum AndroidBuildSystem
	{
		Internal,
		Gradle,
		[Obsolete("ADT/eclipse project export for Android is no longer supported - please use Gradle export instead", true)]
		ADT,
		VisualStudio
	}
}
