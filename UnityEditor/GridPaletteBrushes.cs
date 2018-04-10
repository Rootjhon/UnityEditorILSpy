using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class GridPaletteBrushes : ScriptableSingleton<GridPaletteBrushes>
	{
		public class AssetProcessor : AssetPostprocessor
		{
			public override int GetPostprocessOrder()
			{
				return 1;
			}

			private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
			{
				if (!GridPaintingState.savingPalette)
				{
					GridPaletteBrushes.FlushCache();
				}
			}
		}

		private static readonly string s_LibraryPath = "Library/GridBrush";

		private static readonly string s_GridBrushExtension = ".asset";

		private static bool s_RefreshCache;

		[SerializeField]
		private List<GridBrushBase> m_Brushes;

		private string[] m_BrushNames;

		public static List<GridBrushBase> brushes
		{
			get
			{
				if (ScriptableSingleton<GridPaletteBrushes>.instance.m_Brushes == null || ScriptableSingleton<GridPaletteBrushes>.instance.m_Brushes.Count == 0 || GridPaletteBrushes.s_RefreshCache)
				{
					ScriptableSingleton<GridPaletteBrushes>.instance.RefreshBrushesCache();
					GridPaletteBrushes.s_RefreshCache = false;
				}
				return ScriptableSingleton<GridPaletteBrushes>.instance.m_Brushes;
			}
		}

		public static string[] brushNames
		{
			get
			{
				return ScriptableSingleton<GridPaletteBrushes>.instance.m_BrushNames;
			}
		}

		public static Type GetDefaultBrushType()
		{
			Type result = typeof(GridBrush);
			Assembly[] loadedAssemblies = EditorAssemblies.loadedAssemblies;
			int num = 0;
			for (int i = loadedAssemblies.Length - 1; i >= 0; i--)
			{
				Type[] typesFromAssembly = AssemblyHelper.GetTypesFromAssembly(loadedAssemblies[i]);
				Type[] array = typesFromAssembly;
				for (int j = 0; j < array.Length; j++)
				{
					Type type = array[j];
					CustomGridBrushAttribute[] array2 = type.GetCustomAttributes(typeof(CustomGridBrushAttribute), false) as CustomGridBrushAttribute[];
					if (array2 != null && array2.Length > 0)
					{
						if (array2[0].defaultBrush)
						{
							result = type;
							num++;
						}
					}
				}
			}
			if (num > 1)
			{
				Debug.LogWarning("Multiple occurrences of defaultBrush == true found. It should only be declared once.");
			}
			return result;
		}

		public static void ActiveGridBrushAssetChanged()
		{
			if (!(GridPaintingState.gridBrush == null))
			{
				if (GridPaletteBrushes.IsLibraryBrush(GridPaintingState.gridBrush))
				{
					ScriptableSingleton<GridPaletteBrushes>.instance.SaveLibraryGridBrushAsset(GridPaintingState.gridBrush);
				}
			}
		}

		private void RefreshBrushesCache()
		{
			if (this.m_Brushes == null)
			{
				this.m_Brushes = new List<GridBrushBase>();
			}
			if (this.m_Brushes.Count == 0 || !(this.m_Brushes[0] is GridBrush))
			{
				Type defaultBrushType = GridPaletteBrushes.GetDefaultBrushType();
				GridBrushBase item = this.LoadOrCreateLibraryGridBrushAsset(defaultBrushType);
				this.m_Brushes.Insert(0, item);
				this.m_Brushes[0].name = this.GetBrushDropdownName(this.m_Brushes[0]);
			}
			Assembly[] loadedAssemblies = EditorAssemblies.loadedAssemblies;
			Assembly[] array = loadedAssemblies;
			for (int i = 0; i < array.Length; i++)
			{
				Assembly assembly = array[i];
				try
				{
					IEnumerable<Type> enumerable = from t in assembly.GetTypes()
					where t != typeof(GridBrushBase) && t != typeof(GridBrush) && typeof(GridBrushBase).IsAssignableFrom(t)
					select t;
					foreach (Type current in enumerable)
					{
						if (this.IsDefaultInstanceVisibleGridBrushType(current))
						{
							GridBrushBase gridBrushBase = this.LoadOrCreateLibraryGridBrushAsset(current);
							if (gridBrushBase != null)
							{
								this.m_Brushes.Add(gridBrushBase);
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.Log(string.Format("TilePalette failed to get types from {0}. Error: {1}", assembly.FullName, ex.Message));
				}
			}
			string[] array2 = AssetDatabase.FindAssets("t:GridBrushBase");
			string[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				string guid = array3[j];
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				GridBrushBase gridBrushBase2 = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GridBrushBase)) as GridBrushBase;
				if (gridBrushBase2 != null && this.IsAssetVisibleGridBrushType(gridBrushBase2.GetType()))
				{
					this.m_Brushes.Add(gridBrushBase2);
				}
			}
			this.m_BrushNames = new string[this.m_Brushes.Count];
			for (int k = 0; k < this.m_Brushes.Count; k++)
			{
				this.m_BrushNames[k] = this.m_Brushes[k].name;
			}
		}

		private bool IsDefaultInstanceVisibleGridBrushType(Type brushType)
		{
			CustomGridBrushAttribute[] array = brushType.GetCustomAttributes(typeof(CustomGridBrushAttribute), false) as CustomGridBrushAttribute[];
			return array != null && array.Length > 0 && !array[0].hideDefaultInstance;
		}

		private bool IsAssetVisibleGridBrushType(Type brushType)
		{
			CustomGridBrushAttribute[] array = brushType.GetCustomAttributes(typeof(CustomGridBrushAttribute), false) as CustomGridBrushAttribute[];
			return array != null && array.Length > 0 && !array[0].hideAssetInstances;
		}

		private void SaveLibraryGridBrushAsset(GridBrushBase brush)
		{
			string path = this.GenerateGridBrushInstanceLibraryPath(brush.GetType());
			string directoryName = Path.GetDirectoryName(path);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			InternalEditorUtility.SaveToSerializedFileAndForget(new GridBrushBase[]
			{
				brush
			}, path, true);
		}

		private GridBrushBase LoadOrCreateLibraryGridBrushAsset(Type brushType)
		{
			UnityEngine.Object[] array = InternalEditorUtility.LoadSerializedFileAndForget(this.GenerateGridBrushInstanceLibraryPath(brushType));
			GridBrushBase result;
			if (array != null && array.Length > 0)
			{
				GridBrushBase gridBrushBase = array[0] as GridBrushBase;
				if (gridBrushBase != null && gridBrushBase.GetType() == brushType)
				{
					result = gridBrushBase;
					return result;
				}
			}
			result = this.CreateLibraryGridBrushAsset(brushType);
			return result;
		}

		private GridBrushBase CreateLibraryGridBrushAsset(Type brushType)
		{
			GridBrushBase gridBrushBase = ScriptableObject.CreateInstance(brushType) as GridBrushBase;
			gridBrushBase.hideFlags = HideFlags.DontSave;
			gridBrushBase.name = this.GetBrushDropdownName(gridBrushBase);
			this.SaveLibraryGridBrushAsset(gridBrushBase);
			return gridBrushBase;
		}

		private string GenerateGridBrushInstanceLibraryPath(Type brushType)
		{
			string unityPath = FileUtil.CombinePaths(new string[]
			{
				GridPaletteBrushes.s_LibraryPath,
				brushType.ToString() + GridPaletteBrushes.s_GridBrushExtension
			});
			return FileUtil.NiceWinPath(unityPath);
		}

		private string GetBrushDropdownName(GridBrushBase brush)
		{
			string result;
			if (!GridPaletteBrushes.IsLibraryBrush(brush))
			{
				result = brush.name;
			}
			else
			{
				CustomGridBrushAttribute[] array = brush.GetType().GetCustomAttributes(typeof(CustomGridBrushAttribute), false) as CustomGridBrushAttribute[];
				if (array != null && array.Length > 0 && array[0].defaultName.Length > 0)
				{
					result = array[0].defaultName;
				}
				else if (brush.GetType() == typeof(GridBrush))
				{
					result = "Default Brush";
				}
				else
				{
					result = brush.GetType().Name;
				}
			}
			return result;
		}

		private static bool IsLibraryBrush(GridBrushBase brush)
		{
			return !AssetDatabase.Contains(brush);
		}

		internal static void FlushCache()
		{
			GridPaletteBrushes.s_RefreshCache = true;
			if (ScriptableSingleton<GridPaletteBrushes>.instance.m_Brushes != null)
			{
				ScriptableSingleton<GridPaletteBrushes>.instance.m_Brushes.Clear();
				GridPaintingState.FlushCache();
			}
		}
	}
}
