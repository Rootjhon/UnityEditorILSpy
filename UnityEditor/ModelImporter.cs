using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h")]
	public class ModelImporter : AssetImporter
	{
		public HumanDescription humanDescription
		{
			get
			{
				HumanDescription result;
				this.INTERNAL_get_humanDescription(out result);
				return result;
			}
			set
			{
				this.INTERNAL_set_humanDescription(ref value);
			}
		}

		[Obsolete("Use importMaterials, materialName and materialSearch instead")]
		public extern ModelImporterGenerateMaterials generateMaterials
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool importMaterials
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterMaterialName materialName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterMaterialSearch materialSearch
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterMaterialLocation materialLocation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		internal extern AssetImporter.SourceAssetIdentifier[] sourceMaterials
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public extern float globalScale
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool isUseFileUnitsSupported
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public extern bool importVisibility
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool useFileUnits
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool useFileScale
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[Obsolete("Use useFileScale instead")]
		public bool isFileScaleUsed
		{
			get
			{
				return this.useFileScale;
			}
		}

		public extern bool importBlendShapes
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool importCameras
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool importLights
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool addCollider
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float normalSmoothingAngle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[Obsolete("Please use tangentImportMode instead")]
		public bool splitTangentsAcrossSeams
		{
			get
			{
				return this.importTangents == ModelImporterTangents.CalculateLegacyWithSplitTangents;
			}
			set
			{
				if (this.importTangents == ModelImporterTangents.CalculateLegacyWithSplitTangents && !value)
				{
					this.importTangents = ModelImporterTangents.CalculateLegacy;
				}
				else if (this.importTangents == ModelImporterTangents.CalculateLegacy && value)
				{
					this.importTangents = ModelImporterTangents.CalculateLegacyWithSplitTangents;
				}
			}
		}

		public extern bool swapUVChannels
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool weldVertices
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool keepQuads
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterIndexFormat indexFormat
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool preserveHierarchy
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool generateSecondaryUV
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float secondaryUVAngleDistortion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float secondaryUVAreaDistortion
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float secondaryUVHardAngle
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float secondaryUVPackMargin
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterGenerateAnimations generateAnimations
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern TakeInfo[] importedTakeInfos
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public extern string[] transformPaths
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public string[] referencedClips
		{
			get
			{
				return ModelImporter.INTERNAL_GetReferencedClips(this);
			}
		}

		public extern bool isReadable
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool optimizeMesh
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[Obsolete("normalImportMode is deprecated. Use importNormals instead")]
		public ModelImporterTangentSpaceMode normalImportMode
		{
			get
			{
				return (ModelImporterTangentSpaceMode)this.importNormals;
			}
			set
			{
				this.importNormals = (ModelImporterNormals)value;
			}
		}

		[Obsolete("tangentImportMode is deprecated. Use importTangents instead")]
		public ModelImporterTangentSpaceMode tangentImportMode
		{
			get
			{
				return (ModelImporterTangentSpaceMode)this.importTangents;
			}
			set
			{
				this.importTangents = (ModelImporterTangents)value;
			}
		}

		public extern ModelImporterNormals importNormals
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterNormalCalculationMode normalCalculationMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterTangents importTangents
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool bakeIK
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool isBakeIKSupported
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[Obsolete("use resampleCurves instead.")]
		public extern bool resampleRotations
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool resampleCurves
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool isTangentImportSupported
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[Obsolete("Use animationCompression instead", true)]
		private bool reduceKeyframes
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public ModelImporterMeshCompression meshCompression
		{
			get
			{
				return (ModelImporterMeshCompression)this.internal_meshCompression;
			}
			set
			{
				this.internal_meshCompression = (int)value;
			}
		}

		private extern int internal_meshCompression
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool importAnimation
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool optimizeGameObjects
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public string[] extraExposedTransformPaths
		{
			get
			{
				return this.GetExtraExposedTransformPaths();
			}
			set
			{
				ModelImporter.INTERNAL_set_extraExposedTransformPaths(this, value);
			}
		}

		public string[] extraUserProperties
		{
			get
			{
				return this.GetExtraUserProperties();
			}
			set
			{
				ModelImporter.INTERNAL_set_extraUserProperties(this, value);
			}
		}

		public extern ModelImporterAnimationCompression animationCompression
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern bool importAnimatedCustomProperties
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float animationRotationError
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float animationPositionError
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern float animationScaleError
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern WrapMode animationWrapMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterAnimationType animationType
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern ModelImporterHumanoidOversampling humanoidOversampling
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern string motionNodeName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public Avatar sourceAvatar
		{
			get
			{
				return this.GetSourceAvatar();
			}
			set
			{
				Avatar value2 = value;
				if (value != null)
				{
					ModelImporter modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(value)) as ModelImporter;
					if (modelImporter != null)
					{
						this.humanDescription = modelImporter.humanDescription;
					}
					else
					{
						Debug.LogError("Avatar must be from a ModelImporter, otherwise use ModelImporter.humanDescription");
						value2 = null;
					}
				}
				ModelImporter.SetSourceAvatarInternal(this, value2);
			}
		}

		[Obsolete("splitAnimations has been deprecated please use clipAnimations instead.", true)]
		public bool splitAnimations
		{
			get
			{
				return this.clipAnimations.Length != 0;
			}
			set
			{
			}
		}

		public ModelImporterClipAnimation[] clipAnimations
		{
			get
			{
				return ModelImporter.GetClipAnimations(this);
			}
			set
			{
				ModelImporter.SetClipAnimations(this, value);
			}
		}

		public ModelImporterClipAnimation[] defaultClipAnimations
		{
			get
			{
				return ModelImporter.GetDefaultClipAnimations(this);
			}
		}

		internal extern bool isAssetOlderOr42
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void INTERNAL_get_humanDescription(out HumanDescription value);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void INTERNAL_set_humanDescription(ref HumanDescription value);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void UpdateSkeletonPose(SkeletonBone[] skeletonBones, SerializedProperty serializedProperty);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern string[] INTERNAL_GetReferencedClips(ModelImporter self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string[] GetExtraExposedTransformPaths();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_set_extraExposedTransformPaths([Writable] ModelImporter self, string[] value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string[] GetExtraUserProperties();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_set_extraUserProperties([Writable] ModelImporter self, string[] value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern Avatar GetSourceAvatar();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetSourceAvatarInternal(ModelImporter self, Avatar value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ModelImporterClipAnimation[] GetClipAnimations(ModelImporter self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void SetClipAnimations([Writable] ModelImporter self, ModelImporterClipAnimation[] value);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern ModelImporterClipAnimation[] GetDefaultClipAnimations(ModelImporter self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void UpdateTransformMask(AvatarMask mask, SerializedProperty serializedProperty);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern AnimationClip GetPreviewAnimationClipForTake(string takeName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal extern string CalculateBestFittingPreviewGameObject();

		public void CreateDefaultMaskForClip(ModelImporterClipAnimation clip)
		{
			if (this.defaultClipAnimations.Length > 0)
			{
				AvatarMask avatarMask = new AvatarMask();
				this.defaultClipAnimations[0].ConfigureMaskFromClip(ref avatarMask);
				clip.ConfigureClipFromMask(avatarMask);
				UnityEngine.Object.DestroyImmediate(avatarMask);
			}
			else
			{
				Debug.LogError("Cannot create default mask because the current importer doesn't have any animation information");
			}
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool ExtractTexturesInternal(string folderPath);

		public bool ExtractTextures(string folderPath)
		{
			if (string.IsNullOrEmpty(folderPath))
			{
				throw new ArgumentException("The path cannot be empty", folderPath);
			}
			return this.ExtractTexturesInternal(folderPath);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern bool SearchAndRemapMaterials(ModelImporterMaterialName nameOption, ModelImporterMaterialSearch searchOption);
	}
}
