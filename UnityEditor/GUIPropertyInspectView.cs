using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class GUIPropertyInspectView : BaseInspectView
	{
		private Vector2 m_StacktraceScrollPos = default(Vector2);

		private GUIStyle m_FakeMargingStyleForOverlay = new GUIStyle();

		private List<IMGUIPropertyInstruction> m_PropertyList = new List<IMGUIPropertyInstruction>();

		public GUIPropertyInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
		}

		public override void UpdateInstructions()
		{
			this.m_PropertyList.Clear();
			GUIViewDebuggerHelper.GetPropertyInstructions(this.m_PropertyList);
		}

		public override void ShowOverlay()
		{
			if (!this.isInstructionSelected)
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
			else
			{
				IMGUIPropertyInstruction iMGUIPropertyInstruction = this.m_PropertyList[base.listViewState.row];
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, iMGUIPropertyInstruction.rect, this.m_FakeMargingStyleForOverlay);
			}
		}

		protected override int GetInstructionCount()
		{
			return this.m_PropertyList.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int id)
		{
			GUIContent content = GUIContent.Temp(this.GetInstructionListName(el.row));
			Rect position = el.position;
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(position, content, id, base.listViewState.row == el.row);
		}

		protected override void DrawInspectedStacktrace()
		{
			IMGUIPropertyInstruction iMGUIPropertyInstruction = this.m_PropertyList[base.listViewState.row];
			this.m_StacktraceScrollPos = EditorGUILayout.BeginScrollView(this.m_StacktraceScrollPos, GUIViewDebuggerWindow.Styles.stacktraceBackground, new GUILayoutOption[]
			{
				GUILayout.ExpandHeight(false)
			});
			base.DrawStackFrameList(iMGUIPropertyInstruction.beginStacktrace);
			EditorGUILayout.EndScrollView();
		}

		internal override void DoDrawSelectedInstructionDetails(int selectedInstructionIndex)
		{
			IMGUIPropertyInstruction iMGUIPropertyInstruction = this.m_PropertyList[base.listViewState.row];
			using (new EditorGUI.DisabledScope(true))
			{
				base.DrawInspectedRect(iMGUIPropertyInstruction.rect);
			}
			base.DoSelectableInstructionDataField("Target Type Name", iMGUIPropertyInstruction.targetTypeName);
			base.DoSelectableInstructionDataField("Path", iMGUIPropertyInstruction.path);
		}

		internal override string GetInstructionListName(int index)
		{
			return this.m_PropertyList[index].path;
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
	}
}
