using System;
using System.Collections.Generic;
using UnityEditor.Experimental.U2D;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[Serializable]
	internal class SpriteRect
	{
		[SerializeField]
		private string m_Name;

		[SerializeField]
		private string m_OriginalName;

		[SerializeField]
		private Vector2 m_Pivot;

		[SerializeField]
		private SpriteAlignment m_Alignment;

		[SerializeField]
		private Vector4 m_Border;

		[SerializeField]
		private Rect m_Rect;

		[SerializeField]
		private List<SpriteOutline> m_Outline = new List<SpriteOutline>();

		[SerializeField]
		private List<SpriteOutline> m_PhysicsShape = new List<SpriteOutline>();

		[SerializeField]
		private float m_TessellationDetail;

		public string name
		{
			get
			{
				return this.m_Name;
			}
			set
			{
				this.m_Name = value;
			}
		}

		public string originalName
		{
			get
			{
				if (this.m_OriginalName == null)
				{
					this.m_OriginalName = this.name;
				}
				return this.m_OriginalName;
			}
			set
			{
				this.m_OriginalName = value;
			}
		}

		public Vector2 pivot
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

		public SpriteAlignment alignment
		{
			get
			{
				return this.m_Alignment;
			}
			set
			{
				this.m_Alignment = value;
			}
		}

		public Vector4 border
		{
			get
			{
				return this.m_Border;
			}
			set
			{
				this.m_Border = value;
			}
		}

		public Rect rect
		{
			get
			{
				return this.m_Rect;
			}
			set
			{
				this.m_Rect = value;
			}
		}

		public List<SpriteOutline> outline
		{
			get
			{
				return this.m_Outline;
			}
			set
			{
				this.m_Outline = value;
			}
		}

		public List<SpriteOutline> physicsShape
		{
			get
			{
				return this.m_PhysicsShape;
			}
			set
			{
				this.m_PhysicsShape = value;
			}
		}

		public float tessellationDetail
		{
			get
			{
				return this.m_TessellationDetail;
			}
			set
			{
				this.m_TessellationDetail = value;
			}
		}

		public static List<SpriteOutline> AcquireOutline(List<Vector2[]> outlineSP)
		{
			List<SpriteOutline> list = new List<SpriteOutline>();
			if (outlineSP != null)
			{
				for (int i = 0; i < outlineSP.Count; i++)
				{
					SpriteOutline spriteOutline = new SpriteOutline();
					spriteOutline.m_Path.AddRange(outlineSP[i]);
					list.Add(spriteOutline);
				}
			}
			return list;
		}

		public static List<Vector2[]> ApplyOutlineChanges(List<SpriteOutline> outline)
		{
			List<Vector2[]> list = new List<Vector2[]>();
			if (outline != null)
			{
				for (int i = 0; i < outline.Count; i++)
				{
					list.Add(outline[i].m_Path.ToArray());
				}
			}
			return list;
		}

		public void LoadFromSpriteData(SpriteDataBase sp)
		{
			this.rect = sp.rect;
			this.border = sp.border;
			this.name = sp.name;
			this.alignment = sp.alignment;
			this.pivot = SpriteEditorUtility.GetPivotValue(this.alignment, sp.pivot);
			this.tessellationDetail = sp.tessellationDetail;
			this.outline = SpriteRect.AcquireOutline(sp.outline);
			this.physicsShape = SpriteRect.AcquireOutline(sp.physicsShape);
		}

		public void ApplyToSpriteData(SpriteDataBase sp)
		{
			sp.rect = this.rect;
			sp.border = this.border;
			sp.name = this.name;
			sp.alignment = this.alignment;
			sp.pivot = this.pivot;
			sp.tessellationDetail = this.tessellationDetail;
			sp.outline = SpriteRect.ApplyOutlineChanges(this.outline);
			sp.physicsShape = SpriteRect.ApplyOutlineChanges(this.physicsShape);
		}
	}
}
