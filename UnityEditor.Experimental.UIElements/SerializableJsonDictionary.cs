using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	internal class SerializableJsonDictionary : ScriptableObject, ISerializationCallbackReceiver, ISerializableJsonDictionary
	{
		[SerializeField]
		private List<string> m_Keys = new List<string>();

		[SerializeField]
		private List<string> m_Values = new List<string>();

		[NonSerialized]
		private Dictionary<string, object> m_Dict = new Dictionary<string, object>();

		public void Set<T>(string key, T value) where T : class
		{
			this.m_Dict[key] = value;
		}

		public T Get<T>(string key) where T : class
		{
			T result;
			if (!this.ContainsKey(key))
			{
				result = (T)((object)null);
			}
			else
			{
				if (this.m_Dict[key] is string)
				{
					T t = Activator.CreateInstance<T>();
					EditorJsonUtility.FromJsonOverwrite((string)this.m_Dict[key], t);
					this.m_Dict[key] = t;
				}
				result = (this.m_Dict[key] as T);
			}
			return result;
		}

		public T GetScriptable<T>(string key) where T : ScriptableObject
		{
			T result;
			if (!this.ContainsKey(key))
			{
				result = (T)((object)null);
			}
			else
			{
				if (this.m_Dict[key] is string)
				{
					T t = ScriptableObject.CreateInstance<T>();
					EditorJsonUtility.FromJsonOverwrite((string)this.m_Dict[key], t);
					this.m_Dict[key] = t;
				}
				result = (this.m_Dict[key] as T);
			}
			return result;
		}

		public void Overwrite(object obj, string key)
		{
			if (this.ContainsKey(key))
			{
				if (this.m_Dict[key] is string)
				{
					EditorJsonUtility.FromJsonOverwrite((string)this.m_Dict[key], obj);
					this.m_Dict[key] = obj;
				}
				else if (this.m_Dict[key] != obj)
				{
					string json = EditorJsonUtility.ToJson(this.m_Dict[key]);
					EditorJsonUtility.FromJsonOverwrite(json, obj);
					this.m_Dict[key] = obj;
				}
			}
		}

		public bool ContainsKey(string key)
		{
			return this.m_Dict.ContainsKey(key);
		}

		public void OnBeforeSerialize()
		{
			this.m_Keys.Clear();
			this.m_Values.Clear();
			foreach (KeyValuePair<string, object> current in this.m_Dict)
			{
				if (current.Key != null && current.Value != null)
				{
					this.m_Keys.Add(current.Key);
					this.m_Values.Add(EditorJsonUtility.ToJson(current.Value));
				}
			}
		}

		public void OnAfterDeserialize()
		{
			if (this.m_Keys.Count == this.m_Values.Count)
			{
				this.m_Dict = Enumerable.Range(0, this.m_Keys.Count).ToDictionary((int i) => this.m_Keys[i], (int i) => this.m_Values[i]);
			}
			this.m_Keys.Clear();
			this.m_Values.Clear();
		}
	}
}
