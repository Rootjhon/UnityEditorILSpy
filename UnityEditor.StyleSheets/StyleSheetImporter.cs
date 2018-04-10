using ExCSS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.UIElements.StyleSheets;
using UnityEngine.StyleSheets;

namespace UnityEditor.StyleSheets
{
	[ScriptedImporter(3, "uss", -20)]
	internal class StyleSheetImporter : ScriptedImporter
	{
		private Parser m_Parser;

		private const string k_ResourcePathFunctionName = "resource";

		private StyleSheetBuilder m_Builder;

		private StyleSheetImportErrors m_Errors;

		private static Dictionary<string, StyleValueKeyword> s_NameCache;

		public StyleSheetImporter()
		{
			this.m_Parser = new Parser();
			this.m_Builder = new StyleSheetBuilder();
			this.m_Errors = new StyleSheetImportErrors();
		}

		public override void OnImportAsset(AssetImportContext ctx)
		{
			StyleSheetImporter styleSheetImporter = new StyleSheetImporter();
			string contents = File.ReadAllText(ctx.assetPath);
			StyleSheet styleSheet = ScriptableObject.CreateInstance<StyleSheet>();
			styleSheet.hideFlags = HideFlags.NotEditable;
			styleSheetImporter.Import(styleSheet, contents, ctx.assetPath);
			ctx.AddObjectToAsset("stylesheet", styleSheet);
			StyleSheetAssetPostprocessor.ClearReferencedAssets();
			StyleContext.ClearStyleCache();
		}

		public void Import(StyleSheet asset, string contents, string contextAssetPath)
		{
			StyleSheet styleSheet = this.m_Parser.Parse(contents);
			if (styleSheet.Errors.Count > 0)
			{
				foreach (StylesheetParseError current in styleSheet.Errors)
				{
					this.m_Errors.AddSyntaxError(current.ToString());
				}
			}
			else
			{
				try
				{
					this.VisitSheet(styleSheet);
				}
				catch (Exception ex)
				{
					this.m_Errors.AddInternalError(ex.Message);
				}
			}
			if (this.m_Errors.hasErrors)
			{
				foreach (string current2 in this.m_Errors.FormatErrors())
				{
					Debug.LogErrorFormat(current2, new object[0]);
				}
			}
			else
			{
				this.m_Builder.BuildTo(asset);
			}
		}

		private void VisitSheet(StyleSheet styleSheet)
		{
			foreach (StyleRule current in styleSheet.StyleRules)
			{
				this.m_Builder.BeginRule(current.Line);
				this.VisitBaseSelector(current.Selector);
				foreach (Property current2 in current.Declarations)
				{
					this.m_Builder.BeginProperty(current2.Name);
					StyleSheetImporter.VisitValue(this.m_Errors, this.m_Builder, current2.Term);
					this.m_Builder.EndProperty();
				}
				this.m_Builder.EndRule();
			}
		}

		internal static void VisitValue(StyleSheetImportErrors errors, StyleSheetBuilder ssb, Term term)
		{
			PrimitiveTerm primitiveTerm = term as PrimitiveTerm;
			HtmlColor htmlColor = term as HtmlColor;
			GenericFunction genericFunction = term as GenericFunction;
			TermList termList = term as TermList;
			if (term == Term.Inherit)
			{
				ssb.AddValue(StyleValueKeyword.Inherit);
			}
			else if (primitiveTerm != null)
			{
				string text = term.ToString();
				UnitType primitiveType = primitiveTerm.PrimitiveType;
				switch (primitiveType)
				{
				case UnitType.String:
				{
					string value = text.Trim(new char[]
					{
						'\'',
						'"'
					});
					ssb.AddValue(value, StyleValueType.String);
					goto IL_F9;
				}
				case UnitType.Uri:
					IL_63:
					if (primitiveType != UnitType.Number && primitiveType != UnitType.Pixel)
					{
						errors.AddSemanticError(StyleSheetImportErrorCode.UnsupportedUnit, primitiveTerm.ToString());
						return;
					}
					ssb.AddValue(primitiveTerm.GetFloatValue(UnitType.Pixel).Value);
					goto IL_F9;
				case UnitType.Ident:
				{
					StyleValueKeyword keyword;
					if (StyleSheetImporter.TryParseKeyword(text, out keyword))
					{
						ssb.AddValue(keyword);
					}
					else
					{
						ssb.AddValue(text, StyleValueType.Enum);
					}
					goto IL_F9;
				}
				}
				goto IL_63;
				IL_F9:;
			}
			else if (htmlColor != null)
			{
				Color value2 = new Color((float)htmlColor.R / 255f, (float)htmlColor.G / 255f, (float)htmlColor.B / 255f, (float)htmlColor.A / 255f);
				ssb.AddValue(value2);
			}
			else if (genericFunction != null)
			{
				primitiveTerm = (genericFunction.Arguments.FirstOrDefault<Term>() as PrimitiveTerm);
				if (genericFunction.Name == "resource" && primitiveTerm != null)
				{
					string value3 = primitiveTerm.Value as string;
					ssb.AddValue(value3, StyleValueType.ResourcePath);
				}
				else
				{
					errors.AddSemanticError(StyleSheetImportErrorCode.UnsupportedFunction, genericFunction.Name);
				}
			}
			else if (termList != null)
			{
				foreach (Term current in termList)
				{
					StyleSheetImporter.VisitValue(errors, ssb, current);
				}
			}
			else
			{
				errors.AddInternalError(term.GetType().Name);
			}
		}

