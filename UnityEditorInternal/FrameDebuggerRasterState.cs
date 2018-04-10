using System;
using UnityEngine.Rendering;

namespace UnityEditorInternal
{
	internal struct FrameDebuggerRasterState
	{
		public CullMode cullMode;

		public int depthBias;

		public bool depthClip;

		public float slopeScaledDepthBias;
	}
}
