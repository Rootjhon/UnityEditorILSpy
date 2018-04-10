using System;
using UnityEngine;

namespace UnityEditor
{
	internal abstract class ObjectSelectorReceiver : ScriptableObject
	{
		public abstract void OnSelectionChanged(UnityEngine.Object selection);

		public abstract void OnSelectionClosed(UnityEngine.Object selection);
	}
}
