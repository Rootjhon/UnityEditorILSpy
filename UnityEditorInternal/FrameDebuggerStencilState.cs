using System;
using UnityEngine.Rendering;

namespace UnityEditorInternal
{
	internal struct FrameDebuggerStencilState
	{
		public bool stencilEnable;

		public byte readMask;

		public byte writeMask;

		public byte padding;

		public CompareFunction stencilFuncFront;

		public StencilOp stencilPassOpFront;

		public StencilOp stencilFailOpFront;

		public StencilOp stencilZFailOpFront;

		public CompareFunction stencilFuncBack;

		public StencilOp stencilPassOpBack;

		public StencilOp stencilFailOpBack;

		public StencilOp stencilZFailOpBack;
	}
}
