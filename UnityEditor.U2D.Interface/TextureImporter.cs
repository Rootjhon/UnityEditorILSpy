using System;
using System.Collections.Generic;
using UnityEditor.Experimental.U2D;
using UnityEngine;

namespace UnityEditor.U2D.Interface
{
	internal class TextureImporter : ITextureImporter, ISpriteEditorDataProvider
	{
		protected AssetImporter m_AssetImporter;

		private List<SpriteDataMultipleMode> m_SpritesMultiple;

		private SpriteDataSingleMode m_SpriteSingle;

		private SerializedObject m_TextureImporterSO;

		public override string assetPath
		{
			get
			{
				return this.m_AssetImporter.assetPath;
			}
		}

		public override SpriteImportMode spriteImportMode
		{
			get
			{
				return ((UnityEditor.TextureImporter)this.m_AssetImporter).spriteImportMode;
			}
		}

		public override Vector4 spriteBorder
		{
			get
			{
				return ((UnityEditor.TextureImporter)this.m_AssetImporter).spriteBorder;
			}
		}

		public override Vector2 spritePivot
		{
			get
			{
				return ((UnityEditor.TextureImporter)this.m_AssetImporter).spritePivot;
			}
		}

		public int spriteDataCount
		{
			get
			{
				SpriteImportMode spriteImportMode = this.spriteImportMode;
				int result;
				if (spriteImportMode != SpriteImportMode.Multiple)
				{
					if (spriteImportMode != SpriteImportMode.Single && spriteImportMode != SpriteImportMode.Polygon)
					{
						result = 0;
					}
					else
					{
						result = 1;
					}
				}
				else
				{
					result = this.m_SpritesMultiple.Count;
				}
				return result;
			}
			set
			{
				if (this.spriteImportMode != SpriteImportMode.Multiple)
				{
					Debug.LogError("SetSpriteDataSize can only be called when in SpriteImportMode.Multiple");
				}
				else
				{
					while (this.m_SpritesMultiple.Count < value)
					{
						this.m_SpritesMultiple.Add(new SpriteDataMultipleMode());
					}
					if (this.m_SpritesMultiple.Count > value)
					{
						int num = this.m_SpritesMultiple.Count - value;
						this.m_SpritesMultiple.RemoveRange(this.m_SpritesMultiple.Count - num, num);
					}
				}
			}
		}

		public UnityEngine.Object targetObject
		{
			get
			{
				return this.m_AssetImporter;
			}
		}

		public TextureImporter(UnityEditor.TextureImporter textureImporter)
		{
			this.m_AssetImporter = textureImporter;
		}

		public override bool Equals(object other)
		{
			TextureImporter textureImporter = other as TextureImporter;
			bool result;
			if (object.ReferenceEquals(textureImporter, null))
			{
				result = (this.m_AssetImporter == null);
			}
			else
			{
				result = (this.m_AssetImporter == textureImporter.m_AssetImporter);
			}
			return result;
		}

		public override int GetHashCode()
		{
			return this.m_AssetImporter.GetHashCode();
		}

		public override void GetWidthAndHeight(ref int width, ref int height)
		{
			((UnityEditor.TextureImporter)this.m_AssetImporter).GetWidthAndHeight(ref width, ref height);
		}

		public void InitSpriteEditorDataProvider(SerializedObject so)
		{
			this.m_TextureImporterSO = so;
			SerializedProperty serializedProperty = this.m_TextureImporterSO.FindProperty("m_SpriteSheet.m_Sprites");
			this.m_SpritesMultiple = new List<SpriteDataMultipleMode>();
			this.m_SpriteSingle = new SpriteDataSingleMode();
			this.m_SpriteSingle.Load(this.m_TextureImporterSO);
			for (int i = 0; i < serializedProperty.arraySize; i++)
			{
				SpriteDataMultipleMode spriteDataMultipleMode = new SpriteDataMultipleMode();
				SerializedProperty arrayElementAtIndex = serializedProperty.GetArrayElementAtIndex(i);
				spriteDataMultipleMode.Load(arrayElementAtIndex);
				this.m_SpritesMultiple.Add(spriteDataMultipleMode);
			}
		}

		public SpriteDataBase GetSpriteData(int i)
		{
			SpriteImportMode spriteImportMode = this.spriteImportMode;
			SpriteDataBase result;
			if (spriteImportMode != SpriteImportMode.Multiple)
			{
				if (spriteImportMode == SpriteImportMode.Single || spriteImportMode == SpriteImportMode.Polygon)
				{
					result = this.m_SpriteSingle;
					return result;
				}
			}
			else if (this.m_SpritesMultiple.Count > i)
			{
				result = this.m_SpritesMultiple[i];
				return result;
			}
			result = null;
			return result;
		}

		public void Apply(SerializedObject so)
		{
			this.m_SpriteSingle.Apply(so);
			SerializedProperty serializedProperty = so.FindProperty("m_SpriteSheet.m_Sprites");
			for (int i = 0; i < this.m_SpritesMultiple.Count; i++)
			{
				if (serializedProperty.arraySize < this.m_SpritesMultiple.Count)
				{
					serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);
				}
				SerializedProperty arrayElementAtIndex = serializedProperty.GetArrayElementAtIndex(i);
				this.m_SpritesMultiple[i].Apply(arrayElementAtIndex);
			}
			while (this.m_SpritesMultiple.Count < serializedProperty.arraySize)
			{
				serializedProperty.DeleteArrayElementAtIndex(this.m_SpritesMultiple.Count);
			}
		}

		public void GetTextureActualWidthAndHeight(out int width, out int height)
		{
			width = (height = 0);
			((UnityEditor.TextureImporter)this.m_AssetImporter).GetWidthAndHeight(ref width, ref height);
		}
	}
}
