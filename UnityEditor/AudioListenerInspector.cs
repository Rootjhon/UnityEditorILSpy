using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(AudioListener))]
	internal class AudioListenerInspector : Editor
	{
		private AudioListenerExtensionEditor m_SpatializerEditor = null;

		private bool m_AddSpatializerExtension = false;

		private bool m_AddSpatializerExtensionMixedValues = false;

		private GUIContent addSpatializerExtensionLabel = new GUIContent("Override Spatializer Settings", "Override the Google spatializer's default settings.");

		private void OnEnable()
		{
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
			this.UpdateSpatializerExtensionMixedValues();
			if (this.m_AddSpatializerExtension)
			{
				this.CreateExtensionEditors();
			}
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			bool flag = (this.m_AddSpatializerExtension && !this.m_AddSpatializerExtensionMixedValues) || !base.serializedObject.isEditingMultipleObjects;
			if (AudioExtensionManager.IsListenerSpatializerExtensionRegistered() && flag)
			{
				EditorGUI.showMixedValue = this.m_AddSpatializerExtensionMixedValues;
				bool flag2 = EditorGUILayout.Toggle(this.addSpatializerExtensionLabel, this.m_AddSpatializerExtension, new GUILayoutOption[0]);
				EditorGUI.showMixedValue = false;
				bool flag3 = false;
				if (this.m_AddSpatializerExtension != flag2)
				{
					this.m_AddSpatializerExtension = flag2;
					if (this.m_AddSpatializerExtension)
					{
						this.CreateExtensionEditors();
						if (this.m_SpatializerEditor != null)
						{
							flag3 = this.m_SpatializerEditor.FindAudioExtensionProperties(base.serializedObject);
						}
					}
					else
					{
						this.ClearExtensionProperties();
						this.DestroyExtensionEditors();
						flag3 = false;
					}
				}
				else if (this.m_SpatializerEditor != null)
				{
					flag3 = this.m_SpatializerEditor.FindAudioExtensionProperties(base.serializedObject);
					if (!flag3)
					{
						this.m_AddSpatializerExtension = false;
						this.ClearExtensionProperties();
						this.DestroyExtensionEditors();
					}
				}
				if (this.m_SpatializerEditor != null && flag3)
				{
					EditorGUI.indentLevel++;
					this.m_SpatializerEditor.OnAudioListenerGUI();
					EditorGUI.indentLevel--;
					for (int i = 0; i < base.targets.Length; i++)
					{
						AudioListener audioListener = base.targets[i] as AudioListener;
						if (audioListener != null)
						{
							AudioListenerExtension spatializerExtension = AudioExtensionManager.GetSpatializerExtension(audioListener);
							if (spatializerExtension != null)
							{
								string name = AudioExtensionManager.GetListenerSpatializerExtensionType().Name;
								for (int j = 0; j < this.m_SpatializerEditor.GetNumExtensionProperties(); j++)
								{
									PropertyName extensionPropertyName = this.m_SpatializerEditor.GetExtensionPropertyName(j);
									float propertyValue = 0f;
									if (audioListener.ReadExtensionProperty(name, extensionPropertyName, ref propertyValue))
									{
										spatializerExtension.WriteExtensionProperty(extensionPropertyName, propertyValue);
									}
								}
							}
						}
					}
				}
			}
			base.serializedObject.ApplyModifiedProperties();
		}

		private void OnDisable()
		{
			this.DestroyExtensionEditors();
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.UndoRedoPerformed));
		}

		private void UpdateSpatializerExtensionMixedValues()
		{
			this.m_AddSpatializerExtension = false;
			int num = 0;
			for (int i = 0; i < base.targets.Length; i++)
			{
				AudioListener audioListener = base.targets[i] as AudioListener;
				if (audioListener != null)
				{
					Type listenerSpatializerExtensionType = AudioExtensionManager.GetListenerSpatializerExtensionType();
					if (listenerSpatializerExtensionType != null && audioListener.GetNumExtensionPropertiesForThisExtension(listenerSpatializerExtensionType.Name) > 0)
					{
						this.m_AddSpatializerExtension = true;
						num++;
					}
				}
			}
			this.m_AddSpatializerExtensionMixedValues = (num != 0 && num != base.targets.Length);
			if (this.m_AddSpatializerExtensionMixedValues)
			{
				this.m_AddSpatializerExtension = false;
			}
		}

		private void CreateExtensionEditors()
		{
			if (this.m_SpatializerEditor != null)
			{
				this.DestroyExtensionEditors();
			}
			Type listenerSpatializerExtensionEditorType = AudioExtensionManager.GetListenerSpatializerExtensionEditorType();
			this.m_SpatializerEditor = (ScriptableObject.CreateInstance(listenerSpatializerExtensionEditorType) as AudioListenerExtensionEditor);
			if (this.m_SpatializerEditor != null)
			{
				for (int i = 0; i < base.targets.Length; i++)
				{
					AudioListener audioListener = base.targets[i] as AudioListener;
					if (audioListener != null)
					{
						Undo.RecordObject(audioListener, "Add AudioListener extension properties");
						PropertyName listenerSpatializerExtensionName = AudioExtensionManager.GetListenerSpatializerExtensionName();
						for (int j = 0; j < this.m_SpatializerEditor.GetNumExtensionProperties(); j++)
						{
							PropertyName extensionPropertyName = this.m_SpatializerEditor.GetExtensionPropertyName(j);
							float propertyValue = 0f;
							if (!audioListener.ReadExtensionProperty(listenerSpatializerExtensionName, extensionPropertyName, ref propertyValue))
							{
								propertyValue = this.m_SpatializerEditor.GetExtensionPropertyDefaultValue(j);
								audioListener.WriteExtensionProperty(AudioExtensionManager.GetSpatializerName(), listenerSpatializerExtensionName, extensionPropertyName, propertyValue);
							}
						}
					}
				}
			}
			this.m_AddSpatializerExtensionMixedValues = false;
		}

		private void DestroyExtensionEditors()
		{
			UnityEngine.Object.DestroyImmediate(this.m_SpatializerEditor);
			this.m_SpatializerEditor = null;
		}

		private void ClearExtensionProperties()
		{
			for (int i = 0; i < base.targets.Length; i++)
			{
				AudioListener audioListener = base.targets[i] as AudioListener;
				if (audioListener != null)
				{
					Undo.RecordObject(audioListener, "Remove AudioListener extension properties");
					audioListener.ClearExtensionProperties(AudioExtensionManager.GetListenerSpatializerExtensionName());
				}
			}
			this.m_AddSpatializerExtensionMixedValues = false;
		}

		private void UndoRedoPerformed()
		{
			this.DestroyExtensionEditors();
			this.UpdateSpatializerExtensionMixedValues();
			if (!this.m_AddSpatializerExtension && !this.m_AddSpatializerExtensionMixedValues)
			{
				this.ClearExtensionProperties();
			}
			if (this.m_AddSpatializerExtension)
			{
				this.CreateExtensionEditors();
			}
			base.Repaint();
		}
	}
}
