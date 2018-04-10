using System;

namespace UnityEditor.Sprites
{
	public interface IPackerPolicy
	{
		bool AllowSequentialPacking
		{
			get;
		}

		void OnGroupAtlases(BuildTarget target, PackerJob job, int[] textureImporterInstanceIDs);

		int GetVersion();
	}
}
