using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityEditor
{
	internal class LocalizedEditorFontManager
	{
		private class FontSetting
		{
			private string[] m_fontNames;

			public string[] fontNames
			{
				get
				{
					return this.m_fontNames;
				}
			}

			public FontSetting(string[] fontNames)
			{
				this.m_fontNames = fontNames;
			}
		}

		private class FontDictionary
		{
			private Dictionary<string, LocalizedEditorFontManager.FontSetting> m_dictionary;

			public LocalizedEditorFontManager.FontSetting this[string key]
			{
				get
				{
					return this.m_dictionary[this.trim_key(key)];
				}
			}

			public FontDictionary()
			{
				this.m_dictionary = new Dictionary<string, LocalizedEditorFontManager.FontSetting>();
			}

			public void Add(string key, LocalizedEditorFontManager.FontSetting value)
			{
				this.m_dictionary.Add(key, value);
			}

			private string trim_key(string key)
			{
				int length = key.IndexOf(" (UnityEngine.Font)");
				return key.Substring(0, length);
			}

			public bool ContainsKey(string key)
			{
				return this.m_dictionary.ContainsKey(this.trim_key(key));
			}
		}

		private static Dictionary<SystemLanguage, LocalizedEditorFontManager.FontDictionary> m_fontDictionaries;

		private static LocalizedEditorFontManager.FontDictionary GetFontDictionary(SystemLanguage language)
		{
			LocalizedEditorFontManager.FontDictionary result;
			if (!LocalizedEditorFontManager.m_fontDictionaries.ContainsKey(language))
			{
				result = null;
			}
			else
			{
				result = LocalizedEditorFontManager.m_fontDictionaries[language];
			}
			return result;
		}

		private static void ReadFontSettings()
		{
			if (LocalizedEditorFontManager.m_fontDictionaries == null)
			{
				LocalizedEditorFontManager.m_fontDictionaries = new Dictionary<SystemLanguage, LocalizedEditorFontManager.FontDictionary>();
			}
			LocalizedEditorFontManager.m_fontDictionaries.Clear();
			string text = null;
			if (text == null || !File.Exists(text))
			{
				text = LocalizationDatabase.GetLocalizationResourceFolder() + "/fontsettings.txt";
			}
			if (File.Exists(text))
			{
				byte[] bytes = File.ReadAllBytes(text);
				string @string = Encoding.UTF8.GetString(bytes);
				string[] array = @string.Split(new char[]
				{
					'\n'
				});
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string text2 = array2[i];
					string text3 = text2;
					text3 = text3.Split(new char[]
					{
						'#'
					})[0];
					text3 = text3.Trim();
					if (text3.Length > 0)
					{
						string[] array3 = text3.Split(new char[]
						{
							'|'
						});
						if (array3.Length != 2)
						{
							Debug.LogError("wrong format for the fontsettings.txt.");
						}
						else
						{
							SystemLanguage key = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), array3[0].Trim());
							string[] array4 = array3[1].Split(new char[]
							{
								'='
							});
							if (array4.Length != 2)
							{
								Debug.LogError("wrong format for the fontsettings.txt.");
							}
							else
							{
								string key2 = array4[0].Trim();
								string[] array5 = array4[1].Split(new char[]
								{
									','
								});
								for (int j = 0; j < array5.Length; j++)
								{
									array5[j] = array5[j].Trim();
								}
								if (!LocalizedEditorFontManager.m_fontDictionaries.ContainsKey(key))
								{
									LocalizedEditorFontManager.m_fontDictionaries.Add(key, new LocalizedEditorFontManager.FontDictionary());
								}
								LocalizedEditorFontManager.m_fontDictionaries[key].Add(key2, new LocalizedEditorFontManager.FontSetting(array5));
							}
						}
					}
				}
			}
		}

		private static void ModifyFont(Font font, LocalizedEditorFontManager.FontDictionary dict)
		{
			string text = font.ToString();
			if (dict.ContainsKey(text))
			{
				font.fontNames = dict[text].fontNames;
			}
			else
			{
				Debug.LogError("no matching for:" + text);
			}
		}

		private static void UpdateSkinFontInternal(GUISkin skin, LocalizedEditorFontManager.FontDictionary dict)
		{
			if (!(skin == null))
			{
				IEnumerator enumerator = skin.GetEnumerator();
				while (enumerator.MoveNext())
				{
					GUIStyle gUIStyle = enumerator.Current as GUIStyle;
					if (gUIStyle != null && gUIStyle.font != null)
					{
						LocalizedEditorFontManager.ModifyFont(gUIStyle.font, dict);
					}
				}
			}
		}

		public static void UpdateSkinFont(SystemLanguage language)
		{
			LocalizedEditorFontManager.ReadFontSettings();
			LocalizedEditorFontManager.FontDictionary fontDictionary = LocalizedEditorFontManager.GetFontDictionary(language);
			if (fontDictionary != null)
			{
				LocalizedEditorFontManager.UpdateSkinFontInternal(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector), fontDictionary);
				LocalizedEditorFontManager.UpdateSkinFontInternal(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene), fontDictionary);
			}
		}

		public static void UpdateSkinFont()
		{
			LocalizedEditorFontManager.UpdateSkinFont(LocalizationDatabase.GetCurrentEditorLanguage());
		}

		private static void OnBoot()
		{
			LocalizedEditorFontManager.UpdateSkinFont();
		}
	}
}
