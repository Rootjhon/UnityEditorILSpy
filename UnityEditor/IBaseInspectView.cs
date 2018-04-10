using System;

namespace UnityEditor
{
	internal interface IBaseInspectView
	{
		void UpdateInstructions();

		void DrawInstructionList();

		void DrawSelectedInstructionDetails();

		void ShowOverlay();

		void SelectRow(int index);

		void ClearRowSelection();
	}
}
