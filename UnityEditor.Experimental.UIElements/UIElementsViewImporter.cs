using ExCSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor.StyleSheets;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.StyleSheets;

namespace UnityEditor.Experimental.UIElements
{
	[ScriptedImporter(3, "uxml", 0)]
	internal class UIElementsViewImporter : ScriptedImporter
	{
		internal struct Error
		{
			public enum Level
			{
				Info,
				Warning,
				Fatal
			}

			public readonly UIElementsViewImporter.Error.Level level;

			public readonly ImportErrorType error;

			public readonly ImportErrorCode code;

			public readonly object context;

			public readonly string filePath;

			public readonly IXmlLineInfo xmlLineInfo;

			public Error(ImportErrorType error, ImportErrorCode code, object context, UIElementsViewImporter.Error.Level level, string filePath, IXmlLineInfo xmlLineInfo)
			{
				this.xmlLineInfo = xmlLineInfo;
				this.error = error;
				this.code = code;
				this.context = context;
				this.level = level;
				this.filePath = filePath;
			}

			private static string ErrorMessage(ImportErrorCode errorCode)
			{
				string result;
				switch (errorCode)
				{
				case ImportErrorCode.InvalidRootElement:
					result = "Expected the XML Root element name to be 'UXML', found '{0}'";
					break;
				case ImportErrorCode.DuplicateUsingAlias:
					result = "Duplicate alias '{0}'";
					break;
				case ImportErrorCode.UnknownElement:
					result = "Unknown element name '{0}'";
					break;
				case ImportErrorCode.UnknownAttribute:
					result = "Unknown attribute: '{0}'";
					break;
				case ImportErrorCode.InvalidXml:
					result = "Xml is not valid, exception during parsing: {0}";
					break;
				case ImportErrorCode.InvalidCssInStyleAttribute:
					result = "USS in 'style' attribute is invalid: {0}";
					break;
				case ImportErrorCode.MissingPathAttributeOnUsing:
					result = "'Using' declaration requires a 'path' attribute referencing another uxml file";
					break;
				case ImportErrorCode.UsingHasEmptyAlias:
					result = "'Using' declaration requires a non-empty 'alias' attribute";
					break;
				case ImportErrorCode.StyleReferenceEmptyOrMissingPathAttr:
					result = "USS in 'style' attribute is invalid: {0}";
					break;
				case ImportErrorCode.DuplicateSlotDefinition:
					result = "Slot definition '{0}' is defined more than once";
					break;
				case ImportErrorCode.SlotUsageInNonTemplate:
					result = "Element has an assigned slot, but its parent '{0}' is not a template reference";
					break;
				case ImportErrorCode.SlotDefinitionHasEmptyName:
					result = "Slot definition has an empty name";
					break;
				case ImportErrorCode.SlotUsageHasEmptyName:
					result = "Slot usage has an empty name";
					break;
				case ImportErrorCode.DuplicateContentContainer:
					result = "'contentContainer' attribute must be defined once at most";
					break;
				default:
					throw new ArgumentOutOfRangeException("Unhandled error code " + errorCode);
				}
				return result;
			}

			public override string ToString()
			{
				string format = UIElementsViewImporter.Error.ErrorMessage(this.code);
				string text = (this.xmlLineInfo != null) ? string.Format(" ({0},{1})", this.xmlLineInfo.LineNumber, this.xmlLineInfo.LinePosition) : "";
				return string.Format("{0}{1}: {2} - {3}", new object[]
				{
					this.filePath,
					text,
					this.error,
					string.Format(format, (this.context != null) ? this.context.ToString() : "<null>")
				});
			}
		}

		internal class DefaultLogger
		{
			protected List<UIElementsViewImporter.Error> m_Errors = new List<UIElementsViewImporter.Error>();

			protected string m_Path;

			internal virtual void LogError(ImportErrorType error, ImportErrorCode code, object context, UIElementsViewImporter.Error.Level level, IXmlLineInfo xmlLineInfo)
			{
				this.m_Errors.Add(new UIElementsViewImporter.Error(error, code, context, level, this.m_Path, xmlLineInfo));
			}

			internal virtual void BeginImport(string path)
			{
				this.m_Path = path;
			}

