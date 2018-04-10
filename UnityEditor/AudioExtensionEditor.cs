using System;
using UnityEngine;

namespace UnityEditor
{
	internal class AudioExtensionEditor : ScriptableObject
	{
		public struct ExtensionPropertyInfo
		{
			public PropertyName propertyName;

			public float defaultValue;

			public SerializedProperty serializedProperty;

			public ExtensionPropertyInfo(string nameIn, float defaultValueIn)
			{
				this.propertyName = new PropertyName(nameIn);
				this.defaultValue = defaultValueIn;
				this.serializedProperty = null;
			}
		}

		private bool foundAllExtensionProperties = false;

		protected AudioExtensionEditor.ExtensionPropertyInfo[] m_ExtensionProperties;

		public virtual void InitExtensionPropertyInfo()
		{
		}

		protected virtual int GetNumSerializedExtensionProperties(UnityEngine.Object obj)
		{
			return 0;
		}

		public void OnEnable()
		{
			this.InitExtensionPropertyInfo();
		}

		public int GetNumExtensionProperties()
		{
			return this.m_ExtensionProperties.Length;
		}

		public PropertyName GetExtensionPropertyName(int index)
		{
			return this.m_ExtensionProperties[index].propertyName;
		}

		public float GetExtensionPropertyDefaultValue(int index)
		{
			return this.m_ExtensionProperties[index].defaultValue;
		}

		public bool FindAudioExtensionProperties(SerializedObject serializedObject)
		{
			SerializedProperty serializedProperty = null;
			if (serializedObject != null)
			{
				serializedProperty = serializedObject.FindProperty("m_ExtensionPropertyValues");
			}
			bool result;
			if (serializedProperty == null)
			{
				this.foundAllExtensionProperties = false;
				result = false;
			}
			else
			{
				int num = serializedProperty.arraySize;
				if (serializedProperty.hasMultipleDifferentValues)
				{
					num = this.GetMinNumSerializedExtensionProperties(serializedObject);
				}
				if (serializedProperty == null || num == 0)
				{
					this.foundAllExtensionProperties = false;
					result = false;
				}
				else
				{
					if (!this.foundAllExtensionProperties && serializedObject != null)
					{
						int num2 = 0;
						for (int i = 0; i < num; i++)
						{
							SerializedProperty arrayElementAtIndex = serializedProperty.GetArrayElementAtIndex(i);
							if (arrayElementAtIndex != null)
							{
								SerializedProperty serializedProperty2 = arrayElementAtIndex.FindPropertyRelative("propertyName");
								for (int j = 0; j < this.m_ExtensionProperties.Length; j++)
								{
									if (this.m_ExtensionProperties[j].propertyName == serializedProperty2.stringValue && !serializedProperty2.hasMultipleDifferentValues)
									{
										this.m_ExtensionProperties[j].serializedProperty = arrayElementAtIndex.FindPropertyRelative("propertyValue");
										num2++;
									}
								}
							}
						}
						this.foundAllExtensionProperties = (num2 == this.m_ExtensionProperties.Length);
					}
					result = this.foundAllExtensionProperties;
				}
			}
			return result;
		}

		protected static void PropertyFieldAsBool(SerializedProperty property, GUIContent title)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			title = EditorGUI.BeginProperty(controlRect, title, property);
			EditorGUI.BeginChangeCheck();
			bool flag = EditorGUI.Toggle(controlRect, title, property.floatValue > 0f);
			if (EditorGUI.EndChangeCheck())
			{
				property.floatValue = ((!flag) ? 0f : 1f);
			}
			EditorGUI.EndProperty();
		}

		private int GetMinNumSerializedExtensionProperties(SerializedObject serializedObject)
		{
			UnityEngine.Object[] targetObjects = serializedObject.targetObjects;
			int num = (targetObjects.Length <= 0) ? 0 : 2147483647;
			for (int i = 0; i < targetObjects.Length; i++)
			{
				num = Math.Min(num, this.GetNumSerializedExtensionProperties(targetObjects[i]));
			}
			return num;
		}
	}
}
