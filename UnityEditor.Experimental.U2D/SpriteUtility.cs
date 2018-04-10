using System;
using UnityEditor.Sprites;
using UnityEngine;

namespace UnityEditor.Experimental.U2D
{
	internal static class SpriteUtility
	{
		public static void GenerateOutline(Texture2D texture, Rect rect, float detail, byte alphaTolerance, bool holeDetection, out Vector2[][] paths)
		{
			UnityEditor.Sprites.SpriteUtility.GenerateOutline(texture, rect, detail, alphaTolerance, holeDetection, out paths);
		}
	}
}
