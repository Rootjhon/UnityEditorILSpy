using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Hardware;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class AttachProfilerUI
	{
		private static string kEnterIPText = "<Enter IP>";

		private static GUIContent ms_NotificationMessage;

		private const int PLAYER_DIRECT_IP_CONNECT_GUID = 65261;

		private const int PLAYER_DIRECT_URL_CONNECT_GUID = 65262;

		protected void SelectProfilerClick(object userData, string[] options, int selected)
		{
			List<ProfilerChoise> list = (List<ProfilerChoise>)userData;
			if (selected < list.Count<ProfilerChoise>())
			{
				list[selected].ConnectTo();
			}
		}

		public bool IsEditor()
		{
			return ProfilerDriver.IsConnectionEditor();
		}

		public string GetConnectedProfiler()
		{
			return ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler);
		}

		public static void DirectIPConnect(string ip)
		{
			ConsoleWindow.ShowConsoleWindow(true);
			AttachProfilerUI.ms_NotificationMessage = new GUIContent("Connecting to player...(this can take a while)");
			ProfilerDriver.DirectIPConnect(ip);
			AttachProfilerUI.ms_NotificationMessage = null;
		}

		public static void DirectURLConnect(string url)
		{
			ConsoleWindow.ShowConsoleWindow(true);
			AttachProfilerUI.ms_NotificationMessage = new GUIContent("Connecting to player...(this can take a while)");
			ProfilerDriver.DirectURLConnect(url);
			AttachProfilerUI.ms_NotificationMessage = null;
		}

		public void OnGUILayout(EditorWindow window)
		{
			this.OnGUI();
			if (AttachProfilerUI.ms_NotificationMessage != null)
			{
				window.ShowNotification(AttachProfilerUI.ms_NotificationMessage);
			}
			else
			{
				window.RemoveNotification();
			}
		}

		private static void AddLastIPProfiler(List<ProfilerChoise> profilers)
		{
			string lastIP = ProfilerIPWindow.GetLastIPString();
			if (!string.IsNullOrEmpty(lastIP))
			{
				ProfilerChoise item = default(ProfilerChoise);
				item.Name = lastIP;
				item.Enabled = true;
				item.IsSelected = (() => ProfilerDriver.connectedProfiler == 65261);
				item.ConnectTo = delegate
				{
					AttachProfilerUI.DirectIPConnect(lastIP);
				};
				profilers.Add(item);
			}
		}

		private static void AddPlayerProfilers(List<ProfilerChoise> profilers)
		{
			int[] availableProfilers = ProfilerDriver.GetAvailableProfilers();
			for (int i = 0; i < availableProfilers.Length; i++)
			{
				int guid = availableProfilers[i];
				string text = ProfilerDriver.GetConnectionIdentifier(guid);
				bool flag = ProfilerDriver.IsIdentifierOnLocalhost(guid) && text.Contains("MetroPlayerX");
				bool flag2 = !flag && ProfilerDriver.IsIdentifierConnectable(guid);
				if (!flag2)
				{
					if (flag)
					{
						text += " (Localhost prohibited)";
					}
					else
					{
						text += " (Version mismatch)";
					}
				}
				profilers.Add(new ProfilerChoise
				{
					Name = text,
					Enabled = flag2,
					IsSelected = () => ProfilerDriver.connectedProfiler == guid,
					ConnectTo = delegate
					{
						ProfilerDriver.connectedProfiler = guid;
					}
				});
			}
		}

		private static void AddDeviceProfilers(List<ProfilerChoise> profilers)
		{
			DevDevice[] devices = DevDeviceList.GetDevices();
			for (int i = 0; i < devices.Length; i++)
			{
				DevDevice devDevice = devices[i];
				bool flag = (devDevice.features & DevDeviceFeatures.PlayerConnection) != DevDeviceFeatures.None;
				if (devDevice.isConnected && flag)
				{
					string url = "device://" + devDevice.id;
					profilers.Add(new ProfilerChoise
					{
						Name = devDevice.name,
						Enabled = true,
						IsSelected = () => ProfilerDriver.connectedProfiler == 65262 && ProfilerDriver.directConnectionUrl == url,
						ConnectTo = delegate
						{
							AttachProfilerUI.DirectURLConnect(url);
						}
					});
				}
			}
		}

		private void AddEnterIPProfiler(List<ProfilerChoise> profilers, Rect buttonScreenRect)
		{
			ProfilerChoise item = default(ProfilerChoise);
			item.Name = AttachProfilerUI.kEnterIPText;
			item.Enabled = true;
			item.IsSelected = (() => false);
			item.ConnectTo = delegate
			{
				ProfilerIPWindow.Show(buttonScreenRect);
			};
			profilers.Add(item);
		}

		public void OnGUI()
		{
			GUIContent content = EditorGUIUtility.TextContent(this.GetConnectedProfiler() + "|Specifies the target player for receiving profiler and log data.");
			Vector2 vector = EditorStyles.toolbarDropDown.CalcSize(content);
			Rect rect = GUILayoutUtility.GetRect(vector.x, vector.y);
			if (EditorGUI.DropdownButton(rect, content, FocusType.Passive, EditorStyles.toolbarDropDown))
			{
				List<ProfilerChoise> list = new List<ProfilerChoise>();
				list.Clear();
				AttachProfilerUI.AddPlayerProfilers(list);
				AttachProfilerUI.AddDeviceProfilers(list);
				AttachProfilerUI.AddLastIPProfiler(list);
				if (!ProfilerDriver.IsConnectionEditor())
				{
					if (!list.Any((ProfilerChoise p) => p.IsSelected()))
					{
						List<ProfilerChoise> arg_10B_0 = list;
						ProfilerChoise item = default(ProfilerChoise);
						item.Name = "(Autoconnected Player)";
						item.Enabled = false;
						item.IsSelected = (() => true);
						item.ConnectTo = delegate
						{
						};
						arg_10B_0.Add(item);
					}
				}
				this.AddEnterIPProfiler(list, GUIUtility.GUIToScreenRect(rect));
				this.OnGUIMenu(rect, list);
			}
		}

		protected virtual void OnGUIMenu(Rect connectRect, List<ProfilerChoise> profilers)
		{
			string[] options = (from p in profilers
			select p.Name).ToArray<string>();
			bool[] enabled = (from p in profilers
			select p.Enabled).ToArray<bool>();
			int num = profilers.FindIndex((ProfilerChoise p) => p.IsSelected());
			int[] selected;
			if (num == -1)
			{
				selected = new int[0];
			}
			else
			{
				selected = new int[]
				{
					num
				};
			}
			EditorUtility.DisplayCustomMenu(connectRect, options, enabled, selected, new EditorUtility.SelectMenuItemFunction(this.SelectProfilerClick), profilers);
		}
	}
}
