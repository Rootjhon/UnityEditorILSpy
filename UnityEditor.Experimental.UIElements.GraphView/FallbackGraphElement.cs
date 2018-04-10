using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class FallbackGraphElement : GraphElement
	{
		public FallbackGraphElement()
		{
			base.style.backgroundColor = Color.grey;
			base.text = "";
		}

		public override void OnDataChanged()
		{
			GraphElementPresenter presenter = base.GetPresenter<GraphElementPresenter>();
			base.text = "Fallback for " + presenter.GetType() + ". No GraphElement registered for this type in this view.";
		}
	}
}
