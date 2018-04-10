using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEditor
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	internal sealed class EditorHeaderItemAttribute : CallbackOrderAttribute
	{
		public Type TargetType;

		public EditorHeaderItemAttribute(Type targetType, int priority = 1)
		{
			this.TargetType = targetType;
			this.m_CallbackOrder = priority;
		}

		[RequiredSignature]
		private static bool SignatureBool(Rect rectangle, UnityEngine.Object[] targetObjets)
		{
			return EditorHeaderItemAttribute.SignatureBool_Injected(ref rectangle, targetObjets);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool SignatureBool_Injected(ref Rect rectangle, UnityEngine.Object[] targetObjets);
	}
}
