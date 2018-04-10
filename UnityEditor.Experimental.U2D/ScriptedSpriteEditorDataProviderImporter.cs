using System;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace UnityEditor.Experimental.U2D
{
	internal abstract class ScriptedSpriteEditorDataProviderImporter : ScriptedImporter, ISpriteEditorDataProvider
	{
		public abstract SpriteImportMode spriteImportMode
		{
			get;
		}

		public abstract int spriteDataCount
		{
			get;
			set;
		}

		public abstract UnityEngine.Object targetObject
		{
			get;
		}

		public abstract SpriteDataBase GetSpriteData(int i);

		public abstract void Apply(SerializedObject so);

		public abstract void GetTextureActualWidthAndHeight(out int width, out int height);

		public abstract void InitSpriteEditorDataProvider(SerializedObject so);
	}
}
