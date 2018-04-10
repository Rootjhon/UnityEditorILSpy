using System;
using System.Collections.ObjectModel;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.Player
{
	[UsedByNativeCode]
	[Serializable]
	public struct ScriptCompilationResult
	{
		[NativeName("assemblies")]
		internal string[] m_Assemblies;

		[Ignore]
		internal TypeDB m_TypeDB;

		public ReadOnlyCollection<string> assemblies
		{
			get
			{
				return Array.AsReadOnly<string>(this.m_Assemblies);
			}
		}

		public TypeDB typeDB
		{
			get
			{
				return this.m_TypeDB;
			}
		}
	}
}
