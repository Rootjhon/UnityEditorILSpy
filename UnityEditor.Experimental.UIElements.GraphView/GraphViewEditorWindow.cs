using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class GraphViewEditorWindow : EditorWindow
	{
		private GraphViewPresenter m_Presenter;

		private IUIElementDataWatchRequest dataWatchHandle;

		public GraphView graphView
		{
			get;
			private set;
		}

		public GraphViewPresenter presenter
		{
			get
			{
				return this.m_Presenter;
			}
			private set
			{
				if (this.dataWatchHandle != null && this.graphView != null)
				{
					this.graphView.dataWatch.UnregisterWatch(this.dataWatchHandle);
				}
				this.m_Presenter = value;
				if (this.graphView != null)
				{
					this.dataWatchHandle = this.graphView.dataWatch.RegisterWatch(this.m_Presenter, new Action<UnityEngine.Object>(this.OnChanged));
				}
			}
		}

		public T GetPresenter<T>() where T : GraphViewPresenter
		{
			return this.presenter as T;
		}

		protected void OnEnable()
		{
			this.presenter = this.BuildPresenters();
			this.graphView = this.BuildView();
			this.graphView.name = "theView";
			this.graphView.persistenceKey = "theView";
			this.graphView.presenter = this.presenter;
			this.graphView.StretchToParentSize();
			this.graphView.RegisterCallback<AttachToPanelEvent>(new EventCallback<AttachToPanelEvent>(this.OnEnterPanel), Capture.NoCapture);
			if (this.dataWatchHandle == null)
			{
				this.dataWatchHandle = this.graphView.dataWatch.RegisterWatch(this.m_Presenter, new Action<UnityEngine.Object>(this.OnChanged));
			}
			this.GetRootVisualContainer().Add(this.graphView);
		}

		protected void OnDisable()
		{
			this.GetRootVisualContainer().Remove(this.graphView);
		}

		protected abstract GraphView BuildView();

		protected abstract GraphViewPresenter BuildPresenters();

		private void OnEnterPanel(AttachToPanelEvent e)
		{
			if (this.presenter == null)
			{
				this.presenter = this.BuildPresenters();
				this.graphView.presenter = this.presenter;
			}
		}

		private void OnChanged(UnityEngine.Object changedObject)
		{
			if (this.presenter == null && this.graphView.panel != null)
			{
				this.presenter = this.BuildPresenters();
				this.graphView.presenter = this.presenter;
			}
		}
	}
}
