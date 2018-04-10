using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterAnimationType
	{
		None,
		Legacy,
		Generic,
		Human
	}
}
