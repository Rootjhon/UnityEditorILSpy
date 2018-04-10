using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine;

namespace UnityEditor.Compilation
{
	public static class CompilationPipeline
	{
		private static AssemblyDefinitionPlatform[] assemblyDefinitionPlatforms;

		[CompilerGenerated]
		private static Comparison<AssemblyDefinitionPlatform> <>f__mg$cache0;

		public static event Action<string> assemblyCompilationStarted
		{
			add
			{
				Action<string> action = CompilationPipeline.assemblyCompilationStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref CompilationPipeline.assemblyCompilationStarted, (Action<string>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string> action = CompilationPipeline.assemblyCompilationStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref CompilationPipeline.assemblyCompilationStarted, (Action<string>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public static event Action<string, CompilerMessage[]> assemblyCompilationFinished
		{
			add
			{
				Action<string, CompilerMessage[]> action = CompilationPipeline.assemblyCompilationFinished;
				Action<string, CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, CompilerMessage[]>>(ref CompilationPipeline.assemblyCompilationFinished, (Action<string, CompilerMessage[]>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string, CompilerMessage[]> action = CompilationPipeline.assemblyCompilationFinished;
				Action<string, CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, CompilerMessage[]>>(ref CompilationPipeline.assemblyCompilationFinished, (Action<string, CompilerMessage[]>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		static CompilationPipeline()
		{
			CompilationPipeline.SubscribeToEvents(EditorCompilationInterface.Instance);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ClearEditorCompilationErrors();

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void LogEditorCompilationError(string message, int instanceID);

		internal static void SubscribeToEvents(EditorCompilation editorCompilation)
		{
			editorCompilation.assemblyCompilationStarted += delegate(string assemblyPath)
			{
				try
				{
					if (CompilationPipeline.assemblyCompilationStarted != null)
					{
						CompilationPipeline.assemblyCompilationStarted(assemblyPath);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			};
			editorCompilation.assemblyCompilationFinished += delegate(string assemblyPath, CompilerMessage[] messages)
			{
				try
				{
					if (CompilationPipeline.assemblyCompilationFinished != null)
					{
						CompilationPipeline.assemblyCompilationFinished(assemblyPath, messages);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			};
		}

		public static Assembly[] GetAssemblies()
		{
			return CompilationPipeline.GetAssemblies(EditorCompilationInterface.Instance);
		}

		public static string GetAssemblyNameFromScriptPath(string sourceFilePath)
		{
			return CompilationPipeline.GetAssemblyNameFromScriptPath(EditorCompilationInterface.Instance, sourceFilePath);
		}

		public static string GetAssemblyDefinitionFilePathFromScriptPath(string sourceFilePath)
		{
			return CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath(EditorCompilationInterface.Instance, sourceFilePath);
		}

		public static string GetAssemblyDefinitionFilePathFromAssemblyName(string assemblyName)
		{
			return CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(EditorCompilationInterface.Instance, assemblyName);
		}

		public static AssemblyDefinitionPlatform[] GetAssemblyDefinitionPlatforms()
		{
			if (CompilationPipeline.assemblyDefinitionPlatforms == null)
			{
				CompilationPipeline.assemblyDefinitionPlatforms = (from p in CustomScriptAssembly.Platforms
				select new AssemblyDefinitionPlatform(p.Name, p.DisplayName, p.BuildTarget)).ToArray<AssemblyDefinitionPlatform>();
				AssemblyDefinitionPlatform[] arg_5F_0 = CompilationPipeline.assemblyDefinitionPlatforms;
				if (CompilationPipeline.<>f__mg$cache0 == null)
				{
					CompilationPipeline.<>f__mg$cache0 = new Comparison<AssemblyDefinitionPlatform>(CompilationPipeline.CompareAssemblyDefinitionPlatformByDisplayName);
				}
				Array.Sort<AssemblyDefinitionPlatform>(arg_5F_0, CompilationPipeline.<>f__mg$cache0);
			}
			return CompilationPipeline.assemblyDefinitionPlatforms;
		}

		internal static Assembly[] GetAssemblies(EditorCompilation editorCompilation)
		{
			ScriptAssembly[] allEditorScriptAssemblies = editorCompilation.GetAllEditorScriptAssemblies();
			Assembly[] array = new Assembly[allEditorScriptAssemblies.Length];
			Dictionary<ScriptAssembly, Assembly> scriptAssemblyToAssembly = new Dictionary<ScriptAssembly, Assembly>();
			for (int i = 0; i < allEditorScriptAssemblies.Length; i++)
			{
				scriptAssemblyToAssembly.Add(allEditorScriptAssemblies[i], array[i]);
			}
			for (int j = 0; j < allEditorScriptAssemblies.Length; j++)
			{
				ScriptAssembly scriptAssembly = allEditorScriptAssemblies[j];
				string assemblyNameWithoutExtension = AssetPath.GetAssemblyNameWithoutExtension(scriptAssembly.Filename);
				string fullPath = scriptAssembly.FullPath;
				string[] files = scriptAssembly.Files;
				string[] defines = scriptAssembly.Defines;
				string[] references = scriptAssembly.References;
				Assembly[] assemblyReferences = (from a in scriptAssembly.ScriptAssemblyReferences
				select scriptAssemblyToAssembly[a]).ToArray<Assembly>();
				AssemblyFlags assemblyFlags = AssemblyFlags.None;
				if ((scriptAssembly.Flags & UnityEditor.Scripting.ScriptCompilation.AssemblyFlags.EditorOnly) == UnityEditor.Scripting.ScriptCompilation.AssemblyFlags.EditorOnly)
				{
					assemblyFlags |= AssemblyFlags.EditorAssembly;
				}
				array[j] = new Assembly(assemblyNameWithoutExtension, fullPath, files, defines, assemblyReferences, references, assemblyFlags);
			}
			return array;
		}

		private static int CompareAssemblyDefinitionPlatformByDisplayName(AssemblyDefinitionPlatform p1, AssemblyDefinitionPlatform p2)
		{
			return string.Compare(p1.DisplayName, p2.DisplayName, StringComparison.OrdinalIgnoreCase);
		}

		internal static string GetAssemblyNameFromScriptPath(EditorCompilation editorCompilation, string sourceFilePath)
		{
			string result;
			try
			{
				result = editorCompilation.GetTargetAssembly(sourceFilePath).Name;
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		internal static string GetAssemblyDefinitionFilePathFromAssemblyName(EditorCompilation editorCompilation, string assemblyName)
		{
			string result;
			try
			{
				CustomScriptAssembly customScriptAssembly = editorCompilation.FindCustomScriptAssemblyFromAssemblyName(assemblyName);
				result = customScriptAssembly.FilePath;
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		internal static string GetAssemblyDefinitionFilePathFromScriptPath(EditorCompilation editorCompilation, string sourceFilePath)
		{
			string result;
			try
			{
				CustomScriptAssembly customScriptAssembly = editorCompilation.FindCustomScriptAssemblyFromScriptPath(sourceFilePath);
				result = ((customScriptAssembly == null) ? null : customScriptAssembly.FilePath);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}
	}
}
