using System;
using System.Xml.Linq;

namespace UnityEditor.Experimental.UIElements
{
	internal static class XmlExtensions
	{
		public static string AttributeValue(this XElement elt, string attributeName)
		{
			XAttribute xAttribute = elt.Attribute(attributeName);
			return (xAttribute != null) ? xAttribute.Value : null;
		}
	}
}
