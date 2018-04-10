using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterGenerateAnimations
	{
		None,
		GenerateAnimations = 4,
		InRoot = 3,
		InOriginalRoots = 1,
		InNodes
	}
}
