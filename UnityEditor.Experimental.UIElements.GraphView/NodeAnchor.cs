using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class NodeAnchor : GraphElement
	{
		protected EdgeConnector m_EdgeConnector;

		protected VisualElement m_ConnectorBox;

		protected VisualElement m_ConnectorText;

		public Direction direction
		{
			get;
			private set;
		}

		public Node node
		{
			get
			{
				return base.GetFirstAncestorOfType<Node>();
			}
		}

		protected NodeAnchor()
		{
			base.ClearClassList();
			VisualTreeAsset visualTreeAsset = EditorGUIUtility.Load("UXML/GraphView/NodeAnchor.uxml") as VisualTreeAsset;
			visualTreeAsset.CloneTree(this, null);
			this.m_ConnectorBox = this.Q("connector", null);
			this.m_ConnectorBox.AddToClassList("connector");
			this.m_ConnectorText = this.Q("type", null);
			this.m_ConnectorText.AddToClassList("type");
		}

		public static NodeAnchor Create<TEdgePresenter>(NodeAnchorPresenter presenter) where TEdgePresenter : EdgePresenter
		{
			NodeAnchor nodeAnchor = new NodeAnchor
			{
				m_EdgeConnector = new EdgeConnector<TEdgePresenter>(null),
				presenter = presenter
			};
			nodeAnchor.AddManipulator(nodeAnchor.m_EdgeConnector);
			return nodeAnchor;
		}

		public virtual void UpdateClasses(bool fakeConnection)
		{
			NodeAnchorPresenter presenter = base.GetPresenter<NodeAnchorPresenter>();
			if (presenter.connected || fakeConnection)
			{
				base.AddToClassList("connected");
			}
			else
			{
				base.RemoveFromClassList("connected");
			}
		}

		protected virtual VisualElement CreateConnector()
		{
			return new VisualElement();
		}

		private void UpdateConnector()
		{
			if (this.m_EdgeConnector != null)
			{
				NodeAnchorPresenter presenter = base.GetPresenter<NodeAnchorPresenter>();
				if (this.m_EdgeConnector.target == null || !this.m_EdgeConnector.target.HasCapture())
				{
					if (!presenter.connected || presenter.direction != Direction.Input)
					{
						this.AddManipulator(this.m_EdgeConnector);
					}
					else
					{
						this.RemoveManipulator(this.m_EdgeConnector);
					}
				}
			}
		}

		public override void OnDataChanged()
		{
			this.UpdateConnector();
			this.UpdateClasses(false);
			NodeAnchorPresenter presenter = base.GetPresenter<NodeAnchorPresenter>();
			Type anchorType = presenter.anchorType;
			Type typeFromHandle = typeof(PortSource<>);
			try
			{
				Type type = typeFromHandle.MakeGenericType(new Type[]
				{
					anchorType
				});
				presenter.source = Activator.CreateInstance(type);
			}
			catch (Exception ex)
			{
				Debug.Log("Couldn't build PortSouce<" + ((anchorType != null) ? anchorType.Name : "null") + "> " + ex.Message);
			}
			if (presenter.highlight)
			{
				this.m_ConnectorBox.AddToClassList("anchorHighlight");
			}
			else
			{
				this.m_ConnectorBox.RemoveFromClassList("anchorHighlight");
			}
			string text = (!string.IsNullOrEmpty(presenter.name)) ? presenter.name : anchorType.Name;
			this.m_ConnectorText.text = text;
			presenter.capabilities &= ~Capabilities.Selectable;
			this.direction = presenter.direction;
		}

		public override Vector3 GetGlobalCenter()
		{
			Vector2 vector = this.m_ConnectorBox.layout.center;
			vector = this.m_ConnectorBox.transform.matrix.MultiplyPoint3x4(vector);
			return this.LocalToWorld(vector);
		}

		public override bool ContainsPoint(Vector2 localPoint)
		{
			localPoint -= base.layout.position;
			return this.m_ConnectorBox.ContainsPoint(this.m_ConnectorBox.transform.matrix.MultiplyPoint3x4(localPoint));
		}
	}
}
