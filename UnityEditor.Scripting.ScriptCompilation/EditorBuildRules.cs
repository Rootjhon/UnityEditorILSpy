using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Scripting.Compilers;
using UnityEngine;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal static class EditorBuildRules
	{
		[Flags]
		internal enum TargetAssemblyType
		{
			Undefined = 0,
			Predefined = 1,
			Custom = 2
		}

		internal enum EditorCompatibility
		{
			NotCompatibleWithEditor,
			CompatibleWithEditor
		}

		internal class TargetAssembly
		{
			public string Filename
			{
				get;
				private set;
			}

			public SupportedLanguage Language
			{
				get;
				set;
			}

			public AssemblyFlags Flags
			{
				get;
				private set;
			}

			public Func<string, int> PathFilter
			{
				get;
				private set;
			}

			public Func<BuildTarget, EditorScriptCompilationOptions, bool> IsCompatibleFunc
			{
				get;
				private set;
			}

			public List<EditorBuildRules.TargetAssembly> References
			{
				get;
				private set;
			}

			public EditorBuildRules.TargetAssemblyType Type
			{
				get;
				private set;
			}

			public EditorBuildRules.EditorCompatibility editorCompatibility
			{
				get
				{
					bool flag = this.IsCompatibleFunc == null || this.IsCompatibleFunc(BuildTarget.NoTarget, EditorScriptCompilationOptions.BuildingForEditor);
					return (!flag) ? EditorBuildRules.EditorCompatibility.NotCompatibleWithEditor : EditorBuildRules.EditorCompatibility.CompatibleWithEditor;
				}
			}

			public TargetAssembly()
			{
				this.References = new List<EditorBuildRules.TargetAssembly>();
			}

			public TargetAssembly(string name, SupportedLanguage language, AssemblyFlags flags, EditorBuildRules.TargetAssemblyType type) : this(name, language, flags, type, null, null)
			{
			}

			public TargetAssembly(string name, SupportedLanguage language, AssemblyFlags flags, EditorBuildRules.TargetAssemblyType type, Func<string, int> pathFilter, Func<BuildTarget, EditorScriptCompilationOptions, bool> compatFunc) : this()
			{
				this.Language = language;
				this.Filename = name;
				this.Flags = flags;
				this.PathFilter = pathFilter;
				this.IsCompatibleFunc = compatFunc;
				this.Type = type;
			}

			public string FilenameWithSuffix(string filenameSuffix)
			{
				string result;
				if (!string.IsNullOrEmpty(filenameSuffix))
				{
					result = this.Filename.Replace(".dll", filenameSuffix + ".dll");
				}
				else
				{
					result = this.Filename;
				}
				return result;
			}

			public string FullPath(string outputDirectory, string filenameSuffix)
			{
				return AssetPath.Combine(outputDirectory, this.FilenameWithSuffix(filenameSuffix));
			}
		}

		public class CompilationAssemblies
		{
			public PrecompiledAssembly[] UnityAssemblies
			{
				get;
				set;
			}

			public PrecompiledAssembly[] PrecompiledAssemblies
			{
				get;
				set;
			}

			public EditorBuildRules.TargetAssembly[] CustomTargetAssemblies
			{
				get;
				set;
			}

			public string[] EditorAssemblyReferences
			{
				get;
				set;
			}
		}

		public class GenerateChangedScriptAssembliesArgs
		{
			public IEnumerable<string> AllSourceFiles
			{
				get;
				set;
			}

			public IEnumerable<string> DirtySourceFiles
			{
				get;
				set;
			}

			public string ProjectDirectory
			{
				get;
				set;
			}

			public ScriptAssemblySettings Settings
			{
				get;
				set;
			}

			public EditorBuildRules.CompilationAssemblies Assemblies
			{
				get;
				set;
			}

			public HashSet<string> RunUpdaterAssemblies
			{
				get;
				set;
			}

			public HashSet<EditorBuildRules.TargetAssembly> NotCompiledTargetAssemblies
			{
				get;
				set;
			}

			public GenerateChangedScriptAssembliesArgs()
			{
				this.NotCompiledTargetAssemblies = new HashSet<EditorBuildRules.TargetAssembly>();
			}
		}

		private static readonly EditorBuildRules.TargetAssembly[] predefinedTargetAssemblies;

		[CompilerGenerated]
		private static Func<string, int> <>f__mg$cache0;

		[CompilerGenerated]
		private static Func<string, int> <>f__mg$cache1;

		[CompilerGenerated]
		private static Func<BuildTarget, EditorScriptCompilationOptions, bool> <>f__mg$cache2;

		[CompilerGenerated]
		private static Func<string, int> <>f__mg$cache3;

		[CompilerGenerated]
		private static Func<BuildTarget, EditorScriptCompilationOptions, bool> <>f__mg$cache4;

		static EditorBuildRules()
		{
			EditorBuildRules.predefinedTargetAssemblies = EditorBuildRules.CreatePredefinedTargetAssemblies();
		}

		public static EditorBuildRules.TargetAssembly[] GetPredefinedTargetAssemblies()
		{
			return EditorBuildRules.predefinedTargetAssemblies;
		}

		public static PrecompiledAssembly CreateUserCompiledAssembly(string path)
		{
			AssemblyFlags assemblyFlags = AssemblyFlags.None;
			string text = path.ToLower();
			if (text.Contains("/editor/") || text.Contains("\\editor\\"))
			{
				assemblyFlags |= AssemblyFlags.EditorOnly;
			}
			return new PrecompiledAssembly
			{
				Path = path,
				Flags = assemblyFlags
			};
		}

		public static PrecompiledAssembly CreateEditorCompiledAssembly(string path)
		{
			return new PrecompiledAssembly
			{
				Path = path,
				Flags = AssemblyFlags.EditorOnly
			};
		}

		public static EditorBuildRules.TargetAssembly[] CreateTargetAssemblies(IEnumerable<CustomScriptAssembly> customScriptAssemblies)
		{
			EditorBuildRules.TargetAssembly[] result;
			if (customScriptAssemblies == null)
			{
				result = null;
			}
			else
			{
				using (IEnumerator<CustomScriptAssembly> enumerator = customScriptAssemblies.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						CustomScriptAssembly customAssembly = enumerator.Current;
						if (EditorBuildRules.predefinedTargetAssemblies.Any((EditorBuildRules.TargetAssembly p) => AssetPath.GetAssemblyNameWithoutExtension(p.Filename) == customAssembly.Name))
						{
							throw new Exception(string.Format("Assembly cannot be have reserved name '{0}'. Defined in '{1}'", customAssembly.Name, customAssembly.FilePath));
						}
					}
				}
				List<EditorBuildRules.TargetAssembly> list = new List<EditorBuildRules.TargetAssembly>();
				Dictionary<string, EditorBuildRules.TargetAssembly> dictionary = new Dictionary<string, EditorBuildRules.TargetAssembly>();
				using (IEnumerator<CustomScriptAssembly> enumerator2 = customScriptAssemblies.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						EditorBuildRules.<CreateTargetAssemblies>c__AnonStorey2 <CreateTargetAssemblies>c__AnonStorey2 = new EditorBuildRules.<CreateTargetAssemblies>c__AnonStorey2();
						<CreateTargetAssemblies>c__AnonStorey2.customAssembly = enumerator2.Current;
						string pathPrefixLowerCase = <CreateTargetAssemblies>c__AnonStorey2.customAssembly.PathPrefix.ToLower();
						EditorBuildRules.TargetAssembly targetAssembly = new EditorBuildRules.TargetAssembly(<CreateTargetAssemblies>c__AnonStorey2.customAssembly.Name + ".dll", null, <CreateTargetAssemblies>c__AnonStorey2.customAssembly.AssemblyFlags, EditorBuildRules.TargetAssemblyType.Custom, (string path) => (!path.StartsWith(pathPrefixLowerCase)) ? -1 : pathPrefixLowerCase.Length, (BuildTarget target, EditorScriptCompilationOptions options) => <CreateTargetAssemblies>c__AnonStorey2.customAssembly.IsCompatibleWith(target, options));
						list.Add(targetAssembly);
						dictionary[<CreateTargetAssemblies>c__AnonStorey2.customAssembly.Name] = targetAssembly;
					}
				}
				List<EditorBuildRules.TargetAssembly>.Enumerator enumerator3 = list.GetEnumerator();
				foreach (CustomScriptAssembly current in customScriptAssemblies)
				{
					enumerator3.MoveNext();
					EditorBuildRules.TargetAssembly current2 = enumerator3.Current;
					if (current.References != null)
					{
						string[] references = current.References;
						for (int i = 0; i < references.Length; i++)
						{
							string text = references[i];
							EditorBuildRules.TargetAssembly item = null;
							if (!dictionary.TryGetValue(text, out item))
							{
								Debug.LogWarning(string.Format("Could not find reference '{0}' for assembly '{1}'", text, current.Name));
							}
							else
							{
								current2.References.Add(item);
							}
						}
					}
				}
				result = list.ToArray();
			}
			return result;
		}

		public static ScriptAssembly[] GetAllScriptAssemblies(IEnumerable<string> allSourceFiles, string projectDirectory, ScriptAssemblySettings settings, EditorBuildRules.CompilationAssemblies assemblies)
		{
			ScriptAssembly[] result;
			if (allSourceFiles == null || allSourceFiles.Count<string>() == 0)
			{
				result = new ScriptAssembly[0];
			}
			else
			{
				Dictionary<EditorBuildRules.TargetAssembly, HashSet<string>> dictionary = new Dictionary<EditorBuildRules.TargetAssembly, HashSet<string>>();
				foreach (string current in allSourceFiles)
				{
					EditorBuildRules.TargetAssembly targetAssembly = EditorBuildRules.GetTargetAssembly(current, projectDirectory, assemblies.CustomTargetAssemblies);
					if (EditorBuildRules.IsCompatibleWithPlatform(targetAssembly, settings))
					{
						HashSet<string> hashSet;
						if (!dictionary.TryGetValue(targetAssembly, out hashSet))
						{
							hashSet = new HashSet<string>();
							dictionary[targetAssembly] = hashSet;
						}
						hashSet.Add(AssetPath.Combine(projectDirectory, current));
					}
				}
				result = EditorBuildRules.ToScriptAssemblies(dictionary, settings, assemblies, null);
			}
			return result;
		}

		public static ScriptAssembly[] GenerateChangedScriptAssemblies(EditorBuildRules.GenerateChangedScriptAssembliesArgs args)
		{
			Dictionary<EditorBuildRules.TargetAssembly, HashSet<string>> dictionary = new Dictionary<EditorBuildRules.TargetAssembly, HashSet<string>>();
			EditorBuildRules.TargetAssembly[] array = (args.Assemblies.CustomTargetAssemblies != null) ? EditorBuildRules.predefinedTargetAssemblies.Concat(args.Assemblies.CustomTargetAssemblies).ToArray<EditorBuildRules.TargetAssembly>() : EditorBuildRules.predefinedTargetAssemblies;
			if (args.RunUpdaterAssemblies != null)
			{
				using (HashSet<string>.Enumerator enumerator = args.RunUpdaterAssemblies.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string assemblyFilename = enumerator.Current;
						EditorBuildRules.TargetAssembly key = array.First((EditorBuildRules.TargetAssembly a) => a.FilenameWithSuffix(args.Settings.FilenameSuffix) == assemblyFilename);
						dictionary[key] = new HashSet<string>();
					}
				}
			}
			foreach (string current in args.DirtySourceFiles)
			{
				EditorBuildRules.TargetAssembly targetAssembly = EditorBuildRules.GetTargetAssembly(current, args.ProjectDirectory, args.Assemblies.CustomTargetAssemblies);
				if (EditorBuildRules.IsCompatibleWithPlatform(targetAssembly, args.Settings))
				{
					string extensionOfSourceFile = ScriptCompilers.GetExtensionOfSourceFile(current);
					SupportedLanguage languageFromExtension = ScriptCompilers.GetLanguageFromExtension(extensionOfSourceFile);
					HashSet<string> hashSet;
					if (!dictionary.TryGetValue(targetAssembly, out hashSet))
					{
						hashSet = new HashSet<string>();
						dictionary[targetAssembly] = hashSet;
						if (targetAssembly.Type == EditorBuildRules.TargetAssemblyType.Custom)
						{
							targetAssembly.Language = languageFromExtension;
						}
					}
					hashSet.Add(AssetPath.Combine(args.ProjectDirectory, current));
					if (languageFromExtension != targetAssembly.Language)
					{
						args.NotCompiledTargetAssemblies.Add(targetAssembly);
					}
				}
			}
			bool flag = dictionary.Any((KeyValuePair<EditorBuildRules.TargetAssembly, HashSet<string>> entry) => entry.Key.Type == EditorBuildRules.TargetAssemblyType.Custom);
			if (flag)
			{
				EditorBuildRules.TargetAssembly[] array2 = EditorBuildRules.predefinedTargetAssemblies;
				for (int i = 0; i < array2.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly2 = array2[i];
					if (EditorBuildRules.IsCompatibleWithPlatform(targetAssembly2, args.Settings))
					{
						if (!dictionary.ContainsKey(targetAssembly2))
						{
							dictionary[targetAssembly2] = new HashSet<string>();
						}
					}
				}
			}
			int num;
			do
			{
				num = 0;
				EditorBuildRules.TargetAssembly[] array3 = array;
				for (int j = 0; j < array3.Length; j++)
				{
					EditorBuildRules.TargetAssembly targetAssembly3 = array3[j];
					if (EditorBuildRules.IsCompatibleWithPlatform(targetAssembly3, args.Settings))
					{
						if (!dictionary.ContainsKey(targetAssembly3))
						{
							foreach (EditorBuildRules.TargetAssembly current2 in targetAssembly3.References)
							{
								if (dictionary.ContainsKey(current2))
								{
									dictionary[targetAssembly3] = new HashSet<string>();
									num++;
									break;
								}
							}
						}
					}
				}
			}
			while (num > 0);
			foreach (string current3 in args.AllSourceFiles)
			{
				EditorBuildRules.TargetAssembly targetAssembly4 = EditorBuildRules.GetTargetAssembly(current3, args.ProjectDirectory, args.Assemblies.CustomTargetAssemblies);
				if (EditorBuildRules.IsCompatibleWithPlatform(targetAssembly4, args.Settings))
				{
					string extensionOfSourceFile2 = ScriptCompilers.GetExtensionOfSourceFile(current3);
					SupportedLanguage languageFromExtension2 = ScriptCompilers.GetLanguageFromExtension(extensionOfSourceFile2);
					if (targetAssembly4.Language == null && targetAssembly4.Type == EditorBuildRules.TargetAssemblyType.Custom)
					{
						targetAssembly4.Language = languageFromExtension2;
					}
					if (languageFromExtension2 != targetAssembly4.Language)
					{
						args.NotCompiledTargetAssemblies.Add(targetAssembly4);
					}
					HashSet<string> hashSet2;
					if (dictionary.TryGetValue(targetAssembly4, out hashSet2))
					{
						hashSet2.Add(AssetPath.Combine(args.ProjectDirectory, current3));
					}
				}
			}
			dictionary = (from e in dictionary
			where e.Value.Count > 0
			select e).ToDictionary((KeyValuePair<EditorBuildRules.TargetAssembly, HashSet<string>> e) => e.Key, (KeyValuePair<EditorBuildRules.TargetAssembly, HashSet<string>> e) => e.Value);
			foreach (EditorBuildRules.TargetAssembly current4 in args.NotCompiledTargetAssemblies)
			{
				dictionary.Remove(current4);
			}
			return EditorBuildRules.ToScriptAssemblies(dictionary, args.Settings, args.Assemblies, args.RunUpdaterAssemblies);
		}

		internal static ScriptAssembly[] ToScriptAssemblies(IDictionary<EditorBuildRules.TargetAssembly, HashSet<string>> targetAssemblies, ScriptAssemblySettings settings, EditorBuildRules.CompilationAssemblies assemblies, HashSet<string> runUpdaterAssemblies)
		{
			ScriptAssembly[] array = new ScriptAssembly[targetAssemblies.Count];
			Dictionary<EditorBuildRules.TargetAssembly, ScriptAssembly> dictionary = new Dictionary<EditorBuildRules.TargetAssembly, ScriptAssembly>();
			int num = 0;
			bool buildingForEditor = settings.BuildingForEditor;
			foreach (KeyValuePair<EditorBuildRules.TargetAssembly, HashSet<string>> current in targetAssemblies)
			{
				EditorBuildRules.TargetAssembly key = current.Key;
				HashSet<string> value = current.Value;
				ScriptAssembly scriptAssembly = new ScriptAssembly();
				array[num] = scriptAssembly;
				dictionary[key] = array[num++];
				scriptAssembly.Flags = key.Flags;
				scriptAssembly.BuildTarget = settings.BuildTarget;
				scriptAssembly.Language = key.Language;
				if ((key.Flags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly || (buildingForEditor && settings.ApiCompatibilityLevel == ApiCompatibilityLevel.NET_4_6))
				{
					scriptAssembly.ApiCompatibilityLevel = ((EditorApplication.scriptingRuntimeVersion != ScriptingRuntimeVersion.Latest) ? ApiCompatibilityLevel.NET_2_0 : ApiCompatibilityLevel.NET_4_6);
				}
				else
				{
					scriptAssembly.ApiCompatibilityLevel = settings.ApiCompatibilityLevel;
				}
				if (!string.IsNullOrEmpty(settings.FilenameSuffix))
				{
					string assemblyNameWithoutExtension = AssetPath.GetAssemblyNameWithoutExtension(key.Filename);
					scriptAssembly.Filename = assemblyNameWithoutExtension + settings.FilenameSuffix + ".dll";
				}
				else
				{
					scriptAssembly.Filename = key.Filename;
				}
				if (runUpdaterAssemblies != null && runUpdaterAssemblies.Contains(scriptAssembly.Filename))
				{
					scriptAssembly.RunUpdater = true;
				}
				scriptAssembly.OutputDirectory = settings.OutputDirectory;
				scriptAssembly.Defines = settings.Defines;
				scriptAssembly.Files = value.ToArray<string>();
				Array.Sort<string>(scriptAssembly.Files);
			}
			num = 0;
			foreach (KeyValuePair<EditorBuildRules.TargetAssembly, HashSet<string>> current2 in targetAssemblies)
			{
				EditorBuildRules.AddScriptAssemblyReferences(ref array[num++], current2.Key, settings, assemblies, dictionary, settings.FilenameSuffix);
			}
			return array;
		}

		private static bool IsPrecompiledAssemblyCompatibleWithScriptAssembly(PrecompiledAssembly compiledAssembly, ScriptAssembly scriptAssembly)
		{
			bool flag = WSAHelpers.UseDotNetCore(scriptAssembly);
			bool result;
			if (flag)
			{
				bool flag2 = (compiledAssembly.Flags & AssemblyFlags.UseForDotNet) == AssemblyFlags.UseForDotNet;
				result = flag2;
			}
			else
			{
				bool flag3 = (compiledAssembly.Flags & AssemblyFlags.UseForMono) == AssemblyFlags.UseForMono;
				result = flag3;
			}
			return result;
		}

		internal static void AddScriptAssemblyReferences(ref ScriptAssembly scriptAssembly, EditorBuildRules.TargetAssembly targetAssembly, ScriptAssemblySettings settings, EditorBuildRules.CompilationAssemblies assemblies, IDictionary<EditorBuildRules.TargetAssembly, ScriptAssembly> targetToScriptAssembly, string filenameSuffix)
		{
			List<ScriptAssembly> list = new List<ScriptAssembly>();
			List<string> list2 = new List<string>();
			bool buildingForEditor = settings.BuildingForEditor;
			List<string> unityReferences = EditorBuildRules.GetUnityReferences(scriptAssembly, assemblies.UnityAssemblies, settings.CompilationOptions);
			list2.AddRange(unityReferences);
			foreach (EditorBuildRules.TargetAssembly current in targetAssembly.References)
			{
				ScriptAssembly item;
				if (targetToScriptAssembly.TryGetValue(current, out item))
				{
					list.Add(item);
				}
				else
				{
					string text = current.FullPath(settings.OutputDirectory, filenameSuffix);
					if (File.Exists(text))
					{
						list2.Add(text);
					}
				}
			}
			if (assemblies.CustomTargetAssemblies != null && (targetAssembly.Type & EditorBuildRules.TargetAssemblyType.Predefined) == EditorBuildRules.TargetAssemblyType.Predefined)
			{
				EditorBuildRules.TargetAssembly[] customTargetAssemblies = assemblies.CustomTargetAssemblies;
				for (int i = 0; i < customTargetAssemblies.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly2 = customTargetAssemblies[i];
					ScriptAssembly item2;
					if (targetToScriptAssembly.TryGetValue(targetAssembly2, out item2))
					{
						list.Add(item2);
					}
					else
					{
						string text2 = targetAssembly2.FullPath(settings.OutputDirectory, filenameSuffix);
						if (File.Exists(text2))
						{
							list2.Add(text2);
						}
					}
				}
			}
			List<string> precompiledReferences = EditorBuildRules.GetPrecompiledReferences(scriptAssembly, settings.CompilationOptions, targetAssembly.editorCompatibility, assemblies.PrecompiledAssemblies);
			list2.AddRange(precompiledReferences);
			if (buildingForEditor && assemblies.EditorAssemblyReferences != null)
			{
				list2.AddRange(assemblies.EditorAssemblyReferences);
			}
			scriptAssembly.ScriptAssemblyReferences = list.ToArray();
			scriptAssembly.References = list2.ToArray();
		}

		public static List<string> GetUnityReferences(ScriptAssembly scriptAssembly, PrecompiledAssembly[] unityAssemblies, EditorScriptCompilationOptions options)
		{
			List<string> list = new List<string>();
			bool flag = (scriptAssembly.Flags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly;
			bool flag2 = (options & EditorScriptCompilationOptions.BuildingForEditor) == EditorScriptCompilationOptions.BuildingForEditor;
			if (unityAssemblies != null)
			{
				for (int i = 0; i < unityAssemblies.Length; i++)
				{
					PrecompiledAssembly compiledAssembly = unityAssemblies[i];
					bool flag3 = (compiledAssembly.Flags & AssemblyFlags.ExcludedForRuntimeCode) == AssemblyFlags.ExcludedForRuntimeCode;
					if ((flag2 && !flag3) || flag)
					{
						if ((compiledAssembly.Flags & AssemblyFlags.UseForMono) != AssemblyFlags.None)
						{
							list.Add(compiledAssembly.Path);
						}
					}
					else if ((compiledAssembly.Flags & AssemblyFlags.EditorOnly) != AssemblyFlags.EditorOnly && !flag3)
					{
						if (EditorBuildRules.IsPrecompiledAssemblyCompatibleWithScriptAssembly(compiledAssembly, scriptAssembly))
						{
							list.Add(compiledAssembly.Path);
						}
					}
				}
			}
			return list;
		}

		public static List<string> GetPrecompiledReferences(ScriptAssembly scriptAssembly, EditorScriptCompilationOptions options, EditorBuildRules.EditorCompatibility editorCompatibility, PrecompiledAssembly[] precompiledAssemblies)
		{
			List<string> list = new List<string>();
			bool flag = (options & EditorScriptCompilationOptions.BuildingForEditor) == EditorScriptCompilationOptions.BuildingForEditor;
			bool flag2 = (scriptAssembly.Flags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly;
			if (precompiledAssemblies != null)
			{
				for (int i = 0; i < precompiledAssemblies.Length; i++)
				{
					PrecompiledAssembly compiledAssembly = precompiledAssemblies[i];
					bool flag3 = (compiledAssembly.Flags & AssemblyFlags.EditorOnly) == AssemblyFlags.EditorOnly;
					if (!flag3 || flag2 || (flag && editorCompatibility == EditorBuildRules.EditorCompatibility.CompatibleWithEditor))
					{
						if (EditorBuildRules.IsPrecompiledAssemblyCompatibleWithScriptAssembly(compiledAssembly, scriptAssembly))
						{
							list.Add(compiledAssembly.Path);
						}
					}
				}
			}
			return list;
		}

		public static List<string> GetCompiledCustomAssembliesReferences(ScriptAssembly scriptAssembly, EditorBuildRules.TargetAssembly[] customTargetAssemblies, string outputDirectory, string filenameSuffix)
		{
			List<string> list = new List<string>();
			if (customTargetAssemblies != null)
			{
				for (int i = 0; i < customTargetAssemblies.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly = customTargetAssemblies[i];
					string text = targetAssembly.FullPath(outputDirectory, filenameSuffix);
					if (File.Exists(text))
					{
						list.Add(text);
					}
				}
			}
			return list;
		}

		private static bool IsCompatibleWithEditor(BuildTarget buildTarget, EditorScriptCompilationOptions options)
		{
			return (options & EditorScriptCompilationOptions.BuildingForEditor) == EditorScriptCompilationOptions.BuildingForEditor;
		}

		private static bool IsCompatibleWithPlatform(EditorBuildRules.TargetAssembly assembly, ScriptAssemblySettings settings)
		{
			return assembly.IsCompatibleFunc == null || assembly.IsCompatibleFunc(settings.BuildTarget, settings.CompilationOptions);
		}

		internal static EditorBuildRules.TargetAssembly[] CreatePredefinedTargetAssemblies()
		{
			List<EditorBuildRules.TargetAssembly> list = new List<EditorBuildRules.TargetAssembly>();
			List<EditorBuildRules.TargetAssembly> list2 = new List<EditorBuildRules.TargetAssembly>();
			List<EditorBuildRules.TargetAssembly> list3 = new List<EditorBuildRules.TargetAssembly>();
			List<EditorBuildRules.TargetAssembly> list4 = new List<EditorBuildRules.TargetAssembly>();
			List<SupportedLanguage> supportedLanguages = ScriptCompilers.SupportedLanguages;
			List<EditorBuildRules.TargetAssembly> list5 = new List<EditorBuildRules.TargetAssembly>();
			foreach (SupportedLanguage current in supportedLanguages)
			{
				string languageName = current.GetLanguageName();
				string arg_7C_0 = "Assembly-" + languageName + "-firstpass.dll";
				SupportedLanguage arg_7C_1 = current;
				AssemblyFlags arg_7C_2 = AssemblyFlags.FirstPass;
				EditorBuildRules.TargetAssemblyType arg_7C_3 = EditorBuildRules.TargetAssemblyType.Predefined;
				if (EditorBuildRules.<>f__mg$cache0 == null)
				{
					EditorBuildRules.<>f__mg$cache0 = new Func<string, int>(EditorBuildRules.FilterAssemblyInFirstpassFolder);
				}
				EditorBuildRules.TargetAssembly item = new EditorBuildRules.TargetAssembly(arg_7C_0, arg_7C_1, arg_7C_2, arg_7C_3, EditorBuildRules.<>f__mg$cache0, null);
				EditorBuildRules.TargetAssembly item2 = new EditorBuildRules.TargetAssembly("Assembly-" + languageName + ".dll", current, AssemblyFlags.None, EditorBuildRules.TargetAssemblyType.Predefined);
				string arg_EF_0 = "Assembly-" + languageName + "-Editor-firstpass.dll";
				SupportedLanguage arg_EF_1 = current;
				AssemblyFlags arg_EF_2 = AssemblyFlags.EditorOnly | AssemblyFlags.FirstPass;
				EditorBuildRules.TargetAssemblyType arg_EF_3 = EditorBuildRules.TargetAssemblyType.Predefined;
				if (EditorBuildRules.<>f__mg$cache1 == null)
				{
					EditorBuildRules.<>f__mg$cache1 = new Func<string, int>(EditorBuildRules.FilterAssemblyInFirstpassEditorFolder);
				}
				Func<string, int> arg_EF_4 = EditorBuildRules.<>f__mg$cache1;
				if (EditorBuildRules.<>f__mg$cache2 == null)
				{
					EditorBuildRules.<>f__mg$cache2 = new Func<BuildTarget, EditorScriptCompilationOptions, bool>(EditorBuildRules.IsCompatibleWithEditor);
				}
				EditorBuildRules.TargetAssembly item3 = new EditorBuildRules.TargetAssembly(arg_EF_0, arg_EF_1, arg_EF_2, arg_EF_3, arg_EF_4, EditorBuildRules.<>f__mg$cache2);
				string arg_145_0 = "Assembly-" + languageName + "-Editor.dll";
				SupportedLanguage arg_145_1 = current;
				AssemblyFlags arg_145_2 = AssemblyFlags.EditorOnly;
				EditorBuildRules.TargetAssemblyType arg_145_3 = EditorBuildRules.TargetAssemblyType.Predefined;
				if (EditorBuildRules.<>f__mg$cache3 == null)
				{
					EditorBuildRules.<>f__mg$cache3 = new Func<string, int>(EditorBuildRules.FilterAssemblyInEditorFolder);
				}
				Func<string, int> arg_145_4 = EditorBuildRules.<>f__mg$cache3;
				if (EditorBuildRules.<>f__mg$cache4 == null)
				{
					EditorBuildRules.<>f__mg$cache4 = new Func<BuildTarget, EditorScriptCompilationOptions, bool>(EditorBuildRules.IsCompatibleWithEditor);
				}
				EditorBuildRules.TargetAssembly item4 = new EditorBuildRules.TargetAssembly(arg_145_0, arg_145_1, arg_145_2, arg_145_3, arg_145_4, EditorBuildRules.<>f__mg$cache4);
				list.Add(item);
				list2.Add(item2);
				list3.Add(item3);
				list4.Add(item4);
				list5.Add(item);
				list5.Add(item2);
				list5.Add(item3);
				list5.Add(item4);
			}
			foreach (EditorBuildRules.TargetAssembly current2 in list2)
			{
				current2.References.AddRange(list);
			}
			foreach (EditorBuildRules.TargetAssembly current3 in list3)
			{
				current3.References.AddRange(list);
			}
			foreach (EditorBuildRules.TargetAssembly current4 in list4)
			{
				current4.References.AddRange(list);
				current4.References.AddRange(list2);
				current4.References.AddRange(list3);
			}
			return list5.ToArray();
		}

		internal static EditorBuildRules.TargetAssembly GetTargetAssembly(string scriptPath, string projectDirectory, EditorBuildRules.TargetAssembly[] customTargetAssemblies)
		{
			EditorBuildRules.TargetAssembly customTargetAssembly = EditorBuildRules.GetCustomTargetAssembly(scriptPath, projectDirectory, customTargetAssemblies);
			EditorBuildRules.TargetAssembly result;
			if (customTargetAssembly != null)
			{
				result = customTargetAssembly;
			}
			else
			{
				result = EditorBuildRules.GetPredefinedTargetAssembly(scriptPath);
			}
			return result;
		}

		internal static EditorBuildRules.TargetAssembly GetPredefinedTargetAssembly(string scriptPath)
		{
			EditorBuildRules.TargetAssembly result = null;
			string a = AssetPath.GetExtension(scriptPath).Substring(1).ToLower();
			string arg = "/" + scriptPath.ToLower();
			int num = -1;
			EditorBuildRules.TargetAssembly[] array = EditorBuildRules.predefinedTargetAssemblies;
			for (int i = 0; i < array.Length; i++)
			{
				EditorBuildRules.TargetAssembly targetAssembly = array[i];
				if (!(a != targetAssembly.Language.GetExtensionICanCompile()))
				{
					Func<string, int> pathFilter = targetAssembly.PathFilter;
					int num2;
					if (pathFilter == null)
					{
						num2 = 0;
					}
					else
					{
						num2 = pathFilter(arg);
					}
					if (num2 > num)
					{
						result = targetAssembly;
						num = num2;
					}
				}
			}
			return result;
		}

		internal static EditorBuildRules.TargetAssembly GetCustomTargetAssembly(string scriptPath, string projectDirectory, EditorBuildRules.TargetAssembly[] customTargetAssemblies)
		{
			EditorBuildRules.TargetAssembly result;
			if (customTargetAssemblies == null)
			{
				result = null;
			}
			else
			{
				int num = -1;
				EditorBuildRules.TargetAssembly targetAssembly = null;
				bool flag = AssetPath.IsPathRooted(scriptPath);
				string arg = (!flag) ? AssetPath.Combine(projectDirectory, scriptPath).ToLower() : AssetPath.GetFullPath(scriptPath).ToLower();
				for (int i = 0; i < customTargetAssemblies.Length; i++)
				{
					EditorBuildRules.TargetAssembly targetAssembly2 = customTargetAssemblies[i];
					int num2 = targetAssembly2.PathFilter(arg);
					if (num2 > num)
					{
						targetAssembly = targetAssembly2;
						num = num2;
					}
				}
				result = targetAssembly;
			}
			return result;
		}

		private static int FilterAssemblyInFirstpassFolder(string pathName)
		{
			int num = EditorBuildRules.FilterAssemblyPathBeginsWith(pathName, "/assets/plugins/");
			int result;
			if (num >= 0)
			{
				result = num;
			}
			else
			{
				num = EditorBuildRules.FilterAssemblyPathBeginsWith(pathName, "/assets/standard assets/");
				if (num >= 0)
				{
					result = num;
				}
				else
				{
					num = EditorBuildRules.FilterAssemblyPathBeginsWith(pathName, "/assets/pro standard assets/");
					if (num >= 0)
					{
						result = num;
					}
					else
					{
						num = EditorBuildRules.FilterAssemblyPathBeginsWith(pathName, "/assets/iphone standard assets/");
						if (num >= 0)
						{
							result = num;
						}
						else
						{
							result = -1;
						}
					}
				}
			}
			return result;
		}

		private static int FilterAssemblyInFirstpassEditorFolder(string pathName)
		{
			int num = EditorBuildRules.FilterAssemblyInFirstpassFolder(pathName);
			int result;
			if (num == -1)
			{
				result = -1;
			}
			else
			{
				result = EditorBuildRules.FilterAssemblyInEditorFolder(pathName);
			}
			return result;
		}

		private static int FilterAssemblyInEditorFolder(string pathName)
		{
			int num = pathName.IndexOf("/editor/");
			int result;
			if (num == -1)
			{
				result = -1;
			}
			else
			{
				result = num + "/editor/".Length;
			}
			return result;
		}

		private static int FilterAssemblyPathBeginsWith(string pathName, string prefix)
		{
			return (!pathName.StartsWith(prefix)) ? -1 : prefix.Length;
		}
	}
}
