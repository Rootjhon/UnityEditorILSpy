using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[UsedByNativeCode]
	[Serializable]
	public struct BuildCommandSet
	{
		[UsedByNativeCode]
		[Serializable]
		public struct AssetLoadInfo
		{
			[NativeName("asset")]
			internal GUID m_Asset;

			[NativeName("address")]
			internal string m_Address;

			[NativeName("processedScene")]
			internal string m_ProcessedScene;

			[NativeName("includedObjects")]
			internal ObjectIdentifier[] m_IncludedObjects;

			[NativeName("referencedObjects")]
			internal ObjectIdentifier[] m_ReferencedObjects;

			public GUID asset
			{
				get
				{
					return this.m_Asset;
				}
				set
				{
					this.m_Asset = value;
				}
			}

			public string address
			{
				get
				{
					return this.m_Address;
				}
				set
				{
					this.m_Address = value;
				}
			}

			public string processedScene
			{
				get
				{
					return this.m_ProcessedScene;
				}
				set
				{
					this.m_ProcessedScene = value;
				}
			}

			public ObjectIdentifier[] includedObjects
			{
				get
				{
					return this.m_IncludedObjects;
				}
				set
				{
					this.m_IncludedObjects = value;
				}
			}

			public ObjectIdentifier[] referencedObjects
			{
				get
				{
					return this.m_ReferencedObjects;
				}
				set
				{
					this.m_ReferencedObjects = value;
				}
			}
		}

		[UsedByNativeCode]
		[Serializable]
		public struct SerializationInfo
		{
			[NativeName("serializationObject")]
			internal ObjectIdentifier m_SerializationObject;

			[NativeName("serializationIndex")]
			internal long m_SerializationIndex;

			public ObjectIdentifier serializationObject
			{
				get
				{
					return this.m_SerializationObject;
				}
				set
				{
					this.m_SerializationObject = value;
				}
			}

			public long serializationIndex
			{
				get
				{
					return this.m_SerializationIndex;
				}
				set
				{
					this.m_SerializationIndex = value;
				}
			}
		}

		[UsedByNativeCode]
		[Serializable]
		public struct Command
		{
			[NativeName("assetBundleName")]
			internal string m_AssetBundleName;

			[NativeName("explicitAssets")]
			internal BuildCommandSet.AssetLoadInfo[] m_ExplicitAssets;

			[NativeName("assetBundleObjects")]
			internal BuildCommandSet.SerializationInfo[] m_AssetBundleObjects;

			[NativeName("assetBundleDependencies")]
			internal string[] m_AssetBundleDependencies;

			[NativeName("sceneBundle")]
			internal bool m_SceneBundle;

			[NativeName("globalUsage")]
			internal BuildUsageTagGlobal m_GlobalUsage;

			public string assetBundleName
			{
				get
				{
					return this.m_AssetBundleName;
				}
				set
				{
					this.m_AssetBundleName = value;
				}
			}

			public BuildCommandSet.AssetLoadInfo[] explicitAssets
			{
				get
				{
					return this.m_ExplicitAssets;
				}
				set
				{
					this.m_ExplicitAssets = value;
				}
			}

			public BuildCommandSet.SerializationInfo[] assetBundleObjects
			{
				get
				{
					return this.m_AssetBundleObjects;
				}
				set
				{
					this.m_AssetBundleObjects = value;
				}
			}

			public string[] assetBundleDependencies
			{
				get
				{
					return this.m_AssetBundleDependencies;
				}
				set
				{
					this.m_AssetBundleDependencies = value;
				}
			}

			public bool sceneBundle
			{
				get
				{
					return this.m_SceneBundle;
				}
				set
				{
					this.m_SceneBundle = value;
				}
			}

			public BuildUsageTagGlobal globalUsage
			{
				get
				{
					return this.m_GlobalUsage;
				}
				set
				{
					this.m_GlobalUsage = value;
				}
			}
		}

		[NativeName("commands")]
		internal BuildCommandSet.Command[] m_Commands;

		public BuildCommandSet.Command[] commands
		{
			get
			{
				return this.m_Commands;
			}
			set
			{
				this.m_Commands = value;
			}
		}
	}
}
