using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal static class MaskFieldGUI
	{
		private class MaskCallbackInfo
		{
			public static MaskFieldGUI.MaskCallbackInfo m_Instance;

			private const string kMaskMenuChangedMessage = "MaskMenuChanged";

			private readonly int m_ControlID;

			private int m_NewMask;

			private readonly GUIView m_SourceView;

			public MaskCallbackInfo(int controlID)
			{
				this.m_ControlID = controlID;
				this.m_SourceView = GUIView.current;
			}

			public static int GetSelectedValueForControl(int controlID, int mask, out int changedFlags, out bool changedToValue)
			{
				Event current = Event.current;
				changedFlags = 0;
				changedToValue = false;
				int result;
				if (current.type == EventType.ExecuteCommand && current.commandName == "MaskMenuChanged")
				{
					if (MaskFieldGUI.MaskCallbackInfo.m_Instance == null)
					{
						Debug.LogError("Mask menu has no instance");
						result = mask;
						return result;
					}
					if (MaskFieldGUI.MaskCallbackInfo.m_Instance.m_ControlID == controlID)
					{
						changedFlags = (mask ^ MaskFieldGUI.MaskCallbackInfo.m_Instance.m_NewMask);
						changedToValue = ((MaskFieldGUI.MaskCallbackInfo.m_Instance.m_NewMask & changedFlags) != 0);
						if (changedFlags != 0)
						{
							mask = MaskFieldGUI.MaskCallbackInfo.m_Instance.m_NewMask;
							GUI.changed = true;
						}
						MaskFieldGUI.MaskCallbackInfo.m_Instance = null;
						current.Use();
					}
				}
				result = mask;
				return result;
			}

			internal void SetMaskValueDelegate(object userData, string[] options, int selected)
			{
				int[] array = (int[])userData;
				this.m_NewMask = array[selected];
				if (this.m_SourceView)
				{
					this.m_SourceView.SendEvent(EditorGUIUtility.CommandEvent("MaskMenuChanged"));
				}
			}
		}

		private static readonly List<string[]> s_OptionNames = new List<string[]>();

		private static readonly List<int[]> s_OptionValues = new List<int[]>();

		private static readonly List<int[]> s_SelectedOptions = new List<int[]>();

		private static readonly HashSet<int> s_SelectedOptionsSet = new HashSet<int>();

		internal static int DoMaskField(Rect position, int controlID, int mask, string[] flagNames, GUIStyle style)
		{
			int num;
			bool flag;
			return MaskFieldGUI.DoMaskField(position, controlID, mask, flagNames, style, out num, out flag);
		}

		internal static int DoMaskField(Rect position, int controlID, int mask, string[] flagNames, int[] flagValues, GUIStyle style)
		{
			int num;
			bool flag;
			return MaskFieldGUI.DoMaskField(position, controlID, mask, flagNames, flagValues, style, out num, out flag);
		}

		internal static int DoMaskField(Rect position, int controlID, int mask, string[] flagNames, GUIStyle style, out int changedFlags, out bool changedToValue)
		{
			int[] array = new int[flagNames.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = 1 << i;
			}
			return MaskFieldGUI.DoMaskField(position, controlID, mask, flagNames, array, style, out changedFlags, out changedToValue);
		}

		internal static int DoMaskField(Rect position, int controlID, int mask, string[] flagNames, int[] flagValues, GUIStyle style, out int changedFlags, out bool changedToValue)
		{
			mask = MaskFieldGUI.MaskCallbackInfo.GetSelectedValueForControl(controlID, mask, out changedFlags, out changedToValue);
			string t;
			string[] options;
			int[] array;
			int[] array2;
			MaskFieldGUI.GetMenuOptions(mask, flagNames, flagValues, out t, out options, out array, out array2);
			Event current = Event.current;
			if (current.type == EventType.Repaint)
			{
				GUIContent content = (!EditorGUI.showMixedValue) ? EditorGUIUtility.TempContent(t) : EditorGUI.mixedValueContent;
				style.Draw(position, content, controlID, false);
			}
			else if ((current.type == EventType.MouseDown && position.Contains(current.mousePosition)) || current.MainActionKeyForControl(controlID))
			{
				MaskFieldGUI.MaskCallbackInfo.m_Instance = new MaskFieldGUI.MaskCallbackInfo(controlID);
				current.Use();
				EditorUtility.DisplayCustomMenu(position, options, (!EditorGUI.showMixedValue) ? array2 : new int[0], new EditorUtility.SelectMenuItemFunction(MaskFieldGUI.MaskCallbackInfo.m_Instance.SetMaskValueDelegate), array.Clone());
				GUIUtility.keyboardControl = controlID;
			}
			return mask;
		}

		private static T[] GetBuffer<T>(List<T[]> pool, int bufferLength)
		{
			for (int i = pool.Count; i <= bufferLength; i++)
			{
				pool.Add(null);
			}
			if (pool[bufferLength] == null)
			{
				pool[bufferLength] = new T[bufferLength];
			}
			T[] array = pool[bufferLength];
			int j = 0;
			int num = array.Length;
			while (j < num)
			{
				array[j] = default(T);
				j++;
			}
			return array;
		}

		internal static void GetMenuOptions(int mask, string[] flagNames, int[] flagValues, out string buttonText, out string[] optionNames, out int[] optionMaskValues, out int[] selectedOptions)
		{
			bool flag = flagValues[0] == 0;
			bool flag2 = flagValues[flagValues.Length - 1] == -1;
			string text = (!flag) ? "Nothing" : flagNames[0];
			string text2 = (!flag2) ? "Everything" : flagNames[flagValues.Length - 1];
			int bufferLength = flagNames.Length + ((!flag) ? 1 : 0) + ((!flag2) ? 1 : 0);
			int num = flagNames.Length - ((!flag) ? 0 : 1) - ((!flag2) ? 0 : 1);
			int num2 = (!flag) ? 0 : 1;
			int num3 = num2 + num;
			buttonText = "Mixed ...";
			if (mask == 0)
			{
				buttonText = text;
			}
			else if (mask == -1)
			{
				buttonText = text2;
			}
			else
			{
				for (int i = num2; i < num3; i++)
				{
					if (mask == flagValues[i])
					{
						buttonText = flagNames[i];
					}
				}
			}
			optionNames = MaskFieldGUI.GetBuffer<string>(MaskFieldGUI.s_OptionNames, bufferLength);
			optionNames[0] = text;
			optionNames[1] = text2;
			for (int j = num2; j < num3; j++)
			{
				int num4 = j - num2 + 2;
				optionNames[num4] = flagNames[j];
			}
			int num5 = 0;
			int num6 = 0;
			MaskFieldGUI.s_SelectedOptionsSet.Clear();
			if (mask == 0)
			{
				MaskFieldGUI.s_SelectedOptionsSet.Add(0);
			}
			if (mask == -1)
			{
				MaskFieldGUI.s_SelectedOptionsSet.Add(1);
			}
			for (int k = num2; k < num3; k++)
			{
				int num7 = flagValues[k];
				num5 |= num7;
				if ((mask & num7) == num7)
				{
					int item = k - num2 + 2;
					MaskFieldGUI.s_SelectedOptionsSet.Add(item);
					num6 |= num7;
				}
			}
			selectedOptions = MaskFieldGUI.GetBuffer<int>(MaskFieldGUI.s_SelectedOptions, MaskFieldGUI.s_SelectedOptionsSet.Count);
			int num8 = 0;
			foreach (int current in MaskFieldGUI.s_SelectedOptionsSet)
			{
				selectedOptions[num8] = current;
				num8++;
			}
			optionMaskValues = MaskFieldGUI.GetBuffer<int>(MaskFieldGUI.s_OptionValues, bufferLength);
			optionMaskValues[0] = 0;
			optionMaskValues[1] = -1;
			for (int l = num2; l < num3; l++)
			{
				int num9 = l - num2 + 2;
				int num10 = flagValues[l];
				bool flag3 = (num6 & num10) == num10;
				int num11 = (!flag3) ? (num6 | num10) : (num6 & ~num10);
				if (num11 == num5)
				{
					num11 = -1;
				}
				optionMaskValues[num9] = num11;
			}
		}
	}
}
