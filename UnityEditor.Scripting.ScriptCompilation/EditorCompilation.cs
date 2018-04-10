using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor.Compilation;
using UnityEditor.Modules;
using UnityEditor.Scripting.Compilers;
using UnityEditor.Scripting.Serialization;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal class EditorCompilation
	{
		public enum CompileStatus
		{
			Idle,
			Compiling,
			CompilationStarted,
			CompilationFailed,
			CompilationComplete
		}

		public enum DeleteFileOptions
		{
			NoLogError,
			LogError
		}

		public struct TargetAssemblyInfo
		{
			public string Name;

			public AssemblyFlags Flags;
		}

		public struct AssemblyCompilerMessages
		{
			public string assemblyFilename;

			public UnityEditor.Scripting.Compilers.CompilerMessage[] messages;
		}

		public struct PackageAssembly
		{
			public string DirectoryPath;

			public string Name;
		}

		[Flags]
		public enum CompilationSetupErrorFlags
		{
			none = 0,
			cyclicReferences = 1,
			loadError = 1
		}

		private bool areAllScriptsDirty;

		private string projectDirectory = string.Empty;

		private string assemblySuffix = string.Empty;

		private HashSet<string> allScripts = new HashSet<string>();

		private HashSet<string> dirtyScripts = new HashSet<string>();

		private HashSet<string> runScriptUpdaterAssemblies = new HashSet<string>();

		private PrecompiledAssembly[] precompiledAssemblies;

		private CustomScriptAssembly[] customScriptAssemblies;

		private CustomScriptAssembly[] packageCustomScriptAssemblies;

		private EditorBuildRules.TargetAssembly[] customTargetAssemblies;

		private PrecompiledAssembly[] unityAssemblies;

		private CompilationTask compilationTask;

		private string outputDirectory;

		private EditorCompilation.CompilationSetupErrorFlags setupErrorFlags = EditorCompilation.CompilationSetupErrorFlags.none;

		private List<AssemblyBuilder> assemblyBuilders = new List<AssemblyBuilder>();

		private static readonly string EditorTempPath;

		public Action<EditorCompilation.CompilationSetupErrorFlags> setupErrorFlagsChanged;

		public event Action<string> assemblyCompilationStarted
		{
			add
			{
				Action<string> action = this.assemblyCompilationStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref this.assemblyCompilationStarted, (Action<string>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string> action = this.assemblyCompilationStarted;
				Action<string> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string>>(ref this.assemblyCompilationStarted, (Action<string>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public event Action<string, UnityEditor.Compilation.CompilerMessage[]> assemblyCompilationFinished
		{
			add
			{
				Action<string, UnityEditor.Compilation.CompilerMessage[]> action = this.assemblyCompilationFinished;
				Action<string, UnityEditor.Compilation.CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, UnityEditor.Compilation.CompilerMessage[]>>(ref this.assemblyCompilationFinished, (Action<string, UnityEditor.Compilation.CompilerMessage[]>)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action<string, UnityEditor.Compilation.CompilerMessage[]> action = this.assemblyCompilationFinished;
				Action<string, UnityEditor.Compilation.CompilerMessage[]> action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action<string, UnityEditor.Compilation.CompilerMessage[]>>(ref this.assemblyCompilationFinished, (Action<string, UnityEditor.Compilation.CompilerMessage[]>)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		static EditorCompilation()
		{
			EditorCompilation.EditorTempPath = "Temp";
		}

		internal string GetAssemblyTimestampPath(string editorAssemblyPath)
		{
			return AssetPath.Combine(editorAssemblyPath, "BuiltinAssemblies.stamp");
		}

		internal void SetProjectDirectory(string projectDirectory)
		{
			this.projectDirectory = projectDirectory;
		}

		internal void SetAssemblySuffix(string assemblySuffix)
		{
			this.assemblySuffix = assemblySuffix;
		}

		public void SetAllScripts(string[] allScripts)
		{
			this.allScripts = new HashSet<string>(allScripts);
			foreach (string current in this.dirtyScripts)
			{
				this.allScripts.Add(current);
			}
		}

		public bool IsExtensionSupportedByCompiler(string extension)
		{
			List<SupportedLanguage> supportedLanguages = ScriptCompilers.SupportedLanguages;
			return supportedLanguages.Count((SupportedLanguage l) => l.GetExtensionICanCompile() == extension) > 0;
		}

		public void DirtyAllScripts()
		{
			this.areAllScriptsDirty = true;
		}

		public void DirtyScript(string path)
		{
			this.allScripts.Add(path);
			this.dirtyScripts.Add(path);
		}

		public void RunScriptUpdaterOnAssembly(string assemblyFilename)
		{
			this.runScriptUpdaterAssemblies.Add(assemblyFilename);
		}

		public void SetAllUnityAssemblies(PrecompiledAssembly[] unityAssemblies)
		{
			this.unityAssemblies = unityAssemblies;
		}

		public void SetCompileScriptsOutputDirectory(string directory)
		{
			this.outputDirectory = directory;
		}

		public string GetCompileScriptsOutputDirectory()
		{
			if (string.IsNullOrEmpty(this.outputDirectory))
			{
				throw new Exception("Must set an output directory through SetCompileScriptsOutputDirectory before compiling");
			}
			return this.outputDirectory;
		}

		public void SetCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags flags)
		{
			EditorCompilation.CompilationSetupErrorFlags compilationSetupErrorFlags = this.setupErrorFlags | flags;
			if (compilationSetupErrorFlags != this.setupErrorFlags)
			{
				this.setupErrorFlags = compilationSetupErrorFlags;
				if (this.setupErrorFlagsChanged != null)
				{
					this.setupErrorFlagsChanged(this.setupErrorFlags);
				}
			}
		}

		public void ClearCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags flags)
		{
			EditorCompilation.CompilationSetupErrorFlags compilationSetupErrorFlags = this.setupErrorFlags & ~flags;
			if (compilationSetupErrorFlags != this.setupErrorFlags)
			{
				this.setupErrorFlags = compilationSetupErrorFlags;
				if (this.setupErrorFlagsChanged != null)
				{
					this.setupErrorFlagsChanged(this.setupErrorFlags);
				}
			}
		}

		public bool HaveSetupErrors()
		{
			return this.setupErrorFlags != EditorCompilation.CompilationSetupErrorFlags.none;
		}

		public void SetAllPrecompiledAssemblies(PrecompiledAssembly[] precompiledAssemblies)
		{
			this.precompiledAssemblies = precompiledAssemblies;
		}

		public PrecompiledAssembly[] GetAllPrecompiledAssemblies()
		{
			return this.precompiledAssemblies;
		}

		public EditorCompilation.TargetAssemblyInfo[] GetAllCompiledAndResolvedCustomTargetAssemblies()
		{
			EditorCompilation.TargetAssemblyInfo[] result;
			if (this.customTargetAssemblies == null)
			{
				result = new EditorCompilation.TargetAssemblyInfo[0];
			}
			else
			{
				Dictionary<EditorBuildRules.TargetAssembly, string> dictionary = new Dictionary<EditorBuildRules.TargetAssembly, string>();
				EditorBuildRules.TargetAssembly[] array = this.customTargetAssemblies;
				for (int i = 0; i < array.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly = array[i];
					string text = targetAssembly.FullPath(this.outputDirectory, this.assemblySuffix);
					if (File.Exists(text))
					{
						dictionary.Add(targetAssembly, text);
					}
				}
				bool flag;
				do
				{
					flag = false;
					if (dictionary.Count > 0)
					{
						EditorBuildRules.TargetAssembly[] array2 = this.customTargetAssemblies;
						for (int j = 0; j < array2.Length; j++)
						{
							EditorBuildRules.TargetAssembly targetAssembly2 = array2[j];
							if (dictionary.ContainsKey(targetAssembly2))
							{
								foreach (EditorBuildRules.TargetAssembly current in targetAssembly2.References)
								{
									if (!dictionary.ContainsKey(current))
									{
										dictionary.Remove(targetAssembly2);
										flag = true;
										break;
									}
								}
							}
						}
					}
				}
				while (flag);
				int count = dictionary.Count;
				EditorCompilation.TargetAssemblyInfo[] array3 = new EditorCompilation.TargetAssemblyInfo[dictionary.Count];
				int num = 0;
				foreach (KeyValuePair<EditorBuildRules.TargetAssembly, string> current2 in dictionary)
				{
					EditorBuildRules.TargetAssembly key = current2.Key;
					array3[num++] = this.ToTargetAssemblyInfo(key);
				}
				result = array3;
			}
			return result;
		}

		private static CustomScriptAssembly LoadCustomScriptAssemblyFromJson(string path)
		{
			string json = File.ReadAllText(path);
			CustomScriptAssembly result;
			try
			{
				CustomScriptAssemblyData customScriptAssemblyData = CustomScriptAssemblyData.FromJson(json);
				result = CustomScriptAssembly.FromCustomScriptAssemblyData(path, customScriptAssemblyData);
			}
			catch (Exception ex)
			{
				throw new AssemblyDefinitionException(ex.Message, new string[]
				{
					path
				});
			}
			return result;
		}

		private string[] CustomTargetAssembliesToFilePaths(IEnumerable<EditorBuildRules.TargetAssembly> targetAssemblies)
		{
			IEnumerable<CustomScriptAssembly> source = from a in targetAssemblies
			select this.FindCustomTargetAssemblyFromTargetAssembly(a);
			return (from a in source
			select a.FilePath).ToArray<string>();
		}

		private string CustomTargetAssembliesToFilePaths(EditorBuildRules.TargetAssembly targetAssembly)
		{
			return this.FindCustomTargetAssemblyFromTargetAssembly(targetAssembly).FilePath;
		}

		private void CheckCyclicAssemblyReferencesDFS(EditorBuildRules.TargetAssembly visitAssembly, HashSet<EditorBuildRules.TargetAssembly> visited)
		{
			if (visited.Contains(visitAssembly))
			{
				throw new AssemblyDefinitionException("Assembly with cyclic references detected", this.CustomTargetAssembliesToFilePaths(visited));
			}
			visited.Add(visitAssembly);
			foreach (EditorBuildRules.TargetAssembly current in visitAssembly.References)
			{
				if (current.Filename == visitAssembly.Filename)
				{
					throw new AssemblyDefinitionException("Assembly contains a references to itself", new string[]
					{
						this.CustomTargetAssembliesToFilePaths(visitAssembly)
					});
				}
				this.CheckCyclicAssemblyReferencesDFS(current, visited);
			}
			visited.Remove(visitAssembly);
		}

		private void CheckCyclicAssemblyReferences()
		{
			if (this.customTargetAssemblies != null && this.customTargetAssemblies.Length >= 1)
			{
				HashSet<EditorBuildRules.TargetAssembly> visited = new HashSet<EditorBuildRules.TargetAssembly>();
				try
				{
					EditorBuildRules.TargetAssembly[] array = this.customTargetAssemblies;
					for (int i = 0; i < array.Length; i++)
					{
						EditorBuildRules.TargetAssembly visitAssembly = array[i];
						this.CheckCyclicAssemblyReferencesDFS(visitAssembly, visited);
					}
				}
				catch (Exception ex)
				{
					this.SetCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags.cyclicReferences);
					throw ex;
				}
			}
		}

		private void UpdateCustomTargetAssemblies()
		{
			List<CustomScriptAssembly> list = new List<CustomScriptAssembly>();
			if (this.customScriptAssemblies != null)
			{
				list.AddRange(this.customScriptAssemblies);
			}
			if (this.packageCustomScriptAssemblies != null)
			{
				if (this.customScriptAssemblies == null)
				{
					list.AddRange(from a in this.packageCustomScriptAssemblies
					select CustomScriptAssembly.Create(a.Name, a.FilePath));
				}
				else
				{
					CustomScriptAssembly[] array = this.packageCustomScriptAssemblies;
					for (int i = 0; i < array.Length; i++)
					{
						CustomScriptAssembly customScriptAssembly = array[i];
						string pathPrefix = customScriptAssembly.PathPrefix.ToLower();
						if (!this.customScriptAssemblies.Any((CustomScriptAssembly a) => a.PathPrefix.ToLower().StartsWith(pathPrefix)))
						{
							if (!this.customScriptAssemblies.Any((CustomScriptAssembly a) => a.PathPrefix.ToLower().StartsWith(pathPrefix)))
							{
								list.Add(CustomScriptAssembly.Create(customScriptAssembly.Name, customScriptAssembly.FilePath));
							}
						}
					}
				}
			}
			foreach (CustomScriptAssembly current in list)
			{
				try
				{
					string[] references = current.References;
					for (int j = 0; j < references.Length; j++)
					{
						string reference = references[j];
						if (!list.Any((CustomScriptAssembly a) => a.Name == reference))
						{
							throw new AssemblyDefinitionException(string.Format("Assembly has reference to non-existent assembly '{0}'", reference), new string[]
							{
								current.FilePath
							});
						}
					}
				}
				catch (Exception ex)
				{
					this.SetCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags.cyclicReferences);
					throw ex;
				}
			}
			this.customTargetAssemblies = EditorBuildRules.CreateTargetAssemblies(list);
			this.ClearCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags.cyclicReferences);
		}

		public void SetAllCustomScriptAssemblyJsons(string[] paths)
		{
			List<CustomScriptAssembly> list = new List<CustomScriptAssembly>();
			this.ClearCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags.cyclicReferences);
			for (int i = 0; i < paths.Length; i++)
			{
				string text = paths[i];
				string path = (!AssetPath.IsPathRooted(text)) ? AssetPath.Combine(this.projectDirectory, text) : AssetPath.GetFullPath(text);
				CustomScriptAssembly loadedCustomScriptAssembly = null;
				try
				{
					loadedCustomScriptAssembly = EditorCompilation.LoadCustomScriptAssemblyFromJson(path);
					IEnumerable<CustomScriptAssembly> source = from a in list
					where string.Equals(a.Name, loadedCustomScriptAssembly.Name, StringComparison.OrdinalIgnoreCase)
					select a;
					if (source.Any<CustomScriptAssembly>())
					{
						List<string> list2 = new List<string>();
						list2.Add(loadedCustomScriptAssembly.FilePath);
						list2.AddRange(from a in source
						select a.FilePath);
						throw new AssemblyDefinitionException(string.Format("Assembly with name '{0}' already exists", loadedCustomScriptAssembly.Name), list2.ToArray());
					}
					IEnumerable<CustomScriptAssembly> source2 = from a in list
					where a.PathPrefix == loadedCustomScriptAssembly.PathPrefix
					select a;
					if (source2.Any<CustomScriptAssembly>())
					{
						List<string> list3 = new List<string>();
						list3.Add(loadedCustomScriptAssembly.FilePath);
						list3.AddRange(from a in source2
						select a.FilePath);
						throw new AssemblyDefinitionException(string.Format("Folder '{0}' contains multiple assembly definition files", loadedCustomScriptAssembly.PathPrefix), list3.ToArray());
					}
					if (loadedCustomScriptAssembly.References == null)
					{
						loadedCustomScriptAssembly.References = new string[0];
					}
					if (loadedCustomScriptAssembly.References.Length != loadedCustomScriptAssembly.References.Distinct<string>().Count<string>())
					{
						throw new AssemblyDefinitionException("Assembly has duplicate references", new string[]
						{
							loadedCustomScriptAssembly.FilePath
						});
					}
				}
				catch (Exception ex)
				{
					this.SetCompilationSetupErrorFlags(EditorCompilation.CompilationSetupErrorFlags.cyclicReferences);
					throw ex;
				}
				list.Add(loadedCustomScriptAssembly);
			}
			this.customScriptAssemblies = list.ToArray();
			this.UpdateCustomTargetAssemblies();
		}

		public void SetAllPackageAssemblies(EditorCompilation.PackageAssembly[] packageAssemblies)
		{
			this.packageCustomScriptAssemblies = (from a in packageAssemblies
			select CustomScriptAssembly.Create(a.Name, AssetPath.GetFullPath(a.DirectoryPath))).ToArray<CustomScriptAssembly>();
			this.UpdateCustomTargetAssemblies();
		}

		public void DeleteUnusedAssemblies()
		{
			string text = AssetPath.Combine(AssetPath.GetDirectoryName(Application.dataPath), this.GetCompileScriptsOutputDirectory());
			if (Directory.Exists(text))
			{
				List<string> list = (from f in Directory.GetFiles(text)
				select AssetPath.ReplaceSeparators(f)).ToList<string>();
				string assemblyTimestampPath = this.GetAssemblyTimestampPath(this.GetCompileScriptsOutputDirectory());
				list.Remove(AssetPath.Combine(AssetPath.GetDirectoryName(Application.dataPath), assemblyTimestampPath));
				ScriptAssembly[] allScriptAssemblies = this.GetAllScriptAssemblies(EditorScriptCompilationOptions.BuildingForEditor);
				ScriptAssembly[] array = allScriptAssemblies;
				for (int i = 0; i < array.Length; i++)
				{
					ScriptAssembly scriptAssembly = array[i];
					if (scriptAssembly.Files.Length > 0)
					{
						string text2 = AssetPath.Combine(text, scriptAssembly.Filename);
						list.Remove(text2);
						list.Remove(EditorCompilation.MDBPath(text2));
						list.Remove(EditorCompilation.PDBPath(text2));
					}
				}
				foreach (string current in list)
				{
					EditorCompilation.DeleteFile(current, EditorCompilation.DeleteFileOptions.LogError);
				}
			}
		}

		public void CleanScriptAssemblies()
		{
			string path = AssetPath.Combine(AssetPath.GetDirectoryName(Application.dataPath), this.GetCompileScriptsOutputDirectory());
			if (Directory.Exists(path))
			{
				string[] files = Directory.GetFiles(path);
				for (int i = 0; i < files.Length; i++)
				{
					string path2 = files[i];
					EditorCompilation.DeleteFile(path2, EditorCompilation.DeleteFileOptions.LogError);
				}
			}
		}

		private static void DeleteFile(string path, EditorCompilation.DeleteFileOptions fileOptions = EditorCompilation.DeleteFileOptions.LogError)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception)
			{
				if (fileOptions == EditorCompilation.DeleteFileOptions.LogError)
				{
					Debug.LogErrorFormat("Could not delete file '{0}'\n", new object[]
					{
						path
					});
				}
			}
		}

		private static bool MoveOrReplaceFile(string sourcePath, string destinationPath)
		{
			bool flag = true;
			try
			{
				File.Move(sourcePath, destinationPath);
			}
			catch (IOException)
			{
				flag = false;
			}
			if (!flag)
			{
				flag = true;
				string text = destinationPath + ".bak";
				EditorCompilation.DeleteFile(text, EditorCompilation.DeleteFileOptions.NoLogError);
				try
				{
					File.Replace(sourcePath, destinationPath, text, true);
				}
				catch (IOException)
				{
					flag = false;
				}
				EditorCompilation.DeleteFile(text, EditorCompilation.DeleteFileOptions.NoLogError);
			}
			return flag;
		}

		private static string PDBPath(string dllPath)
		{
			return dllPath.Replace(".dll", ".pdb");
		}

		private static string MDBPath(string dllPath)
		{
			return dllPath + ".mdb";
		}

		private static bool CopyAssembly(string sourcePath, string destinationPath)
		{
			bool result;
			if (!EditorCompilation.MoveOrReplaceFile(sourcePath, destinationPath))
			{
				result = false;
			}
			else
			{
				string text = EditorCompilation.MDBPath(sourcePath);
				string text2 = EditorCompilation.MDBPath(destinationPath);
				if (File.Exists(text))
				{
					EditorCompilation.MoveOrReplaceFile(text, text2);
				}
				else if (File.Exists(text2))
				{
					EditorCompilation.DeleteFile(text2, EditorCompilation.DeleteFileOptions.LogError);
				}
				string text3 = EditorCompilation.PDBPath(sourcePath);
				string text4 = EditorCompilation.PDBPath(destinationPath);
				if (File.Exists(text3))
				{
					EditorCompilation.MoveOrReplaceFile(text3, text4);
				}
				else if (File.Exists(text4))
				{
					EditorCompilation.DeleteFile(text4, EditorCompilation.DeleteFileOptions.LogError);
				}
				result = true;
			}
			return result;
		}

		public CustomScriptAssembly FindCustomScriptAssemblyFromAssemblyName(string assemblyName)
		{
			List<CustomScriptAssembly> list = new List<CustomScriptAssembly>();
			if (this.customScriptAssemblies != null)
			{
				list.AddRange(this.customScriptAssemblies);
			}
			if (this.packageCustomScriptAssemblies != null)
			{
				list.AddRange(this.packageCustomScriptAssemblies);
			}
			return list.Single((CustomScriptAssembly a) => a.Name == AssetPath.GetAssemblyNameWithoutExtension(assemblyName));
		}

		internal CustomScriptAssembly FindCustomScriptAssemblyFromScriptPath(string scriptPath)
		{
			EditorBuildRules.TargetAssembly customTargetAssembly = EditorBuildRules.GetCustomTargetAssembly(scriptPath, this.projectDirectory, this.customTargetAssemblies);
			return (customTargetAssembly == null) ? null : this.FindCustomScriptAssemblyFromAssemblyName(customTargetAssembly.Filename);
		}

		internal CustomScriptAssembly FindCustomTargetAssemblyFromTargetAssembly(EditorBuildRules.TargetAssembly assembly)
		{
			string assemblyNameWithoutExtension = AssetPath.GetAssemblyNameWithoutExtension(assembly.Filename);
			return this.FindCustomScriptAssemblyFromAssemblyName(assemblyNameWithoutExtension);
		}

		public bool CompileScripts(EditorScriptCompilationOptions options, BuildTargetGroup platformGroup, BuildTarget platform)
		{
			ScriptAssemblySettings scriptAssemblySettings = this.CreateScriptAssemblySettings(platformGroup, platform, options);
			EditorBuildRules.TargetAssembly[] array = null;
			bool result = this.CompileScripts(scriptAssemblySettings, EditorCompilation.EditorTempPath, options, ref array);
			if (array != null)
			{
				EditorBuildRules.TargetAssembly[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly = array2[i];
					CustomScriptAssembly customScriptAssembly = this.customScriptAssemblies.Single((CustomScriptAssembly a) => a.Name == AssetPath.GetAssemblyNameWithoutExtension(targetAssembly.Filename));
					string text = customScriptAssembly.FilePath;
					if (text.StartsWith(this.projectDirectory))
					{
						text = text.Substring(this.projectDirectory.Length);
					}
					Debug.LogWarning(string.Format("Script assembly '{0}' has not been compiled. Folder containing assembly definition file '{1}' contains script files for different script languages. Folder must only contain script files for one script language.", targetAssembly.Filename, text));
				}
			}
			return result;
		}

		internal bool CompileScripts(ScriptAssemblySettings scriptAssemblySettings, string tempBuildDirectory, EditorScriptCompilationOptions options, ref EditorBuildRules.TargetAssembly[] notCompiledTargetAssemblies)
		{
			this.StopAllCompilation();
			bool result;
			if (this.setupErrorFlags != EditorCompilation.CompilationSetupErrorFlags.none)
			{
				result = false;
			}
			else
			{
				this.CheckCyclicAssemblyReferences();
				this.DeleteUnusedAssemblies();
				if (!Directory.Exists(scriptAssemblySettings.OutputDirectory))
				{
					Directory.CreateDirectory(scriptAssemblySettings.OutputDirectory);
				}
				if (!Directory.Exists(tempBuildDirectory))
				{
					Directory.CreateDirectory(tempBuildDirectory);
				}
				IEnumerable<string> enumerable = (!this.areAllScriptsDirty) ? this.dirtyScripts.ToArray<string>() : this.allScripts.ToArray<string>();
				this.areAllScriptsDirty = false;
				this.dirtyScripts.Clear();
				if (!enumerable.Any<string>() && this.runScriptUpdaterAssemblies.Count == 0)
				{
					result = false;
				}
				else
				{
					EditorBuildRules.CompilationAssemblies assemblies = new EditorBuildRules.CompilationAssemblies
					{
						UnityAssemblies = this.unityAssemblies,
						PrecompiledAssemblies = this.precompiledAssemblies,
						CustomTargetAssemblies = this.customTargetAssemblies,
						EditorAssemblyReferences = ModuleUtils.GetAdditionalReferencesForUserScripts()
					};
					EditorBuildRules.GenerateChangedScriptAssembliesArgs generateChangedScriptAssembliesArgs = new EditorBuildRules.GenerateChangedScriptAssembliesArgs
					{
						AllSourceFiles = this.allScripts,
						DirtySourceFiles = enumerable,
						ProjectDirectory = this.projectDirectory,
						Settings = scriptAssemblySettings,
						Assemblies = assemblies,
						RunUpdaterAssemblies = this.runScriptUpdaterAssemblies
					};
					ScriptAssembly[] array = EditorBuildRules.GenerateChangedScriptAssemblies(generateChangedScriptAssembliesArgs);
					notCompiledTargetAssemblies = generateChangedScriptAssembliesArgs.NotCompiledTargetAssemblies.ToArray<EditorBuildRules.TargetAssembly>();
					if (!array.Any<ScriptAssembly>())
					{
						result = false;
					}
					else
					{
						this.compilationTask = new CompilationTask(array, tempBuildDirectory, options, SystemInfo.processorCount);
						this.compilationTask.OnCompilationStarted += delegate(ScriptAssembly assembly, int phase)
						{
							string text = AssetPath.Combine(scriptAssemblySettings.OutputDirectory, assembly.Filename);
							Console.WriteLine("- Starting compile {0}", text);
							this.InvokeAssemblyCompilationStarted(text);
						};
						this.compilationTask.OnCompilationFinished += delegate(ScriptAssembly assembly, List<UnityEditor.Scripting.Compilers.CompilerMessage> messages)
						{
							string text = AssetPath.Combine(scriptAssemblySettings.OutputDirectory, assembly.Filename);
							Console.WriteLine("- Finished compile {0}", text);
							if (this.runScriptUpdaterAssemblies.Contains(assembly.Filename))
							{
								this.runScriptUpdaterAssemblies.Remove(assembly.Filename);
							}
							if (messages.Any((UnityEditor.Scripting.Compilers.CompilerMessage m) => m.type == UnityEditor.Scripting.Compilers.CompilerMessageType.Error))
							{
								this.InvokeAssemblyCompilationFinished(text, messages);
							}
							else
							{
								bool buildingForEditor = scriptAssemblySettings.BuildingForEditor;
								string engineCoreModuleAssemblyPath = InternalEditorUtility.GetEngineCoreModuleAssemblyPath();
								string unityUNet = EditorApplication.applicationContentsPath + "/UnityExtensions/Unity/Networking/UnityEngine.Networking.dll";
								if (!Weaver.WeaveUnetFromEditor(assembly, tempBuildDirectory, tempBuildDirectory, engineCoreModuleAssemblyPath, unityUNet, buildingForEditor))
								{
									messages.Add(new UnityEditor.Scripting.Compilers.CompilerMessage
									{
										message = "UNet Weaver failed",
										type = UnityEditor.Scripting.Compilers.CompilerMessageType.Error,
										file = assembly.FullPath,
										line = -1,
										column = -1
									});
									this.StopAllCompilation();
									this.InvokeAssemblyCompilationFinished(text, messages);
								}
								else if (!EditorCompilation.CopyAssembly(AssetPath.Combine(tempBuildDirectory, assembly.Filename), assembly.FullPath))
								{
									messages.Add(new UnityEditor.Scripting.Compilers.CompilerMessage
									{
										message = string.Format("Copying assembly from directory {0} to {1} failed", tempBuildDirectory, assembly.OutputDirectory),
										type = UnityEditor.Scripting.Compilers.CompilerMessageType.Error,
										file = assembly.FullPath,
										line = -1,
										column = -1
									});
									this.StopAllCompilation();
									this.InvokeAssemblyCompilationFinished(text, messages);
								}
								else
								{
									this.InvokeAssemblyCompilationFinished(text, messages);
								}
							}
						};
						this.compilationTask.Poll();
						result = true;
					}
				}
			}
			return result;
		}

		public void InvokeAssemblyCompilationStarted(string assemblyOutputPath)
		{
			if (this.assemblyCompilationStarted != null)
			{
				this.assemblyCompilationStarted(assemblyOutputPath);
			}
		}

		public void InvokeAssemblyCompilationFinished(string assemblyOutputPath, List<UnityEditor.Scripting.Compilers.CompilerMessage> messages)
		{
			if (this.assemblyCompilationFinished != null)
			{
				UnityEditor.Compilation.CompilerMessage[] arg = EditorCompilation.ConvertCompilerMessages(messages);
				this.assemblyCompilationFinished(assemblyOutputPath, arg);
			}
		}

		public bool DoesProjectFolderHaveAnyDirtyScripts()
		{
			return (this.areAllScriptsDirty && this.allScripts.Count > 0) || this.dirtyScripts.Count > 0;
		}

		public bool DoesProjectFolderHaveAnyScripts()
		{
			return this.allScripts != null && this.allScripts.Count > 0;
		}

		private ScriptAssemblySettings CreateScriptAssemblySettings(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget, EditorScriptCompilationOptions options)
		{
			string[] compilationDefines = InternalEditorUtility.GetCompilationDefines(options, buildTargetGroup, buildTarget);
			return new ScriptAssemblySettings
			{
				BuildTarget = buildTarget,
				BuildTargetGroup = buildTargetGroup,
				OutputDirectory = this.GetCompileScriptsOutputDirectory(),
				Defines = compilationDefines,
				ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup),
				CompilationOptions = options,
				FilenameSuffix = this.assemblySuffix
			};
		}

		private ScriptAssemblySettings CreateEditorScriptAssemblySettings(EditorScriptCompilationOptions options)
		{
			return this.CreateScriptAssemblySettings(EditorUserBuildSettings.activeBuildTargetGroup, EditorUserBuildSettings.activeBuildTarget, options);
		}

		public EditorCompilation.AssemblyCompilerMessages[] GetCompileMessages()
		{
			EditorCompilation.AssemblyCompilerMessages[] result;
			if (this.compilationTask == null)
			{
				result = null;
			}
			else
			{
				EditorCompilation.AssemblyCompilerMessages[] array = new EditorCompilation.AssemblyCompilerMessages[this.compilationTask.CompilerMessages.Count];
				int num = 0;
				foreach (KeyValuePair<ScriptAssembly, UnityEditor.Scripting.Compilers.CompilerMessage[]> current in this.compilationTask.CompilerMessages)
				{
					ScriptAssembly key = current.Key;
					UnityEditor.Scripting.Compilers.CompilerMessage[] value = current.Value;
					array[num++] = new EditorCompilation.AssemblyCompilerMessages
					{
						assemblyFilename = key.Filename,
						messages = value
					};
				}
				Array.Sort<EditorCompilation.AssemblyCompilerMessages>(array, (EditorCompilation.AssemblyCompilerMessages m1, EditorCompilation.AssemblyCompilerMessages m2) => string.Compare(m1.assemblyFilename, m2.assemblyFilename));
				result = array;
			}
			return result;
		}

		public bool IsCompilationPending()
		{
			return this.setupErrorFlags == EditorCompilation.CompilationSetupErrorFlags.none && (this.DoesProjectFolderHaveAnyDirtyScripts() || this.runScriptUpdaterAssemblies.Count<string>() > 0);
		}

		public bool IsAnyAssemblyBuilderCompiling()
		{
			bool result;
			if (this.assemblyBuilders.Count > 0)
			{
				bool flag = false;
				List<AssemblyBuilder> removeAssemblyBuilders = new List<AssemblyBuilder>();
				foreach (AssemblyBuilder current in this.assemblyBuilders)
				{
					AssemblyBuilderStatus status = current.status;
					if (status == AssemblyBuilderStatus.IsCompiling)
					{
						flag = true;
					}
					else if (status == AssemblyBuilderStatus.Finished)
					{
						removeAssemblyBuilders.Add(current);
					}
				}
				if (removeAssemblyBuilders.Count > 0)
				{
					this.assemblyBuilders.RemoveAll((AssemblyBuilder t) => removeAssemblyBuilders.Contains(t));
				}
				result = flag;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public bool IsCompiling()
		{
			return this.IsCompilationTaskCompiling() || this.IsCompilationPending() || this.IsAnyAssemblyBuilderCompiling();
		}

		public bool IsCompilationTaskCompiling()
		{
			return this.compilationTask != null && this.compilationTask.IsCompiling;
		}

		public void StopAllCompilation()
		{
			if (this.compilationTask != null)
			{
				this.compilationTask.Stop();
				this.compilationTask = null;
			}
		}

		public EditorCompilation.CompileStatus TickCompilationPipeline(EditorScriptCompilationOptions options, BuildTargetGroup platformGroup, BuildTarget platform)
		{
			EditorCompilation.CompileStatus result;
			if (this.IsAnyAssemblyBuilderCompiling())
			{
				result = EditorCompilation.CompileStatus.Compiling;
			}
			else
			{
				if (!this.IsCompilationTaskCompiling() && this.IsCompilationPending())
				{
					if (this.CompileScripts(options, platformGroup, platform))
					{
						result = EditorCompilation.CompileStatus.CompilationStarted;
						return result;
					}
				}
				if (this.IsCompilationTaskCompiling())
				{
					if (this.compilationTask.Poll())
					{
						result = ((this.compilationTask != null && !this.compilationTask.CompileErrors) ? EditorCompilation.CompileStatus.CompilationComplete : EditorCompilation.CompileStatus.CompilationFailed);
					}
					else
					{
						result = EditorCompilation.CompileStatus.Compiling;
					}
				}
				else
				{
					result = EditorCompilation.CompileStatus.Idle;
				}
			}
			return result;
		}

		private string AssemblyFilenameWithSuffix(string assemblyFilename)
		{
			string result;
			if (!string.IsNullOrEmpty(this.assemblySuffix))
			{
				string assemblyNameWithoutExtension = AssetPath.GetAssemblyNameWithoutExtension(assemblyFilename);
				result = assemblyNameWithoutExtension + this.assemblySuffix + ".dll";
			}
			else
			{
				result = assemblyFilename;
			}
			return result;
		}

		public EditorCompilation.TargetAssemblyInfo[] GetTargetAssemblies()
		{
			EditorBuildRules.TargetAssembly[] predefinedTargetAssemblies = EditorBuildRules.GetPredefinedTargetAssemblies();
			EditorCompilation.TargetAssemblyInfo[] array = new EditorCompilation.TargetAssemblyInfo[predefinedTargetAssemblies.Length + ((this.customTargetAssemblies == null) ? 0 : this.customTargetAssemblies.Count<EditorBuildRules.TargetAssembly>())];
			for (int i = 0; i < predefinedTargetAssemblies.Length; i++)
			{
				array[i] = this.ToTargetAssemblyInfo(predefinedTargetAssemblies[i]);
			}
			if (this.customTargetAssemblies != null)
			{
				for (int j = 0; j < this.customTargetAssemblies.Count<EditorBuildRules.TargetAssembly>(); j++)
				{
					int num = predefinedTargetAssemblies.Length + j;
					array[num] = this.ToTargetAssemblyInfo(this.customTargetAssemblies[j]);
				}
			}
			return array;
		}

		public ScriptAssembly[] GetAllScriptAssembliesForLanguage<T>() where T : SupportedLanguage
		{
			return (from a in this.GetAllScriptAssemblies(EditorScriptCompilationOptions.BuildingForEditor)
			where a.Language.GetType() == typeof(T)
			select a).ToArray<ScriptAssembly>();
		}

		public ScriptAssembly GetScriptAssemblyForLanguage<T>(string assemblyNameOrPath) where T : SupportedLanguage
		{
			string assemblyName = AssetPath.GetAssemblyNameWithoutExtension(assemblyNameOrPath);
			ScriptAssembly[] allScriptAssembliesForLanguage = this.GetAllScriptAssembliesForLanguage<T>();
			return allScriptAssembliesForLanguage.SingleOrDefault((ScriptAssembly a) => string.Compare(assemblyName, AssetPath.GetAssemblyNameWithoutExtension(a.Filename), StringComparison.OrdinalIgnoreCase) == 0);
		}

		public EditorBuildRules.TargetAssembly[] GetCustomTargetAssemblies()
		{
			return this.customTargetAssemblies;
		}

		public PrecompiledAssembly[] GetUnityAssemblies()
		{
			return this.unityAssemblies;
		}

		public EditorCompilation.TargetAssemblyInfo GetTargetAssembly(string scriptPath)
		{
			EditorBuildRules.TargetAssembly targetAssembly = EditorBuildRules.GetTargetAssembly(scriptPath, this.projectDirectory, this.customTargetAssemblies);
			return this.ToTargetAssemblyInfo(targetAssembly);
		}

		public EditorBuildRules.TargetAssembly GetTargetAssemblyDetails(string scriptPath)
		{
			return EditorBuildRules.GetTargetAssembly(scriptPath, this.projectDirectory, this.customTargetAssemblies);
		}

		public ScriptAssembly[] GetAllEditorScriptAssemblies()
		{
			return this.GetAllScriptAssemblies(EditorScriptCompilationOptions.BuildingForEditor);
		}

		private ScriptAssembly[] GetAllScriptAssemblies(EditorScriptCompilationOptions options)
		{
			return this.GetAllScriptAssemblies(options, this.unityAssemblies, this.precompiledAssemblies);
		}

		private ScriptAssembly[] GetAllScriptAssemblies(EditorScriptCompilationOptions options, PrecompiledAssembly[] unityAssembliesArg, PrecompiledAssembly[] precompiledAssembliesArg)
		{
			EditorBuildRules.CompilationAssemblies assemblies = new EditorBuildRules.CompilationAssemblies
			{
				UnityAssemblies = unityAssembliesArg,
				PrecompiledAssemblies = precompiledAssembliesArg,
				CustomTargetAssemblies = this.customTargetAssemblies,
				EditorAssemblyReferences = ModuleUtils.GetAdditionalReferencesForUserScripts()
			};
			ScriptAssemblySettings settings = this.CreateEditorScriptAssemblySettings(options);
			return EditorBuildRules.GetAllScriptAssemblies(this.allScripts, this.projectDirectory, settings, assemblies);
		}

		public MonoIsland[] GetAllMonoIslands()
		{
			return this.GetAllMonoIslands(this.unityAssemblies, this.precompiledAssemblies, EditorScriptCompilationOptions.BuildingForEditor);
		}

		public MonoIsland[] GetAllMonoIslands(PrecompiledAssembly[] unityAssembliesArg, PrecompiledAssembly[] precompiledAssembliesArg, EditorScriptCompilationOptions options)
		{
			ScriptAssembly[] allScriptAssemblies = this.GetAllScriptAssemblies(options, unityAssembliesArg, precompiledAssembliesArg);
			MonoIsland[] array = new MonoIsland[allScriptAssemblies.Length];
			for (int i = 0; i < allScriptAssemblies.Length; i++)
			{
				array[i] = allScriptAssemblies[i].ToMonoIsland(EditorScriptCompilationOptions.BuildingForEditor, EditorCompilation.EditorTempPath);
			}
			return array;
		}

		public bool IsRuntimeScriptAssembly(string assemblyNameOrPath)
		{
			string assemblyFilename = AssetPath.GetFileName(assemblyNameOrPath);
			if (!assemblyFilename.EndsWith(".dll"))
			{
				assemblyFilename += ".dll";
			}
			EditorBuildRules.TargetAssembly[] predefinedTargetAssemblies = EditorBuildRules.GetPredefinedTargetAssemblies();
			return predefinedTargetAssemblies.Any((EditorBuildRules.TargetAssembly a) => (a.Flags & AssemblyFlags.EditorOnly) != AssemblyFlags.EditorOnly && a.Filename == assemblyFilename) || (this.customTargetAssemblies != null && this.customTargetAssemblies.Any((EditorBuildRules.TargetAssembly a) => (a.Flags & AssemblyFlags.EditorOnly) != AssemblyFlags.EditorOnly && a.Filename == assemblyFilename));
		}

		private EditorCompilation.TargetAssemblyInfo ToTargetAssemblyInfo(EditorBuildRules.TargetAssembly targetAssembly)
		{
			return new EditorCompilation.TargetAssemblyInfo
			{
				Name = this.AssemblyFilenameWithSuffix(targetAssembly.Filename),
				Flags = targetAssembly.Flags
			};
		}

		public ScriptAssembly CreateScriptAssembly(AssemblyBuilder assemblyBuilder)
		{
			EditorScriptCompilationOptions editorScriptCompilationOptions = EditorScriptCompilationOptions.BuildingEmpty;
			AssemblyFlags assemblyFlags = AssemblyFlags.None;
			bool flag = false;
			if ((assemblyBuilder.flags & AssemblyBuilderFlags.DevelopmentBuild) == AssemblyBuilderFlags.DevelopmentBuild)
			{
				editorScriptCompilationOptions |= EditorScriptCompilationOptions.BuildingDevelopmentBuild;
			}
			if ((assemblyBuilder.flags & AssemblyBuilderFlags.EditorAssembly) == AssemblyBuilderFlags.EditorAssembly)
			{
				editorScriptCompilationOptions |= EditorScriptCompilationOptions.BuildingForEditor;
				assemblyFlags |= AssemblyFlags.EditorOnly;
				flag = true;
			}
			string[] files = (from p in assemblyBuilder.scriptPaths
			select AssetPath.Combine(this.projectDirectory, p)).ToArray<string>();
			string path = AssetPath.Combine(this.projectDirectory, assemblyBuilder.assemblyPath);
			string[] array = InternalEditorUtility.GetCompilationDefines(editorScriptCompilationOptions, assemblyBuilder.buildTargetGroup, assemblyBuilder.buildTarget);
			if (assemblyBuilder.additionalDefines != null)
			{
				array = array.Concat(assemblyBuilder.additionalDefines).ToArray<string>();
			}
			ScriptAssembly scriptAssembly = new ScriptAssembly();
			scriptAssembly.Flags = assemblyFlags;
			scriptAssembly.BuildTarget = assemblyBuilder.buildTarget;
			scriptAssembly.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(assemblyBuilder.buildTargetGroup);
			scriptAssembly.Language = ScriptCompilers.GetLanguageFromExtension(ScriptCompilers.GetExtensionOfSourceFile(assemblyBuilder.scriptPaths[0]));
			scriptAssembly.Files = files;
			scriptAssembly.Filename = AssetPath.GetFileName(path);
			scriptAssembly.OutputDirectory = AssetPath.GetDirectoryName(path);
			scriptAssembly.Defines = array;
			scriptAssembly.ScriptAssemblyReferences = new ScriptAssembly[0];
			List<string> unityReferences = EditorBuildRules.GetUnityReferences(scriptAssembly, this.unityAssemblies, editorScriptCompilationOptions);
			List<string> compiledCustomAssembliesReferences = EditorBuildRules.GetCompiledCustomAssembliesReferences(scriptAssembly, this.customTargetAssemblies, this.GetCompileScriptsOutputDirectory(), this.assemblySuffix);
			List<string> precompiledReferences = EditorBuildRules.GetPrecompiledReferences(scriptAssembly, editorScriptCompilationOptions, EditorBuildRules.EditorCompatibility.CompatibleWithEditor, this.precompiledAssemblies);
			string[] second = (!flag) ? new string[0] : ModuleUtils.GetAdditionalReferencesForUserScripts();
			IEnumerable<string> enumerable = unityReferences.Concat(compiledCustomAssembliesReferences).Concat(precompiledReferences).Concat(second);
			if (assemblyBuilder.additionalReferences != null && assemblyBuilder.additionalReferences.Length > 0)
			{
				enumerable = enumerable.Concat(assemblyBuilder.additionalReferences);
			}
			if (assemblyBuilder.excludeReferences != null && assemblyBuilder.excludeReferences.Length > 0)
			{
				enumerable = from r in enumerable
				where !assemblyBuilder.excludeReferences.Contains(r)
				select r;
			}
			scriptAssembly.References = enumerable.ToArray<string>();
			return scriptAssembly;
		}

		public void AddAssemblyBuilder(AssemblyBuilder assemblyBuilder)
		{
			this.assemblyBuilders.Add(assemblyBuilder);
		}

		public static UnityEditor.Compilation.CompilerMessage[] ConvertCompilerMessages(List<UnityEditor.Scripting.Compilers.CompilerMessage> messages)
		{
			UnityEditor.Compilation.CompilerMessage[] array = new UnityEditor.Compilation.CompilerMessage[messages.Count];
			int num = 0;
			foreach (UnityEditor.Scripting.Compilers.CompilerMessage current in messages)
			{
				UnityEditor.Compilation.CompilerMessage compilerMessage = default(UnityEditor.Compilation.CompilerMessage);
				compilerMessage.message = current.message;
				compilerMessage.file = current.file;
				compilerMessage.line = current.line;
				compilerMessage.column = current.column;
				UnityEditor.Scripting.Compilers.CompilerMessageType type = current.type;
				if (type != UnityEditor.Scripting.Compilers.CompilerMessageType.Error)
				{
					if (type == UnityEditor.Scripting.Compilers.CompilerMessageType.Warning)
					{
						compilerMessage.type = UnityEditor.Compilation.CompilerMessageType.Warning;
					}
				}
				else
				{
					compilerMessage.type = UnityEditor.Compilation.CompilerMessageType.Error;
				}
				array[num++] = compilerMessage;
			}
			return array;
		}
	}
}
