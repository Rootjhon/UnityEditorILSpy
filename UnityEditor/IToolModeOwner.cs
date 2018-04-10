using System;
using UnityEngine;

namespace UnityEditor
{
	internal interface IToolModeOwner
	{
		bool areToolModesAvailable
		{
			get;
		}

		int GetInstanceID();

		Bounds GetWorldBoundsOfTargets();

		bool ModeSurvivesSelectionChange(int toolMode);
	}
}