		private void VisitBaseSelector(BaseSelector selector)
		{
			AggregateSelectorList aggregateSelectorList = selector as AggregateSelectorList;
			if (aggregateSelectorList != null)
			{
				this.VisitSelectorList(aggregateSelectorList);
			}
			else
			{
				ComplexSelector complexSelector = selector as ComplexSelector;
				if (complexSelector != null)
				{
					this.VisitComplexSelector(complexSelector);
				}
				else
				{
					SimpleSelector simpleSelector = selector as SimpleSelector;
					if (simpleSelector != null)
					{
						this.VisitSimpleSelector(simpleSelector.ToString());
					}
				}
			}
		}

		private void VisitSelectorList(AggregateSelectorList selectorList)
		{
			if (selectorList.Delimiter == ",")
			{
				foreach (BaseSelector current in selectorList)
				{
					this.VisitBaseSelector(current);
				}
			}
			else if (selectorList.Delimiter == string.Empty)
			{
				this.VisitSimpleSelector(selectorList.ToString());
			}
			else
			{
				this.m_Errors.AddSemanticError(StyleSheetImportErrorCode.InvalidSelectorListDelimiter, selectorList.Delimiter);
			}
		}

		private void VisitComplexSelector(ComplexSelector complexSelector)
		{
			int selectorSpecificity = CSSSpec.GetSelectorSpecificity(complexSelector.ToString());
			if (selectorSpecificity == 0)
			{
				this.m_Errors.AddInternalError("Failed to calculate selector specificity " + complexSelector);
			}
			else
			{
				using (this.m_Builder.BeginComplexSelector(selectorSpecificity))
				{
					StyleSelectorRelationship previousRelationsip = StyleSelectorRelationship.None;
					foreach (CombinatorSelector current in complexSelector)
					{
						string text = this.ExtractSimpleSelector(current.Selector);
						if (string.IsNullOrEmpty(text))
						{
							this.m_Errors.AddInternalError("Expected simple selector inside complex selector " + text);
							break;
						}
						StyleSelectorPart[] parts;
						if (!this.CheckSimpleSelector(text, out parts))
						{
							break;
						}
						this.m_Builder.AddSimpleSelector(parts, previousRelationsip);
						Combinator delimiter = current.Delimiter;
						if (delimiter != Combinator.Child)
						{
							if (delimiter != Combinator.Descendent)
							{
								this.m_Errors.AddSemanticError(StyleSheetImportErrorCode.InvalidComplexSelectorDelimiter, complexSelector.ToString());
								break;
							}
							previousRelationsip = StyleSelectorRelationship.Descendent;
						}
						else
						{
							previousRelationsip = StyleSelectorRelationship.Child;
						}
					}
				}
			}
		}

		private void VisitSimpleSelector(string selector)
		{
			StyleSelectorPart[] parts;
			if (this.CheckSimpleSelector(selector, out parts))
			{
				int selectorSpecificity = CSSSpec.GetSelectorSpecificity(parts);
				if (selectorSpecificity == 0)
				{
					this.m_Errors.AddInternalError("Failed to calculate selector specificity " + selector);
				}
				else
				{
					using (this.m_Builder.BeginComplexSelector(selectorSpecificity))
					{
						this.m_Builder.AddSimpleSelector(parts, StyleSelectorRelationship.None);
					}
				}
			}
		}

		private string ExtractSimpleSelector(BaseSelector selector)
		{
			SimpleSelector simpleSelector = selector as SimpleSelector;
			string result;
			if (simpleSelector != null)
			{
				result = selector.ToString();
			}
			else
			{
				AggregateSelectorList aggregateSelectorList = selector as AggregateSelectorList;
				if (aggregateSelectorList != null && aggregateSelectorList.Delimiter == string.Empty)
				{
					result = aggregateSelectorList.ToString();
				}
				else
				{
					result = string.Empty;
				}
			}
			return result;
		}

		private static bool TryParseKeyword(string rawStr, out StyleValueKeyword value)
		{
			if (StyleSheetImporter.s_NameCache == null)
			{
				StyleSheetImporter.s_NameCache = new Dictionary<string, StyleValueKeyword>();
				IEnumerator enumerator = Enum.GetValues(typeof(StyleValueKeyword)).GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						StyleValueKeyword value2 = (StyleValueKeyword)enumerator.Current;
						StyleSheetImporter.s_NameCache[value2.ToString().ToLower()] = value2;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			return StyleSheetImporter.s_NameCache.TryGetValue(rawStr.ToLower(), out value);
		}

		private bool CheckSimpleSelector(string selector, out StyleSelectorPart[] parts)
		{
			bool result;
			if (!CSSSpec.ParseSelector(selector, out parts))
			{
				this.m_Errors.AddSemanticError(StyleSheetImportErrorCode.UnsupportedSelectorFormat, selector);
				result = false;
			}
			else if (parts.Any((StyleSelectorPart p) => p.type == StyleSelectorType.Unknown))
			{
				this.m_Errors.AddSemanticError(StyleSheetImportErrorCode.UnsupportedSelectorFormat, selector);
				result = false;
			}
			else if (parts.Any((StyleSelectorPart p) => p.type == StyleSelectorType.RecursivePseudoClass))
			{
				this.m_Errors.AddSemanticError(StyleSheetImportErrorCode.RecursiveSelectorDetected, selector);
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}
	}
}
