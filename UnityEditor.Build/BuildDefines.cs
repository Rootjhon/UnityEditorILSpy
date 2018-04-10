using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Scripting;

namespace UnityEditor.Build
{
	[RequiredByNativeCode]
	internal class BuildDefines
	{
		public static event GetScriptCompilationDefinesDelegate getScriptCompilationDefinesDelegates
		{
			add
			{
				GetScriptCompilationDefinesDelegate getScriptCompilationDefinesDelegate = BuildDefines.getScriptCompilationDefinesDelegates;
				GetScriptCompilationDefinesDelegate getScriptCompilationDefinesDelegate2;
				do
				{
					getScriptCompilationDefinesDelegate2 = getScriptCompilationDefinesDelegate;
					getScriptCompilationDefinesDelegate = Interlocked.CompareExchange<GetScriptCompilationDefinesDelegate>(ref BuildDefines.getScriptCompilationDefinesDelegates, (GetScriptCompilationDefinesDelegate)Delegate.Combine(getScriptCompilationDefinesDelegate2, value), getScriptCompilationDefinesDelegate);
				}
				while (getScriptCompilationDefinesDelegate != getScriptCompilationDefinesDelegate2);
			}
			remove
			{
				GetScriptCompilationDefinesDelegate getScriptCompilationDefinesDelegate = BuildDefines.getScriptCompilationDefinesDelegates;
				GetScriptCompilationDefinesDelegate getScriptCompilationDefinesDelegate2;
				do
				{
					getScriptCompilationDefinesDelegate2 = getScriptCompilationDefinesDelegate;
					getScriptCompilationDefinesDelegate = Interlocked.CompareExchange<GetScriptCompilationDefinesDelegate>(ref BuildDefines.getScriptCompilationDefinesDelegates, (GetScriptCompilationDefinesDelegate)Delegate.Remove(getScriptCompilationDefinesDelegate2, value), getScriptCompilationDefinesDelegate);
				}
				while (getScriptCompilationDefinesDelegate != getScriptCompilationDefinesDelegate2);
			}
		}

		[RequiredByNativeCode]
		public static string[] GetScriptCompilationDefines(BuildTarget target, string[] defines)
		{
			HashSet<string> hashSet = new HashSet<string>(defines);
			if (BuildDefines.getScriptCompilationDefinesDelegates != null)
			{
				BuildDefines.getScriptCompilationDefinesDelegates(target, hashSet);
			}
			string[] array = new string[hashSet.Count];
			hashSet.CopyTo(array);
			return array;
		}
	}
}
