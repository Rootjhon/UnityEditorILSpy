using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/AssetPipeline/ModelImporting/ModelImporter.h"), UsedByNativeCode]
	public struct TakeInfo
	{
		public string name;

		public string defaultClipName;

		public float startTime;

		public float stopTime;

		public float bakeStartTime;

		public float bakeStopTime;

		public float sampleRate;
	}
}
