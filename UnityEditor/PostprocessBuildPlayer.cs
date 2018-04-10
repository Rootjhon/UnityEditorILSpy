using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.BuildReporting;
using UnityEditor.DeploymentTargets;
using UnityEditor.Modules;
using UnityEditor.Utils;
using UnityEngine;

namespace UnityEditor
{
	internal class PostprocessBuildPlayer
	{
		private class NoTargetsFoundException : Exception
		{
			public NoTargetsFoundException()
			{
			}

			public NoTargetsFoundException(string message) : base(message)
			{
			}
		}

		internal const string StreamingAssets = "Assets/StreamingAssets";

		public static string subDir32Bit
		{
			get
			{
				return "x86";
			}
		}

		public static string subDir64Bit
		{
			get
			{
				return "x86_64";
			}
		}

		internal static string GenerateBundleIdentifier(string companyName, string productName)
		{
			return "unity." + companyName + "." + productName;
		}

		internal static bool InstallPluginsByExtension(string pluginSourceFolder, string extension, string debugExtension, string destPluginFolder, bool copyDirectories)
		{
			bool flag = false;
			bool result;
			if (!Directory.Exists(pluginSourceFolder))
			{
				result = flag;
			}
			else
			{
				string[] fileSystemEntries = Directory.GetFileSystemEntries(pluginSourceFolder);
				string[] array = fileSystemEntries;
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					string fileName = Path.GetFileName(text);
					string extension2 = Path.GetExtension(text);
					bool flag2 = extension2.Equals(extension, StringComparison.OrdinalIgnoreCase) || fileName.Equals(extension, StringComparison.OrdinalIgnoreCase);
					bool flag3 = debugExtension != null && debugExtension.Length != 0 && (extension2.Equals(debugExtension, StringComparison.OrdinalIgnoreCase) || fileName.Equals(debugExtension, StringComparison.OrdinalIgnoreCase));
					if (flag2 || flag3)
					{
						if (!Directory.Exists(destPluginFolder))
						{
							Directory.CreateDirectory(destPluginFolder);
						}
						string text2 = Path.Combine(destPluginFolder, fileName);
						if (copyDirectories)
						{
							FileUtil.CopyDirectoryRecursive(text, text2);
						}
						else if (!Directory.Exists(text))
						{
							FileUtil.UnityFileCopy(text, text2);
						}
						flag = true;
					}
				}
				result = flag;
			}
			return result;
		}

		internal static void InstallStreamingAssets(string stagingAreaDataPath)
		{
			PostprocessBuildPlayer.InstallStreamingAssets(stagingAreaDataPath, null);
		}

		internal static void InstallStreamingAssets(string stagingAreaDataPath, BuildReport report)
		{
			if (Directory.Exists("Assets/StreamingAssets"))
			{
				string text = Path.Combine(stagingAreaDataPath, "StreamingAssets");
				FileUtil.CopyDirectoryRecursiveForPostprocess("Assets/StreamingAssets", text, true);
				if (report != null)
				{
					report.AddFilesRecursive(text, "Streaming Assets");
				}
			}
		}

