using System;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class SimpleElement : GraphElement
	{
		public SimpleElement()
		{
			base.text = "";
		}

		public override void OnDataChanged()
		{
			base.OnDataChanged();
			SimpleElementPresenter presenter = base.GetPresenter<SimpleElementPresenter>();
			base.text = presenter.title;
		}
	}
}
