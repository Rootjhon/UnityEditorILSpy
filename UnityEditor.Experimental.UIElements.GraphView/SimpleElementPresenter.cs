using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	[Serializable]
	internal class SimpleElementPresenter : GraphElementPresenter
	{
		[SerializeField]
		private string m_Title;

		public string title
		{
			get
			{
				return this.m_Title;
			}
			set
			{
				this.m_Title = value;
			}
		}

		protected SimpleElementPresenter()
		{
		}

		protected new void OnEnable()
		{
			base.OnEnable();
			this.title = string.Empty;
		}
	}
}
