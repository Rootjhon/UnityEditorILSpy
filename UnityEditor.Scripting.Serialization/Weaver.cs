using Mono.Cecil;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UNetWeaver;
using UnityEditor.Modules;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor.Scripting.Serialization
{
	internal static class Weaver
	{
		[CompilerGenerated]
		private static Action<string> <>f__mg$cache0;

		[CompilerGenerated]
		private static Action<string> <>f__mg$cache1;

		private static ManagedProgram SerializationWeaverProgramWith(string arguments, string playerPackage)
		{
			return Weaver.ManagedProgramFor(playerPackage + "/SerializationWeaver/SerializationWeaver.exe", arguments);
		}

		private static ManagedProgram ManagedProgramFor(string exe, string arguments)
		{
			return new ManagedProgram(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), null, exe, arguments, false, null);
		}

		private static ICompilationExtension GetCompilationExtension()
		{
			string targetStringFromBuildTarget = ModuleManager.GetTargetStringFromBuildTarget(EditorUserBuildSettings.activeBuildTarget);
			return ModuleManager.GetCompilationExtension(targetStringFromBuildTarget);
		}

		private static void QueryAssemblyPathsAndResolver(ICompilationExtension compilationExtension, string file, bool editor, out string[] assemblyPaths, out IAssemblyResolver assemblyResolver)
		{
			assemblyResolver = compilationExtension.GetAssemblyResolver(editor, file, null);
			assemblyPaths = compilationExtension.GetCompilerExtraAssemblyPaths(editor, file).ToArray<string>();
		}

		public static bool WeaveUnetFromEditor(ScriptAssembly assembly, string assemblyDirectory, string outputDirectory, string unityEngine, string unityUNet, bool buildingForEditor)
		{
			bool result;
			if ((assembly.Flags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly)
			{
				result = true;
			}
			else
			{
				string text = Path.Combine(assemblyDirectory, assembly.Filename);
				ICompilationExtension compilationExtension = Weaver.GetCompilationExtension();
				string[] extraAssemblyPaths;
				IAssemblyResolver assemblyResolver;
				Weaver.QueryAssemblyPathsAndResolver(compilationExtension, text, buildingForEditor, out extraAssemblyPaths, out assemblyResolver);
				result = Weaver.WeaveInto(assembly, text, outputDirectory, unityEngine, unityUNet, extraAssemblyPaths, assemblyResolver);
			}
			return result;
		}

		private static bool WeaveInto(ScriptAssembly assembly, string assemblyPath, string outputDirectory, string unityEngine, string unityUNet, string[] extraAssemblyPaths, IAssemblyResolver assemblyResolver)
		{
			string[] allReferences = assembly.GetAllReferences();
			string[] array = new string[allReferences.Count<string>() + ((extraAssemblyPaths == null) ? 0 : extraAssemblyPaths.Length)];
			int index = 0;
			string[] array2 = allReferences;
			for (int i = 0; i < array2.Length; i++)
			{
				string path = array2[i];
				array[index++] = Path.GetDirectoryName(path);
			}
			if (extraAssemblyPaths != null)
			{
				extraAssemblyPaths.CopyTo(array, index);
			}
			bool result;
			try
			{
				string[] expr_73 = new string[]
				{
					assemblyPath
				};
				string[] arg_B4_4 = array;
				if (Weaver.<>f__mg$cache0 == null)
				{
					Weaver.<>f__mg$cache0 = new Action<string>(Debug.LogWarning);
				}
				Action<string> arg_B4_6 = Weaver.<>f__mg$cache0;
				if (Weaver.<>f__mg$cache1 == null)
				{
					Weaver.<>f__mg$cache1 = new Action<string>(Debug.LogError);
				}
				if (!Unity.UNetWeaver.Program.Process(unityEngine, unityUNet, outputDirectory, expr_73, arg_B4_4, assemblyResolver, arg_B4_6, Weaver.<>f__mg$cache1))
				{
					Debug.LogError("Failure generating network code.");
					result = false;
					return result;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception generating network code: " + ex.ToString() + " " + ex.StackTrace);
			}
			result = true;
			return result;
		}
	}
}
