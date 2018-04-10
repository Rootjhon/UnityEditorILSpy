using System;
using UnityEditor.Web;
using UnityEngine;

namespace UnityEditor.Connect
{
	[Serializable]
	internal class UnityConnectConsentView : WebViewEditorWindow
	{
		private string code = "";

		private string error = "";

		public string Code
		{
			get
			{
				return this.code;
			}
		}

		public string Error
		{
			get
			{
				return this.error;
			}
		}

		internal override WebView webView
		{
			get;
			set;
		}

		public static UnityConnectConsentView ShowUnityConnectConsentView(string URL)
		{
			UnityConnectConsentView unityConnectConsentView = ScriptableObject.CreateInstance<UnityConnectConsentView>();
			Rect position = new Rect(100f, 100f, 800f, 605f);
			unityConnectConsentView.titleContent = EditorGUIUtility.TextContent("Unity Application Consent Window");
			unityConnectConsentView.minSize = new Vector2(position.width, position.height);
			unityConnectConsentView.maxSize = new Vector2(position.width, position.height);
			unityConnectConsentView.position = position;
			unityConnectConsentView.m_InitialOpenURL = URL;
			unityConnectConsentView.ShowModal();
			unityConnectConsentView.m_Parent.window.m_DontSaveToLayout = true;
			return unityConnectConsentView;
		}

		public override void OnDestroy()
		{
			base.OnBecameInvisible();
		}

		public override void OnInitScripting()
		{
			base.SetScriptObject();
		}

		public override void OnLocationChanged(string url)
		{
			Uri uri = new Uri(url);
			string[] array = uri.Query.Split(new char[]
			{
				'&'
			});
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				string[] array2 = text.Replace("?", string.Empty).Split(new char[]
				{
					'='
				});
				if (array2[0] == "code")
				{
					this.code = array2[1];
					break;
				}
				if (array2[0] == "error")
				{
					this.error = array2[1];
					break;
				}
			}
			if (!string.IsNullOrEmpty(this.code) || !string.IsNullOrEmpty(this.error))
			{
				base.Close();
			}
			else
			{
				base.OnLocationChanged(url);
			}
		}
	}
}
