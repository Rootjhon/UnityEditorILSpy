using System;

namespace UnityEditor.Scripting.Compilers
{
	internal class UWPSDK
	{
		public readonly Version Version;

		public readonly Version MinVSVersion;

		public UWPSDK(Version version, Version minVSVersion)
		{
			this.Version = version;
			this.MinVSVersion = minVSVersion;
		}
	}
}
