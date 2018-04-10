using System;
using UnityEngine.Bindings;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[NativeType(CodegenOptions.Custom, "SInt32")]
	public enum FileType
	{
		NonAssetType,
		DeprecatedCachedAssetType,
		SerializedAssetType,
		MetaAssetType
	}
}
