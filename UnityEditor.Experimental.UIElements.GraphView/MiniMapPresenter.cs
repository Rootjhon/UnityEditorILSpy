using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class MiniMapPresenter : GraphElementPresenter
	{
		public float maxHeight;

		public float maxWidth;

		[SerializeField]
		private bool m_Anchored;

		public bool anchored
		{
			get
			{
				return this.m_Anchored;
			}
			set
			{
				this.m_Anchored = value;
			}
		}

		protected MiniMapPresenter()
		{
			this.maxWidth = 200f;
			this.maxHeight = 180f;
		}

		protected new void OnEnable()
		{
			base.OnEnable();
			base.capabilities = (Capabilities.Floating | Capabilities.Movable);
		}
	}
}
