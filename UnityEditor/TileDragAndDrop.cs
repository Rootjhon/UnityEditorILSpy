using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	internal static class TileDragAndDrop
	{
		private enum UserTileCreationMode
		{
			Overwrite,
			CreateUnique,
			Reuse
		}

		private static readonly string k_TileExtension = "asset";

		public static List<Sprite> GetSpritesFromTexture(Texture2D texture)
		{
			string assetPath = AssetDatabase.GetAssetPath(texture);
			UnityEngine.Object[] array = AssetDatabase.LoadAllAssetsAtPath(assetPath);
			List<Sprite> list = new List<Sprite>();
			UnityEngine.Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				UnityEngine.Object @object = array2[i];
				if (@object is Sprite)
				{
					list.Add(@object as Sprite);
				}
			}
			return list;
		}

		public static bool AllSpritesAreSameSize(List<Sprite> sprites)
		{
			bool result;
			if (!sprites.Any<Sprite>())
			{
				result = false;
			}
			else
			{
				for (int i = 1; i < sprites.Count - 1; i++)
				{
					if ((int)sprites[i].rect.width != (int)sprites[i + 1].rect.width || (int)sprites[i].rect.height != (int)sprites[i + 1].rect.height)
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}

		public static Dictionary<Vector2Int, UnityEngine.Object> CreateHoverData(List<Texture2D> sheetTextures, List<Sprite> singleSprites, List<TileBase> tiles)
		{
			Dictionary<Vector2Int, UnityEngine.Object> dictionary = new Dictionary<Vector2Int, UnityEngine.Object>();
			Vector2Int vector2Int = new Vector2Int(0, 0);
			int num = 0;
			if (sheetTextures != null)
			{
				foreach (Texture2D current in sheetTextures)
				{
					Dictionary<Vector2Int, UnityEngine.Object> dictionary2 = TileDragAndDrop.CreateHoverData(current);
					foreach (KeyValuePair<Vector2Int, UnityEngine.Object> current2 in dictionary2)
					{
						dictionary.Add(current2.Key + vector2Int, current2.Value);
					}
					Vector2Int min = TileDragAndDrop.GetMinMaxRect(dictionary2.Keys.ToList<Vector2Int>()).min;
					vector2Int += new Vector2Int(0, min.y - 1);
				}
			}
			if (vector2Int.x > 0)
			{
				vector2Int = new Vector2Int(0, vector2Int.y - 1);
			}
			if (singleSprites != null)
			{
				num = Mathf.FloorToInt(Mathf.Sqrt((float)singleSprites.Count));
				foreach (Sprite current3 in singleSprites)
				{
					dictionary.Add(vector2Int, current3);
					vector2Int += new Vector2Int(1, 0);
					if (vector2Int.x > num)
					{
						vector2Int = new Vector2Int(0, vector2Int.y - 1);
					}
				}
			}
			if (vector2Int.x > 0)
			{
				vector2Int = new Vector2Int(0, vector2Int.y - 1);
			}
			if (tiles != null)
			{
				num = Math.Max(Mathf.FloorToInt(Mathf.Sqrt((float)tiles.Count)), num);
				foreach (TileBase current4 in tiles)
				{
					dictionary.Add(vector2Int, current4);
					vector2Int += new Vector2Int(1, 0);
					if (vector2Int.x > num)
					{
						vector2Int = new Vector2Int(0, vector2Int.y - 1);
					}
				}
			}
			return dictionary;
		}

		public static List<Texture2D> GetValidSpritesheets(UnityEngine.Object[] objects)
		{
			List<Texture2D> list = new List<Texture2D>();
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				if (@object is Texture2D)
				{
					Texture2D texture2D = @object as Texture2D;
					List<Sprite> spritesFromTexture = TileDragAndDrop.GetSpritesFromTexture(texture2D);
					if (spritesFromTexture.Count<Sprite>() > 1 && TileDragAndDrop.AllSpritesAreSameSize(spritesFromTexture))
					{
						list.Add(texture2D);
					}
				}
			}
			return list;
		}

		public static List<Sprite> GetValidSingleSprites(UnityEngine.Object[] objects)
		{
			List<Sprite> list = new List<Sprite>();
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				if (@object is Sprite)
				{
					list.Add(@object as Sprite);
				}
				else if (@object is Texture2D)
				{
					Texture2D texture = @object as Texture2D;
					List<Sprite> spritesFromTexture = TileDragAndDrop.GetSpritesFromTexture(texture);
					if (spritesFromTexture.Count == 1 || !TileDragAndDrop.AllSpritesAreSameSize(spritesFromTexture))
					{
						list.AddRange(spritesFromTexture);
					}
				}
			}
			return list;
		}

		public static List<TileBase> GetValidTiles(UnityEngine.Object[] objects)
		{
			List<TileBase> list = new List<TileBase>();
			for (int i = 0; i < objects.Length; i++)
			{
				UnityEngine.Object @object = objects[i];
				if (@object is TileBase)
				{
					list.Add(@object as TileBase);
				}
			}
			return list;
		}

		public static Dictionary<Vector2Int, UnityEngine.Object> CreateHoverData(Texture2D sheet)
		{
			Dictionary<Vector2Int, UnityEngine.Object> dictionary = new Dictionary<Vector2Int, UnityEngine.Object>();
			List<Sprite> spritesFromTexture = TileDragAndDrop.GetSpritesFromTexture(sheet);
			Vector2Int cellPixelSize = TileDragAndDrop.EstimateGridPixelSize(spritesFromTexture);
			foreach (Sprite current in spritesFromTexture)
			{
				Vector2Int gridPosition = TileDragAndDrop.GetGridPosition(current, cellPixelSize);
				dictionary[gridPosition] = current;
			}
			return dictionary;
		}

		public static Dictionary<Vector2Int, TileBase> ConvertToTileSheet(Dictionary<Vector2Int, UnityEngine.Object> sheet)
		{
			Dictionary<Vector2Int, TileBase> dictionary = new Dictionary<Vector2Int, TileBase>();
			string text = (!ProjectBrowser.s_LastInteractedProjectBrowser) ? "Assets" : ProjectBrowser.s_LastInteractedProjectBrowser.GetActiveFolderPath();
			Dictionary<Vector2Int, TileBase> result;
			if (sheet.Values.ToList<UnityEngine.Object>().FindAll((UnityEngine.Object obj) => obj is TileBase).Count == sheet.Values.Count)
			{
				foreach (KeyValuePair<Vector2Int, UnityEngine.Object> current in sheet)
				{
					dictionary.Add(current.Key, current.Value as TileBase);
				}
				result = dictionary;
			}
			else
			{
				TileDragAndDrop.UserTileCreationMode userTileCreationMode = TileDragAndDrop.UserTileCreationMode.Overwrite;
				string text2 = "";
				bool flag = sheet.Count > 1;
				if (flag)
				{
					bool flag2 = false;
					text2 = EditorUtility.SaveFolderPanel("Generate tiles into folder ", text, "");
					text2 = FileUtil.GetProjectRelativePath(text2);
					foreach (UnityEngine.Object current2 in sheet.Values)
					{
						if (current2 is Sprite)
						{
							string path = FileUtil.CombinePaths(new string[]
							{
								text2,
								string.Format("{0}.{1}", current2.name, TileDragAndDrop.k_TileExtension)
							});
							if (File.Exists(path))
							{
								flag2 = true;
								break;
							}
						}
					}
					if (flag2)
					{
						int num = EditorUtility.DisplayDialogComplex("Overwrite?", string.Format("Assets exist at {0}. Do you wish to overwrite existing assets?", text2), "Overwrite", "Create New Copy", "Reuse");
						if (num != 0)
						{
							if (num != 1)
							{
								if (num == 2)
								{
									userTileCreationMode = TileDragAndDrop.UserTileCreationMode.Reuse;
								}
							}
							else
							{
								userTileCreationMode = TileDragAndDrop.UserTileCreationMode.CreateUnique;
							}
						}
						else
						{
							userTileCreationMode = TileDragAndDrop.UserTileCreationMode.Overwrite;
						}
					}
				}
				else
				{
					text2 = EditorUtility.SaveFilePanelInProject("Generate new tile", sheet.Values.First<UnityEngine.Object>().name, TileDragAndDrop.k_TileExtension, "Generate new tile", text);
				}
				if (string.IsNullOrEmpty(text2))
				{
					result = dictionary;
				}
				else
				{
					int num2 = 0;
					EditorUtility.DisplayProgressBar(string.Concat(new object[]
					{
						"Generating Tile Assets (",
						num2,
						"/",
						sheet.Count,
						")"
					}), "Generating tiles", 0f);
					foreach (KeyValuePair<Vector2Int, UnityEngine.Object> current3 in sheet)
					{
						string text3 = "";
						TileBase tileBase;
						if (current3.Value is Sprite)
						{
							tileBase = TileDragAndDrop.CreateTile(current3.Value as Sprite);
							text3 = ((!flag) ? text2 : FileUtil.CombinePaths(new string[]
							{
								text2,
								string.Format("{0}.{1}", tileBase.name, TileDragAndDrop.k_TileExtension)
							}));
							if (userTileCreationMode != TileDragAndDrop.UserTileCreationMode.CreateUnique)
							{
								if (userTileCreationMode != TileDragAndDrop.UserTileCreationMode.Overwrite)
								{
									if (userTileCreationMode == TileDragAndDrop.UserTileCreationMode.Reuse)
									{
										if (File.Exists(text3))
										{
											tileBase = AssetDatabase.LoadAssetAtPath<TileBase>(text3);
										}
										else
										{
											AssetDatabase.CreateAsset(tileBase, text3);
										}
									}
								}
								else
								{
									AssetDatabase.CreateAsset(tileBase, text3);
								}
							}
							else
							{
								if (File.Exists(text3))
								{
									text3 = AssetDatabase.GenerateUniqueAssetPath(text3);
								}
								AssetDatabase.CreateAsset(tileBase, text3);
							}
						}
						else
						{
							tileBase = (current3.Value as TileBase);
						}
						EditorUtility.DisplayProgressBar(string.Concat(new object[]
						{
							"Generating Tile Assets (",
							num2,
							"/",
							sheet.Count,
							")"
						}), "Generating " + text3, (float)num2++ / (float)sheet.Count);
						dictionary.Add(current3.Key, tileBase);
					}
					EditorUtility.ClearProgressBar();
					AssetDatabase.Refresh();
					result = dictionary;
				}
			}
			return result;
		}

		public static Vector2Int EstimateGridPixelSize(List<Sprite> sprites)
		{
			Vector2Int result;
			if (!sprites.Any<Sprite>())
			{
				result = new Vector2Int(0, 0);
			}
			else if (sprites.Count == 1)
			{
				result = Vector2Int.FloorToInt(sprites[0].rect.size);
			}
			else
			{
				Vector2 min = TileDragAndDrop.GetMin(sprites, new Vector2(-3.40282347E+38f, -3.40282347E+38f));
				Vector2 min2 = TileDragAndDrop.GetMin(sprites, min);
				Vector2Int vector2Int = Vector2Int.FloorToInt(min2 - min);
				vector2Int.x = Math.Max(Mathf.FloorToInt(sprites[0].rect.width), vector2Int.x);
				vector2Int.y = Math.Max(Mathf.FloorToInt(sprites[0].rect.height), vector2Int.y);
				result = vector2Int;
			}
			return result;
		}

		private static Vector2 GetMin(List<Sprite> sprites, Vector2 biggerThan)
		{
			List<Sprite> list = sprites.FindAll((Sprite sprite) => sprite.rect.xMin > biggerThan.x);
			List<Sprite> list2 = sprites.FindAll((Sprite sprite) => (float)sprite.texture.height - sprite.rect.yMax > biggerThan.y);
			float arg_6D_0;
			if (list.Count > 0)
			{
				arg_6D_0 = list.Min((Sprite s) => s.rect.xMin);
			}
			else
			{
				arg_6D_0 = 0f;
			}
			float x = arg_6D_0;
			float arg_A7_0;
			if (list2.Count > 0)
			{
				arg_A7_0 = list2.Min((Sprite s) => (float)s.texture.height - s.rect.yMax);
			}
			else
			{
				arg_A7_0 = 0f;
			}
			float y = arg_A7_0;
			return new Vector2(x, y);
		}

		public static Vector2Int GetGridPosition(Sprite sprite, Vector2Int cellPixelSize)
		{
			return new Vector2Int(Mathf.FloorToInt(sprite.rect.center.x / (float)cellPixelSize.x), Mathf.FloorToInt(-((float)sprite.texture.height - sprite.rect.center.y) / (float)cellPixelSize.y) + 1);
		}

		public static RectInt GetMinMaxRect(List<Vector2Int> positions)
		{
			RectInt result;
			if (positions == null || positions.Count == 0)
			{
				result = default(RectInt);
			}
			else
			{
				result = GridEditorUtility.GetMarqueeRect(new Vector2Int(positions.Min((Vector2Int p1) => p1.x), positions.Min((Vector2Int p1) => p1.y)), new Vector2Int(positions.Max((Vector2Int p1) => p1.x), positions.Max((Vector2Int p1) => p1.y)));
			}
			return result;
		}

		public static Tile CreateTile(Sprite sprite)
		{
			Tile tile = ScriptableObject.CreateInstance<Tile>();
			tile.name = sprite.name;
			tile.sprite = sprite;
			tile.color = Color.white;
			return tile;
		}
	}
}
