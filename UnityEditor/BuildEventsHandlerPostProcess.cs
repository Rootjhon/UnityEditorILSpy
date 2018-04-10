using System;
using UnityEditor.Build;
using UnityEngine;

namespace UnityEditor
{
	internal class BuildEventsHandlerPostProcess : IPostprocessBuild, IOrderedCallback
	{
		private static bool s_EventSent = false;

		private static int s_NumOfSceneViews = 0;

		private static int s_NumOf2dSceneViews = 0;

		public int callbackOrder
		{
			get
			{
				return 0;
			}
		}

		public void OnPostprocessBuild(BuildTarget target, string path)
		{
			UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(SceneView));
			int num = 0;
			UnityEngine.Object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				SceneView sceneView = (SceneView)array2[i];
				if (sceneView.in2DMode)
				{
					num++;
				}
			}
			if (BuildEventsHandlerPostProcess.s_NumOfSceneViews != array.Length || BuildEventsHandlerPostProcess.s_NumOf2dSceneViews != num || !BuildEventsHandlerPostProcess.s_EventSent)
			{
				BuildEventsHandlerPostProcess.s_EventSent = true;
				BuildEventsHandlerPostProcess.s_NumOfSceneViews = array.Length;
				BuildEventsHandlerPostProcess.s_NumOf2dSceneViews = num;
				EditorAnalytics.SendEventSceneViewInfo(new SceneViewInfo
				{
					total_scene_views = BuildEventsHandlerPostProcess.s_NumOfSceneViews,
					num_of_2d_views = BuildEventsHandlerPostProcess.s_NumOf2dSceneViews,
					is_default_2d_mode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D
				});
			}
		}
	}
}
