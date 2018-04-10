using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class ContentZoomer : Manipulator
	{
		public static readonly Vector3 DefaultMinScale = new Vector3(0.1f, 0.1f, 1f);

		public static readonly Vector3 DefaultMaxScale = new Vector3(3f, 3f, 1f);

		private IVisualElementScheduledItem m_OnTimerTicker;

		public float zoomStep
		{
			get;
			set;
		}

		public Vector3 minScale
		{
			get;
			set;
		}

		public Vector3 maxScale
		{
			get;
			set;
		}

		public bool keepPixelCacheOnZoom
		{
			get;
			set;
		}

		private bool delayRepaintScheduled
		{
			get;
			set;
		}

		public ContentZoomer()
		{
			this.zoomStep = 0.01f;
			this.minScale = ContentZoomer.DefaultMinScale;
			this.maxScale = ContentZoomer.DefaultMaxScale;
			this.keepPixelCacheOnZoom = false;
		}

		public ContentZoomer(Vector3 minScale, Vector3 maxScale)
		{
			this.zoomStep = 0.01f;
			this.minScale = minScale;
			this.maxScale = maxScale;
			this.keepPixelCacheOnZoom = false;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			if (!(base.target is GraphView))
			{
				throw new InvalidOperationException("Manipulator can only be added to a GraphView");
			}
			base.target.RegisterCallback<WheelEvent>(new EventCallback<WheelEvent>(this.OnWheel), Capture.NoCapture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<WheelEvent>(new EventCallback<WheelEvent>(this.OnWheel), Capture.NoCapture);
		}

		private void OnTimer(TimerState timerState)
		{
			GraphView graphView = base.target as GraphView;
			if (graphView != null)
			{
				if (graphView.elementPanel != null)
				{
					graphView.elementPanel.keepPixelCacheOnWorldBoundChange = false;
				}
				this.delayRepaintScheduled = false;
			}
		}

		private void OnWheel(WheelEvent evt)
		{
			GraphView graphView = base.target as GraphView;
			if (graphView != null)
			{
				Vector3 vector = graphView.viewTransform.position;
				Vector3 vector2 = graphView.viewTransform.scale;
				Vector2 vector3 = base.target.ChangeCoordinatesTo(graphView.contentViewContainer, evt.localMousePosition);
				float x = vector3.x + graphView.contentViewContainer.layout.x;
				float y = vector3.y + graphView.contentViewContainer.layout.y;
				vector += Vector3.Scale(new Vector3(x, y, 0f), vector2);
				Vector3 b = Vector3.one - Vector3.one * evt.delta.y * this.zoomStep;
				b.z = 1f;
				vector2 = Vector3.Scale(vector2, b);
				vector2.x = Mathf.Max(Mathf.Min(this.maxScale.x, vector2.x), this.minScale.x);
				vector2.y = Mathf.Max(Mathf.Min(this.maxScale.y, vector2.y), this.minScale.y);
				vector2.z = Mathf.Max(Mathf.Min(this.maxScale.z, vector2.z), this.minScale.z);
				vector -= Vector3.Scale(new Vector3(x, y, 0f), vector2);
				if (graphView.elementPanel != null && this.keepPixelCacheOnZoom)
				{
					graphView.elementPanel.keepPixelCacheOnWorldBoundChange = true;
					if (this.m_OnTimerTicker == null)
					{
						this.m_OnTimerTicker = graphView.schedule.Execute(new Action<TimerState>(this.OnTimer));
					}
					this.m_OnTimerTicker.ExecuteLater(500L);
					this.delayRepaintScheduled = true;
				}
				graphView.UpdateViewTransform(vector, vector2);
				evt.StopPropagation();
			}
		}
	}
}
