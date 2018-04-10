using System;
using UnityEngine;

namespace UnityEditor.Experimental.U2D
{
	internal interface ISpriteEditorDataProvider
	{
		SpriteImportMode spriteImportMode
		{
			get;
		}

		int spriteDataCount
		{
			get;
			set;
		}

		UnityEngine.Object targetObject
		{
			get;
		}

		SpriteDataBase GetSpriteData(int i);

		void Apply(SerializedObject so);

		void GetTextureActualWidthAndHeight(out int width, out int height);

		void InitSpriteEditorDataProvider(SerializedObject so);
	}
}
