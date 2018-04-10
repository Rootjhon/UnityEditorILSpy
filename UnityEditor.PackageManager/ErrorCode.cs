using System;

namespace UnityEditor.PackageManager
{
	public enum ErrorCode : uint
	{
		Unknown,
		NotFound,
		Forbidden,
		InvalidParameter,
		Conflict
	}
}
