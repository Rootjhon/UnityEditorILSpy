using System;
using System.IO;
using UnityEditor.Utils;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal static class AssetPath
	{
		public static readonly char Separator = '/';

		public static string GetFullPath(string path)
		{
			return AssetPath.ReplaceSeparators(Path.GetFullPath(path.NormalizePath()));
		}

		public static string Combine(string path1, string path2)
		{
			return AssetPath.ReplaceSeparators(Path.Combine(path1, path2));
		}

		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path.NormalizePath());
		}

		public static string GetFileName(string path)
		{
			return Path.GetFileName(path.NormalizePath());
		}

		public static string GetExtension(string path)
		{
			return Path.GetExtension(path.NormalizePath());
		}

		public static string GetDirectoryName(string path)
		{
			return AssetPath.ReplaceSeparators(Path.GetDirectoryName(path.NormalizePath()));
		}

		public static string ReplaceSeparators(string path)
		{
			return path.Replace('\\', AssetPath.Separator);
		}

		public static string GetAssemblyNameWithoutExtension(string assemblyName)
		{
			string result;
			if (AssetPath.GetExtension(assemblyName) == ".dll")
			{
				result = Path.GetFileNameWithoutExtension(assemblyName.NormalizePath());
			}
			else
			{
				result = Path.GetFileName(assemblyName.NormalizePath());
			}
			return result;
		}
	}
}
