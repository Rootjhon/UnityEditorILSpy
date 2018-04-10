using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Utils;

namespace UnityEditor.Scripting.Compilers
{
	internal abstract class MonoScriptCompilerBase : ScriptCompilerBase
	{
		protected MonoScriptCompilerBase(MonoIsland island, bool runUpdater) : base(island, runUpdater)
		{
		}

		protected ManagedProgram StartCompiler(BuildTarget target, string compiler, List<string> arguments)
		{
			base.AddCustomResponseFileIfPresent(arguments, Path.GetFileNameWithoutExtension(compiler) + ".rsp");
			string monodistro = (this._island._api_compatibility_level != ApiCompatibilityLevel.NET_4_6) ? MonoInstallationFinder.GetMonoInstallation() : MonoInstallationFinder.GetMonoBleedingEdgeInstallation();
			return this.StartCompiler(target, compiler, arguments, true, monodistro);
		}

		protected ManagedProgram StartCompiler(BuildTarget target, string compiler, List<string> arguments, bool setMonoEnvironmentVariables, string monodistro)
		{
			string text = CommandLineFormatter.GenerateResponseFile(arguments);
			base.RunAPIUpdaterIfRequired(text);
			ManagedProgram managedProgram = new ManagedProgram(monodistro, BuildPipeline.CompatibilityProfileToClassLibFolder(this._island._api_compatibility_level), compiler, " @" + text, setMonoEnvironmentVariables, null);
			managedProgram.Start();
			return managedProgram;
		}
	}
}
