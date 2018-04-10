using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class Node : GraphElement
	{
		private readonly Label m_TitleLabel;

		protected readonly Button m_CollapseButton;

		protected virtual VisualElement mainContainer
		{
			get;
			private set;
		}

		protected virtual VisualElement leftContainer
		{
			get;
			private set;
		}

		protected virtual VisualElement rightContainer
		{
			get;
			private set;
		}

		protected virtual VisualElement titleContainer
		{
			get;
			private set;
		}

		protected virtual VisualElement inputContainer
		{
			get;
			private set;
		}

		protected virtual VisualElement outputContainer
		{
			get;
			private set;
		}

		public Node()
		{
			base.clippingOptions = VisualElement.ClippingOptions.ClipAndCacheContents;
			VisualTreeAsset visualTreeAsset = EditorGUIUtility.Load("UXML/GraphView/Node.uxml") as VisualTreeAsset;
			this.mainContainer = visualTreeAsset.CloneTree(null);
			this.leftContainer = this.mainContainer.Q("left", null);
			this.rightContainer = this.mainContainer.Q("right", null);
			this.titleContainer = this.mainContainer.Q("title", null);
			this.inputContainer = this.mainContainer.Q("input", null);
			this.outputContainer = this.mainContainer.Q("output", null);
			this.m_TitleLabel = this.mainContainer.Q("titleLabel", null);
			this.m_CollapseButton = this.mainContainer.Q("collapseButton", null);
			this.m_CollapseButton.clickable.clicked += new Action(this.ToggleCollapse);
			base.elementTypeColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
			base.Add(this.mainContainer);
			this.mainContainer.AddToClassList("mainContainer");
			base.ClearClassList();
			base.AddToClassList("node");
		}

		public override void SetPosition(Rect newPos)
		{
			if (base.ClassListContains("vertical"))
			{
				base.SetPosition(newPos);
			}
			else
			{
				base.style.positionType = PositionType.Absolute;
				base.style.positionLeft = newPos.x;
				base.style.positionTop = newPos.y;
			}
		}

		protected virtual void SetLayoutClassLists(NodePresenter nodePresenter)
		{
			if (!base.ClassListContains("vertical") && !base.ClassListContains("horizontal"))
			{
				base.AddToClassList((nodePresenter.orientation != Orientation.Vertical) ? "horizontal" : "vertical");
			}
		}

		protected virtual void OnAnchorRemoved(NodeAnchor anchor)
		{
		}

		private void ProcessRemovedAnchors(IList<NodeAnchor> currentAnchors, VisualElement anchorContainer, IList<NodeAnchorPresenter> currentPresenters)
		{
			foreach (NodeAnchor current in currentAnchors)
			{
				bool flag = false;
				NodeAnchorPresenter presenter = current.GetPresenter<NodeAnchorPresenter>();
				foreach (NodeAnchorPresenter current2 in currentPresenters)
				{
					if (current2 == presenter)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.OnAnchorRemoved(current);
					anchorContainer.Remove(current);
				}
			}
		}

		private void ProcessAddedAnchors(IList<NodeAnchor> currentAnchors, VisualElement anchorContainer, IList<NodeAnchorPresenter> currentPresenters)
		{
			int num = 0;
			foreach (NodeAnchorPresenter current in currentPresenters)
			{
				bool flag = false;
				foreach (NodeAnchor current2 in currentAnchors)
				{
					if (current == current2.GetPresenter<NodeAnchorPresenter>())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					anchorContainer.Insert(num, this.InstantiateNodeAnchor(current));
				}
				num++;
			}
		}

		public virtual NodeAnchor InstantiateNodeAnchor(NodeAnchorPresenter newPres)
		{
			return NodeAnchor.Create<EdgePresenter>(newPres);
		}

		private int ShowAnchors(bool show, IList<NodeAnchor> currentAnchors)
		{
			int num = 0;
			foreach (NodeAnchor current in currentAnchors)
			{
				NodeAnchorPresenter presenter = current.GetPresenter<NodeAnchorPresenter>();
				if ((show || presenter.connected) && !presenter.collapsed)
				{
					current.visible = true;
					current.RemoveFromClassList("hidden");
					num++;
				}
				else
				{
					current.visible = false;
					current.AddToClassList("hidden");
				}
			}
			return num;
		}

		public void RefreshAnchors()
		{
			NodePresenter presenter = base.GetPresenter<NodePresenter>();
			List<NodeAnchor> currentAnchors = this.inputContainer.Query(null, null).ToList();
			List<NodeAnchor> currentAnchors2 = this.outputContainer.Query(null, null).ToList();
			this.ProcessRemovedAnchors(currentAnchors, this.inputContainer, presenter.inputAnchors);
			this.ProcessRemovedAnchors(currentAnchors2, this.outputContainer, presenter.outputAnchors);
			this.ProcessAddedAnchors(currentAnchors, this.inputContainer, presenter.inputAnchors);
			this.ProcessAddedAnchors(currentAnchors2, this.outputContainer, presenter.outputAnchors);
			currentAnchors = this.inputContainer.Query(null, null).ToList();
			currentAnchors2 = this.outputContainer.Query(null, null).ToList();
			this.ShowAnchors(presenter.expanded, currentAnchors);
			int num = this.ShowAnchors(presenter.expanded, currentAnchors2);
			if (num > 0)
			{
				if (!this.mainContainer.Contains(this.rightContainer))
				{
					this.mainContainer.Add(this.rightContainer);
				}
			}
			else if (this.mainContainer.Contains(this.rightContainer))
			{
				this.mainContainer.Remove(this.rightContainer);
			}
		}

		public override void OnDataChanged()
		{
			base.OnDataChanged();
			NodePresenter presenter = base.GetPresenter<NodePresenter>();
			this.RefreshAnchors();
			this.m_TitleLabel.text = presenter.title;
			this.m_CollapseButton.text = ((!presenter.expanded) ? "expand" : "collapse");
			this.SetLayoutClassLists(presenter);
		}

		protected virtual void ToggleCollapse()
		{
			NodePresenter presenter = base.GetPresenter<NodePresenter>();
			presenter.expanded = !presenter.expanded;
		}
	}
}
