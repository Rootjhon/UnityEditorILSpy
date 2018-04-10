using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	public class GridBrush : GridBrushBase
	{
		[Serializable]
		public class BrushCell
		{
			[SerializeField]
			private TileBase m_Tile;

			[SerializeField]
			private Matrix4x4 m_Matrix = Matrix4x4.identity;

			[SerializeField]
			private Color m_Color = Color.white;

			public TileBase tile
			{
				get
				{
					return this.m_Tile;
				}
				set
				{
					this.m_Tile = value;
				}
			}

			public Matrix4x4 matrix
			{
				get
				{
					return this.m_Matrix;
				}
				set
				{
					this.m_Matrix = value;
				}
			}

			public Color color
			{
				get
				{
					return this.m_Color;
				}
				set
				{
					this.m_Color = value;
				}
			}

			public override int GetHashCode()
			{
				int num = (!(this.tile != null)) ? 0 : this.tile.GetInstanceID();
				num = num * 33 + this.matrix.GetHashCode();
				return num * 33 + this.color.GetHashCode();
			}
		}

		[HideInInspector, SerializeField]
		private GridBrush.BrushCell[] m_Cells;

		[HideInInspector, SerializeField]
		private Vector3Int m_Size;

		[HideInInspector, SerializeField]
		private Vector3Int m_Pivot;

		public Vector3Int size
		{
			get
			{
				return this.m_Size;
			}
			set
			{
				this.m_Size = value;
				this.SizeUpdated();
			}
		}

		public Vector3Int pivot
		{
			get
			{
				return this.m_Pivot;
			}
			set
			{
				this.m_Pivot = value;
			}
		}

		public GridBrush.BrushCell[] cells
		{
			get
			{
				return this.m_Cells;
			}
		}

		public int cellCount
		{
			get
			{
				return (this.m_Cells == null) ? 0 : this.m_Cells.Length;
			}
		}

		public GridBrush()
		{
			this.Init(Vector3Int.one, Vector3Int.zero);
			this.SizeUpdated();
		}

		public void Init(Vector3Int size)
		{
			this.Init(size, Vector3Int.zero);
			this.SizeUpdated();
		}

		public void Init(Vector3Int size, Vector3Int pivot)
		{
			this.m_Size = size;
			this.m_Pivot = pivot;
			this.SizeUpdated();
		}

		public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			Vector3Int position2 = position - this.pivot;
			BoundsInt position3 = new BoundsInt(position2, this.m_Size);
			this.BoxFill(gridLayout, brushTarget, position3);
		}

		private void PaintCell(Vector3Int position, Tilemap tilemap, GridBrush.BrushCell cell)
		{
			if (cell.tile != null)
			{
				GridBrush.SetTilemapCell(tilemap, position, cell.tile, cell.matrix, cell.color);
			}
		}

		public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			Vector3Int position2 = position - this.pivot;
			BoundsInt position3 = new BoundsInt(position2, this.m_Size);
			this.BoxErase(gridLayout, brushTarget, position3);
		}

		private void EraseCell(Vector3Int position, Tilemap tilemap)
		{
			GridBrush.ClearTilemapCell(tilemap, position);
		}

		public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			if (!(brushTarget == null))
			{
				Tilemap component = brushTarget.GetComponent<Tilemap>();
				foreach (Vector3Int current in position.allPositionsWithin)
				{
					Vector3Int vector3Int = current - position.min;
					GridBrush.BrushCell cell = this.m_Cells[this.GetCellIndexWrapAround(vector3Int.x, vector3Int.y, vector3Int.z)];
					this.PaintCell(current, component, cell);
				}
			}
		}

		public override void BoxErase(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			if (!(brushTarget == null))
			{
				Tilemap component = brushTarget.GetComponent<Tilemap>();
				foreach (Vector3Int current in position.allPositionsWithin)
				{
					this.EraseCell(current, component);
				}
			}
		}

		public override void FloodFill(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
		{
			if (this.cellCount != 0)
			{
				if (!(brushTarget == null))
				{
					Tilemap component = brushTarget.GetComponent<Tilemap>();
					if (component != null)
					{
						component.FloodFill(position, this.cells[0].tile);
					}
				}
			}
		}

		public override void Rotate(GridBrushBase.RotationDirection direction, GridLayout.CellLayout layout)
		{
			Vector3Int size = this.m_Size;
			GridBrush.BrushCell[] array = this.m_Cells.Clone() as GridBrush.BrushCell[];
			this.size = new Vector3Int(size.y, size.x, size.z);
			BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, size);
			foreach (Vector3Int current in boundsInt.allPositionsWithin)
			{
				int x = (direction != GridBrushBase.RotationDirection.Clockwise) ? current.y : (size.y - current.y - 1);
				int y = (direction != GridBrushBase.RotationDirection.Clockwise) ? (size.x - current.x - 1) : current.x;
				int cellIndex = this.GetCellIndex(x, y, current.z);
				int cellIndex2 = this.GetCellIndex(current.x, current.y, current.z, size.x, size.y, size.z);
				this.m_Cells[cellIndex] = array[cellIndex2];
			}
			int x2 = (direction != GridBrushBase.RotationDirection.Clockwise) ? this.pivot.y : (size.y - this.pivot.y - 1);
			int y2 = (direction != GridBrushBase.RotationDirection.Clockwise) ? (size.x - this.pivot.x - 1) : this.pivot.x;
			this.pivot = new Vector3Int(x2, y2, this.pivot.z);
			Matrix4x4 rhs = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, (direction != GridBrushBase.RotationDirection.Clockwise) ? -90f : 90f), Vector3.one);
			GridBrush.BrushCell[] cells = this.m_Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				GridBrush.BrushCell brushCell = cells[i];
				Matrix4x4 matrix = brushCell.matrix;
				brushCell.matrix = matrix * rhs;
			}
		}

		public override void Flip(GridBrushBase.FlipAxis flip, GridLayout.CellLayout layout)
		{
			if (flip == GridBrushBase.FlipAxis.X)
			{
				this.FlipX();
			}
			else
			{
				this.FlipY();
			}
		}

		public override void Pick(GridLayout gridLayout, GameObject brushTarget, BoundsInt position, Vector3Int pickStart)
		{
			this.Reset();
			this.UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1), new Vector3Int(pickStart.x, pickStart.y, 0));
			Tilemap component = brushTarget.GetComponent<Tilemap>();
			foreach (Vector3Int current in position.allPositionsWithin)
			{
				Vector3Int brushPosition = new Vector3Int(current.x - position.x, current.y - position.y, 0);
				this.PickCell(current, brushPosition, component);
			}
		}

		private void PickCell(Vector3Int position, Vector3Int brushPosition, Tilemap tilemap)
		{
			if (tilemap != null)
			{
				this.SetTile(brushPosition, tilemap.GetTile(position));
				this.SetMatrix(brushPosition, tilemap.GetTransformMatrix(position));
				this.SetColor(brushPosition, tilemap.GetColor(position));
			}
		}

		public override void MoveStart(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			this.Reset();
			this.UpdateSizeAndPivot(new Vector3Int(position.size.x, position.size.y, 1), Vector3Int.zero);
			Tilemap component = brushTarget.GetComponent<Tilemap>();
			if (component != null)
			{
				foreach (Vector3Int current in position.allPositionsWithin)
				{
					Vector3Int brushPosition = new Vector3Int(current.x - position.x, current.y - position.y, 0);
					this.PickCell(current, brushPosition, component);
					component.SetTile(current, null);
				}
			}
		}

		public override void MoveEnd(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
		{
			this.Paint(gridLayout, brushTarget, position.min);
			this.Reset();
		}

		public void Reset()
		{
			this.UpdateSizeAndPivot(Vector3Int.one, Vector3Int.zero);
		}

		private void FlipX()
		{
			GridBrush.BrushCell[] array = this.m_Cells.Clone() as GridBrush.BrushCell[];
			BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, this.m_Size);
			foreach (Vector3Int current in boundsInt.allPositionsWithin)
			{
				int x = this.m_Size.x - current.x - 1;
				int cellIndex = this.GetCellIndex(x, current.y, current.z);
				int cellIndex2 = this.GetCellIndex(current);
				this.m_Cells[cellIndex] = array[cellIndex2];
			}
			int x2 = this.m_Size.x - this.pivot.x - 1;
			this.pivot = new Vector3Int(x2, this.pivot.y, this.pivot.z);
			Matrix4x4 rhs = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
			GridBrush.BrushCell[] cells = this.m_Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				GridBrush.BrushCell brushCell = cells[i];
				Matrix4x4 matrix = brushCell.matrix;
				brushCell.matrix = matrix * rhs;
			}
		}

		private void FlipY()
		{
			GridBrush.BrushCell[] array = this.m_Cells.Clone() as GridBrush.BrushCell[];
			BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, this.m_Size);
			foreach (Vector3Int current in boundsInt.allPositionsWithin)
			{
				int y = this.m_Size.y - current.y - 1;
				int cellIndex = this.GetCellIndex(current.x, y, current.z);
				int cellIndex2 = this.GetCellIndex(current);
				this.m_Cells[cellIndex] = array[cellIndex2];
			}
			int y2 = this.m_Size.y - this.pivot.y - 1;
			this.pivot = new Vector3Int(this.pivot.x, y2, this.pivot.z);
			Matrix4x4 rhs = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
			GridBrush.BrushCell[] cells = this.m_Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				GridBrush.BrushCell brushCell = cells[i];
				Matrix4x4 matrix = brushCell.matrix;
				brushCell.matrix = matrix * rhs;
			}
		}

		public void UpdateSizeAndPivot(Vector3Int size, Vector3Int pivot)
		{
			this.m_Size = size;
			this.m_Pivot = pivot;
			this.SizeUpdated();
		}

		public void SetTile(Vector3Int position, TileBase tile)
		{
			if (this.ValidateCellPosition(position))
			{
				this.m_Cells[this.GetCellIndex(position)].tile = tile;
			}
		}

		public void SetMatrix(Vector3Int position, Matrix4x4 matrix)
		{
			if (this.ValidateCellPosition(position))
			{
				this.m_Cells[this.GetCellIndex(position)].matrix = matrix;
			}
		}

		public void SetColor(Vector3Int position, Color color)
		{
			if (this.ValidateCellPosition(position))
			{
				this.m_Cells[this.GetCellIndex(position)].color = color;
			}
		}

		public int GetCellIndex(Vector3Int brushPosition)
		{
			return this.GetCellIndex(brushPosition.x, brushPosition.y, brushPosition.z);
		}

		public int GetCellIndex(int x, int y, int z)
		{
			return x + this.m_Size.x * y + this.m_Size.x * this.m_Size.y * z;
		}

		public int GetCellIndex(int x, int y, int z, int sizex, int sizey, int sizez)
		{
			return x + sizex * y + sizex * sizey * z;
		}

		public int GetCellIndexWrapAround(int x, int y, int z)
		{
			return x % this.m_Size.x + this.m_Size.x * (y % this.m_Size.y) + this.m_Size.x * this.m_Size.y * (z % this.m_Size.z);
		}

		private bool ValidateCellPosition(Vector3Int position)
		{
			bool flag = position.x >= 0 && position.x < this.size.x && position.y >= 0 && position.y < this.size.y && position.z >= 0 && position.z < this.size.z;
			if (!flag)
			{
				throw new ArgumentException(string.Format("Position {0} is an invalid cell position. Valid range is between [{1}, {2}).", position, Vector3Int.zero, this.size));
			}
			return flag;
		}

		private void SizeUpdated()
		{
			this.m_Cells = new GridBrush.BrushCell[this.m_Size.x * this.m_Size.y * this.m_Size.z];
			BoundsInt boundsInt = new BoundsInt(Vector3Int.zero, this.m_Size);
			foreach (Vector3Int current in boundsInt.allPositionsWithin)
			{
				this.m_Cells[this.GetCellIndex(current)] = new GridBrush.BrushCell();
			}
		}

		private static void SetTilemapCell(Tilemap map, Vector3Int location, TileBase tile, Matrix4x4 transformMatrix, Color color)
		{
			if (!(map == null))
			{
				map.SetTile(location, tile);
				map.SetTransformMatrix(location, transformMatrix);
				map.SetColor(location, color);
			}
		}

		private static void ClearTilemapCell(Tilemap map, Vector3Int location)
		{
			if (!(map == null))
			{
				map.SetTile(location, null);
				map.SetTransformMatrix(location, Matrix4x4.identity);
				map.SetColor(location, Color.white);
			}
		}

		public override int GetHashCode()
		{
			int num = 0;
			GridBrush.BrushCell[] cells = this.cells;
			for (int i = 0; i < cells.Length; i++)
			{
				GridBrush.BrushCell brushCell = cells[i];
				num = num * 33 + brushCell.GetHashCode();
			}
			return num;
		}
	}
}
