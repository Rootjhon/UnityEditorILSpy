using System;
using UnityEditor.BuildReporting;

namespace UnityEditor.Modules
{
	internal struct BuildLaunchPlayerArgs
	{
		public BuildTarget target;

		public string playerPackage;

		public string installPath;

		public string productName;

		public BuildOptions options;

		public BuildReport report;
	}
}
