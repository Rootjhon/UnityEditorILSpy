using System;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[RequiredByNativeCode]
	internal class EditorPreviewTilemap : ITilemap
	{
		private EditorPreviewTilemap()
		{
		}

		public override Sprite GetSprite(Vector3Int position)
		{
			TileBase editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile(position);
			return (!editorPreviewTile) ? this.m_Tilemap.GetSprite(position) : this.m_Tilemap.GetEditorPreviewSprite(position);
		}

		public override Color GetColor(Vector3Int position)
		{
			TileBase editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile(position);
			return (!editorPreviewTile) ? this.m_Tilemap.GetColor(position) : this.m_Tilemap.GetEditorPreviewColor(position);
		}

		public override Matrix4x4 GetTransformMatrix(Vector3Int position)
		{
			TileBase editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile(position);
			return (!editorPreviewTile) ? this.m_Tilemap.GetTransformMatrix(position) : this.m_Tilemap.GetEditorPreviewTransformMatrix(position);
		}

		public override TileFlags GetTileFlags(Vector3Int position)
		{
			TileBase editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile(position);
			return (!editorPreviewTile) ? this.m_Tilemap.GetTileFlags(position) : this.m_Tilemap.GetEditorPreviewTileFlags(position);
		}

		public override TileBase GetTile(Vector3Int position)
		{
			TileBase editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile(position);
			return editorPreviewTile ?? this.m_Tilemap.GetTile(position);
		}

		public override T GetTile<T>(Vector3Int position)
		{
			T editorPreviewTile = this.m_Tilemap.GetEditorPreviewTile<T>(position);
			T arg_27_0;
			if ((arg_27_0 = editorPreviewTile) == null)
			{
				arg_27_0 = this.m_Tilemap.GetTile<T>(position);
			}
			return arg_27_0;
		}

		private TileBase CreateInvalidTile()
		{
			Texture2D whiteTexture = Texture2D.whiteTexture;
			Sprite sprite = Sprite.Create(whiteTexture, new Rect(0f, 0f, (float)whiteTexture.width, (float)whiteTexture.height), new Vector2(0.5f, 0.5f), (float)whiteTexture.width);
			Tile tile = ScriptableObject.CreateInstance<Tile>();
			tile.sprite = sprite;
			tile.color = UnityEngine.Random.ColorHSV(0.9444444f, 1f, 0.3f, 0.6f, 0.7f, 1f);
			tile.transform = Matrix4x4.identity;
			tile.flags = TileFlags.LockAll;
			return tile;
		}

		private static ITilemap CreateInstance()
		{
			ITilemap.s_Instance = new EditorPreviewTilemap();
			return ITilemap.s_Instance;
		}
	}
}
