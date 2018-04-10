using System;
using UnityEngine;

namespace UnityEditor
{
	internal class AudioSourceExtensionEditor : AudioExtensionEditor
	{
		public virtual void OnAudioSourceGUI()
		{
		}

		public virtual void OnAudioSourceSceneGUI(AudioSource source)
		{
		}

		protected override int GetNumSerializedExtensionProperties(UnityEngine.Object obj)
		{
			AudioSource audioSource = obj as AudioSource;
			return (!audioSource) ? 0 : audioSource.GetNumExtensionProperties();
		}
	}
}
