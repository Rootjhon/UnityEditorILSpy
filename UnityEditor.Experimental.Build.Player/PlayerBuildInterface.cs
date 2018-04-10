using System;
using System.Runtime.CompilerServices;

namespace UnityEditor.Experimental.Build.Player
{
	public class PlayerBuildInterface
	{
		private static ScriptCompilationResult CompilePlayerScriptsNative(ScriptCompilationSettings input, string outputFolder, bool editorScripts)
		{
			ScriptCompilationResult result;
			PlayerBuildInterface.CompilePlayerScriptsNative_Injected(ref input, outputFolder, editorScripts, out result);
			return result;
		}

		public static ScriptCompilationResult CompilePlayerScripts(ScriptCompilationSettings input, string outputFolder)
		{
			return PlayerBuildInterface.CompilePlayerScriptsInternal(input, outputFolder, false);
		}

		internal static ScriptCompilationResult CompilePlayerScriptsInternal(ScriptCompilationSettings input, string outputFolder, bool editorScripts)
		{
			input.m_ResultTypeDB = new TypeDB();
			ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScriptsNative(input, outputFolder, editorScripts);
			result.m_TypeDB = ((result.m_Assemblies.Length == 0) ? null : input.m_ResultTypeDB);
			return result;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void CompilePlayerScriptsNative_Injected(ref ScriptCompilationSettings input, string outputFolder, bool editorScripts, out ScriptCompilationResult ret);
	}
}
