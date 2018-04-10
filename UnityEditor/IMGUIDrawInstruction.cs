using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[RequiredByNativeCode]
	internal struct IMGUIDrawInstruction
	{
		public Rect rect;

		public Rect visibleRect;

		public GUIStyle usedGUIStyle;

		public GUIContent usedGUIContent;

		public StackFrame[] stackframes;

		public void Reset()
		{
			this.rect = default(Rect);
			this.visibleRect = default(Rect);
			this.usedGUIStyle = GUIStyle.none;
			this.usedGUIContent = GUIContent.none;
		}
	}
}
