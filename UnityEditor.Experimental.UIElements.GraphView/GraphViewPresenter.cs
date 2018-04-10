using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal abstract class GraphViewPresenter : ScriptableObject
	{
		[SerializeField]
		protected List<GraphElementPresenter> m_Elements = new List<GraphElementPresenter>();

		[SerializeField]
		private List<GraphElementPresenter> m_TempElements = new List<GraphElementPresenter>();

		[SerializeField]
		public Vector3 position;

		[SerializeField]
		public Vector3 scale;

		public IEnumerable<GraphElementPresenter> allChildren
		{
			get
			{
				return this.m_Elements.SelectMany((GraphElementPresenter e) => e.allElements);
			}
		}

		public IEnumerable<GraphElementPresenter> elements
		{
			get
			{
				return this.m_Elements.Union(this.m_TempElements);
			}
		}

		public virtual void AddElement(GraphElementPresenter element)
		{
			this.m_Elements.Add(element);
		}

		public virtual void AddElement(EdgePresenter edge)
		{
			this.AddElement(edge);
		}

		public virtual void RemoveElement(GraphElementPresenter element)
		{
			element.OnRemoveFromGraph();
			this.m_Elements.Remove(element);
		}

		protected void OnEnable()
		{
			this.m_Elements.Clear();
			this.m_TempElements.Clear();
		}

		public void AddTempElement(GraphElementPresenter element)
		{
			this.m_TempElements.Add(element);
		}

		public void RemoveTempElement(GraphElementPresenter element)
		{
			element.OnRemoveFromGraph();
			this.m_TempElements.Remove(element);
		}

		public void ClearTempElements()
		{
			this.m_TempElements.Clear();
		}

		public virtual List<NodeAnchorPresenter> GetCompatibleAnchors(NodeAnchorPresenter startAnchor, NodeAdapter nodeAdapter)
		{
			return (from nap in this.allChildren.OfType<NodeAnchorPresenter>()
			where nap.IsConnectable() && nap.orientation == startAnchor.orientation && nap.direction != startAnchor.direction && nodeAdapter.GetAdapter(nap.source, startAnchor.source) != null
			select nap).ToList<NodeAnchorPresenter>();
		}
	}
}
