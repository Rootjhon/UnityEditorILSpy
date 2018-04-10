using System;

namespace UnityEditor.Scripting.ScriptCompilation
{
	internal static class WSAHelpers
	{
		public static bool IsCSharpAssembly(ScriptAssembly scriptAssembly)
		{
			return !scriptAssembly.Filename.ToLower().Contains("firstpass") && scriptAssembly.Language == ScriptCompilers.CSharpSupportedLanguage;
		}

		public static bool IsCSharpFirstPassAssembly(ScriptAssembly scriptAssembly)
		{
			return scriptAssembly.Filename.ToLower().Contains("firstpass") && scriptAssembly.Language == ScriptCompilers.CSharpSupportedLanguage;
		}

		public static bool UseDotNetCore(ScriptAssembly scriptAssembly)
		{
			PlayerSettings.WSACompilationOverrides compilationOverrides = PlayerSettings.WSA.compilationOverrides;
			bool flag = scriptAssembly.BuildTarget == BuildTarget.WSAPlayer && compilationOverrides != PlayerSettings.WSACompilationOverrides.None;
			return flag && (WSAHelpers.IsCSharpAssembly(scriptAssembly) || (compilationOverrides != PlayerSettings.WSACompilationOverrides.UseNetCorePartially && WSAHelpers.IsCSharpFirstPassAssembly(scriptAssembly)));
		}
	}
}
