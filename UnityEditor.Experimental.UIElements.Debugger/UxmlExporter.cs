using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements.Debugger
{
	internal class UxmlExporter
	{
		[Flags]
		public enum ExportOptions
		{
			None = 0,
			NewLineOnAttributes = 1,
			StyleFields = 2,
			AutoNameElements = 4
		}

		private const string UIElementsNamespace = "UnityEngine.Experimental.UIElements";

		public static string Dump(VisualElement selectedElement, string templateId, UxmlExporter.ExportOptions options)
		{
			Dictionary<XNamespace, string> dictionary = new Dictionary<XNamespace, string>
			{
				{
					"UnityEngine.Experimental.UIElements",
					"ui"
				}
			};
			HashSet<string> hashSet = new HashSet<string>();
			XDocument xDocument = new XDocument();
			XElement xElement = new XElement("UXML");
			xDocument.Add(xElement);
			UxmlExporter.Recurse(xElement, dictionary, hashSet, selectedElement, options);
			foreach (KeyValuePair<XNamespace, string> current in dictionary)
			{
				xElement.Add(new XAttribute(XNamespace.Xmlns + current.Value, current.Key));
			}
			foreach (string current2 in from x in hashSet
			orderby x descending
			select x)
			{
				xElement.AddFirst(new XElement("Using", new object[]
				{
					new XAttribute("alias", current2),
					new XAttribute("path", current2)
				}));
			}
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\n",
				OmitXmlDeclaration = true,
				NewLineOnAttributes = (options & UxmlExporter.ExportOptions.NewLineOnAttributes) == UxmlExporter.ExportOptions.NewLineOnAttributes,
				NewLineHandling = NewLineHandling.Replace
			};
			StringBuilder stringBuilder = new StringBuilder();
			using (XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings))
			{
				xDocument.Save(xmlWriter);
			}
			return stringBuilder.ToString();
		}

		private static void Recurse(XElement parent, Dictionary<XNamespace, string> nsToPrefix, HashSet<string> usings, VisualElement ve, UxmlExporter.ExportOptions options)
		{
			string namespaceName = ve.GetType().Namespace ?? "";
			string typeName = ve.typeName;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			XElement xElement;
			string text;
			if (ve is TemplateContainer)
			{
				string templateId = ((TemplateContainer)ve).templateId;
				xElement = new XElement(templateId);
				usings.Add(templateId);
			}
			else if (nsToPrefix.TryGetValue(namespaceName, out text))
			{
				xElement = new XElement(namespaceName + typeName);
			}
			else
			{
				xElement = new XElement(typeName);
			}
			parent.Add(xElement);
			foreach (KeyValuePair<string, string> current in dictionary)
			{
				xElement.SetAttributeValue(current.Key, current.Value);
			}
			if (!string.IsNullOrEmpty(ve.name) && ve.name[0] != '_')
			{
				xElement.SetAttributeValue("name", ve.name);
			}
			else if ((options & UxmlExporter.ExportOptions.AutoNameElements) == UxmlExporter.ExportOptions.AutoNameElements)
			{
				string value = ve.GetType().Name + ((ve.text == null) ? "" : ve.text.Replace(" ", ""));
				xElement.SetAttributeValue("name", value);
			}
			if (!string.IsNullOrEmpty(ve.text))
			{
				xElement.SetAttributeValue("text", ve.text);
			}
			IEnumerable<string> classes = ve.GetClasses();
			if (classes.Any<string>())
			{
				xElement.SetAttributeValue("class", string.Join(" ", classes.ToArray<string>()));
			}
			if (!(ve is TemplateContainer))
			{
				foreach (VisualElement current2 in ve.Children())
				{
					UxmlExporter.Recurse(xElement, nsToPrefix, usings, current2, options);
				}
			}
		}
	}
}
