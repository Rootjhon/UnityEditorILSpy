using System;
using UnityEditor;

namespace UnityEditorInternal
{
	[CustomEditor(typeof(ProjectSettingsBase), true)]
	internal class ProjectSettingsBaseEditor : Editor
	{
		protected override bool ShouldHideOpenButton()
		{
			return true;
		}
	}
}
