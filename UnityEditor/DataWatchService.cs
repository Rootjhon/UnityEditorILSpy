using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor
{
	internal class DataWatchService : IDataWatchService
	{
		private struct Spy
		{
			public readonly int handleID;

			public readonly Action<UnityEngine.Object> onDataChanged;

			public Spy(int handleID, Action<UnityEngine.Object> onDataChanged)
			{
				this.handleID = handleID;
				this.onDataChanged = onDataChanged;
			}
		}

		private class Watchers
		{
			public List<DataWatchService.Spy> spyList;

			public ChangeTrackerHandle tracker;

			public IScheduledItem scheduledItem;

			public UnityEngine.Object watchedObject;

			private DataWatchService service
			{
				get
				{
					return DataWatchService.sharedInstance;
				}
			}

			public bool isModified
			{
				get;
				set;
			}

			public Watchers(UnityEngine.Object watched)
			{
				this.spyList = new List<DataWatchService.Spy>();
				this.tracker = ChangeTrackerHandle.AcquireTracker(watched);
				this.watchedObject = watched;
			}

			public void AddSpy(int handle, Action<UnityEngine.Object> onDataChanged)
			{
				this.spyList.Add(new DataWatchService.Spy(handle, onDataChanged));
			}

			public bool IsEmpty()
			{
				return this.spyList == null;
			}

			public void OnTimerPoolForChanges(TimerState ts)
			{
				if (this.PollForChanges())
				{
					this.isModified = false;
					this.service.NotifyDataChanged(this);
				}
			}

			public bool PollForChanges()
			{
				if (this.watchedObject == null)
				{
					this.isModified = true;
				}
				else if (this.tracker.PollForChanges())
				{
					this.isModified = true;
				}
				return this.isModified;
			}
		}

		public static DataWatchService sharedInstance = new DataWatchService();

		private TimerEventScheduler m_Scheduler = new TimerEventScheduler();

		private Dictionary<int, DataWatchHandle> m_Handles = new Dictionary<int, DataWatchHandle>();

		private Dictionary<UnityEngine.Object, DataWatchService.Watchers> m_Watched = new Dictionary<UnityEngine.Object, DataWatchService.Watchers>();

		private static List<DataWatchService.Spy> notificationTmpSpies = new List<DataWatchService.Spy>();

		private static int s_WatchID;

		public DataWatchService()
		{
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Combine(Undo.postprocessModifications, new Undo.PostprocessModifications(this.PostProcessUndo));
		}

		~DataWatchService()
		{
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Remove(Undo.postprocessModifications, new Undo.PostprocessModifications(this.PostProcessUndo));
		}

		public UndoPropertyModification[] PostProcessUndo(UndoPropertyModification[] modifications)
		{
			for (int i = 0; i < modifications.Length; i++)
			{
				UndoPropertyModification undoPropertyModification = modifications[i];
				PropertyModification currentValue = undoPropertyModification.currentValue;
				if (currentValue != null && !(currentValue.target == null))
				{
					DataWatchService.Watchers watchers;
					if (this.m_Watched.TryGetValue(currentValue.target, out watchers))
					{
						watchers.isModified = true;
					}
				}
			}
			return modifications;
		}

		public void ForceDirtyNextPoll(UnityEngine.Object obj)
		{
			DataWatchService.Watchers watchers;
			if (this.m_Watched.TryGetValue(obj, out watchers))
			{
				watchers.tracker.ForceDirtyNextPoll();
				watchers.isModified = true;
			}
		}

		public void PollNativeData()
		{
			this.m_Scheduler.UpdateScheduledEvents();
		}

		private void NotifyDataChanged(DataWatchService.Watchers w)
		{
			DataWatchService.notificationTmpSpies.Clear();
			DataWatchService.notificationTmpSpies.AddRange(w.spyList);
			foreach (DataWatchService.Spy current in DataWatchService.notificationTmpSpies)
			{
				current.onDataChanged(w.watchedObject);
			}
			if (w.watchedObject == null)
			{
				this.DoRemoveWatcher(w);
			}
		}

		private void DoRemoveWatcher(DataWatchService.Watchers watchers)
		{
			this.m_Watched.Remove(watchers.watchedObject);
			this.m_Scheduler.Unschedule(watchers.scheduledItem);
			watchers.tracker.ReleaseTracker();
		}

		public IDataWatchHandle AddWatch(UnityEngine.Object watched, Action<UnityEngine.Object> onDataChanged)
		{
			if (watched == null)
			{
				throw new ArgumentException("Object watched cannot be null");
			}
			DataWatchHandle dataWatchHandle = new DataWatchHandle(++DataWatchService.s_WatchID, this, watched);
			this.m_Handles[dataWatchHandle.id] = dataWatchHandle;
			DataWatchService.Watchers watchers;
			if (!this.m_Watched.TryGetValue(watched, out watchers))
			{
				watchers = new DataWatchService.Watchers(watched);
				this.m_Watched[watched] = watchers;
				watchers.scheduledItem = this.m_Scheduler.ScheduleUntil(new Action<TimerState>(watchers.OnTimerPoolForChanges), 0L, 0L, null);
			}
			watchers.spyList.Add(new DataWatchService.Spy(dataWatchHandle.id, onDataChanged));
			return dataWatchHandle;
		}

		public void RemoveWatch(IDataWatchHandle handle)
		{
			DataWatchHandle dataWatchHandle = (DataWatchHandle)handle;
			if (this.m_Handles.Remove(dataWatchHandle.id))
			{
				DataWatchService.Watchers watchers;
				if (this.m_Watched.TryGetValue(dataWatchHandle.watched, out watchers))
				{
					List<DataWatchService.Spy> spyList = watchers.spyList;
					for (int i = 0; i < spyList.Count; i++)
					{
						if (spyList[i].handleID == dataWatchHandle.id)
						{
							spyList.RemoveAt(i);
							if (watchers.IsEmpty())
							{
								this.DoRemoveWatcher(watchers);
							}
							return;
						}
					}
				}
			}
			throw new ArgumentException("Data watch was not registered");
		}
	}
}
