using System;

namespace UnityEditor.Compilation
{
	[Flags]
	public enum AssemblyBuilderFlags
	{
		None = 0,
		EditorAssembly = 1,
		DevelopmentBuild = 2
	}
}
