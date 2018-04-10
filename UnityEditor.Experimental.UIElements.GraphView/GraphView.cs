using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class GraphView : DataWatchContainer, ISelection
	{
		private class Layer : VisualElement
		{
		}

		private class ContentViewContainer : VisualElement
		{
			public override bool Overlaps(Rect r)
			{
				return true;
			}
		}

		public enum FrameType
		{
			All,
			Selection,
			Origin
		}

		[Serializable]
		private class PersistedViewTransform
		{
			public Vector3 position = Vector3.zero;

			public Vector3 scale = Vector3.one;
		}

		private GraphViewPresenter m_Presenter;

		private bool m_FrameAnimate = false;

		private readonly Dictionary<int, GraphView.Layer> m_ContainerLayers = new Dictionary<int, GraphView.Layer>();

		private GraphView.PersistedViewTransform m_PersistedViewTransform;

		private ContentZoomer m_Zoomer;

		private int m_ZoomerMaxElementCountWithPixelCacheRegen = 100;

		private Vector3 m_MinScale = ContentZoomer.DefaultMinScale;

		private Vector3 m_MaxScale = ContentZoomer.DefaultMaxScale;

		public GraphViewPresenter presenter
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
					this.m_Presenter = value;
					this.OnDataChanged();
					base.AddWatch();
				}
			}
		}

		protected GraphViewTypeFactory typeFactory
		{
			get;
			set;
		}

		public VisualElement contentViewContainer
		{
			get;
			private set;
		}

		public VisualElement viewport
		{
			get
			{
				return this;
			}
		}

		public ITransform viewTransform
		{
			get
			{
				return this.contentViewContainer.transform;
			}
		}

		public UQuery.QueryState<GraphElement> graphElements
		{
			get;
			private set;
		}

		public UQuery.QueryState<Node> nodes
		{
			get;
			private set;
		}

		public Vector3 minScale
		{
			get
			{
				return this.m_MinScale;
			}
		}

		public Vector3 maxScale
		{
			get
			{
				return this.m_MaxScale;
			}
		}

		public float scale
		{
			get
			{
				return this.viewTransform.scale.x;
			}
		}

		public int zoomerMaxElementCountWithPixelCacheRegen
		{
			get
			{
				return this.m_ZoomerMaxElementCountWithPixelCacheRegen;
			}
			set
			{
				if (this.m_ZoomerMaxElementCountWithPixelCacheRegen != value)
				{
					this.m_ZoomerMaxElementCountWithPixelCacheRegen = value;
					if (this.m_Presenter != null)
					{
						this.m_Zoomer.keepPixelCacheOnZoom = (this.m_Presenter.elements.Count<GraphElementPresenter>() > this.m_ZoomerMaxElementCountWithPixelCacheRegen);
					}
				}
			}
		}

		protected override UnityEngine.Object[] toWatch
		{
			get
			{
				return new UnityEngine.Object[]
				{
					this.presenter
				};
			}
		}

		public List<ISelectable> selection
		{
			get;
			protected set;
		}

		protected GraphView()
		{
			this.selection = new List<ISelectable>();
			base.clippingOptions = VisualElement.ClippingOptions.ClipContents;
			this.contentViewContainer = new GraphView.ContentViewContainer
			{
				name = "contentViewContainer",
				clippingOptions = VisualElement.ClippingOptions.NoClipping,
				pickingMode = PickingMode.Ignore
			};
			base.Add(this.contentViewContainer);
			this.typeFactory = new GraphViewTypeFactory();
			this.typeFactory[typeof(EdgePresenter)] = typeof(Edge);
			base.AddStyleSheetPath("StyleSheets/GraphView/GraphView.uss");
			this.graphElements = this.contentViewContainer.Query().Children<GraphView.Layer>(null, null).Children<GraphElement>(null, null).Build();
			this.nodes = this.Query(null, null).Children<Node>(null, null).Build();
		}

		public T GetPresenter<T>() where T : GraphViewPresenter
		{
			return this.presenter as T;
		}

		public void UpdateViewTransform(Vector3 newPosition, Vector3 newScale)
		{
			this.contentViewContainer.transform.position = newPosition;
			this.contentViewContainer.transform.scale = newScale;
			if (this.m_Presenter != null)
			{
				this.m_Presenter.position = newPosition;
				this.m_Presenter.scale = newScale;
			}
			this.UpdatePersistedViewTransform();
		}

		private void AddLayer(int index)
		{
			this.m_ContainerLayers.Add(index, new GraphView.Layer
			{
				clippingOptions = VisualElement.ClippingOptions.NoClipping,
				pickingMode = PickingMode.Ignore
			});
			foreach (GraphView.Layer current in from t in this.m_ContainerLayers
			orderby t.Key
			select t.Value)
			{
				if (current.parent != null)
				{
					this.contentViewContainer.Remove(current);
				}
				this.contentViewContainer.Add(current);
			}
		}

		private VisualElement GetLayer(int index)
		{
			return this.m_ContainerLayers[index];
		}

		public void SetupZoom(Vector3 minScaleSetup, Vector3 maxScaleSetup)
		{
			this.m_MinScale = minScaleSetup;
			this.m_MaxScale = maxScaleSetup;
			this.UpdateContentZoomer();
		}

		private void UpdatePersistedViewTransform()
		{
			if (this.m_PersistedViewTransform != null)
			{
				this.m_PersistedViewTransform.position = this.contentViewContainer.transform.position;
				this.m_PersistedViewTransform.scale = this.contentViewContainer.transform.scale;
				base.SavePersistentData();
			}
		}

		public override void OnPersistentDataReady()
		{
			base.OnPersistentDataReady();
			string fullHierarchicalPersistenceKey = base.GetFullHierarchicalPersistenceKey();
			this.m_PersistedViewTransform = base.GetOrCreatePersistentData<GraphView.PersistedViewTransform>(this.m_PersistedViewTransform, fullHierarchicalPersistenceKey);
			this.UpdateViewTransform(this.m_PersistedViewTransform.position, this.m_PersistedViewTransform.scale);
		}

		private void UpdateContentZoomer()
		{
			if (this.m_MinScale != this.m_MaxScale)
			{
				if (this.m_Zoomer == null)
				{
					this.m_Zoomer = new ContentZoomer(this.m_MinScale, this.m_MaxScale);
					this.AddManipulator(this.m_Zoomer);
				}
				else
				{
					this.m_Zoomer.minScale = this.m_MinScale;
					this.m_Zoomer.maxScale = this.m_MaxScale;
				}
			}
			else if (this.m_Zoomer != null)
			{
				this.RemoveManipulator(this.m_Zoomer);
			}
			this.ValidateTransform();
		}

		private void ValidateTransform()
		{
			if (this.contentViewContainer != null)
			{
				Vector3 scale = this.viewTransform.scale;
				scale.x = Mathf.Max(Mathf.Min(this.maxScale.x, scale.x), this.minScale.x);
				scale.y = Mathf.Max(Mathf.Min(this.maxScale.y, scale.y), this.minScale.y);
				this.viewTransform.scale = scale;
			}
		}

		public override void OnDataChanged()
		{
			if (!(this.m_Presenter == null))
			{
				this.contentViewContainer.transform.position = this.m_Presenter.position;
				this.contentViewContainer.transform.scale = ((!(this.m_Presenter.scale != Vector3.zero)) ? Vector3.one : this.m_Presenter.scale);
				this.ValidateTransform();
				this.UpdatePersistedViewTransform();
				List<GraphElement> list = this.graphElements.ToList();
				foreach (GraphElement current in list)
				{
					if (!this.m_Presenter.elements.Contains(current.presenter))
					{
						current.parent.Remove(current);
						this.selection.Remove(current);
					}
				}
				int num = 0;
				foreach (GraphElementPresenter current2 in this.m_Presenter.elements)
				{
					num++;
					bool flag = false;
					if ((current2.capabilities & Capabilities.Floating) == (Capabilities)0)
					{
						foreach (GraphElement current3 in list)
						{
							if (current3 != null && current3.presenter == current2)
							{
								flag = true;
								break;
							}
						}
					}
					else
					{
						foreach (VisualElement current4 in base.Children())
						{
							if (current4 != this.contentViewContainer)
							{
								GraphElement graphElement = current4 as GraphElement;
								if (graphElement != null)
								{
									if (graphElement.presenter == current2)
									{
										flag = true;
										break;
									}
								}
							}
						}
					}
					if (!flag)
					{
						this.InstantiateElement(current2);
					}
				}
				this.m_Zoomer.keepPixelCacheOnZoom = (num > this.m_ZoomerMaxElementCountWithPixelCacheRegen);
			}
		}

		public virtual void AddToSelection(ISelectable selectable)
		{
			GraphElement graphElement = selectable as GraphElement;
			if (graphElement != null)
			{
				graphElement.OnSelected();
				if (graphElement.presenter != null)
				{
					graphElement.presenter.selected = true;
				}
				this.selection.Add(selectable);
				this.contentViewContainer.Dirty(ChangeType.Repaint);
			}
		}

		public virtual void RemoveFromSelection(ISelectable selectable)
		{
			GraphElement graphElement = selectable as GraphElement;
			if (graphElement != null)
			{
				if (graphElement.presenter != null)
				{
					graphElement.presenter.selected = false;
				}
				this.selection.Remove(selectable);
				this.contentViewContainer.Dirty(ChangeType.Repaint);
			}
		}

		public virtual void ClearSelection()
		{
			foreach (GraphElement current in this.selection.OfType<GraphElement>())
			{
				if (current.presenter != null)
				{
					current.presenter.selected = false;
				}
			}
			this.selection.Clear();
			this.contentViewContainer.Dirty(ChangeType.Repaint);
		}

		private void InstantiateElement(GraphElementPresenter elementPresenter)
		{
			GraphElement graphElement = this.typeFactory.Create(elementPresenter);
			if (graphElement != null)
			{
				graphElement.SetPosition(elementPresenter.position);
				graphElement.presenter = elementPresenter;
				if ((elementPresenter.capabilities & Capabilities.Resizable) != (Capabilities)0)
				{
					graphElement.Add(new Resizer());
					graphElement.style.borderBottom = 6f;
				}
				bool flag = (elementPresenter.capabilities & Capabilities.Floating) == (Capabilities)0;
				if (flag)
				{
					int layer = graphElement.layer;
					if (!this.m_ContainerLayers.ContainsKey(layer))
					{
						this.AddLayer(layer);
					}
					this.GetLayer(layer).Add(graphElement);
				}
				else
				{
					base.Add(graphElement);
				}
			}
		}

		public EventPropagation DeleteSelection()
		{
			EventPropagation result;
			if (this.presenter == null)
			{
				result = EventPropagation.Stop;
			}
			else
			{
				HashSet<GraphElementPresenter> hashSet = new HashSet<GraphElementPresenter>();
				foreach (GraphElement current in from GraphElement e in this.selection
				where e != null && e.presenter != null
				select e)
				{
					if ((current.presenter.capabilities & Capabilities.Deletable) != (Capabilities)0)
					{
						hashSet.Add(current.presenter);
						NodePresenter presenter = current.GetPresenter<NodePresenter>();
						if (!(presenter == null))
						{
							hashSet.UnionWith(presenter.inputAnchors.SelectMany((NodeAnchorPresenter c) => c.connections).Where((EdgePresenter d) => (d.capabilities & Capabilities.Deletable) != (Capabilities)0).Cast<GraphElementPresenter>());
							hashSet.UnionWith(presenter.outputAnchors.SelectMany((NodeAnchorPresenter c) => c.connections).Where((EdgePresenter d) => (d.capabilities & Capabilities.Deletable) != (Capabilities)0).Cast<GraphElementPresenter>());
						}
					}
				}
				foreach (GraphElementPresenter current2 in hashSet)
				{
					this.presenter.RemoveElement(current2);
				}
				foreach (EdgePresenter current3 in hashSet.OfType<EdgePresenter>())
				{
					current3.output = null;
					current3.input = null;
					if (current3.output != null)
					{
						current3.output.Disconnect(current3);
					}
					if (current3.input != null)
					{
						current3.input.Disconnect(current3);
					}
				}
				result = ((hashSet.Count <= 0) ? EventPropagation.Continue : EventPropagation.Stop);
			}
			return result;
		}

		public EventPropagation FrameAll()
		{
			return this.Frame(GraphView.FrameType.All);
		}

		public EventPropagation FrameSelection()
		{
			return this.Frame(GraphView.FrameType.Selection);
		}

		public EventPropagation FrameOrigin()
		{
			return this.Frame(GraphView.FrameType.Origin);
		}

		public EventPropagation FramePrev()
		{
			EventPropagation result;
			if (this.contentViewContainer.childCount == 0)
			{
				result = EventPropagation.Continue;
			}
			else
			{
				List<GraphElement> list = this.graphElements.ToList();
				list.Reverse();
				result = this.FramePrevNext(list);
			}
			return result;
		}

		public EventPropagation FrameNext()
		{
			EventPropagation result;
			if (this.contentViewContainer.childCount == 0)
			{
				result = EventPropagation.Continue;
			}
			else
			{
				result = this.FramePrevNext(this.graphElements.ToList());
			}
			return result;
		}

		private EventPropagation FramePrevNext(List<GraphElement> childrenEnum)
		{
			GraphElement graphElement = null;
			if (this.selection.Count != 0)
			{
				graphElement = (this.selection[0] as GraphElement);
			}
			for (int i = 0; i < childrenEnum.Count; i++)
			{
				if (childrenEnum[i] == graphElement)
				{
					if (i < childrenEnum.Count - 1)
					{
						graphElement = childrenEnum[i + 1];
					}
					else
					{
						graphElement = childrenEnum[0];
					}
					break;
				}
			}
			EventPropagation result;
			if (graphElement == null)
			{
				result = EventPropagation.Continue;
			}
			else
			{
				this.ClearSelection();
				this.AddToSelection(graphElement);
				result = this.Frame(GraphView.FrameType.Selection);
			}
			return result;
		}

		private EventPropagation Frame(GraphView.FrameType frameType)
		{
			this.contentViewContainer.transform.position = Vector3.zero;
			this.contentViewContainer.transform.scale = Vector3.one;
			this.contentViewContainer.Dirty(ChangeType.Repaint);
			EventPropagation result;
			if (frameType == GraphView.FrameType.Origin)
			{
				result = EventPropagation.Stop;
			}
			else
			{
				Rect rect = this.contentViewContainer.layout;
				if (frameType == GraphView.FrameType.Selection)
				{
					if (this.selection.Count == 0)
					{
						result = EventPropagation.Continue;
						return result;
					}
					GraphElement graphElement = this.selection[0] as GraphElement;
					if (graphElement != null)
					{
						rect = graphElement.localBound;
					}
					rect = this.selection.OfType<GraphElement>().Aggregate(rect, (Rect current, GraphElement e) => RectUtils.Encompass(current, e.localBound));
				}
				else
				{
					rect = this.CalculateRectToFitAll();
				}
				int border = 30;
				Vector3 vector;
				Vector3 vector2;
				GraphView.CalculateFrameTransform(rect, base.layout, border, out vector, out vector2);
				if (!this.m_FrameAnimate)
				{
					Matrix4x4.TRS(vector, Quaternion.identity, vector2);
					this.UpdateViewTransform(vector, vector2);
				}
				this.contentViewContainer.Dirty(ChangeType.Repaint);
				this.UpdatePersistedViewTransform();
				result = EventPropagation.Stop;
			}
			return result;
		}

		public Rect CalculateRectToFitAll()
		{
			Rect rectToFit = this.contentViewContainer.layout;
			bool reachedFirstChild = false;
			this.graphElements.ForEach(delegate(GraphElement ge)
			{
				GraphElementPresenter graphElementPresenter = (ge == null) ? null : ge.GetPresenter<GraphElementPresenter>();
				if (!(graphElementPresenter == null) && (graphElementPresenter.capabilities & Capabilities.Floating) == (Capabilities)0 && !(graphElementPresenter is EdgePresenter))
				{
					if (!reachedFirstChild)
					{
						rectToFit = ge.localBound;
						reachedFirstChild = true;
					}
					else
					{
						rectToFit = RectUtils.Encompass(rectToFit, ge.localBound);
					}
				}
			});
			return rectToFit;
		}

		public static void CalculateFrameTransform(Rect rectToFit, Rect clientRect, int border, out Vector3 frameTranslation, out Vector3 frameScaling)
		{
			Rect screenRect = new Rect
			{
				xMin = (float)border,
				xMax = clientRect.width - (float)border,
				yMin = (float)border,
				yMax = clientRect.height - (float)border
			};
			Matrix4x4 matrix = GUI.matrix;
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
			Rect rect = GUIUtility.ScreenToGUIRect(screenRect);
			float num = Math.Min(rect.width / rectToFit.width, rect.height / rectToFit.height);
			num = Mathf.Clamp(num, ContentZoomer.DefaultMinScale.y, 1f);
			Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(num, num, 1f));
			Vector2 max = new Vector2(clientRect.width, clientRect.height);
			Vector2 min = new Vector2(0f, 0f);
			Rect rect2 = new Rect
			{
				min = min,
				max = max
			};
			Vector3 vector = new Vector3(matrix4x.GetColumn(0).magnitude, matrix4x.GetColumn(1).magnitude, matrix4x.GetColumn(2).magnitude);
			Vector2 vector2 = rect2.center - rectToFit.center * vector.x;
			frameTranslation = new Vector3(vector2.x, vector2.y, 0f);
			frameScaling = vector;
			GUI.matrix = matrix;
		}
	}
}
