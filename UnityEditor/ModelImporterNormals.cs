using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ImportMesh.h")]
	public enum ModelImporterNormals
	{
		Import,
		Calculate,
		None
	}
}
