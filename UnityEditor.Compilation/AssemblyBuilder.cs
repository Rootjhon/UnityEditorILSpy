using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Scripting.Compilers;
using UnityEditor.Scripting.ScriptCompilation;
using UnityEngine;

namespace UnityEditor.Compilation
{
	public class AssemblyBuilder
	{
		private CompilationTask compilationTask;

		public event Action<string> buildStarted
		{
			add
			{
				Action<string> action = this.buildStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref this.buildStarted, (Action<string>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string> action = this.buildStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref this.buildStarted, (Action<string>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public event Action<string, CompilerMessage[]> buildFinished
		{
			add
			{
				Action<string, CompilerMessage[]> action = this.buildFinished;
				Action<string, CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, CompilerMessage[]>>(ref this.buildFinished, (Action<string, CompilerMessage[]>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string, CompilerMessage[]> action = this.buildFinished;
				Action<string, CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, CompilerMessage[]>>(ref this.buildFinished, (Action<string, CompilerMessage[]>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public string[] scriptPaths
		{
			get;
			private set;
		}

		public string assemblyPath
		{
			get;
			private set;
		}

		public string[] additionalDefines
		{
			get;
			set;
		}

		public string[] additionalReferences
		{
			get;
			set;
		}

		public string[] excludeReferences
		{
			get;
			set;
		}

		public AssemblyBuilderFlags flags
		{
			get;
			set;
		}

		public BuildTargetGroup buildTargetGroup
		{
			get;
			set;
		}

		public BuildTarget buildTarget
		{
			get;
			set;
		}

		public AssemblyBuilderStatus status
		{
			get
			{
				AssemblyBuilderStatus result;
				if (this.compilationTask == null)
				{
					result = AssemblyBuilderStatus.NotStarted;
				}
				else if (this.compilationTask.IsCompiling)
				{
					result = ((!this.compilationTask.Poll()) ? AssemblyBuilderStatus.IsCompiling : AssemblyBuilderStatus.Finished);
				}
				else
				{
					result = AssemblyBuilderStatus.Finished;
				}
				return result;
			}
		}

		public AssemblyBuilder(string assemblyPath, params string[] scriptPaths)
		{
			if (string.IsNullOrEmpty(assemblyPath))
			{
				throw new ArgumentException("assemblyPath cannot be null or empty");
			}
			if (scriptPaths == null || scriptPaths.Length == 0)
			{
				throw new ArgumentException("scriptPaths cannot be null or empty");
			}
			this.scriptPaths = scriptPaths;
			this.assemblyPath = assemblyPath;
			this.flags = AssemblyBuilderFlags.None;
			this.buildTargetGroup = EditorUserBuildSettings.activeBuildTargetGroup;
			this.buildTarget = EditorUserBuildSettings.activeBuildTarget;
		}

		public bool Build()
		{
			return this.Build(EditorCompilationInterface.Instance);
		}

		internal bool Build(EditorCompilation editorCompilation)
		{
			bool result;
			if (editorCompilation.IsCompilationTaskCompiling())
			{
				result = false;
			}
			else
			{
				if (this.status != AssemblyBuilderStatus.NotStarted)
				{
					throw new Exception(string.Format("Cannot start AssemblyBuilder with status {0}. Expected {1}", this.status, AssemblyBuilderStatus.NotStarted));
				}
				ScriptAssembly scriptAssembly = editorCompilation.CreateScriptAssembly(this);
				this.compilationTask = new CompilationTask(new ScriptAssembly[]
				{
					scriptAssembly
				}, scriptAssembly.OutputDirectory, EditorScriptCompilationOptions.BuildingEmpty, 1);
				this.compilationTask.OnCompilationStarted += delegate(ScriptAssembly assembly, int phase)
				{
					editorCompilation.InvokeAssemblyCompilationStarted(this.assemblyPath);
					this.OnCompilationStarted(assembly, phase);
				};
				this.compilationTask.OnCompilationFinished += delegate(ScriptAssembly assembly, List<UnityEditor.Scripting.Compilers.CompilerMessage> messages)
				{
					editorCompilation.InvokeAssemblyCompilationFinished(this.assemblyPath, messages);
					this.OnCompilationFinished(assembly, messages);
				};
				this.compilationTask.Poll();
				editorCompilation.AddAssemblyBuilder(this);
				result = true;
			}
			return result;
		}

		private void OnCompilationStarted(ScriptAssembly assembly, int phase)
		{
			if (this.buildStarted != null)
			{
				try
				{
					this.buildStarted(this.assemblyPath);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		private void OnCompilationFinished(ScriptAssembly assembly, List<UnityEditor.Scripting.Compilers.CompilerMessage> messages)
		{
			if (this.buildFinished != null)
			{
				CompilerMessage[] arg = EditorCompilation.ConvertCompilerMessages(messages);
				try
				{
					this.buildFinished(this.assemblyPath, arg);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}
