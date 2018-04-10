using System;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[NativeType(CodegenOptions = CodegenOptions.Custom, IntermediateScriptingStructName = "MonoClipAnimationInfoCurve"), UsedByNativeCode]
	public struct ClipAnimationInfoCurve
	{
		public string name;

		public AnimationCurve curve;
	}
}
