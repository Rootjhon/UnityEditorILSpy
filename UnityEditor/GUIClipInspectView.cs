using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class GUIClipInspectView : BaseInspectView
	{
		private Vector2 m_StacktraceScrollPos = default(Vector2);

		private List<IMGUIClipInstruction> m_ClipList = new List<IMGUIClipInstruction>();

		public GUIClipInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
		}

		public override void UpdateInstructions()
		{
			this.m_ClipList.Clear();
			GUIViewDebuggerHelper.GetClipInstructions(this.m_ClipList);
		}

		public override void ShowOverlay()
		{
			if (!this.isInstructionSelected)
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
			else
			{
				IMGUIClipInstruction iMGUIClipInstruction = this.m_ClipList[base.listViewState.row];
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, iMGUIClipInstruction.unclippedScreenRect, GUIStyle.none);
			}
		}

		protected override int GetInstructionCount()
		{
			return this.m_ClipList.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int id)
		{
			IMGUIClipInstruction iMGUIClipInstruction = this.m_ClipList[el.row];
			string instructionListName = this.GetInstructionListName(el.row);
			GUIContent content = GUIContent.Temp(instructionListName);
			Rect position = el.position;
			position.xMin += (float)(iMGUIClipInstruction.level * 12);
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(el.position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(position, content, id, base.listViewState.row == el.row);
		}

		protected override void DrawInspectedStacktrace()
		{
			IMGUIClipInstruction iMGUIClipInstruction = this.m_ClipList[base.listViewState.row];
			this.m_StacktraceScrollPos = EditorGUILayout.BeginScrollView(this.m_StacktraceScrollPos, GUIViewDebuggerWindow.Styles.stacktraceBackground, new GUILayoutOption[]
			{
				GUILayout.ExpandHeight(false)
			});
			base.DrawStackFrameList(iMGUIClipInstruction.pushStacktrace);
			EditorGUILayout.EndScrollView();
		}

		internal override void DoDrawSelectedInstructionDetails(int selectedInstructionIndex)
		{
			IMGUIClipInstruction iMGUIClipInstruction = this.m_ClipList[selectedInstructionIndex];
			base.DoSelectableInstructionDataField("RenderOffset", iMGUIClipInstruction.renderOffset.ToString());
			base.DoSelectableInstructionDataField("ResetOffset", iMGUIClipInstruction.resetOffset.ToString());
			base.DoSelectableInstructionDataField("screenRect", iMGUIClipInstruction.screenRect.ToString());
			base.DoSelectableInstructionDataField("scrollOffset", iMGUIClipInstruction.scrollOffset.ToString());
		}

		internal override string GetInstructionListName(int index)
		{
			StackFrame[] pushStacktrace = this.m_ClipList[index].pushStacktrace;
			string result;
			if (pushStacktrace.Length == 0)
			{
				result = "Empty";
			}
			else
			{
				int interestingFrameIndex = this.GetInterestingFrameIndex(pushStacktrace);
				StackFrame stackFrame = pushStacktrace[interestingFrameIndex];
				string methodName = stackFrame.methodName;
				result = methodName;
			}
			return result;
		}

		internal override void OnDoubleClickInstruction(int index)
		{
			throw new NotImplementedException();
		}

		internal override void OnSelectedInstructionChanged(int index)
		{
			base.listViewState.row = index;
			this.ShowOverlay();
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
					if (!stackFrame.signature.StartsWith("UnityEngine.GUIClip"))
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
			if (num != -1)
			{
				result = num;
				return result;
			}
			result = stacktrace.Length - 1;
			return result;
		}
	}
}
