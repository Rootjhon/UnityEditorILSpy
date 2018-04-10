using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterAnimationCompression
	{
		Off,
		KeyframeReduction,
		KeyframeReductionAndCompression,
		Optimal
	}
}
