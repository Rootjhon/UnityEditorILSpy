using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal abstract class GraphElementPresenter : ScriptableObject
	{
		[SerializeField]
		private bool m_Selected;

		[SerializeField]
		private Rect m_Position;

		[SerializeField]
		private Capabilities m_Capabilities;

		public virtual Rect position
		{
			get
			{
				return this.m_Position;
			}
			set
			{
				this.m_Position = value;
			}
		}

		public Capabilities capabilities
		{
			get
			{
				return this.m_Capabilities;
			}
			set
			{
				this.m_Capabilities = value;
			}
		}

		public bool selected
		{
			get
			{
				return this.m_Selected;
			}
			set
			{
				if ((this.capabilities & Capabilities.Selectable) == Capabilities.Selectable)
				{
					this.m_Selected = value;
				}
			}
		}

		public virtual IEnumerable<GraphElementPresenter> allChildren
		{
			get
			{
				return Enumerable.Empty<GraphElementPresenter>();
			}
		}

		public virtual IEnumerable<GraphElementPresenter> allElements
		{
			get
			{
				GraphElementPresenter.<>c__Iterator0 <>c__Iterator = new GraphElementPresenter.<>c__Iterator0();
				<>c__Iterator.$this = this;
				GraphElementPresenter.<>c__Iterator0 expr_0E = <>c__Iterator;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		public virtual UnityEngine.Object[] GetObjectsToWatch()
		{
			return new UnityEngine.Object[]
			{
				this
			};
		}

		protected virtual void OnEnable()
		{
			this.capabilities = (Capabilities.Normal | Capabilities.Selectable | Capabilities.Movable);
		}

		public virtual void OnRemoveFromGraph()
		{
		}

		public virtual void CommitChanges()
		{
		}
	}
}
