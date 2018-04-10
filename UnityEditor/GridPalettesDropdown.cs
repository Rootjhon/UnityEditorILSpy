using System;

namespace UnityEditor
{
	internal class GridPalettesDropdown : FlexibleMenu
	{
		internal class MenuItemProvider : IFlexibleMenuItemProvider
		{
			public int Count()
			{
				return GridPalettes.palettes.Count + 1;
			}

			public object GetItem(int index)
			{
				object result;
				if (index < GridPalettes.palettes.Count)
				{
					result = GridPalettes.palettes[index];
				}
				else
				{
					result = null;
				}
				return result;
			}

			public int Add(object obj)
			{
				throw new NotImplementedException();
			}

			public void Replace(int index, object newPresetObject)
			{
				throw new NotImplementedException();
			}

			public void Remove(int index)
			{
				throw new NotImplementedException();
			}

			public object Create()
			{
				throw new NotImplementedException();
			}

			public void Move(int index, int destIndex, bool insertAfterDestIndex)
			{
				throw new NotImplementedException();
			}

			public string GetName(int index)
			{
				string result;
				if (index < GridPalettes.palettes.Count)
				{
					result = GridPalettes.palettes[index].name;
				}
				else if (index == GridPalettes.palettes.Count)
				{
					result = "Create New Palette";
				}
				else
				{
					result = "";
				}
				return result;
			}

			public bool IsModificationAllowed(int index)
			{
				return false;
			}

			public int[] GetSeperatorIndices()
			{
				return new int[]
				{
					GridPalettes.palettes.Count - 1
				};
			}
		}

		public GridPalettesDropdown(IFlexibleMenuItemProvider itemProvider, int selectionIndex, FlexibleMenuModifyItemUI modifyItemUi, Action<int, object> itemClickedCallback, float minWidth) : base(itemProvider, selectionIndex, modifyItemUi, itemClickedCallback)
		{
			base.minTextWidth = minWidth;
		}
	}
}
