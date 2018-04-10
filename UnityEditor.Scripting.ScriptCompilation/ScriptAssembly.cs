using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Scripting.Compilers;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal class ScriptAssembly
	{
		public AssemblyFlags Flags
		{
			get;
			set;
		}

		public BuildTarget BuildTarget
		{
			get;
			set;
		}

		public SupportedLanguage Language
		{
			get;
			set;
		}

		public ApiCompatibilityLevel ApiCompatibilityLevel
		{
			get;
			set;
		}

		public string Filename
		{
			get;
			set;
		}

		public string OutputDirectory
		{
			get;
			set;
		}

		public ScriptAssembly[] ScriptAssemblyReferences
		{
			get;
			set;
		}

		public string[] References
		{
			get;
			set;
		}

		public string[] Defines
		{
			get;
			set;
		}

		public string[] Files
		{
			get;
			set;
		}

		public bool RunUpdater
		{
			get;
			set;
		}

		public string FullPath
		{
			get
			{
				return AssetPath.Combine(this.OutputDirectory, this.Filename);
			}
		}

		public string[] GetAllReferences()
		{
			return this.References.Concat(from a in this.ScriptAssemblyReferences
			select a.FullPath).ToArray<string>();
		}

		public MonoIsland ToMonoIsland(EditorScriptCompilationOptions options, string buildOutputDirectory)
		{
			bool editor = (options & EditorScriptCompilationOptions.BuildingForEditor) == EditorScriptCompilationOptions.BuildingForEditor;
			bool development_player = (options & EditorScriptCompilationOptions.BuildingDevelopmentBuild) == EditorScriptCompilationOptions.BuildingDevelopmentBuild;
			IEnumerable<string> first = from a in this.ScriptAssemblyReferences
			select AssetPath.Combine(a.OutputDirectory, a.Filename);
			string[] references = first.Concat(this.References).ToArray<string>();
			string output = AssetPath.Combine(buildOutputDirectory, this.Filename);
			return new MonoIsland(this.BuildTarget, editor, development_player, this.ApiCompatibilityLevel, this.Files, references, this.Defines, output);
		}
	}
}
