using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class GUINamedControlInspectView : BaseInspectView
	{
		private readonly List<IMGUINamedControlInstruction> m_NamedControlInstructions = new List<IMGUINamedControlInstruction>();

		private GUIStyle m_FakeMargingStyleForOverlay = new GUIStyle();

		public GUINamedControlInspectView(GUIViewDebuggerWindow guiViewDebuggerWindow) : base(guiViewDebuggerWindow)
		{
		}

		public override void UpdateInstructions()
		{
			this.m_NamedControlInstructions.Clear();
			GUIViewDebuggerHelper.GetNamedControlInstructions(this.m_NamedControlInstructions);
		}

		protected override int GetInstructionCount()
		{
			return this.m_NamedControlInstructions.Count;
		}

		protected override void DoDrawInstruction(ListViewElement el, int id)
		{
			GUIContent content = GUIContent.Temp(this.GetInstructionListName(el.row));
			Rect position = el.position;
			GUIViewDebuggerWindow.Styles.listItemBackground.Draw(position, false, false, base.listViewState.row == el.row, false);
			GUIViewDebuggerWindow.Styles.listItem.Draw(position, content, id, base.listViewState.row == el.row);
		}

		internal override string GetInstructionListName(int index)
		{
			return "\"" + this.m_NamedControlInstructions[index].name + "\"";
		}

		internal override void OnDoubleClickInstruction(int index)
		{
		}

		protected override void DrawInspectedStacktrace()
		{
		}

		internal override void DoDrawSelectedInstructionDetails(int index)
		{
			IMGUINamedControlInstruction iMGUINamedControlInstruction = this.m_NamedControlInstructions[base.listViewState.row];
			using (new EditorGUI.DisabledScope(true))
			{
				base.DrawInspectedRect(iMGUINamedControlInstruction.rect);
			}
			base.DoSelectableInstructionDataField("Name", iMGUINamedControlInstruction.name);
			base.DoSelectableInstructionDataField("ID", iMGUINamedControlInstruction.id.ToString());
		}

		internal override void OnSelectedInstructionChanged(int index)
		{
			base.listViewState.row = index;
			this.ShowOverlay();
		}

		public override void ShowOverlay()
		{
			if (!this.isInstructionSelected)
			{
				base.debuggerWindow.ClearInstructionHighlighter();
			}
			else
			{
				IMGUINamedControlInstruction iMGUINamedControlInstruction = this.m_NamedControlInstructions[base.listViewState.row];
				base.debuggerWindow.HighlightInstruction(base.debuggerWindow.inspected, iMGUINamedControlInstruction.rect, this.m_FakeMargingStyleForOverlay);
			}
		}
	}
}
