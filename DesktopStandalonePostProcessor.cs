using System;
using System.IO;
using UnityEditor;
using UnityEditor.Modules;
using UnityEditorInternal;
using UnityEngine;

internal abstract class DesktopStandalonePostProcessor : DefaultBuildPostprocessor
{
	internal class ScriptingImplementations : IScriptingImplementations
	{
		public ScriptingImplementation[] Supported()
		{
			return new ScriptingImplementation[]
			{
				ScriptingImplementation.Mono2x,
				ScriptingImplementation.IL2CPP
			};
		}

		public ScriptingImplementation[] Enabled()
		{
			ScriptingImplementation[] result;
			if (Unsupported.IsDeveloperBuild())
			{
				result = new ScriptingImplementation[]
				{
					ScriptingImplementation.Mono2x,
					ScriptingImplementation.IL2CPP
				};
			}
			else
			{
				result = new ScriptingImplementation[1];
			}
			return result;
		}
	}

	protected BuildPostProcessArgs m_PostProcessArgs;

	protected abstract string StagingAreaPluginsFolder
	{
		get;
	}

	protected abstract string DestinationFolderForInstallingIntoBuildsFolder
	{
		get;
	}

	protected bool InstallingIntoBuildsFolder
	{
		get
		{
			return (this.m_PostProcessArgs.options & BuildOptions.InstallInBuildFolder) != BuildOptions.None;
		}
	}

