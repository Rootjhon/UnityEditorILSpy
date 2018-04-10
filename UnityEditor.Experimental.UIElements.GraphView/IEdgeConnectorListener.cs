using System;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal interface IEdgeConnectorListener
	{
		void OnDropOutsideAnchor(EdgePresenter edge, Vector2 position);
	}
}
