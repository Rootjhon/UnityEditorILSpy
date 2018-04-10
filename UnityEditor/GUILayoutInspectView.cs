using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class GUILayoutInspectView : BaseInspectView
	{
		private Vector2 m_StacktraceScrollPos = default(Vector2);

		private readonly List<IMGUILayoutInstruction> m_LayoutInstructions = new List<IMGUILayoutInstruction>();

		private GUIStyle m_FakeMarginStyleForOverlay = new GUIStyle();

		public GUILayoutInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
		}

		public override void UpdateInstructions()
		{
			this.m_LayoutInstructions.Clear();
			GUIViewDebuggerHelper.GetLayoutInstructions(this.m_LayoutInstructions);
		}

		public override void ShowOverlay()
		{
			if (!this.isInstructionSelected)
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
			else
			{
				IMGUILayoutInstruction iMGUILayoutInstruction = this.m_LayoutInstructions[base.listViewState.row];
				RectOffset rectOffset = new RectOffset();
				rectOffset.left = iMGUILayoutInstruction.marginLeft;
				rectOffset.right = iMGUILayoutInstruction.marginRight;
				rectOffset.top = iMGUILayoutInstruction.marginTop;
				rectOffset.bottom = iMGUILayoutInstruction.marginBottom;
				this.m_FakeMarginStyleForOverlay.padding = rectOffset;
				Rect rect = iMGUILayoutInstruction.unclippedRect;
				rect = rectOffset.Add(rect);
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, rect, this.m_FakeMarginStyleForOverlay);
			}
		}

		protected override int GetInstructionCount()
		{
			return this.m_LayoutInstructions.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int id)
		{
			IMGUILayoutInstruction iMGUILayoutInstruction = this.m_LayoutInstructions[el.row];
			GUIContent content = GUIContent.Temp(this.GetInstructionListName(el.row));
			Rect position = el.position;
			position.xMin += (float)(iMGUILayoutInstruction.level * 10);
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(position, content, id, base.listViewState.row == el.row);
		}

		protected override void DrawInspectedStacktrace()
		{
			IMGUILayoutInstruction iMGUILayoutInstruction = this.m_LayoutInstructions[base.listViewState.row];
			this.m_StacktraceScrollPos = EditorGUILayout.BeginScrollView(this.m_StacktraceScrollPos, GUIViewDebuggerWindow.Styles.stacktraceBackground, new GUILayoutOption[]
			{
				GUILayout.ExpandHeight(false)
			});
			base.DrawStackFrameList(iMGUILayoutInstruction.stack);
			EditorGUILayout.EndScrollView();
		}

		internal override void DoDrawSelectedInstructionDetails(int selectedInstructionIndex)
		{
			IMGUILayoutInstruction iMGUILayoutInstruction = this.m_LayoutInstructions[selectedInstructionIndex];
			using (new EditorGUI.DisabledScope(true))
			{
				base.DrawInspectedRect(iMGUILayoutInstruction.unclippedRect);
			}
			base.DoSelectableInstructionDataField("margin.left", iMGUILayoutInstruction.marginLeft.ToString());
			base.DoSelectableInstructionDataField("margin.top", iMGUILayoutInstruction.marginTop.ToString());
			base.DoSelectableInstructionDataField("margin.right", iMGUILayoutInstruction.marginRight.ToString());
			base.DoSelectableInstructionDataField("margin.bottom", iMGUILayoutInstruction.marginBottom.ToString());
			if (iMGUILayoutInstruction.style != null)
			{
				base.DoSelectableInstructionDataField("Style Name", iMGUILayoutInstruction.style.name);
			}
			if (iMGUILayoutInstruction.isGroup != 1)
			{
				base.DoSelectableInstructionDataField("IsVertical", (iMGUILayoutInstruction.isVertical == 1).ToString());
			}
		}

		internal override string GetInstructionListName(int index)
		{
			StackFrame[] stack = this.m_LayoutInstructions[index].stack;
			int num = this.GetInterestingFrameIndex(stack);
			if (num > 0)
			{
				num--;
			}
			StackFrame stackFrame = stack[num];
			return stackFrame.methodName;
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
					if (!stackFrame.signature.StartsWith("UnityEngine.GUIDebugger"))
					{
						if (!stackFrame.signature.StartsWith("UnityEngine.GUILayoutUtility"))
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
	}
}
