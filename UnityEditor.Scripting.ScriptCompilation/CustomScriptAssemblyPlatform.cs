using System;
using System.Runtime.InteropServices;

namespace UnityEditor.Scripting.ScriptCompilation
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct CustomScriptAssemblyPlatform
	{
		public string Name
		{
			get;
			private set;
		}

		public string DisplayName
		{
			get;
			private set;
		}

		public BuildTarget BuildTarget
		{
			get;
			private set;
		}

		public CustomScriptAssemblyPlatform(string name, string displayName, BuildTarget buildTarget)
		{
			this = default(CustomScriptAssemblyPlatform);
			this.Name = name;
			this.DisplayName = displayName;
			this.BuildTarget = buildTarget;
		}

		public CustomScriptAssemblyPlatform(string name, BuildTarget buildTarget)
		{
			this = new CustomScriptAssemblyPlatform(name, name, buildTarget);
		}
	}
}
