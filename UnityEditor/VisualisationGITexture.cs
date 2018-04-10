using System;
using UnityEngine;
using UnityEngineInternal;

namespace UnityEditor
{
	internal struct VisualisationGITexture
	{
		public GITextureType type;

		public GITextureAvailability textureAvailability;

		public Texture2D texture;

		public Hash128 contentHash;
	}
}
