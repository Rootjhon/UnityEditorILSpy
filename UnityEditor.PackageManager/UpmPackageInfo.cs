using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor.PackageManager
{
	[RequiredByNativeCode]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	internal class UpmPackageInfo
	{
		[SerializeField]
		private string m_PackageId;

		[SerializeField]
		private string m_Version;

		[SerializeField]
		private string m_ResolvedPath;

		[SerializeField]
		private string m_Name;

		[SerializeField]
		private string m_DisplayName;

		[SerializeField]
		private string m_Category;

		[SerializeField]
		private string m_Description;

		public string packageId
		{
			get
			{
				return this.m_PackageId;
			}
		}

		public string version
		{
			get
			{
				return this.m_Version;
			}
		}

		public string resolvedPath
		{
			get
			{
				return this.m_ResolvedPath;
			}
		}

		public string name
		{
			get
			{
				return this.m_Name;
			}
		}

		public string displayName
		{
			get
			{
				return this.m_DisplayName;
			}
		}

		public string category
		{
			get
			{
				return this.m_Category;
			}
		}

		public string description
		{
			get
			{
				return this.m_Description;
			}
		}

		private UpmPackageInfo()
		{
		}

		internal UpmPackageInfo(string packageId, string displayName = "", string category = "", string description = "", string resolvedPath = "", string tag = "")
		{
			this.m_PackageId = packageId;
			this.m_DisplayName = displayName;
			this.m_Category = category;
			this.m_Description = description;
			this.m_ResolvedPath = resolvedPath;
			string[] array = packageId.Split(new char[]
			{
				'@'
			});
			this.m_Name = array[0];
			this.m_Version = array[1];
		}

		public static implicit operator PackageInfo(UpmPackageInfo source)
		{
			return new PackageInfo(source);
		}
	}
}