	protected bool UseIl2Cpp
	{
		get
		{
			return PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone) == ScriptingImplementation.IL2CPP;
		}
	}

	protected string StagingArea
	{
		get
		{
			return this.m_PostProcessArgs.stagingArea;
		}
	}

	protected string InstallPath
	{
		get
		{
			return this.m_PostProcessArgs.installPath;
		}
	}

	protected string DataFolder
	{
		get
		{
			return this.StagingArea + "/Data";
		}
	}

	protected BuildTarget Target
	{
		get
		{
			return this.m_PostProcessArgs.target;
		}
	}

	protected virtual string DestinationFolder
	{
		get
		{
			return FileUtil.UnityGetDirectoryName(this.m_PostProcessArgs.installPath);
		}
	}

	protected bool Development
	{
		get
		{
			return (this.m_PostProcessArgs.options & BuildOptions.Development) != BuildOptions.None;
		}
	}

	protected DesktopStandalonePostProcessor()
	{
	}

	protected DesktopStandalonePostProcessor(BuildPostProcessArgs postProcessArgs)
	{
		this.m_PostProcessArgs = postProcessArgs;
	}

	public void PostProcess()
	{
		this.SetupStagingArea();
		this.CopyStagingAreaIntoDestination();
	}

	public override void UpdateBootConfig(BuildTarget target, BootConfigData config, BuildOptions options)
	{
		base.UpdateBootConfig(target, config, options);
		if (PlayerSettings.forceSingleInstance)
		{
			config.AddKey("single-instance");
		}
		if (EditorApplication.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest)
		{
			config.Set("scripting-runtime-version", "latest");
		}
		if (IL2CPPUtils.UseIl2CppCodegenWithMonoBackend(BuildPipeline.GetBuildTargetGroup(target)))
		{
			config.Set("mono-codegen", "il2cpp");
		}
	}

	private void CopyNativePlugins()
	{
		string buildTargetName = BuildPipeline.GetBuildTargetName(this.m_PostProcessArgs.target);
		IPluginImporterExtension pluginImporterExtension = new DesktopPluginImporterExtension();
		string stagingAreaPluginsFolder = this.StagingAreaPluginsFolder;
		string path = Path.Combine(stagingAreaPluginsFolder, "x86");
		string path2 = Path.Combine(stagingAreaPluginsFolder, "x86_64");
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		PluginImporter[] importers = PluginImporter.GetImporters(this.m_PostProcessArgs.target);
		for (int i = 0; i < importers.Length; i++)
		{
			PluginImporter pluginImporter = importers[i];
			BuildTarget target = this.m_PostProcessArgs.target;
			if (pluginImporter.isNativePlugin)
			{
				if (string.IsNullOrEmpty(pluginImporter.assetPath))
				{
					Debug.LogWarning("Got empty plugin importer path for " + this.m_PostProcessArgs.target.ToString());
				}
				else
				{
					if (!flag)
					{
						Directory.CreateDirectory(stagingAreaPluginsFolder);
						flag = true;
					}
					bool flag4 = Directory.Exists(pluginImporter.assetPath);
					string platformData = pluginImporter.GetPlatformData(target, "CPU");
					if (platformData != null)
					{
						if (!(platformData == "x86"))
						{
							if (!(platformData == "x86_64"))
							{
								if (platformData == "None")
								{
									goto IL_1F8;
								}
							}
							else
							{
								if (target != BuildTarget.StandaloneOSX && target != BuildTarget.StandaloneWindows64 && target != BuildTarget.StandaloneLinux64 && target != BuildTarget.StandaloneLinuxUniversal)
								{
									goto IL_1F8;
								}
								if (!flag3)
								{
									Directory.CreateDirectory(path2);
									flag3 = true;
								}
							}
						}
						else
						{
							if (target == BuildTarget.StandaloneWindows64 || target == BuildTarget.StandaloneLinux64)
							{
								goto IL_1F8;
							}
							if (!flag2)
							{
								Directory.CreateDirectory(path);
								flag2 = true;
							}
						}
					}
					string text = pluginImporterExtension.CalculateFinalPluginPath(buildTargetName, pluginImporter);
					if (!string.IsNullOrEmpty(text))
					{
						string text2 = Path.Combine(stagingAreaPluginsFolder, text);
						if (flag4)
						{
							FileUtil.CopyDirectoryRecursive(pluginImporter.assetPath, text2);
						}
						else
						{
							FileUtil.UnityFileCopy(pluginImporter.assetPath, text2);
						}
					}
				}
			}
			IL_1F8:;
		}
		foreach (PluginDesc current in PluginImporter.GetExtensionPlugins(this.m_PostProcessArgs.target))
		{
			if (!flag)
			{
				Directory.CreateDirectory(stagingAreaPluginsFolder);
				flag = true;
			}
			string text3 = Path.Combine(stagingAreaPluginsFolder, Path.GetFileName(current.pluginPath));
			if (!Directory.Exists(text3) && !File.Exists(text3))
			{
				if (Directory.Exists(current.pluginPath))
				{
					FileUtil.CopyDirectoryRecursive(current.pluginPath, text3);
				}
				else
				{
					FileUtil.CopyFileIfExists(current.pluginPath, text3, false);
				}
			}
		}
	}

	protected virtual void SetupStagingArea()
	{
		Directory.CreateDirectory(this.DataFolder);
		this.CopyNativePlugins();
		if (this.m_PostProcessArgs.target == BuildTarget.StandaloneWindows || this.m_PostProcessArgs.target == BuildTarget.StandaloneWindows64)
		{
			this.CreateApplicationData();
		}
		PostprocessBuildPlayer.InstallStreamingAssets(this.DataFolder);
		if (this.UseIl2Cpp)
		{
			this.CopyVariationFolderIntoStagingArea();
			string text = this.StagingArea + "/Data";
			string text2 = this.DataFolder + "/Managed";
			string text3 = text2 + "/Resources";
			string text4 = text2 + "/Metadata";
			IL2CPPUtils.RunIl2Cpp(text, this.GetPlatformProvider(this.m_PostProcessArgs.target), delegate(string s)
			{
			}, this.m_PostProcessArgs.usedClassRegistry, false);
			FileUtil.CreateOrCleanDirectory(text3);
			IL2CPPUtils.CopyEmbeddedResourceFiles(text, text3);
			FileUtil.CreateOrCleanDirectory(text4);
			IL2CPPUtils.CopyMetadataFiles(text, text4);
			IL2CPPUtils.CopySymmapFile(text + "/Native/Data", text2);
			if (IL2CPPUtils.UseIl2CppCodegenWithMonoBackend(BuildPipeline.GetBuildTargetGroup(this.m_PostProcessArgs.target)))
			{
				DesktopStandalonePostProcessor.StripAssembliesToLeaveOnlyMetadata(this.m_PostProcessArgs.target, text2);
			}
		}
		if (this.InstallingIntoBuildsFolder)
		{
			this.CopyDataForBuildsFolder();
		}
		else
		{
			if (!this.UseIl2Cpp)
			{
				this.CopyVariationFolderIntoStagingArea();
			}
			this.RenameFilesInStagingArea();
			this.m_PostProcessArgs.report.AddFilesRecursive(this.StagingArea, "");
			this.m_PostProcessArgs.report.RelocateFiles(this.StagingArea, "");
		}
	}

	private static void StripAssembliesToLeaveOnlyMetadata(BuildTarget target, string stagingAreaDataManaged)
	{
		AssemblyReferenceChecker assemblyReferenceChecker = new AssemblyReferenceChecker();
		assemblyReferenceChecker.CollectReferences(stagingAreaDataManaged, true, 0f, false);
		EditorUtility.DisplayProgressBar("Removing bytecode from assemblies", "Stripping assemblies so that only metadata remains", 0.95f);
		MonoAssemblyStripping.MonoCilStrip(target, stagingAreaDataManaged, assemblyReferenceChecker.GetAssemblyFileNames());
	}

	protected void CreateApplicationData()
	{
		File.WriteAllText(Path.Combine(this.DataFolder, "app.info"), string.Join("\n", new string[]
		{
			this.m_PostProcessArgs.companyName,
			this.m_PostProcessArgs.productName
		}));
	}

	protected virtual bool CopyFilter(string path)
	{
		bool flag = true;
		if (Path.GetExtension(path) == ".mdb" && Path.GetFileName(path).StartsWith("UnityEngine."))
		{
			flag = false;
		}
		return flag & !path.Contains("UnityEngine.xml");
	}

	protected virtual void CopyVariationFolderIntoStagingArea()
	{
		string source = this.m_PostProcessArgs.playerPackage + "/Variations/" + this.GetVariationName();
		FileUtil.CopyDirectoryFiltered(source, this.StagingArea, true, (string f) => this.CopyFilter(f), true);
	}

	protected void CopyStagingAreaIntoDestination()
	{
		if (this.InstallingIntoBuildsFolder)
		{
			string text = Unsupported.GetBaseUnityDeveloperFolder() + "/" + this.DestinationFolderForInstallingIntoBuildsFolder;
			if (!Directory.Exists(Path.GetDirectoryName(text)))
			{
				throw new Exception("Installing in builds folder failed because the player has not been built (You most likely want to enable 'Development build').");
			}
			FileUtil.CopyDirectoryFiltered(this.DataFolder, text, true, (string f) => true, true);
		}
		else
		{
			this.DeleteDestination();
			FileUtil.CopyDirectoryFiltered(this.StagingArea, this.DestinationFolder, true, (string f) => true, true);
		}
	}

	protected abstract void DeleteDestination();

	protected void DeleteUnusedMono(string dataFolder)
	{
		bool flag = IL2CPPUtils.UseIl2CppCodegenWithMonoBackend(BuildTargetGroup.Standalone);
		if (flag || EditorApplication.scriptingRuntimeVersion == ScriptingRuntimeVersion.Latest)
		{
			FileUtil.DeleteFileOrDirectory(Path.Combine(dataFolder, "Mono"));
		}
		if (flag || EditorApplication.scriptingRuntimeVersion == ScriptingRuntimeVersion.Legacy)
		{
			FileUtil.DeleteFileOrDirectory(Path.Combine(dataFolder, "MonoBleedingEdge"));
		}
	}

	protected abstract void CopyDataForBuildsFolder();

	protected virtual string GetVariationName()
	{
		return string.Format("{0}_{1}", this.PlatformStringFor(this.m_PostProcessArgs.target), (!this.Development) ? "nondevelopment" : "development");
	}

	protected abstract string PlatformStringFor(BuildTarget target);

	protected abstract void RenameFilesInStagingArea();

	protected abstract IIl2CppPlatformProvider GetPlatformProvider(BuildTarget target);
}
