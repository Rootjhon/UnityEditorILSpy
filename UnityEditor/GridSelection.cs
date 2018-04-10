using System;
using System.Threading;
using UnityEngine;

namespace UnityEditor
{
	public class GridSelection : ScriptableObject
	{
		private BoundsInt m_Position;

		private GameObject m_Target;

		[SerializeField]
		private UnityEngine.Object m_PreviousSelection;

		public static event Action gridSelectionChanged
		{
			add
			{
				Action action = GridSelection.gridSelectionChanged;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref GridSelection.gridSelectionChanged, (Action)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action action = GridSelection.gridSelectionChanged;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref GridSelection.gridSelectionChanged, (Action)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public static bool active
		{
			get
			{
				return Selection.activeObject is GridSelection && GridSelection.selection.m_Target != null;
			}
		}

		private static GridSelection selection
		{
			get
			{
				return Selection.activeObject as GridSelection;
			}
		}

		public static BoundsInt position
		{
			get
			{
				return (!(GridSelection.selection != null)) ? default(BoundsInt) : GridSelection.selection.m_Position;
			}
			set
			{
				if (GridSelection.selection != null && GridSelection.selection.m_Position != value)
				{
					GridSelection.selection.m_Position = value;
					if (GridSelection.gridSelectionChanged != null)
					{
						GridSelection.gridSelectionChanged();
					}
				}
			}
		}

		public static GameObject target
		{
			get
			{
				return (!(GridSelection.selection != null)) ? null : GridSelection.selection.m_Target;
			}
		}

		public static Grid grid
		{
			get
			{
				return (!(GridSelection.selection != null) || !(GridSelection.selection.m_Target != null)) ? null : GridSelection.selection.m_Target.GetComponentInParent<Grid>();
			}
		}

		public static void Select(UnityEngine.Object target, BoundsInt bounds)
		{
			GridSelection gridSelection = ScriptableObject.CreateInstance<GridSelection>();
			gridSelection.m_PreviousSelection = Selection.activeObject;
			gridSelection.m_Target = (target as GameObject);
			gridSelection.m_Position = bounds;
			Selection.activeObject = gridSelection;
			if (GridSelection.gridSelectionChanged != null)
			{
				GridSelection.gridSelectionChanged();
			}
		}

		public static void Clear()
		{
			if (GridSelection.active)
			{
				GridSelection.selection.m_Position = default(BoundsInt);
				Selection.activeObject = GridSelection.selection.m_PreviousSelection;
				if (GridSelection.gridSelectionChanged != null)
				{
					GridSelection.gridSelectionChanged();
				}
			}
		}
	}
}
