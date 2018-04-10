using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor.PackageManager
{
	[RequiredByNativeCode]
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	internal class OutdatedPackage
	{
		[SerializeField]
		private UpmPackageInfo m_Current;

		[SerializeField]
		private UpmPackageInfo m_Latest;

		public UpmPackageInfo current
		{
			get
			{
				return this.m_Current;
			}
		}

		public UpmPackageInfo latest
		{
			get
			{
				return this.m_Latest;
			}
		}

		private OutdatedPackage()
		{
		}

		public OutdatedPackage(UpmPackageInfo current, UpmPackageInfo latest)
		{
			this.m_Current = current;
			this.m_Latest = latest;
		}
	}
}
