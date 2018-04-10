using System;
using System.Collections.Generic;

namespace UnityEditor.Build
{
	internal delegate void GetScriptCompilationDefinesDelegate(BuildTarget target, HashSet<string> defines);
}
