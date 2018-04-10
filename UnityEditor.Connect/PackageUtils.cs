using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UnityEditor.Connect
{
	internal class PackageUtils
	{
		private static readonly PackageUtils s_Instance;

		private bool m_outdatedOperationRunning = false;

		private long m_outdatedOperationId = 0L;

		private bool m_listOperationRunning = false;

		private long m_listOperationId = 0L;

		private Dictionary<string, UpmPackageInfo> m_currentPackages = new Dictionary<string, UpmPackageInfo>();

		private Dictionary<string, UpmPackageInfo> m_outdatedPackages = new Dictionary<string, UpmPackageInfo>();

		public static PackageUtils instance
		{
			get
			{
				return PackageUtils.s_Instance;
			}
		}

		static PackageUtils()
		{
			PackageUtils.s_Instance = new PackageUtils();
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool WaitForPackageManagerOperation(long operationId, string progressBarText);

		public void RetrievePackageInfo()
		{
			if (NativeClient.List(out this.m_listOperationId) == NativeClient.StatusCode.Error)
			{
				Debug.LogWarning("Failed to call list packages!");
			}
			else
			{
				this.m_listOperationRunning = true;
				if (NativeClient.Outdated(out this.m_outdatedOperationId) == NativeClient.StatusCode.Error)
				{
					Debug.LogWarning("Failed to call outdated package!");
				}
				else
				{
					this.m_outdatedOperationRunning = true;
				}
			}
		}

		public string GetCurrentVersion(string packageName)
		{
			this.CheckRunningOperations();
			string result;
			if (this.m_currentPackages.ContainsKey(packageName))
			{
				result = this.m_currentPackages[packageName].version;
			}
			else
			{
				result = string.Empty;
			}
			return result;
		}

		public string GetLatestVersion(string packageName)
		{
			this.CheckRunningOperations();
			string result;
			if (this.m_outdatedPackages.ContainsKey(packageName))
			{
				result = this.m_outdatedPackages[packageName].version;
			}
			else
			{
				result = this.GetCurrentVersion(packageName);
			}
			return result;
		}

		public bool UpdateLatest(string packageName)
		{
			bool result;
			if (!this.m_outdatedPackages.ContainsKey(packageName))
			{
				result = false;
			}
			else
			{
				long operationId = 0L;
				if (NativeClient.Add(out operationId, this.m_outdatedPackages[packageName].packageId) == NativeClient.StatusCode.Error)
				{
					Debug.LogWarningFormat("Failed to update outdated package {0}!", new object[]
					{
						packageName
					});
					result = false;
				}
				else if (PackageUtils.WaitForPackageManagerOperation(operationId, string.Format("Updating Package {0} to version {1}", packageName, this.m_outdatedPackages[packageName].version)))
				{
					UpmPackageInfo addOperationData = NativeClient.GetAddOperationData(operationId);
					this.m_currentPackages[this.GetPackageRootName(addOperationData)] = addOperationData;
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		private void CheckRunningOperations()
		{
			if (this.m_outdatedOperationRunning)
			{
				switch (NativeClient.GetOperationStatus(this.m_outdatedOperationId))
				{
				case NativeClient.StatusCode.Done:
				{
					this.m_outdatedPackages.Clear();
					Dictionary<string, OutdatedPackage> outdatedOperationData = NativeClient.GetOutdatedOperationData(this.m_outdatedOperationId);
					foreach (string current in outdatedOperationData.Keys)
					{
						this.m_outdatedPackages[current] = outdatedOperationData[current].latest;
					}
					this.m_outdatedOperationRunning = false;
					break;
				}
				case NativeClient.StatusCode.Error:
				case NativeClient.StatusCode.NotFound:
					this.m_outdatedOperationRunning = false;
					Debug.LogWarning("Failed to retrieve outdated package list!");
					break;
				}
			}
			if (this.m_listOperationRunning)
			{
				switch (NativeClient.GetOperationStatus(this.m_listOperationId))
				{
				case NativeClient.StatusCode.Done:
				{
					this.m_currentPackages.Clear();
					OperationStatus listOperationData = NativeClient.GetListOperationData(this.m_listOperationId);
					for (int i = 0; i < listOperationData.packageList.Length; i++)
					{
						this.m_currentPackages[this.GetPackageRootName(listOperationData.packageList[i])] = listOperationData.packageList[i];
					}
					this.m_listOperationRunning = false;
					break;
				}
				case NativeClient.StatusCode.Error:
				case NativeClient.StatusCode.NotFound:
					this.m_listOperationRunning = false;
					Debug.LogWarning("Failed to retrieve package list!");
					break;
				}
			}
		}

		private string GetPackageRootName(UpmPackageInfo packageInfo)
		{
			return packageInfo.packageId.Substring(0, packageInfo.packageId.Length - packageInfo.version.Length - 1);
		}
	}
}
