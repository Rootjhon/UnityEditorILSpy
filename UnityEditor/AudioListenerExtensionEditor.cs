using System;
using UnityEngine;

namespace UnityEditor
{
	internal class AudioListenerExtensionEditor : AudioExtensionEditor
	{
		public virtual void OnAudioListenerGUI()
		{
		}

		protected override int GetNumSerializedExtensionProperties(UnityEngine.Object obj)
		{
			AudioListener audioListener = obj as AudioListener;
			return (!audioListener) ? 0 : audioListener.GetNumExtensionProperties();
		}
	}
}
