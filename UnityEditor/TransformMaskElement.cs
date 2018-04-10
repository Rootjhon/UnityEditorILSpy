using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[NativeType(CodegenOptions = CodegenOptions.Custom, Header = "Runtime/Animation/AvatarMask.h", IntermediateScriptingStructName = "MonoTransformMaskElement"), UsedByNativeCode]
	internal struct TransformMaskElement
	{
		public string path;

		public float weight;
	}
}
