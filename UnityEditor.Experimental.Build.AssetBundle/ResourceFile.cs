using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[UsedByNativeCode]
	[Serializable]
	public struct ResourceFile
	{
		[NativeName("fileName")]
		internal string m_FileName;

		[NativeName("fileAlias")]
		internal string m_FileAlias;

		[NativeName("serializedFile")]
		internal bool m_SerializedFile;

		public string fileName
		{
			get
			{
				return this.m_FileName;
			}
		}

		public string fileAlias
		{
			get
			{
				return this.m_FileAlias;
			}
		}

		public bool serializedFile
		{
			get
			{
				return this.m_SerializedFile;
			}
		}
	}
}
