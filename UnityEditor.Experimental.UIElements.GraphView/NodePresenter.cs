using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class NodePresenter : SimpleElementPresenter
	{
		[SerializeField]
		protected List<NodeAnchorPresenter> m_InputAnchors;

		[SerializeField]
		protected List<NodeAnchorPresenter> m_OutputAnchors;

		[SerializeField]
		private bool m_expanded;

		protected Orientation m_Orientation;

		public List<NodeAnchorPresenter> inputAnchors
		{
			get
			{
				List<NodeAnchorPresenter> arg_1C_0;
				if ((arg_1C_0 = this.m_InputAnchors) == null)
				{
					arg_1C_0 = (this.m_InputAnchors = new List<NodeAnchorPresenter>());
				}
				return arg_1C_0;
			}
		}

		public List<NodeAnchorPresenter> outputAnchors
		{
			get
			{
				List<NodeAnchorPresenter> arg_1C_0;
				if ((arg_1C_0 = this.m_OutputAnchors) == null)
				{
					arg_1C_0 = (this.m_OutputAnchors = new List<NodeAnchorPresenter>());
				}
				return arg_1C_0;
			}
		}

		public virtual bool expanded
		{
			get
			{
				return this.m_expanded;
			}
			set
			{
				this.m_expanded = value;
			}
		}

		public virtual Orientation orientation
		{
			get
			{
				return this.m_Orientation;
			}
		}

		public override IEnumerable<GraphElementPresenter> allChildren
		{
			get
			{
				return this.inputAnchors.Concat(this.outputAnchors).Cast<GraphElementPresenter>();
			}
		}

		public override IEnumerable<GraphElementPresenter> allElements
		{
			get
			{
				NodePresenter.<>c__Iterator0 <>c__Iterator = new NodePresenter.<>c__Iterator0();
				<>c__Iterator.$this = this;
				NodePresenter.<>c__Iterator0 expr_0E = <>c__Iterator;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		protected NodePresenter()
		{
			this.m_expanded = true;
			this.m_Orientation = Orientation.Horizontal;
		}

		protected new void OnEnable()
		{
			base.OnEnable();
			base.capabilities |= Capabilities.Deletable;
		}
	}
}
