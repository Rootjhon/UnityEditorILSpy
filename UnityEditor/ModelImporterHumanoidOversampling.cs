using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterHumanoidOversampling
	{
		X1 = 1,
		X2,
		X4 = 4,
		X8 = 8
	}
}
