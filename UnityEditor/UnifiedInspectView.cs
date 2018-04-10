using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class UnifiedInspectView : BaseInspectView
	{
		private Vector2 m_StacktraceScrollPos = default(Vector2);

		private readonly List<IMGUIInstruction> m_Instructions = new List<IMGUIInstruction>();

		private BaseInspectView m_InstructionClipView;

		private BaseInspectView m_InstructionStyleView;

		private BaseInspectView m_InstructionPropertyView;

		private BaseInspectView m_InstructionLayoutView;

		private BaseInspectView m_InstructionNamedControlView;

		public UnifiedInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
			this.m_InstructionClipView = new GUIClipInspectView(guiViewDebuggerWindow);
			this.m_InstructionStyleView = new StyleDrawInspectView(guiViewDebuggerWindow);
			this.m_InstructionLayoutView = new GUILayoutInspectView(guiViewDebuggerWindow);
			this.m_InstructionPropertyView = new GUIPropertyInspectView(guiViewDebuggerWindow);
			this.m_InstructionNamedControlView = new GUINamedControlInspectView(guiViewDebuggerWindow);
		}

		public override void UpdateInstructions()
		{
			this.m_InstructionClipView.UpdateInstructions();
			this.m_InstructionStyleView.UpdateInstructions();
			this.m_InstructionLayoutView.UpdateInstructions();
			this.m_InstructionPropertyView.UpdateInstructions();
			this.m_InstructionNamedControlView.UpdateInstructions();
			this.m_Instructions.Clear();
			GUIViewDebuggerHelper.GetUnifiedInstructions(this.m_Instructions);
		}

		public override void ShowOverlay()
		{
			if (!this.isInstructionSelected)
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
			else
			{
				BaseInspectView inspectViewForType = this.GetInspectViewForType(this.m_Instructions[base.listViewState.row].type);
				inspectViewForType.ShowOverlay();
			}
		}

		protected override int GetInstructionCount()
		{
			return this.m_Instructions.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int controlId)
		{
			IMGUIInstruction iMGUIInstruction = this.m_Instructions[el.row];
			string instructionListName = this.GetInstructionListName(el.row);
			GUIContent content = GUIContent.Temp(instructionListName);
			Rect position = el.position;
			position.xMin += (float)(iMGUIInstruction.level * 10);
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(position, content, controlId, base.listViewState.row == el.row);
		}

		protected override void DrawInspectedStacktrace()
		{
			IMGUIInstruction iMGUIInstruction = this.m_Instructions[base.listViewState.row];
			this.m_StacktraceScrollPos = EditorGUILayout.BeginScrollView(this.m_StacktraceScrollPos, GUIViewDebuggerWindow.Styles.stacktraceBackground, new GUILayoutOption[]
			{
				GUILayout.ExpandHeight(false)
			});
			base.DrawStackFrameList(iMGUIInstruction.stack);
			EditorGUILayout.EndScrollView();
		}

		internal override void DoDrawSelectedInstructionDetails(int selectedInstructionIndex)
		{
			IMGUIInstruction iMGUIInstruction = this.m_Instructions[selectedInstructionIndex];
			BaseInspectView inspectViewForType = this.GetInspectViewForType(iMGUIInstruction.type);
			inspectViewForType.DoDrawSelectedInstructionDetails(iMGUIInstruction.typeInstructionIndex);
		}

		internal override string GetInstructionListName(int index)
		{
			IMGUIInstruction iMGUIInstruction = this.m_Instructions[index];
			BaseInspectView inspectViewForType = this.GetInspectViewForType(iMGUIInstruction.type);
			return inspectViewForType.GetInstructionListName(iMGUIInstruction.typeInstructionIndex);
		}

		internal override void OnSelectedInstructionChanged(int index)
		{
			base.listViewState.row = index;
			if (base.listViewState.row >= 0)
			{
				IMGUIInstruction iMGUIInstruction = this.m_Instructions[base.listViewState.row];
				BaseInspectView inspectViewForType = this.GetInspectViewForType(iMGUIInstruction.type);
				inspectViewForType.OnSelectedInstructionChanged(iMGUIInstruction.typeInstructionIndex);
				this.ShowOverlay();
			}
			else
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
		}

		internal override void OnDoubleClickInstruction(int index)
		{
			IMGUIInstruction iMGUIInstruction = this.m_Instructions[index];
			BaseInspectView inspectViewForType = this.GetInspectViewForType(iMGUIInstruction.type);
			inspectViewForType.OnDoubleClickInstruction(iMGUIInstruction.typeInstructionIndex);
		}

		private BaseInspectView GetInspectViewForType(InstructionType type)
		{
			BaseInspectView result;
			switch (type)
			{
			case InstructionType.kStyleDraw:
				result = this.m_InstructionStyleView;
				break;
			case InstructionType.kClipPush:
			case InstructionType.kClipPop:
				result = this.m_InstructionClipView;
				break;
			case InstructionType.kLayoutBeginGroup:
			case InstructionType.kLayoutEndGroup:
			case InstructionType.kLayoutEntry:
				result = this.m_InstructionLayoutView;
				break;
			case InstructionType.kPropertyBegin:
			case InstructionType.kPropertyEnd:
				result = this.m_InstructionPropertyView;
				break;
			case InstructionType.kLayoutNamedControl:
				result = this.m_InstructionNamedControlView;
				break;
			default:
				throw new NotImplementedException("Unhandled InstructionType");
			}
			return result;
		}
	}
}
