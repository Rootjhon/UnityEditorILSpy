using System;
using UnityEditor.Modules;
using UnityEngine;

namespace UnityEditor
{
	internal class DisplayUtility
	{
		private static string s_DisplayStr = "Display {0}";

		private static GUIContent[] s_GenericDisplayNames = new GUIContent[]
		{
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 1)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 2)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 3)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 4)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 5)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 6)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 7)),
			EditorGUIUtility.TextContent(string.Format(DisplayUtility.s_DisplayStr, 8))
		};

		private static readonly int[] s_DisplayIndices = new int[]
		{
			0,
			1,
			2,
			3,
			4,
			5,
			6,
			7
		};

		public static GUIContent[] GetGenericDisplayNames()
		{
			return DisplayUtility.s_GenericDisplayNames;
		}

		public static int[] GetDisplayIndices()
		{
			return DisplayUtility.s_DisplayIndices;
		}

		public static GUIContent[] GetDisplayNames()
		{
			GUIContent[] displayNames = ModuleManager.GetDisplayNames(EditorUserBuildSettings.activeBuildTarget.ToString());
			return (displayNames == null) ? DisplayUtility.s_GenericDisplayNames : displayNames;
		}
	}
}
