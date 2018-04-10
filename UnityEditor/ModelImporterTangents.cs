using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ImportMesh.h")]
	public enum ModelImporterTangents
	{
		Import,
		CalculateLegacy,
		CalculateLegacyWithSplitTangents = 4,
		CalculateMikk = 3,
		None = 2
	}
}
