using System;
using UnityEngine;

namespace UnityEditor
{
	internal static class WaveformPreviewFactory
	{
		public static WaveformPreview Create(int initialSize, AudioClip clip)
		{
			return new StreamedAudioClipPreview(clip, initialSize);
		}
	}
}
