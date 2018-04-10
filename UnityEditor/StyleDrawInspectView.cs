using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class StyleDrawInspectView : BaseInspectView
	{
		[Serializable]
		private class CachedInstructionInfo
		{
			public SerializedObject styleContainerSerializedObject = null;

			public SerializedProperty styleSerializedProperty = null;

			public readonly GUIStyleHolder styleContainer;

			public CachedInstructionInfo()
			{
				this.styleContainer = ScriptableObject.CreateInstance<GUIStyleHolder>();
			}
		}

		private Vector2 m_StacktraceScrollPos = default(Vector2);

		[NonSerialized]
		private List<IMGUIDrawInstruction> m_Instructions = new List<IMGUIDrawInstruction>();

		[NonSerialized]
		private IMGUIDrawInstruction m_Instruction;

		[NonSerialized]
		private StyleDrawInspectView.CachedInstructionInfo m_CachedInstructionInfo;

		protected override bool isInstructionSelected
		{
			get
			{
				return this.m_CachedInstructionInfo != null;
			}
		}

		public StyleDrawInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
		}

		public override void UpdateInstructions()
		{
			this.m_Instructions.Clear();
			GUIViewDebuggerHelper.GetDrawInstructions(this.m_Instructions);
		}

		public override void ClearRowSelection()
		{
			base.ClearRowSelection();
			this.m_CachedInstructionInfo = null;
		}

		public override void ShowOverlay()
		{
			if (this.m_CachedInstructionInfo != null)
			{
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, this.m_Instruction.rect, this.m_Instruction.usedGUIStyle);
			}
		}

		protected override int GetInstructionCount()
		{
			return this.m_Instructions.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int id)
		{
			string instructionListName = this.GetInstructionListName(el.row);
			GUIContent content = GUIContent.Temp(instructionListName);
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(el.position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(el.position, content, id, base.listViewState.row == el.row);
		}

		protected override void DrawInspectedStacktrace()
		{
			this.m_StacktraceScrollPos = EditorGUILayout.BeginScrollView(this.m_StacktraceScrollPos, GUIViewDebuggerWindow.Styles.stacktraceBackground, new GUILayoutOption[]
			{
				GUILayout.ExpandHeight(false)
			});
			base.DrawStackFrameList(this.m_Instruction.stackframes);
			EditorGUILayout.EndScrollView();
		}

		internal override void DoDrawSelectedInstructionDetails(int selectedInstructionIndex)
		{
			using (new EditorGUI.DisabledScope(true))
			{
				base.DrawInspectedRect(this.m_Instruction.rect);
			}
			base.DoSelectableInstructionDataField("VisibleRect", this.m_Instruction.visibleRect.ToString());
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(this.m_CachedInstructionInfo.styleSerializedProperty, GUIContent.Temp("Style"), true, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_CachedInstructionInfo.styleContainerSerializedObject.ApplyModifiedPropertiesWithoutUndo();
				base.debuggerWindow.inspected.Repaint();
			}
			GUILayout.Label(GUIContent.Temp("GUIContent"), new GUILayoutOption[0]);
			using (new EditorGUI.IndentLevelScope())
			{
				base.DoSelectableInstructionDataField("Text", this.m_Instruction.usedGUIContent.text);
				base.DoSelectableInstructionDataField("Tooltip", this.m_Instruction.usedGUIContent.tooltip);
				using (new EditorGUI.DisabledScope(true))
				{
					EditorGUILayout.ObjectField("Icon", this.m_Instruction.usedGUIContent.image, typeof(Texture2D), false, new GUILayoutOption[0]);
				}
			}
		}

		internal override string GetInstructionListName(int index)
		{
			string instructionListName = this.GetInstructionListName(this.m_Instructions[index].stackframes);
			return string.Format("{0}. {1}", index, instructionListName);
		}

		private string GetInstructionListName(StackFrame[] stacktrace)
		{
			int num = this.GetInterestingFrameIndex(stacktrace);
			if (num > 0)
			{
				num--;
			}
			StackFrame stackFrame = stacktrace[num];
			return stackFrame.methodName;
		}

		internal override void OnDoubleClickInstruction(int index)
		{
			this.ShowInstructionInExternalEditor(this.m_Instructions[index].stackframes);
		}

		internal override void OnSelectedInstructionChanged(int index)
		{
			base.listViewState.row = index;
			if (base.listViewState.row >= 0)
			{
				if (this.m_CachedInstructionInfo == null)
				{
					this.m_CachedInstructionInfo = new StyleDrawInspectView.CachedInstructionInfo();
				}
				this.m_Instruction = this.m_Instructions[base.listViewState.row];
				this.m_CachedInstructionInfo.styleContainer.inspectedStyle = this.m_Instruction.usedGUIStyle;
				this.m_CachedInstructionInfo.styleContainerSerializedObject = null;
				this.m_CachedInstructionInfo.styleSerializedProperty = null;
				this.GetSelectedStyleProperty(out this.m_CachedInstructionInfo.styleContainerSerializedObject, out this.m_CachedInstructionInfo.styleSerializedProperty);
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, this.m_Instruction.rect, this.m_Instruction.usedGUIStyle);
			}
			else
			{
				this.m_CachedInstructionInfo = null;
				base.debuggerWindow.ClearInstructionHighlighter();
			}
		}

		private int GetInterestingFrameIndex(StackFrame[] stacktrace)
		{
			string dataPath = Application.dataPath;
			int num = -1;
			int result;
			for (int i = 0; i < stacktrace.Length; i++)
			{
				StackFrame stackFrame = stacktrace[i];
				if (!string.IsNullOrEmpty(stackFrame.sourceFile))
				{
					if (!stackFrame.signature.StartsWith("UnityEngine.GUI"))
					{
						if (!stackFrame.signature.StartsWith("UnityEditor.EditorGUI"))
						{
							if (num == -1)
							{
								num = i;
							}
							if (stackFrame.sourceFile.StartsWith(dataPath))
							{
								result = i;
								return result;
							}
						}
					}
				}
			}
			if (num != -1)
			{
				result = num;
				return result;
			}
			result = stacktrace.Length - 1;
			return result;
		}

		private void GetSelectedStyleProperty(out SerializedObject serializedObject, out SerializedProperty styleProperty)
		{
			GUISkin gUISkin = null;
			GUISkin current = GUISkin.current;
			GUIStyle gUIStyle = current.FindStyle(this.m_Instruction.usedGUIStyle.name);
			if (gUIStyle != null && gUIStyle == this.m_Instruction.usedGUIStyle)
			{
				gUISkin = current;
			}
			styleProperty = null;
			if (gUISkin != null)
			{
				serializedObject = new SerializedObject(gUISkin);
				SerializedProperty iterator = serializedObject.GetIterator();
				bool enterChildren = true;
				while (iterator.NextVisible(enterChildren))
				{
					if (iterator.type == "GUIStyle")
					{
						enterChildren = false;
						SerializedProperty serializedProperty = iterator.FindPropertyRelative("m_Name");
						if (serializedProperty.stringValue == this.m_Instruction.usedGUIStyle.name)
						{
							styleProperty = iterator;
							return;
						}
					}
					else
					{
						enterChildren = true;
					}
				}
				Debug.Log(string.Format("Showing editable Style from GUISkin: {0}, IsPersistant: {1}", gUISkin.name, EditorUtility.IsPersistent(gUISkin)));
			}
			serializedObject = new SerializedObject(this.m_CachedInstructionInfo.styleContainer);
			styleProperty = serializedObject.FindProperty("inspectedStyle");
		}

		private void ShowInstructionInExternalEditor(StackFrame[] frames)
		{
			int interestingFrameIndex = this.GetInterestingFrameIndex(frames);
			StackFrame stackFrame = frames[interestingFrameIndex];
			InternalEditorUtility.OpenFileAtLineExternal(stackFrame.sourceFile, (int)stackFrame.lineNumber);
		}
	}
}
