using System;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public enum ModelImporterMaterialName
	{
		BasedOnTextureName,
		BasedOnMaterialName,
		BasedOnModelNameAndMaterialName,
		[Obsolete("You should use ModelImporterMaterialName.BasedOnTextureName instead, because it it less complicated and behaves in more consistent way.")]
		BasedOnTextureName_Or_ModelNameAndMaterialName
	}
}
