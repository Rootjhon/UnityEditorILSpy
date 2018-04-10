using System;
using UnityEngine;

namespace UnityEditor
{
	internal class GridPaletteAddPopup : EditorWindow
	{
		private static class Styles
		{
			public static readonly GUIContent nameLabel = EditorGUIUtility.TextContent("Name");

			public static readonly GUIContent ok = EditorGUIUtility.TextContent("Create");

			public static readonly GUIContent cancel = EditorGUIUtility.TextContent("Cancel");

			public static readonly GUIContent header = EditorGUIUtility.TextContent("Create New Palette");

			public static readonly GUIContent gridLabel = EditorGUIUtility.TextContent("Grid");

			public static readonly GUIContent sizeLabel = EditorGUIUtility.TextContent("Cell Size");
		}

		private static long s_LastClosedTime;

		private string m_Name = "New Palette";

		private static GridPaletteAddPopup s_Instance;

		private GridPaintPaletteWindow m_Owner;

		private GridLayout.CellLayout m_Layout;

		private GridPalette.CellSizing m_CellSizing;

		private Vector3 m_CellSize;

		private void Init(Rect buttonRect, GridPaintPaletteWindow owner)
		{
			this.m_Owner = owner;
			this.m_CellSize = new Vector3(1f, 1f, 0f);
			buttonRect = GUIUtility.GUIToScreenRect(buttonRect);
			base.ShowAsDropDown(buttonRect, new Vector2(312f, 140f), null, ShowMode.PopupMenuWithKeyboardFocus);
		}

		internal void OnGUI()
		{
			GUI.Label(new Rect(0f, 0f, base.position.width, base.position.height), GUIContent.none, "grey_border");
			GUILayout.Space(3f);
			GUILayout.Label(GridPaletteAddPopup.Styles.header, EditorStyles.boldLabel, new GUILayoutOption[0]);
			GUILayout.Space(4f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(GridPaletteAddPopup.Styles.nameLabel, new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			this.m_Name = EditorGUILayout.TextField(this.m_Name, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(GridPaletteAddPopup.Styles.gridLabel, new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			this.m_Layout = (GridLayout.CellLayout)EditorGUILayout.EnumPopup(this.m_Layout, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(GridPaletteAddPopup.Styles.sizeLabel, new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			this.m_CellSizing = (GridPalette.CellSizing)EditorGUILayout.EnumPopup(this.m_CellSizing, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			using (new EditorGUI.DisabledScope(this.m_CellSizing == GridPalette.CellSizing.Automatic))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Label(GUIContent.none, new GUILayoutOption[]
				{
					GUILayout.Width(90f)
				});
				this.m_CellSize = EditorGUILayout.Vector3Field(GUIContent.none, this.m_CellSize, new GUILayoutOption[0]);
				GUILayout.EndHorizontal();
			}
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			if (GUILayout.Button(GridPaletteAddPopup.Styles.cancel, new GUILayoutOption[0]))
			{
				base.Close();
			}
			using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(this.m_Name)))
			{
				if (GUILayout.Button(GridPaletteAddPopup.Styles.ok, new GUILayoutOption[0]))
				{
					GameObject gameObject = GridPaletteUtility.CreateNewPaletteNamed(this.m_Name, this.m_Layout, this.m_CellSizing, this.m_CellSize);
					if (gameObject != null)
					{
						this.m_Owner.palette = gameObject;
						this.m_Owner.Repaint();
					}
					base.Close();
					GUIUtility.ExitGUI();
				}
			}
			GUILayout.Space(10f);
			GUILayout.EndHorizontal();
		}

		internal static bool ShowAtPosition(Rect buttonRect, GridPaintPaletteWindow owner)
		{
			long num = DateTime.Now.Ticks / 10000L;
			bool result;
			if (num >= GridPaletteAddPopup.s_LastClosedTime + 50L)
			{
				Event.current.Use();
				if (GridPaletteAddPopup.s_Instance == null)
				{
					GridPaletteAddPopup.s_Instance = ScriptableObject.CreateInstance<GridPaletteAddPopup>();
				}
				GridPaletteAddPopup.s_Instance.Init(buttonRect, owner);
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
	}
}
