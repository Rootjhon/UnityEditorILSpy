using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterMaterialSearch
	{
		Local,
		RecursiveUp,
		Everywhere
	}
}
