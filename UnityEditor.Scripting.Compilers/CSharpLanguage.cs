using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Visitors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Modules;

namespace UnityEditor.Scripting.Compilers
{
	internal class CSharpLanguage : SupportedLanguage
	{
		private class VisitorData
		{
			public string TargetClassName;

			public Stack<string> CurrentNamespaces;

			public string DiscoveredNamespace;

			public VisitorData()
			{
				this.CurrentNamespaces = new Stack<string>();
			}
		}

		private class NamespaceVisitor : AbstractAstVisitor
		{
			public override object VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, object data)
			{
				CSharpLanguage.VisitorData visitorData = (CSharpLanguage.VisitorData)data;
				visitorData.CurrentNamespaces.Push(namespaceDeclaration.Name);
				namespaceDeclaration.AcceptChildren(this, visitorData);
				visitorData.CurrentNamespaces.Pop();
				return null;
			}

			public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
			{
				CSharpLanguage.VisitorData visitorData = (CSharpLanguage.VisitorData)data;
				if (typeDeclaration.Name == visitorData.TargetClassName)
				{
					string text = string.Empty;
					foreach (string current in visitorData.CurrentNamespaces)
					{
						if (text == string.Empty)
						{
							text = current;
						}
						else
						{
							text = current + "." + text;
						}
					}
					visitorData.DiscoveredNamespace = text;
				}
				return null;
			}
		}

		private static Regex _crOnlyRegex = new Regex("\r(?!\n)", RegexOptions.Compiled);

		private static Regex _lfOnlyRegex = new Regex("(?<!\r)\n", RegexOptions.Compiled);

		public override string GetExtensionICanCompile()
		{
			return "cs";
		}

		public override string GetLanguageName()
		{
			return "CSharp";
		}

		internal static CSharpCompiler GetCSharpCompiler(BuildTarget targetPlatform, bool buildingForEditor, string assemblyName)
		{
			string targetStringFromBuildTarget = ModuleManager.GetTargetStringFromBuildTarget(targetPlatform);
			ICompilationExtension compilationExtension = ModuleManager.GetCompilationExtension(targetStringFromBuildTarget);
			return compilationExtension.GetCsCompiler(buildingForEditor, assemblyName);
		}

		public override ScriptCompilerBase CreateCompiler(MonoIsland island, bool buildingForEditor, BuildTarget targetPlatform, bool runUpdater)
		{
			CSharpCompiler cSharpCompiler = CSharpLanguage.GetCSharpCompiler(targetPlatform, buildingForEditor, island._output);
			ScriptCompilerBase result;
			if (cSharpCompiler != CSharpCompiler.Microsoft)
			{
				if (cSharpCompiler != CSharpCompiler.Mono)
				{
				}
				result = new MonoCSharpCompiler(island, runUpdater);
			}
			else
			{
				result = new MicrosoftCSharpCompiler(island, runUpdater);
			}
			return result;
		}

		public override string GetNamespace(string fileName, string definedSymbols)
		{
			string result;
			using (IParser parser = ParserFactory.CreateParser(ICSharpCode.NRefactory.SupportedLanguage.CSharp, CSharpLanguage.ReadAndConverteNewLines(fileName)))
			{
				HashSet<string> hashSet = new HashSet<string>(definedSymbols.Split(new char[]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries));
				foreach (string current in hashSet)
				{
					parser.Lexer.ConditionalCompilationSymbols.Add(current, string.Empty);
				}
				parser.Lexer.EvaluateConditionalCompilation = true;
				parser.Parse();
				try
				{
					CSharpLanguage.NamespaceVisitor visitor = new CSharpLanguage.NamespaceVisitor();
					CSharpLanguage.VisitorData visitorData = new CSharpLanguage.VisitorData
					{
						TargetClassName = Path.GetFileNameWithoutExtension(fileName)
					};
					parser.CompilationUnit.AcceptVisitor(visitor, visitorData);
					result = ((!string.IsNullOrEmpty(visitorData.DiscoveredNamespace)) ? visitorData.DiscoveredNamespace : string.Empty);
					return result;
				}
				catch
				{
				}
			}
			result = string.Empty;
			return result;
		}

		private static StringReader ReadAndConverteNewLines(string filePath)
		{
			string text = File.ReadAllText(filePath);
			text = CSharpLanguage._crOnlyRegex.Replace(text, "\r\n");
			text = CSharpLanguage._lfOnlyRegex.Replace(text, "\r\n");
			return new StringReader(text);
		}
	}
}