		public static string PrepareForBuild(BuildOptions options, BuildTargetGroup targetGroup, BuildTarget target)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			string result;
			if (buildPostProcessor == null)
			{
				result = null;
			}
			else
			{
				result = buildPostProcessor.PrepareForBuild(options, target);
			}
			return result;
		}

		public static bool SupportsScriptsOnlyBuild(BuildTargetGroup targetGroup, BuildTarget target)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			return buildPostProcessor != null && buildPostProcessor.SupportsScriptsOnlyBuild();
		}

		public static string GetExtensionForBuildTarget(BuildTargetGroup targetGroup, BuildTarget target, BuildOptions options)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			string result;
			if (buildPostProcessor == null)
			{
				result = string.Empty;
			}
			else
			{
				result = buildPostProcessor.GetExtension(target, options);
			}
			return result;
		}

		public static bool SupportsInstallInBuildFolder(BuildTargetGroup targetGroup, BuildTarget target)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			bool result;
			if (buildPostProcessor != null)
			{
				result = buildPostProcessor.SupportsInstallInBuildFolder();
			}
			else
			{
				switch (target)
				{
				case BuildTarget.PSP2:
				case BuildTarget.PSM:
					goto IL_46;
				case BuildTarget.PS4:
					IL_31:
					if (target != BuildTarget.Android && target != BuildTarget.WSAPlayer)
					{
						result = false;
						return result;
					}
					goto IL_46;
				}
				goto IL_31;
				IL_46:
				result = true;
			}
			return result;
		}

		public static bool SupportsLz4Compression(BuildTargetGroup targetGroup, BuildTarget target)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			return buildPostProcessor != null && buildPostProcessor.SupportsLz4Compression();
		}

		public static void Launch(BuildTargetGroup targetGroup, BuildTarget buildTarget, string path, string productName, BuildOptions options, BuildReport buildReport)
		{
			try
			{
				if (buildReport == null)
				{
					throw new NotSupportedException();
				}
				ProgressHandler handler = new ProgressHandler("Deploying Player", delegate(string title, string message, float globalProgress)
				{
					if (EditorUtility.DisplayCancelableProgressBar(title, message, globalProgress))
					{
						throw new OperationAbortedException();
					}
				}, 0.1f, 1f);
				ProgressTaskManager taskManager = new ProgressTaskManager(handler);
				List<DeploymentTargetId> validTargetIds = null;
				taskManager.AddTask(delegate
				{
					taskManager.UpdateProgress("Finding valid devices for build");
					validTargetIds = DeploymentTargetManager.FindValidTargetsForLaunchBuild(targetGroup, buildReport);
					if (!validTargetIds.Any<DeploymentTargetId>())
					{
						throw new PostprocessBuildPlayer.NoTargetsFoundException("Could not find any valid targets for build");
					}
				});
				taskManager.AddTask(delegate
				{
					foreach (DeploymentTargetId current in validTargetIds)
					{
						bool flag = current == validTargetIds[validTargetIds.Count - 1];
						try
						{
							DeploymentTargetManager.LaunchBuildOnTarget(targetGroup, buildReport, current, taskManager.SpawnProgressHandlerFromCurrentTask());
							return;
						}
						catch (OperationFailedException ex2)
						{
							UnityEngine.Debug.LogException(ex2);
							if (flag)
							{
								throw ex2;
							}
						}
					}
					throw new PostprocessBuildPlayer.NoTargetsFoundException("Could not find any target that managed to launch build");
				});
				taskManager.Run();
			}
			catch (OperationFailedException ex)
			{
				UnityEngine.Debug.LogException(ex);
				EditorUtility.DisplayDialog(ex.title, ex.Message, "Ok");
			}
			catch (OperationAbortedException)
			{
				Console.WriteLine("Deployment aborted");
			}
			catch (PostprocessBuildPlayer.NoTargetsFoundException)
			{
				throw new UnityException(string.Format("Could not find any valid targets to launch on for {0}", buildTarget));
			}
			catch (NotSupportedException)
			{
				IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, buildTarget);
				if (buildPostProcessor == null)
				{
					throw new UnityException(string.Format("Launching {0} build target via mono is not supported", buildTarget));
				}
				BuildLaunchPlayerArgs args;
				args.target = buildTarget;
				args.playerPackage = BuildPipeline.GetPlaybackEngineDirectory(buildTarget, options);
				args.installPath = path;
				args.productName = productName;
				args.options = options;
				args.report = buildReport;
				buildPostProcessor.LaunchPlayer(args);
			}
		}

		public static void UpdateBootConfig(BuildTargetGroup targetGroup, BuildTarget target, BootConfigData config, BuildOptions options)
		{
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			if (buildPostProcessor != null)
			{
				buildPostProcessor.UpdateBootConfig(target, config, options);
			}
		}

		public static void Postprocess(BuildTargetGroup targetGroup, BuildTarget target, string installPath, string companyName, string productName, int width, int height, BuildOptions options, RuntimeClassRegistry usedClassRegistry, BuildReport report)
		{
			string stagingArea = "Temp/StagingArea";
			string stagingAreaData = "Temp/StagingArea/Data";
			string stagingAreaDataManaged = "Temp/StagingArea/Data/Managed";
			string playbackEngineDirectory = BuildPipeline.GetPlaybackEngineDirectory(target, options);
			bool flag = (options & BuildOptions.InstallInBuildFolder) != BuildOptions.None && PostprocessBuildPlayer.SupportsInstallInBuildFolder(targetGroup, target);
			if (installPath == string.Empty && !flag)
			{
				throw new Exception(installPath + " must not be an empty string");
			}
			IBuildPostprocessor buildPostProcessor = ModuleManager.GetBuildPostProcessor(targetGroup, target);
			if (buildPostProcessor != null)
			{
				BuildPostProcessArgs args;
				args.target = target;
				args.stagingAreaData = stagingAreaData;
				args.stagingArea = stagingArea;
				args.stagingAreaDataManaged = stagingAreaDataManaged;
				args.playerPackage = playbackEngineDirectory;
				args.installPath = installPath;
				args.companyName = companyName;
				args.productName = productName;
				args.productGUID = PlayerSettings.productGUID;
				args.options = options;
				args.usedClassRegistry = usedClassRegistry;
				args.report = report;
				buildPostProcessor.PostProcess(args);
				return;
			}
			throw new UnityException(string.Format("Build target '{0}' not supported", target));
		}

		internal static string ExecuteSystemProcess(string command, string args, string workingdir)
		{
			ProcessStartInfo si = new ProcessStartInfo
			{
				FileName = command,
				Arguments = args,
				WorkingDirectory = workingdir,
				CreateNoWindow = true
			};
			Program program = new Program(si);
			program.Start();
			while (!program.WaitForExit(100))
			{
			}
			string standardOutputAsString = program.GetStandardOutputAsString();
			program.Dispose();
			return standardOutputAsString;
		}
	}
}
