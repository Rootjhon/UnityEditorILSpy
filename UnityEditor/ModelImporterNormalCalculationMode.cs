using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ImportMesh.h")]
	public enum ModelImporterNormalCalculationMode
	{
		Unweighted_Legacy,
		Unweighted,
		AreaWeighted,
		AngleWeighted,
		AreaAndAngleWeighted
	}
}
