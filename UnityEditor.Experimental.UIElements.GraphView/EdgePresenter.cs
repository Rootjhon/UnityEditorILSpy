using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class EdgePresenter : GraphElementPresenter
	{
		[SerializeField]
		private NodeAnchorPresenter m_OutputPresenter;

		[SerializeField]
		private NodeAnchorPresenter m_InputPresenter;

		public virtual NodeAnchorPresenter output
		{
			get
			{
				return this.m_OutputPresenter;
			}
			set
			{
				if (this.m_OutputPresenter != null)
				{
					this.m_OutputPresenter.Disconnect(this);
				}
				this.m_OutputPresenter = value;
				if (this.m_OutputPresenter != null)
				{
					this.m_OutputPresenter.Connect(this);
				}
			}
		}

		public virtual NodeAnchorPresenter input
		{
			get
			{
				return this.m_InputPresenter;
			}
			set
			{
				if (this.m_InputPresenter != null)
				{
					this.m_InputPresenter.Disconnect(this);
				}
				this.m_InputPresenter = value;
				if (this.m_InputPresenter != null)
				{
					this.m_InputPresenter.Connect(this);
				}
			}
		}

		public Vector2 candidatePosition
		{
			get;
			set;
		}

		public bool candidate
		{
			get;
			set;
		}

		protected EdgePresenter()
		{
		}

		protected new void OnEnable()
		{
			base.OnEnable();
			base.capabilities = (Capabilities.Selectable | Capabilities.Deletable);
		}
	}
}
