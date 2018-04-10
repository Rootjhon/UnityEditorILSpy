using System;

namespace UnityEditor.Experimental.UIElements
{
	[LibraryFolderPath("UIElements/EditorWindows")]
	internal class EditorWindowPersistentViewData : ScriptableSingletonDictionary<EditorWindowPersistentViewData, SerializableJsonDictionary>
	{
		public static SerializableJsonDictionary GetEditorData(EditorWindow window)
		{
			string preferencesFileName = window.GetType().ToString();
			return ScriptableSingletonDictionary<EditorWindowPersistentViewData, SerializableJsonDictionary>.instance[preferencesFileName];
		}
	}
}
