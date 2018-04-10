using System;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;

namespace UnityEditor.PackageManager
{
	[RequiredByNativeCode]
	[StructLayout(LayoutKind.Sequential)]
	internal class OperationStatus
	{
		private StatusCode m_Status;

		private string m_Id;

		private string m_Type;

		private UpmPackageInfo[] m_PackageList;

		private float m_Progress;

		public string id
		{
			get
			{
				return this.m_Id;
			}
		}

		public StatusCode status
		{
			get
			{
				return this.m_Status;
			}
		}

		public string type
		{
			get
			{
				return this.m_Type;
			}
		}

		public UpmPackageInfo[] packageList
		{
			get
			{
				return this.m_PackageList;
			}
		}

		public float progress
		{
			get
			{
				return this.m_Progress;
			}
		}

		private OperationStatus()
		{
		}
	}
}
