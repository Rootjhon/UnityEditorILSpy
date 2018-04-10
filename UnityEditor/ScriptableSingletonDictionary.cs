using System;
using System.IO;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal abstract class ScriptableSingletonDictionary<TDerived, TValue> : ScriptableObject where TDerived : ScriptableObject where TValue : ScriptableObject
	{
		private static TDerived s_Instance;

		private static readonly string k_Extension = ".pref";

		protected string m_PreferencesFileName;

		public static TDerived instance
		{
			get
			{
				if (ScriptableSingletonDictionary<TDerived, TValue>.s_Instance == null)
				{
					ScriptableSingletonDictionary<TDerived, TValue>.s_Instance = ScriptableObject.CreateInstance<TDerived>();
					ScriptableSingletonDictionary<TDerived, TValue>.s_Instance.hideFlags = HideFlags.HideAndDontSave;
				}
				return ScriptableSingletonDictionary<TDerived, TValue>.s_Instance;
			}
		}

		public TValue this[string preferencesFileName]
		{
			get
			{
				return this.Load(preferencesFileName);
			}
		}

		private TValue CreateNewValue()
		{
			TValue result = ScriptableObject.CreateInstance<TValue>();
			result.hideFlags |= HideFlags.HideAndDontSave;
			return result;
		}

		private string GetProjectRelativePath(string file)
		{
			return this.GetFolderPath() + "/" + file + ScriptableSingletonDictionary<TDerived, TValue>.k_Extension;
		}

		public void Save(string preferencesFileName, TValue value)
		{
			if (!string.IsNullOrEmpty(preferencesFileName) && !(value == null))
			{
				if (!string.IsNullOrEmpty(preferencesFileName))
				{
					string path = Application.dataPath + "/../" + this.GetFolderPath();
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
					InternalEditorUtility.SaveToSerializedFileAndForget(new TValue[]
					{
						value
					}, this.GetProjectRelativePath(preferencesFileName), true);
				}
			}
		}

		public void Clear(string preferencesFileName)
		{
			string path = Application.dataPath + "/../" + this.GetProjectRelativePath(preferencesFileName);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		private TValue Load(string preferencesFileName)
		{
			TValue tValue = (TValue)((object)null);
			if (!string.IsNullOrEmpty(preferencesFileName))
			{
				UnityEngine.Object[] array = InternalEditorUtility.LoadSerializedFileAndForget(this.GetProjectRelativePath(preferencesFileName));
				if (array != null && array.Length > 0)
				{
					tValue = (array[0] as TValue);
					if (tValue != null)
					{
						tValue.hideFlags |= HideFlags.HideAndDontSave;
					}
				}
			}
			this.m_PreferencesFileName = preferencesFileName;
			TValue arg_8A_0;
			if ((arg_8A_0 = tValue) == null)
			{
				arg_8A_0 = this.CreateNewValue();
			}
			return arg_8A_0;
		}

		private string GetFolderPath()
		{
			Type type = base.GetType();
			object[] customAttributes = type.GetCustomAttributes(true);
			object[] array = customAttributes;
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array[i];
				if (obj is LibraryFolderPathAttribute)
				{
					LibraryFolderPathAttribute libraryFolderPathAttribute = obj as LibraryFolderPathAttribute;
					return libraryFolderPathAttribute.folderPath;
				}
			}
			throw new ArgumentException("The LibraryFolderPathAttribute[] attribute is required for this class.");
		}
	}
}
