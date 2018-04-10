using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Src/Animation/HumanTemplate.h"), UsedByNativeCode]
	public sealed class HumanTemplate : UnityEngine.Object
	{
		public HumanTemplate()
		{
			HumanTemplate.Internal_Create(this);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_Create([Writable] HumanTemplate self);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void Insert(string name, string templateName);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern string Find(string name);

		[MethodImpl(MethodImplOptions.InternalCall)]
		public extern void ClearTemplate();
	}
}
