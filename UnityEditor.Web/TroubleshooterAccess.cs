using System;
using UnityEditor.Connect;

namespace UnityEditor.Web
{
	[InitializeOnLoad]
	internal class TroubleshooterAccess
	{
		private TroubleshooterAccess()
		{
		}

		static TroubleshooterAccess()
		{
			JSProxyMgr.GetInstance().AddGlobalObject("/unity/editor/troubleshooter", new TroubleshooterAccess());
		}

		public string GetUserName()
		{
			UnityConnect instance = UnityConnect.instance;
			string result;
			if (!instance.GetConnectInfo().loggedIn)
			{
				result = "Anonymous";
			}
			else
			{
				result = instance.GetUserName();
			}
			return result;
		}

		public string GetUserId()
		{
			UnityConnect instance = UnityConnect.instance;
			string result;
			if (!instance.GetConnectInfo().loggedIn)
			{
				result = string.Empty;
			}
			else
			{
				result = instance.GetUserInfo().userId;
			}
			return result;
		}

		public void SignIn()
		{
			UnityConnect.instance.ShowLogin();
		}

		public void SignOut()
		{
			UnityConnect.instance.Logout();
		}

		public void StartBugReporter()
		{
			EditorUtility.LaunchBugReporter();
		}
	}
}
