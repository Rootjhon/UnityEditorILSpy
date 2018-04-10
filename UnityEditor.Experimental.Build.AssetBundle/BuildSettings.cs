using System;
using UnityEditor.Experimental.Build.Player;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[UsedByNativeCode]
	[Serializable]
	public struct BuildSettings
	{
		[NativeName("typeDB")]
		internal TypeDB m_TypeDB;

		[NativeName("target")]
		internal BuildTarget m_Target;

		[NativeName("group")]
		internal BuildTargetGroup m_Group;

		public TypeDB typeDB
		{
			get
			{
				return this.m_TypeDB;
			}
			set
			{
				this.m_TypeDB = value;
			}
		}

		public BuildTarget target
		{
			get
			{
				return this.m_Target;
			}
			set
			{
				this.m_Target = value;
			}
		}

		public BuildTargetGroup group
		{
			get
			{
				return this.m_Group;
			}
			set
			{
				this.m_Group = value;
			}
		}
	}
}
