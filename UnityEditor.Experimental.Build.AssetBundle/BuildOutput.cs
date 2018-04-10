using System;
using System.Collections.ObjectModel;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[UsedByNativeCode]
	[Serializable]
	public struct BuildOutput
	{
		[UsedByNativeCode]
		[Serializable]
		public struct ObjectLocation
		{
			[NativeName("fileName")]
			internal string m_FileName;

			[NativeName("offset")]
			internal ulong m_Offset;

			[NativeName("size")]
			internal ulong m_Size;

			public string fileName
			{
				get
				{
					return this.m_FileName;
				}
			}

			public ulong offset
			{
				get
				{
					return this.m_Offset;
				}
			}

			public ulong size
			{
				get
				{
					return this.m_Size;
				}
			}
		}

		[UsedByNativeCode]
		[Serializable]
		public struct SerializedObject
		{
			[NativeName("serializedObject")]
			internal ObjectIdentifier m_SerializedObject;

			[NativeName("header")]
			internal BuildOutput.ObjectLocation m_Header;

			[NativeName("rawData")]
			internal BuildOutput.ObjectLocation m_RawData;

			public ObjectIdentifier serializedObject
			{
				get
				{
					return this.m_SerializedObject;
				}
			}

			public BuildOutput.ObjectLocation header
			{
				get
				{
					return this.m_Header;
				}
			}

			public BuildOutput.ObjectLocation rawData
			{
				get
				{
					return this.m_RawData;
				}
			}
		}

		[UsedByNativeCode]
		[Serializable]
		public struct Result
		{
			[NativeName("assetBundleName")]
			internal string m_AssetBundleName;

			[NativeName("assetBundleAssets")]
			internal GUID[] m_AssetBundleAssets;

			[NativeName("assetBundleObjects")]
			internal BuildOutput.SerializedObject[] m_AssetBundleObjects;

			[NativeName("resourceFiles")]
			internal ResourceFile[] m_ResourceFiles;

			[NativeName("includedTypes")]
			internal Type[] m_IncludedTypes;

			public string assetBundleName
			{
				get
				{
					return this.m_AssetBundleName;
				}
			}

			public ReadOnlyCollection<GUID> assetBundleAssets
			{
				get
				{
					return Array.AsReadOnly<GUID>(this.m_AssetBundleAssets);
				}
			}

			public ReadOnlyCollection<BuildOutput.SerializedObject> assetBundleObjects
			{
				get
				{
					return Array.AsReadOnly<BuildOutput.SerializedObject>(this.m_AssetBundleObjects);
				}
			}

			public ReadOnlyCollection<ResourceFile> resourceFiles
			{
				get
				{
					return Array.AsReadOnly<ResourceFile>(this.m_ResourceFiles);
				}
			}

			public ReadOnlyCollection<Type> includedTypes
			{
				get
				{
					return Array.AsReadOnly<Type>(this.m_IncludedTypes);
				}
			}
		}

		[NativeName("results")]
		internal BuildOutput.Result[] m_Results;

		public ReadOnlyCollection<BuildOutput.Result> results
		{
			get
			{
				return Array.AsReadOnly<BuildOutput.Result>(this.m_Results);
			}
		}
	}
}
