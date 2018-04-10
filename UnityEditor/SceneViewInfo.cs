using System;

namespace UnityEditor
{
	[Serializable]
	internal struct SceneViewInfo
	{
		public int total_scene_views;

		public int num_of_2d_views;

		public bool is_default_2d_mode;
	}
}
