using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	internal class GUIViewDebuggerHelper
	{
		internal static Action onViewInstructionsChanged;

		internal static void GetViews(List<GUIView> views)
		{
			GUIViewDebuggerHelper.GetViewsInternal(views);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetViewsInternal(object views);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void DebugWindow(GUIView view);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void StopDebugging();

		private static GUIContent CreateGUIContent(string text, Texture image, string tooltip)
		{
			return new GUIContent(text, image, tooltip);
		}

		internal static void GetDrawInstructions(List<IMGUIDrawInstruction> drawInstructions)
		{
			GUIViewDebuggerHelper.GetDrawInstructionsInternal(drawInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetDrawInstructionsInternal(object drawInstructions);

		internal static void GetClipInstructions(List<IMGUIClipInstruction> clipInstructions)
		{
			GUIViewDebuggerHelper.GetClipInstructionsInternal(clipInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetClipInstructionsInternal(object clipInstructions);

		internal static void GetNamedControlInstructions(List<IMGUINamedControlInstruction> namedControlInstructions)
		{
			GUIViewDebuggerHelper.GetNamedControlInstructionsInternal(namedControlInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetNamedControlInstructionsInternal(object namedControlInstructions);

		internal static void GetPropertyInstructions(List<IMGUIPropertyInstruction> namedControlInstructions)
		{
			GUIViewDebuggerHelper.GetPropertyInstructionsInternal(namedControlInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetPropertyInstructionsInternal(object propertyInstructions);

		internal static void GetLayoutInstructions(List<IMGUILayoutInstruction> layoutInstructions)
		{
			GUIViewDebuggerHelper.GetLayoutInstructionsInternal(layoutInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetLayoutInstructionsInternal(object layoutInstructions);

		internal static void GetUnifiedInstructions(List<IMGUIInstruction> layoutInstructions)
		{
			GUIViewDebuggerHelper.GetUnifiedInstructionsInternal(layoutInstructions);
		}

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void GetUnifiedInstructionsInternal(object instructions);

		[GeneratedByOldBindingsGenerator]
		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void ClearInstructions();

		[RequiredByNativeCode]
		private static void CallOnViewInstructionsChanged()
		{
			if (GUIViewDebuggerHelper.onViewInstructionsChanged != null)
			{
				GUIViewDebuggerHelper.onViewInstructionsChanged();
			}
		}
	}
}
