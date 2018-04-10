using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngineInternal;

namespace UnityEditor
{
	internal sealed class LightmapVisualizationUtility
	{
		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool IsTextureTypeEnabled(GITextureType textureType);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern bool IsBakedTextureType(GITextureType textureType);

		internal static VisualisationGITexture GetSelectedObjectGITexture(GITextureType textureType)
		{
			VisualisationGITexture result = default(VisualisationGITexture);
			LightmapVisualizationUtility.GetSelectedObjectGITextureInternal(textureType, ref result);
			return result;
		}

		private static void GetSelectedObjectGITextureInternal(GITextureType textureType, ref VisualisationGITexture result)
		{
			LightmapVisualizationUtility.INTERNAL_CALL_GetSelectedObjectGITextureInternal(textureType, ref result);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_GetSelectedObjectGITextureInternal(GITextureType textureType, ref VisualisationGITexture result);

		internal static Hash128 GetSelectedObjectGITextureHash(GITextureType textureType)
		{
			Hash128 result = default(Hash128);
			LightmapVisualizationUtility.GetSelectedObjectGITextureHashInternal(textureType, ref result);
			return result;
		}

		private static void GetSelectedObjectGITextureHashInternal(GITextureType textureType, ref Hash128 result)
		{
			LightmapVisualizationUtility.INTERNAL_CALL_GetSelectedObjectGITextureHashInternal(textureType, ref result);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_GetSelectedObjectGITextureHashInternal(GITextureType textureType, ref Hash128 result);

		public static void DrawTextureWithUVOverlay(Texture2D texture, GameObject gameObject, Rect drawableArea, Rect position, GITextureType textureType)
		{
			LightmapVisualizationUtility.INTERNAL_CALL_DrawTextureWithUVOverlay(texture, gameObject, ref drawableArea, ref position, textureType);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_DrawTextureWithUVOverlay(Texture2D texture, GameObject gameObject, ref Rect drawableArea, ref Rect position, GITextureType textureType);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		public static extern LightmapType GetLightmapType(GITextureType textureType);

		public static Vector4 GetLightmapTilingOffset(LightmapType lightmapType)
		{
			Vector4 result;
			LightmapVisualizationUtility.INTERNAL_CALL_GetLightmapTilingOffset(lightmapType, out result);
			return result;
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void INTERNAL_CALL_GetLightmapTilingOffset(LightmapType lightmapType, out Vector4 value);
	}
}
