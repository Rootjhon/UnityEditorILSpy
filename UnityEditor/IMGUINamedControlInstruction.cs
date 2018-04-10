using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	[RequiredByNativeCode]
	internal struct IMGUINamedControlInstruction
	{
		public string name;

		public Rect rect;

		public int id;
	}
}
