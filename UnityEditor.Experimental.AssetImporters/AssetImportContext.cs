using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Experimental.AssetImporters
{
	public class AssetImportContext
	{
		private List<ImportedObject> m_ImportedObjects = new List<ImportedObject>();

		public string assetPath
		{
			get;
			internal set;
		}

		public BuildTarget selectedBuildTarget
		{
			get;
			internal set;
		}

		internal List<ImportedObject> importedObjects
		{
			get
			{
				return this.m_ImportedObjects;
			}
		}

		internal AssetImportContext()
		{
		}

		public void SetMainObject(UnityEngine.Object obj)
		{
			if (!(obj == null))
			{
				ImportedObject importedObject = this.m_ImportedObjects.FirstOrDefault((ImportedObject x) => x.mainAssetObject);
				if (importedObject != null)
				{
					if (importedObject.obj == obj)
					{
						return;
					}
					Debug.LogWarning(string.Format("An object was already set as the main object: \"{0}\" conflicting on \"{1}\"", this.assetPath, importedObject.localIdentifier));
					importedObject.mainAssetObject = false;
				}
				importedObject = this.m_ImportedObjects.FirstOrDefault((ImportedObject x) => x.obj == obj);
				if (importedObject == null)
				{
					throw new Exception("Before an object can be set as main, it must first be added using AddObjectToAsset.");
				}
				importedObject.mainAssetObject = true;
				this.m_ImportedObjects.Remove(importedObject);
				this.m_ImportedObjects.Insert(0, importedObject);
			}
		}

		public void AddObjectToAsset(string identifier, UnityEngine.Object obj)
		{
			this.AddObjectToAsset(identifier, obj, null);
		}

		public void AddObjectToAsset(string identifier, UnityEngine.Object obj, Texture2D thumbnail)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj", "Cannot add a null object : " + (identifier ?? "<null>"));
			}
			ImportedObject item = new ImportedObject
			{
				mainAssetObject = false,
				localIdentifier = identifier,
				obj = obj,
				thumbnail = thumbnail
			};
			this.m_ImportedObjects.Add(item);
		}
	}
}
