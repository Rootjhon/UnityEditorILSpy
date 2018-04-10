using System;
using UnityEngine;

namespace UnityEditor.PackageManager
{
	[Serializable]
	public class PackageInfo
	{
		[SerializeField]
		private UpmPackageInfo m_UpmPackageInfo;

		public string packageId
		{
			get
			{
				return this.m_UpmPackageInfo.packageId;
			}
		}

		public string version
		{
			get
			{
				return this.m_UpmPackageInfo.version;
			}
		}

		public string resolvedPath
		{
			get
			{
				return this.m_UpmPackageInfo.resolvedPath;
			}
		}

		public string name
		{
			get
			{
				return this.m_UpmPackageInfo.name;
			}
		}

		public string displayName
		{
			get
			{
				return this.m_UpmPackageInfo.displayName;
			}
		}

		public string category
		{
			get
			{
				return this.m_UpmPackageInfo.category;
			}
		}

		public string description
		{
			get
			{
				return this.m_UpmPackageInfo.description;
			}
		}

		private PackageInfo()
		{
		}

		internal PackageInfo(UpmPackageInfo upmPackageInfo)
		{
			this.m_UpmPackageInfo = upmPackageInfo;
		}
	}
}
