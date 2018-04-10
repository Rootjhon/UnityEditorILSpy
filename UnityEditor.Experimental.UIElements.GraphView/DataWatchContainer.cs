using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class DataWatchContainer : VisualElement
	{
		private IUIElementDataWatchRequest[] handles;

		public bool forceNotififcationOnAdd
		{
			get;
			set;
		}

		protected abstract UnityEngine.Object[] toWatch
		{
			get;
		}

		private void OnDataChanged(UnityEngine.Object obj)
		{
			this.OnDataChanged();
		}

		public virtual void OnDataChanged()
		{
		}

		protected void AddWatch()
		{
			UnityEngine.Object[] toWatch = this.toWatch;
			this.handles = new IUIElementDataWatchRequest[toWatch.Length];
			for (int i = 0; i < toWatch.Length; i++)
			{
				if (base.panel != null && toWatch[i] != null)
				{
					this.handles[i] = base.dataWatch.RegisterWatch(toWatch[i], new Action<UnityEngine.Object>(this.OnDataChanged));
					if (this.forceNotififcationOnAdd)
					{
						this.OnDataChanged();
					}
				}
			}
		}

		protected void RemoveWatch()
		{
			if (this.handles != null)
			{
				IUIElementDataWatchRequest[] array = this.handles;
				for (int i = 0; i < array.Length; i++)
				{
					IUIElementDataWatchRequest iUIElementDataWatchRequest = array[i];
					if (iUIElementDataWatchRequest != null)
					{
						iUIElementDataWatchRequest.Dispose();
					}
				}
				this.handles = null;
			}
		}

		protected internal override void ExecuteDefaultAction(EventBase evt)
		{
			base.ExecuteDefaultAction(evt);
			if (evt.GetEventTypeId() == EventBase<AttachToPanelEvent>.TypeId())
			{
				this.AddWatch();
			}
			else if (evt.GetEventTypeId() == EventBase<DetachFromPanelEvent>.TypeId())
			{
				this.RemoveWatch();
			}
		}
	}
}
