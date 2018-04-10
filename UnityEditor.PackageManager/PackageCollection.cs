using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.PackageManager
{
	[Serializable]
	public class PackageCollection : IEnumerable<PackageInfo>, IEnumerable
	{
		[SerializeField]
		private PackageInfo[] m_PackageList;

		private PackageCollection()
		{
		}

		internal PackageCollection(IEnumerable<PackageInfo> packages)
		{
			this.m_PackageList = (packages ?? new PackageInfo[0]).ToArray<PackageInfo>();
		}

		IEnumerator<PackageInfo> IEnumerable<PackageInfo>.GetEnumerator()
		{
			return ((IEnumerable<PackageInfo>)this.m_PackageList).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.m_PackageList.GetEnumerator();
		}
	}
}
