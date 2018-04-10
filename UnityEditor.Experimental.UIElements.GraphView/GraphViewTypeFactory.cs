using System;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class GraphViewTypeFactory : BaseTypeFactory<GraphElementPresenter, GraphElement>
	{
		public GraphViewTypeFactory() : base(typeof(FallbackGraphElement))
		{
		}

		public override GraphElement Create(GraphElementPresenter key)
		{
			GraphElement graphElement = base.Create(key);
			if (graphElement != null)
			{
				graphElement.presenter = key;
			}
			return graphElement;
		}
	}
}
