using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Experimental.U2D
{
	internal abstract class SpriteDataBase
	{
		public abstract string name
		{
			get;
			set;
		}

		public abstract Rect rect
		{
			get;
			set;
		}

		public abstract SpriteAlignment alignment
		{
			get;
			set;
		}

		public abstract Vector2 pivot
		{
			get;
			set;
		}

		public abstract Vector4 border
		{
			get;
			set;
		}

		public abstract float tessellationDetail
		{
			get;
			set;
		}

		public abstract List<Vector2[]> outline
		{
			get;
			set;
		}

		public abstract List<Vector2[]> physicsShape
		{
			get;
			set;
		}
	}
}
