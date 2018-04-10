using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	internal class ContextualMenu : Manipulator
	{
		private struct Action
		{
			public GUIContent name;

			public GenericMenu.MenuFunction action;

			public bool enabled;
		}

		public enum ActionStatus
		{
			Off,
			Enabled,
			Disabled
		}

		public delegate ContextualMenu.ActionStatus ActionStatusCallback();

		private List<ContextualMenu.Action> menuActions = new List<ContextualMenu.Action>();

		protected override void RegisterCallbacksOnTarget()
		{
			base.target.RegisterCallback<IMGUIEvent>(new EventCallback<IMGUIEvent>(this.OnIMGUIEvent), Capture.Capture);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			base.target.UnregisterCallback<IMGUIEvent>(new EventCallback<IMGUIEvent>(this.OnIMGUIEvent), Capture.Capture);
		}

		private void OnIMGUIEvent(IMGUIEvent evt)
		{
			if (evt.imguiEvent.type == EventType.ContextClick)
			{
				GenericMenu genericMenu = new GenericMenu();
				foreach (ContextualMenu.Action current in this.menuActions)
				{
					if (current.enabled)
					{
						genericMenu.AddItem(current.name, false, current.action);
					}
					else
					{
						genericMenu.AddDisabledItem(current.name);
					}
				}
				genericMenu.ShowAsContext();
			}
		}

		public void AddAction(string actionName, GenericMenu.MenuFunction action, ContextualMenu.ActionStatusCallback actionStatusCallback)
		{
			ContextualMenu.ActionStatus actionStatus = (actionStatusCallback == null) ? ContextualMenu.ActionStatus.Off : actionStatusCallback();
			if (actionStatus > ContextualMenu.ActionStatus.Off)
			{
				ContextualMenu.Action item = default(ContextualMenu.Action);
				item.name = new GUIContent(actionName);
				item.action = action;
				item.enabled = (actionStatus == ContextualMenu.ActionStatus.Enabled);
				this.menuActions.Add(item);
			}
		}
	}
}
