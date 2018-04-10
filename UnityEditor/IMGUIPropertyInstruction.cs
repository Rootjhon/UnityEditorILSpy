using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[RequiredByNativeCode]
	internal struct IMGUIPropertyInstruction
	{
		public string targetTypeName;

		public string path;

		public Rect rect;

		public StackFrame[] beginStacktrace;

		public StackFrame[] endStacktrace;
	}
}