			private void LogError(VisualTreeAsset obj, UIElementsViewImporter.Error error)
			{
				try
				{
					switch (error.level)
					{
					case UIElementsViewImporter.Error.Level.Info:
						Debug.LogFormat(obj, error.ToString(), new object[0]);
						break;
					case UIElementsViewImporter.Error.Level.Warning:
						Debug.LogWarningFormat(obj, error.ToString(), new object[0]);
						break;
					case UIElementsViewImporter.Error.Level.Fatal:
						Debug.LogErrorFormat(obj, error.ToString(), new object[0]);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				catch (FormatException)
				{
					switch (error.level)
					{
					case UIElementsViewImporter.Error.Level.Info:
						Debug.Log(error.ToString());
						break;
					case UIElementsViewImporter.Error.Level.Warning:
						Debug.LogWarning(error.ToString());
						break;
					case UIElementsViewImporter.Error.Level.Fatal:
						Debug.LogError(error.ToString());
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
			}

			internal virtual void FinishImport()
			{
				Dictionary<string, VisualTreeAsset> dictionary = new Dictionary<string, VisualTreeAsset>();
				foreach (UIElementsViewImporter.Error current in this.m_Errors)
				{
					VisualTreeAsset obj;
					if (!dictionary.TryGetValue(current.filePath, out obj))
					{
						dictionary.Add(current.filePath, obj = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(current.filePath));
					}
					this.LogError(obj, current);
				}
				this.m_Errors.Clear();
			}
		}

		private const string k_XmlTemplate = "<UXML xmlns:ui=\"UnityEngine.Experimental.UIElements\">\n  <ui:Label text=\"New UXML\" />\n</UXML>";

		private const StringComparison k_Comparison = StringComparison.InvariantCulture;

		private const string k_TemplateNode = "UXML";

		private const string k_UsingNode = "Using";

		private const string k_UsingAliasAttr = "alias";

		private const string k_UsingPathAttr = "path";

		private const string k_StyleReferenceNode = "Style";

		private const string k_StylePathAttr = "path";

		private const string k_SlotDefinitionAttr = "slot-name";

		private const string k_SlotUsageAttr = "slot";

		internal static UIElementsViewImporter.DefaultLogger logger = new UIElementsViewImporter.DefaultLogger();

		[MenuItem("Assets/Create/UIElements View")]
		public static void CreateTemplateMenuItem()
		{
			ProjectWindowUtil.CreateAssetWithContent("New UXML.uxml", "<UXML xmlns:ui=\"UnityEngine.Experimental.UIElements\">\n  <ui:Label text=\"New UXML\" />\n</UXML>");
		}

		public override void OnImportAsset(AssetImportContext args)
		{
			UIElementsViewImporter.logger.BeginImport(args.assetPath);
			VisualTreeAsset visualTreeAsset;
			UIElementsViewImporter.ImportXml(args.assetPath, out visualTreeAsset);
			args.AddObjectToAsset("tree", visualTreeAsset);
			args.SetMainObject(visualTreeAsset);
			if (!visualTreeAsset.inlineSheet)
			{
				visualTreeAsset.inlineSheet = ScriptableObject.CreateInstance<StyleSheet>();
			}
			args.AddObjectToAsset("inlineStyle", visualTreeAsset.inlineSheet);
		}

		internal static void ImportXml(string xmlPath, out VisualTreeAsset vta)
		{
			vta = ScriptableObject.CreateInstance<VisualTreeAsset>();
			vta.visualElementAssets = new List<VisualElementAsset>();
			vta.templateAssets = new List<TemplateAsset>();
			XDocument doc;
			try
			{
				doc = XDocument.Load(xmlPath, LoadOptions.SetLineInfo);
			}
			catch (Exception context)
			{
				UIElementsViewImporter.logger.LogError(ImportErrorType.Syntax, ImportErrorCode.InvalidXml, context, UIElementsViewImporter.Error.Level.Fatal, null);
				return;
			}
			StyleSheetBuilder styleSheetBuilder = new StyleSheetBuilder();
			UIElementsViewImporter.LoadXmlRoot(doc, vta, styleSheetBuilder);
			StyleSheet styleSheet = ScriptableObject.CreateInstance<StyleSheet>();
			styleSheet.name = "inlineStyle";
			styleSheetBuilder.BuildTo(styleSheet);
			vta.inlineSheet = styleSheet;
		}

		private static void LoadXmlRoot(XDocument doc, VisualTreeAsset vta, StyleSheetBuilder ssb)
		{
			XElement root = doc.Root;
			if (!string.Equals(root.Name.LocalName, "UXML", StringComparison.InvariantCulture))
			{
				UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.InvalidRootElement, root.Name, UIElementsViewImporter.Error.Level.Fatal, root);
			}
			else
			{
				foreach (XElement current in root.Elements())
				{
					string localName = current.Name.LocalName;
					if (localName != null)
					{
						if (localName == "Using")
						{
							UIElementsViewImporter.LoadUsingNode(vta, root, current);
							continue;
						}
					}
					UIElementsViewImporter.LoadXml(current, null, vta, ssb);
				}
			}
		}

		private static void LoadUsingNode(VisualTreeAsset vta, XElement elt, XElement child)
		{
			bool flag = false;
			string text = null;
			string path = null;
			foreach (XAttribute current in child.Attributes())
			{
				string localName = current.Name.LocalName;
				if (localName == null)
				{
					goto IL_99;
				}
				if (!(localName == "path"))
				{
					if (!(localName == "alias"))
					{
						goto IL_99;
					}
					text = current.Value;
					if (text == string.Empty)
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.UsingHasEmptyAlias, child, UIElementsViewImporter.Error.Level.Fatal, child);
					}
				}
				else
				{
					flag = true;
					path = current.Value;
				}
				continue;
				IL_99:
				UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.UnknownAttribute, current.Name.LocalName, UIElementsViewImporter.Error.Level.Fatal, child);
			}
			if (!flag)
			{
				UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.MissingPathAttributeOnUsing, null, UIElementsViewImporter.Error.Level.Fatal, elt);
			}
			else
			{
				if (string.IsNullOrEmpty(text))
				{
					text = Path.GetFileNameWithoutExtension(path);
				}
				if (vta.AliasExists(text))
				{
					UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.DuplicateUsingAlias, text, UIElementsViewImporter.Error.Level.Fatal, elt);
				}
				else
				{
					vta.RegisterUsing(text, path);
				}
			}
		}

		private static void LoadXml(XElement elt, VisualElementAsset parent, VisualTreeAsset vta, StyleSheetBuilder ssb)
		{
			VisualElementAsset visualElementAsset;
			if (!UIElementsViewImporter.ResolveType(elt, vta, out visualElementAsset))
			{
				UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.UnknownElement, elt.Name.LocalName, UIElementsViewImporter.Error.Level.Fatal, elt);
			}
			else
			{
				int num = (parent != null) ? parent.id : 0;
				int id = num << 1 ^ visualElementAsset.GetHashCode();
				visualElementAsset.parentId = num;
				visualElementAsset.id = id;
				bool flag = UIElementsViewImporter.ParseAttributes(elt, visualElementAsset, ssb, vta, parent);
				visualElementAsset.ruleIndex = ((!flag) ? -1 : ssb.EndRule());
				if (visualElementAsset is TemplateAsset)
				{
					vta.templateAssets.Add((TemplateAsset)visualElementAsset);
				}
				else
				{
					vta.visualElementAssets.Add(visualElementAsset);
				}
				if (elt.HasElements)
				{
					foreach (XElement current in elt.Elements())
					{
						if (current.Name.LocalName == "Style")
						{
							UIElementsViewImporter.LoadStyleReferenceNode(visualElementAsset, current);
						}
						else
						{
							UIElementsViewImporter.LoadXml(current, visualElementAsset, vta, ssb);
						}
					}
				}
			}
		}

		private static void LoadStyleReferenceNode(VisualElementAsset vea, XElement styleElt)
		{
			XAttribute xAttribute = styleElt.Attribute("path");
			if (xAttribute == null || string.IsNullOrEmpty(xAttribute.Value))
			{
				UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.StyleReferenceEmptyOrMissingPathAttr, null, UIElementsViewImporter.Error.Level.Warning, styleElt);
			}
			else
			{
				vea.stylesheets.Add(xAttribute.Value);
			}
		}

		private static bool ResolveType(XElement elt, VisualTreeAsset visualTreeAsset, out VisualElementAsset vea)
		{
			if (visualTreeAsset.AliasExists(elt.Name.LocalName))
			{
				vea = new TemplateAsset(elt.Name.LocalName);
			}
			else
			{
				string text = (!string.IsNullOrEmpty(elt.Name.NamespaceName)) ? (elt.Name.NamespaceName + "." + elt.Name.LocalName) : elt.Name.LocalName;
				if (text == typeof(VisualElement).FullName)
				{
					text = typeof(VisualContainer).FullName;
				}
				vea = new VisualElementAsset(text);
			}
			return true;
		}

		private static bool ParseAttributes(XElement elt, VisualElementAsset res, StyleSheetBuilder ssb, VisualTreeAsset vta, VisualElementAsset parent)
		{
			res.name = "_" + res.GetType().Name;
			bool result = false;
			foreach (XAttribute current in elt.Attributes())
			{
				string localName = current.Name.LocalName;
				switch (localName)
				{
				case "name":
					res.name = current.Value;
					continue;
				case "text":
					res.text = current.Value;
					continue;
				case "class":
					res.classes = current.Value.Split(new char[]
					{
						' '
					});
					continue;
				case "contentContainer":
					if (vta.contentContainerId != 0)
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.DuplicateContentContainer, null, UIElementsViewImporter.Error.Level.Fatal, elt);
						continue;
					}
					vta.contentContainerId = res.id;
					continue;
				case "slot-name":
					if (string.IsNullOrEmpty(current.Value))
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.SlotDefinitionHasEmptyName, null, UIElementsViewImporter.Error.Level.Fatal, elt);
					}
					else if (!vta.AddSlotDefinition(current.Value, res.id))
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.DuplicateSlotDefinition, current.Value, UIElementsViewImporter.Error.Level.Fatal, elt);
					}
					continue;
				case "slot":
				{
					TemplateAsset templateAsset = parent as TemplateAsset;
					if (templateAsset == null)
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.SlotUsageInNonTemplate, parent, UIElementsViewImporter.Error.Level.Fatal, elt);
						continue;
					}
					if (string.IsNullOrEmpty(current.Value))
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.SlotUsageHasEmptyName, null, UIElementsViewImporter.Error.Level.Fatal, elt);
						continue;
					}
					templateAsset.AddSlotUsage(current.Value, res.id);
					continue;
				}
				case "style":
				{
					StyleSheet styleSheet = new Parser().Parse("* { " + current.Value + " }");
					if (styleSheet.Errors.Count != 0)
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.InvalidCssInStyleAttribute, styleSheet.Errors.Aggregate("", (string s, StylesheetParseError error) => s + error.ToString() + "\n"), UIElementsViewImporter.Error.Level.Warning, current);
						continue;
					}
					if (styleSheet.StyleRules.Count != 1)
					{
						UIElementsViewImporter.logger.LogError(ImportErrorType.Semantic, ImportErrorCode.InvalidCssInStyleAttribute, "Expected one style rule, found " + styleSheet.StyleRules.Count, UIElementsViewImporter.Error.Level.Warning, current);
						continue;
					}
					ssb.BeginRule(-1);
					result = true;
					StyleSheetImportErrors errors = new StyleSheetImportErrors();
					foreach (Property current2 in styleSheet.StyleRules[0].Declarations)
					{
						ssb.BeginProperty(current2.Name);
						StyleSheetImporter.VisitValue(errors, ssb, current2.Term);
						ssb.EndProperty();
					}
					continue;
				}
				case "pickingMode":
					if (!Enum.IsDefined(typeof(PickingMode), current.Value))
					{
						Debug.LogErrorFormat("Could not parse value of '{0}', because it isn't defined in the PickingMode enum.", new object[]
						{
							current.Value
						});
						continue;
					}
					res.pickingMode = (PickingMode)Enum.Parse(typeof(PickingMode), current.Value);
					continue;
				}
				res.AddProperty(current.Name.LocalName, current.Value);
			}
			return result;
		}
	}
}
