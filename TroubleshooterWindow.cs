using System;
using UnityEditor;
using UnityEditor.Web;
using UnityEngine;

internal class TroubleshooterWindow : WebViewEditorWindow, IHasCustomMenu
{
	private WebView m_WebView;

	internal override WebView webView
	{
		get
		{
			return this.m_WebView;
		}
		set
		{
			this.m_WebView = value;
		}
	}

	protected TroubleshooterWindow()
	{
		this.m_InitialOpenURL = "https://bugservices.unity3d.com/troubleshooter/";
	}

	public new void OnInitScripting()
	{
		base.OnInitScripting();
	}

	[MenuItem("Help/Troubleshoot Issue...")]
	public static void RunTroubleshooter()
	{
		TroubleshooterWindow windowWithRect = EditorWindow.GetWindowWithRect<TroubleshooterWindow>(new Rect(100f, 100f, 990f, 680f), true, "Troubleshooter");
		if (windowWithRect != null)
		{
			windowWithRect.ShowUtility();
		}
	}
}
