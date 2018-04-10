using System;

namespace UnityEditor.BuildReporting
{
	internal enum BuildResult
	{
		Unknown = 1,
		Succeeded,
		Failed = 4,
		Canceled = 8
	}
}
