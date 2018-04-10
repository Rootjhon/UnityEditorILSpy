using System;
using UnityEngine;

namespace UnityEditor
{
	public class GridPalette : ScriptableObject
	{
		public enum CellSizing
		{
			Automatic,
			Manual = 100
		}

		[SerializeField]
		public GridPalette.CellSizing cellSizing;
	}
}
