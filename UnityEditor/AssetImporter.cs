using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;

namespace UnityEditor
{
	public class AssetImporter : UnityEngine.Object
	{
		[NativeType(CodegenOptions.Custom, "MonoSourceAssetIdentifier")]
		public struct SourceAssetIdentifier
		{
			public Type type;

			public string name;

			public SourceAssetIdentifier(UnityEngine.Object asset)
			{
				if (asset == null)
				{
					throw new ArgumentNullException("asset");
				}
				this.type = asset.GetType();
				this.name = asset.name;
			}

			public SourceAssetIdentifier(Type type, string name)
			{
				if (type == null)
				{
					throw new ArgumentNullException("type");
				}
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				if (string.IsNullOrEmpty(name))
				{
					throw new ArgumentException("The name is empty", "name");
				}
				this.type = type;
				this.name = name;
			}
		}

		public extern string assetPath
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public extern ulong assetTimeStamp
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public extern string userData
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern string assetBundleName
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public extern string assetBundleVariant
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void SetAssetBundleNameAndVariant(string assetBundleName, string assetBundleVariant);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern AssetImporter GetAtPath(string path);

		public void SaveAndReimport()
		{
			AssetDatabase.ImportAsset(this.assetPath);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern int LocalFileIDToClassID(long fileId);

		public void AddRemap(AssetImporter.SourceAssetIdentifier identifier, UnityEngine.Object externalObject)
		{
			this.AddRemap_Injected(ref identifier, externalObject);
		}

		public bool RemoveRemap(AssetImporter.SourceAssetIdentifier identifier)
		{
			return this.RemoveRemap_Injected(ref identifier);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern AssetImporter.SourceAssetIdentifier[] GetIdentifiers(AssetImporter self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern UnityEngine.Object[] GetExternalObjects(AssetImporter self);

		public Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> GetExternalObjectMap()
		{
			AssetImporter.SourceAssetIdentifier[] identifiers = AssetImporter.GetIdentifiers(this);
			UnityEngine.Object[] externalObjects = AssetImporter.GetExternalObjects(this);
			Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object> dictionary = new Dictionary<AssetImporter.SourceAssetIdentifier, UnityEngine.Object>();
			for (int i = 0; i < identifiers.Length; i++)
			{
				dictionary.Add(identifiers[i], externalObjects[i]);
			}
			return dictionary;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void RegisterImporter(Type importer, int importerVersion, int queuePos, string fileExt);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void AddRemap_Injected(ref AssetImporter.SourceAssetIdentifier identifier, UnityEngine.Object externalObject);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern bool RemoveRemap_Injected(ref AssetImporter.SourceAssetIdentifier identifier);
	}
}
