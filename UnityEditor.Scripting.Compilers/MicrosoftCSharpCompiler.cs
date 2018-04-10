using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.Modules;
using UnityEditor.Utils;

namespace UnityEditor.Scripting.Compilers
{
	internal class MicrosoftCSharpCompiler : ScriptCompilerBase
	{
		private BuildTarget BuildTarget
		{
			get
			{
				return this._island._target;
			}
		}

		public MicrosoftCSharpCompiler(MonoIsland island, bool runUpdater) : base(island, runUpdater)
		{
		}

		private static string[] GetReferencesFromMonoDistribution()
		{
			return new string[]
			{
				"mscorlib.dll",
				"System.dll",
				"System.Core.dll",
				"System.Runtime.Serialization.dll",
				"System.Xml.dll",
				"System.Xml.Linq.dll",
				"UnityScript.dll",
				"UnityScript.Lang.dll",
				"Boo.Lang.dll"
			};
		}

		private string[] GetClassLibraries()
		{
			BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(this.BuildTarget);
			string[] result;
			if (PlayerSettings.GetScriptingBackend(buildTargetGroup) != ScriptingImplementation.WinRTDotNET)
			{
				string monoAssemblyDirectory = base.GetMonoProfileLibDirectory();
				List<string> list = new List<string>();
				list.AddRange(from dll in MicrosoftCSharpCompiler.GetReferencesFromMonoDistribution()
				select Path.Combine(monoAssemblyDirectory, dll));
				if (PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup) == ApiCompatibilityLevel.NET_4_6)
				{
					string path = Path.Combine(monoAssemblyDirectory, "Facades");
					list.AddRange(Directory.GetFiles(path, "*.dll"));
				}
				result = list.ToArray();
			}
			else
			{
				if (this.BuildTarget != BuildTarget.WSAPlayer)
				{
					throw new InvalidOperationException(string.Format("MicrosoftCSharpCompiler cannot build for .NET Scripting backend for BuildTarget.{0}.", this.BuildTarget));
				}
				NuGetPackageResolver nuGetPackageResolver = new NuGetPackageResolver
				{
					ProjectLockFile = "UWP\\project.lock.json"
				};
				result = nuGetPackageResolver.Resolve();
			}
			return result;
		}

		private void FillCompilerOptions(List<string> arguments, out string argsPrefix)
		{
			argsPrefix = "/noconfig ";
			arguments.Add("/nostdlib+");
			arguments.Add("/preferreduilang:en-US");
			IPlatformSupportModule platformSupportModule = ModuleManager.FindPlatformSupportModule(ModuleManager.GetTargetStringFromBuildTarget(this.BuildTarget));
			ICompilationExtension compilationExtension = platformSupportModule.CreateCompilationExtension();
			arguments.AddRange(from r in this.GetClassLibraries()
			select "/reference:\"" + r + "\"");
			arguments.AddRange(from r in compilationExtension.GetAdditionalAssemblyReferences()
			select "/reference:\"" + r + "\"");
			arguments.AddRange(from r in compilationExtension.GetWindowsMetadataReferences()
			select "/reference:\"" + r + "\"");
			arguments.AddRange(from d in compilationExtension.GetAdditionalDefines()
			select "/define:" + d);
			arguments.AddRange(compilationExtension.GetAdditionalSourceFiles());
		}

		private static void ThrowCompilerNotFoundException(string path)
		{
			throw new Exception(string.Format("'{0}' not found. Is your Unity installation corrupted?", path));
		}

		private Program StartCompilerImpl(List<string> arguments, string argsPrefix)
		{
			string[] references = this._island._references;
			for (int i = 0; i < references.Length; i++)
			{
				string fileName = references[i];
				arguments.Add("/reference:" + ScriptCompilerBase.PrepareFileName(fileName));
			}
			foreach (string current in this._island._defines.Distinct<string>())
			{
				arguments.Add("/define:" + current);
			}
			string[] files = this._island._files;
			for (int j = 0; j < files.Length; j++)
			{
				string fileName2 = files[j];
				arguments.Add(ScriptCompilerBase.PrepareFileName(fileName2).Replace('/', '\\'));
			}
			string text = Paths.Combine(new string[]
			{
				EditorApplication.applicationContentsPath,
				"Tools",
				"Roslyn",
				"CoreRun.exe"
			}).Replace('/', '\\');
			string text2 = Paths.Combine(new string[]
			{
				EditorApplication.applicationContentsPath,
				"Tools",
				"Roslyn",
				"csc.exe"
			}).Replace('/', '\\');
			if (!File.Exists(text))
			{
				MicrosoftCSharpCompiler.ThrowCompilerNotFoundException(text);
			}
			if (!File.Exists(text2))
			{
				MicrosoftCSharpCompiler.ThrowCompilerNotFoundException(text2);
			}
			base.AddCustomResponseFileIfPresent(arguments, "csc.rsp");
			string text3 = CommandLineFormatter.GenerateResponseFile(arguments);
			base.RunAPIUpdaterIfRequired(text3);
			ProcessStartInfo si = new ProcessStartInfo
			{
				Arguments = string.Concat(new string[]
				{
					"\"",
					text2,
					"\" ",
					argsPrefix,
					"@",
					text3
				}),
				FileName = text,
				CreateNoWindow = true
			};
			Program program = new Program(si);
			program.Start();
			return program;
		}

		protected override Program StartCompiler()
		{
			string str = ScriptCompilerBase.PrepareFileName(this._island._output);
			List<string> list = new List<string>
			{
				"/target:library",
				"/nowarn:0169",
				"/unsafe",
				"/out:" + str
			};
			if (!this._island._development_player)
			{
				list.Add("/debug:pdbonly");
				list.Add("/optimize+");
			}
			else
			{
				list.Add("/debug:full");
				list.Add("/optimize-");
			}
			string argsPrefix;
			this.FillCompilerOptions(list, out argsPrefix);
			return this.StartCompilerImpl(list, argsPrefix);
		}

		protected override string[] GetStreamContainingCompilerMessages()
		{
			return base.GetStandardOutput();
		}

		protected override CompilerOutputParserBase CreateOutputParser()
		{
			return new MicrosoftCSharpCompilerOutputParser();
		}
	}
}
