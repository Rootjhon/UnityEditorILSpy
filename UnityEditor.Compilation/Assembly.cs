using System;
using System.Linq;

namespace UnityEditor.Compilation
{
	public class Assembly
	{
		public string name
		{
			get;
			private set;
		}

		public string outputPath
		{
			get;
			private set;
		}

		public string[] sourceFiles
		{
			get;
			private set;
		}

		public string[] defines
		{
			get;
			private set;
		}

		public Assembly[] assemblyReferences
		{
			get;
			private set;
		}

		public string[] compiledAssemblyReferences
		{
			get;
			private set;
		}

		public AssemblyFlags flags
		{
			get;
			private set;
		}

		public string[] allReferences
		{
			get
			{
				return (from a in this.assemblyReferences
				select a.outputPath).Concat(this.compiledAssemblyReferences).ToArray<string>();
			}
		}

		public Assembly(string name, string outputPath, string[] sourceFiles, string[] defines, Assembly[] assemblyReferences, string[] compiledAssemblyReferences, AssemblyFlags flags)
		{
			this.name = name;
			this.outputPath = outputPath;
			this.sourceFiles = sourceFiles;
			this.defines = defines;
			this.assemblyReferences = assemblyReferences;
			this.compiledAssemblyReferences = compiledAssemblyReferences;
			this.flags = flags;
		}
	}
}
