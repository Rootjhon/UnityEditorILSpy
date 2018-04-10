using System;
using System.Runtime.CompilerServices;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal static class EditorCompilationInterface
	{
		private static EditorCompilation editorCompilation;

		[CompilerGenerated]
		private static Action<EditorCompilation.CompilationSetupErrorFlags> <>f__mg$cache0;

		public static EditorCompilation Instance
		{
			get
			{
				if (EditorCompilationInterface.editorCompilation == null)
				{
					EditorCompilationInterface.editorCompilation = new EditorCompilation();
					EditorCompilation expr_1B = EditorCompilationInterface.editorCompilation;
					Delegate arg_3E_0 = expr_1B.setupErrorFlagsChanged;
					if (EditorCompilationInterface.<>f__mg$cache0 == null)
					{
						EditorCompilationInterface.<>f__mg$cache0 = new Action<EditorCompilation.CompilationSetupErrorFlags>(EditorCompilationInterface.ClearErrors);
					}
					expr_1B.setupErrorFlagsChanged = (Action<EditorCompilation.CompilationSetupErrorFlags>)Delegate.Combine(arg_3E_0, EditorCompilationInterface.<>f__mg$cache0);
					CompilationPipeline.ClearEditorCompilationErrors();
				}
				return EditorCompilationInterface.editorCompilation;
			}
		}

		private static void ClearErrors(EditorCompilation.CompilationSetupErrorFlags flags)
		{
			if (flags == EditorCompilation.CompilationSetupErrorFlags.none)
			{
				CompilationPipeline.ClearEditorCompilationErrors();
			}
		}

		private static void LogException(Exception exception)
		{
			AssemblyDefinitionException ex = exception as AssemblyDefinitionException;
			if (ex != null && ex.filePaths.Length > 0)
			{
				string[] filePaths = ex.filePaths;
				for (int i = 0; i < filePaths.Length; i++)
				{
					string text = filePaths[i];
					string message = string.Format("{0} ({1})", exception.Message, text);
					AssemblyDefinitionAsset assemblyDefinitionAsset = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(text);
					int instanceID = assemblyDefinitionAsset.GetInstanceID();
					CompilationPipeline.LogEditorCompilationError(message, instanceID);
				}
			}
			else
			{
				Debug.LogException(exception);
			}
		}

		private static void EmitExceptionAsError(Action action)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				EditorCompilationInterface.LogException(exception);
			}
		}

		private static T EmitExceptionAsError<T>(Func<T> func, T returnValue)
		{
			T result;
			try
			{
				result = func();
			}
			catch (Exception exception)
			{
				EditorCompilationInterface.LogException(exception);
				result = returnValue;
			}
			return result;
		}

		[RequiredByNativeCode]
		public static void SetAssemblySuffix(string suffix)
		{
			EditorCompilationInterface.Instance.SetAssemblySuffix(suffix);
		}

		[RequiredByNativeCode]
		public static void SetAllScripts(string[] allScripts)
		{
			EditorCompilationInterface.Instance.SetAllScripts(allScripts);
		}

		[RequiredByNativeCode]
		public static bool IsExtensionSupportedByCompiler(string extension)
		{
			return EditorCompilationInterface.Instance.IsExtensionSupportedByCompiler(extension);
		}

		[RequiredByNativeCode]
		public static void DirtyAllScripts()
		{
			EditorCompilationInterface.Instance.DirtyAllScripts();
		}

		[RequiredByNativeCode]
		public static void DirtyScript(string path)
		{
			EditorCompilationInterface.Instance.DirtyScript(path);
		}

		[RequiredByNativeCode]
		public static void RunScriptUpdaterOnAssembly(string assemblyFilename)
		{
			EditorCompilationInterface.Instance.RunScriptUpdaterOnAssembly(assemblyFilename);
		}

		[RequiredByNativeCode]
		public static void SetAllPrecompiledAssemblies(PrecompiledAssembly[] precompiledAssemblies)
		{
			EditorCompilationInterface.Instance.SetAllPrecompiledAssemblies(precompiledAssemblies);
		}

		[RequiredByNativeCode]
		public static void SetAllUnityAssemblies(PrecompiledAssembly[] unityAssemblies)
		{
			EditorCompilationInterface.Instance.SetAllUnityAssemblies(unityAssemblies);
		}

		[RequiredByNativeCode]
		public static void SetCompileScriptsOutputDirectory(string directory)
		{
			EditorCompilationInterface.Instance.SetCompileScriptsOutputDirectory(directory);
		}

		[RequiredByNativeCode]
		public static string GetCompileScriptsOutputDirectory()
		{
			return EditorCompilationInterface.EmitExceptionAsError<string>(() => EditorCompilationInterface.Instance.GetCompileScriptsOutputDirectory(), string.Empty);
		}

		[RequiredByNativeCode]
		public static void SetAllCustomScriptAssemblyJsons(string[] allAssemblyJsons)
		{
			EditorCompilationInterface.EmitExceptionAsError(delegate
			{
				EditorCompilationInterface.Instance.SetAllCustomScriptAssemblyJsons(allAssemblyJsons);
			});
		}

		[RequiredByNativeCode]
		public static void SetAllPackageAssemblies(EditorCompilation.PackageAssembly[] packageAssemblies)
		{
			EditorCompilationInterface.EmitExceptionAsError(delegate
			{
				EditorCompilationInterface.Instance.SetAllPackageAssemblies(packageAssemblies);
			});
		}

		[RequiredByNativeCode]
		public static EditorCompilation.TargetAssemblyInfo[] GetAllCompiledAndResolvedCustomTargetAssemblies()
		{
			return EditorCompilationInterface.EmitExceptionAsError<EditorCompilation.TargetAssemblyInfo[]>(() => EditorCompilationInterface.Instance.GetAllCompiledAndResolvedCustomTargetAssemblies(), new EditorCompilation.TargetAssemblyInfo[0]);
		}

		[RequiredByNativeCode]
		public static bool HaveSetupErrors()
		{
			return EditorCompilationInterface.Instance.HaveSetupErrors();
		}

		[RequiredByNativeCode]
		public static void DeleteUnusedAssemblies()
		{
			EditorCompilationInterface.EmitExceptionAsError(delegate
			{
				EditorCompilationInterface.Instance.DeleteUnusedAssemblies();
			});
		}

		[RequiredByNativeCode]
		public static bool CompileScripts(EditorScriptCompilationOptions definesOptions, BuildTargetGroup platformGroup, BuildTarget platform)
		{
			return EditorCompilationInterface.EmitExceptionAsError<bool>(() => EditorCompilationInterface.Instance.CompileScripts(definesOptions, platformGroup, platform), false);
		}

		[RequiredByNativeCode]
		public static bool DoesProjectFolderHaveAnyDirtyScripts()
		{
			return EditorCompilationInterface.Instance.DoesProjectFolderHaveAnyDirtyScripts();
		}

		[RequiredByNativeCode]
		public static bool DoesProjectFolderHaveAnyScripts()
		{
			return EditorCompilationInterface.Instance.DoesProjectFolderHaveAnyScripts();
		}

		[RequiredByNativeCode]
		public static EditorCompilation.AssemblyCompilerMessages[] GetCompileMessages()
		{
			return EditorCompilationInterface.Instance.GetCompileMessages();
		}

		[RequiredByNativeCode]
		public static bool IsCompilationPending()
		{
			return EditorCompilationInterface.Instance.IsCompilationPending();
		}

		[RequiredByNativeCode]
		public static bool IsCompiling()
		{
			return EditorCompilationInterface.Instance.IsCompiling();
		}

		[RequiredByNativeCode]
		public static void StopAllCompilation()
		{
			EditorCompilationInterface.Instance.StopAllCompilation();
		}

		[RequiredByNativeCode]
		public static EditorCompilation.CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platformGroup, BuildTarget platform)
		{
			return EditorCompilationInterface.EmitExceptionAsError<EditorCompilation.CompileStatus>(() => EditorCompilationInterface.Instance.TickCompilationPipeline(options, platformGroup, platform), EditorCompilation.CompileStatus.Idle);
		}

		[RequiredByNativeCode]
		public static EditorCompilation.TargetAssemblyInfo[] GetTargetAssemblies()
		{
			return EditorCompilationInterface.Instance.GetTargetAssemblies();
		}

		[RequiredByNativeCode]
		public static EditorCompilation.TargetAssemblyInfo GetTargetAssembly(string scriptPath)
		{
			return EditorCompilationInterface.Instance.GetTargetAssembly(scriptPath);
		}

		[RequiredByNativeCode]
		public static MonoIsland[] GetAllMonoIslands()
		{
			return EditorCompilationInterface.Instance.GetAllMonoIslands();
		}
	}
}
