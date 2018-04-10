using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class GraphElement : DataWatchContainer, ISelectable
	{
		private GraphElementPresenter m_Presenter;

		private ClickSelector m_ClickSelector;

		public Color elementTypeColor
		{
			get;
			set;
		}

		public virtual int layer
		{
			get
			{
				return 0;
			}
		}

		public GraphElementPresenter presenter
		{
			get
			{
				return this.m_Presenter;
			}
			set
			{
				if (!(this.m_Presenter == value))
				{
					base.RemoveWatch();
					if (this.m_ClickSelector != null)
					{
						this.RemoveManipulator(this.m_ClickSelector);
						this.m_ClickSelector = null;
					}
					this.m_Presenter = value;
					if (this.IsSelectable())
					{
						this.m_ClickSelector = new ClickSelector();
						this.AddManipulator(this.m_ClickSelector);
					}
					this.OnDataChanged();
					base.AddWatch();
				}
			}
		}

		protected override UnityEngine.Object[] toWatch
		{
			get
			{
				return this.presenter.GetObjectsToWatch();
			}
		}

		protected GraphElement()
		{
			base.ClearClassList();
			base.AddToClassList("graphElement");
			this.elementTypeColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
		}

		public T GetPresenter<T>() where T : GraphElementPresenter
		{
			return this.presenter as T;
		}

		public override void OnDataChanged()
		{
			if (!(this.presenter == null))
			{
				foreach (VisualElement current in this)
				{
					GraphElement graphElement = current as GraphElement;
					if (graphElement != null)
					{
						GraphElementPresenter presenter = graphElement.presenter;
						if (presenter != null)
						{
							presenter.selected = this.presenter.selected;
						}
					}
				}
				if (this.presenter.selected)
				{
					base.AddToClassList("selected");
				}
				else
				{
					base.RemoveFromClassList("selected");
				}
				this.SetPosition(this.presenter.position);
			}
		}

		public virtual bool IsSelectable()
		{
			return (this.presenter.capabilities & Capabilities.Selectable) == Capabilities.Selectable;
		}

		public virtual Vector3 GetGlobalCenter()
		{
			Vector2 center = base.layout.center;
			Vector3 point = new Vector3(center.x + base.parent.layout.x, center.y + base.parent.layout.y);
			return base.parent.worldTransform.MultiplyPoint3x4(point);
		}

		public virtual void SetPosition(Rect newPos)
		{
			base.layout = newPos;
		}

		public virtual void OnSelected()
		{
		}

		public virtual void Select(GraphView selectionContainer, bool additive)
		{
			if (selectionContainer != null)
			{
				if (!selectionContainer.selection.Contains(this))
				{
					if (!additive)
					{
						selectionContainer.ClearSelection();
					}
					selectionContainer.AddToSelection(this);
				}
			}
		}

		public virtual void Unselect(GraphView selectionContainer)
		{
			if (selectionContainer != null && base.parent == selectionContainer.contentViewContainer)
			{
				if (selectionContainer.selection.Contains(this))
				{
					selectionContainer.RemoveFromSelection(this);
				}
			}
		}

		public virtual bool IsSelected(GraphView selectionContainer)
		{
			bool result;
			if (selectionContainer != null && base.parent == selectionContainer.contentViewContainer)
			{
				if (selectionContainer.selection.Contains(this))
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
	}
}
