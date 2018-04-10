using System;
using UnityEngine;

namespace UnityEditor.Experimental.AssetImporters
{
	internal class ImportedObject
	{
		public bool mainAssetObject
		{
			get;
			set;
		}

		public UnityEngine.Object obj
		{
			get;
			set;
		}

		public string localIdentifier
		{
			get;
			set;
		}

		public Texture2D thumbnail
		{
			get;
			set;
		}
	}
}
