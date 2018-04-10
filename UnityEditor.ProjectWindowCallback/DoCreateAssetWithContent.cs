using System;
using UnityEngine;

namespace UnityEditor.ProjectWindowCallback
{
	internal class DoCreateAssetWithContent : EndNameEditAction
	{
		public string filecontent;

		public override void Action(int instanceId, string pathName, string resourceFile)
		{
			UnityEngine.Object o = ProjectWindowUtil.CreateScriptAssetWithContent(pathName, this.filecontent);
			ProjectWindowUtil.ShowCreatedAsset(o);
		}
	}
}
