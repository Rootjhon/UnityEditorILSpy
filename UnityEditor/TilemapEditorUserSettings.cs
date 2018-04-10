using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Modules/TilemapEditor/Editor/TilemapEditorUserSettings.h")]
	internal sealed class TilemapEditorUserSettings
	{
		public enum FocusMode
		{
			None,
			Tilemap,
			Grid
		}

		public static extern GameObject lastUsedPalette
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}

		public static extern TilemapEditorUserSettings.FocusMode focusMode
		{
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
			[MethodImpl(MethodImplOptions.InternalCall)]
			set;
		}
	}
}
