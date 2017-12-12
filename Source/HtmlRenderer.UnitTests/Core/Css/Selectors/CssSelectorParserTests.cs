namespace HtmlRenderer.UnitTests.Core.Css.Selectors
{
	using System;
	using System.Collections.Generic;
	using NUnit.Framework;
	using Pidgin;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;

	[TestFixture]
	public class CssSelectorParserTests
	{
		#region NamespacePrefix

		[Test]
		[TestCaseSource(nameof(NamespacePrefixTestCases))]
		public void NamespacePrefix(string css, string expected)
		{
			VerifyRule(CssSelectorGrammar.NamespacePrefix, css, expected);
		}

		public static IEnumerable<TestCaseData> NamespacePrefixTestCases
		{
			get
			{
				yield return new TestCaseData("*|", "*");
				yield return new TestCaseData("|", "");
				yield return new TestCaseData("x|", "x");
				yield return new TestCaseData("abc|", "abc");
				yield return new TestCaseData("_z|", "_z");
				yield return new TestCaseData("-z|", "-z");
			}
		}

		#endregion

		#region ElementName

		[Test]
		[TestCaseSource(nameof(ElementNameTestCases))]
		public void ElementName(string css, CssSelectorGrammar.QualifiedName expected)
		{
			VerifyRule(CssSelectorGrammar.ElementName, css, expected);
		}

		public static IEnumerable<TestCaseData> ElementNameTestCases
		{
			get
			{
				yield return new TestCaseData("*|*", new CssSelectorGrammar.QualifiedName("*", "*"));
				yield return new TestCaseData("|*", new CssSelectorGrammar.QualifiedName("*", ""));
				yield return new TestCaseData("*", new CssSelectorGrammar.QualifiedName("*", null));
				yield return new TestCaseData("*|x", new CssSelectorGrammar.QualifiedName("x", "*"));
				yield return new TestCaseData("abc|*", new CssSelectorGrammar.QualifiedName("*", "abc"));
				yield return new TestCaseData("|abc", new CssSelectorGrammar.QualifiedName("abc", ""));
				yield return new TestCaseData("abc", new CssSelectorGrammar.QualifiedName("abc", null));
				yield return new TestCaseData("a|x", new CssSelectorGrammar.QualifiedName("x", "a"));
				yield return new TestCaseData("abc|href", new CssSelectorGrammar.QualifiedName("href", "abc"));
			}
		}

		#endregion

		#region TypeSelector

		[Test]
		[TestCaseSource(nameof(TypeSelectorTestCases))]
		public void TypeSelector(string css, CssTypeSelector expected)
		{
			VerifyRule(CssSelectorGrammar.TypeSelector, css, expected);
		}

		public static IEnumerable<TestCaseData> TypeSelectorTestCases
		{
			get
			{
				yield return new TestCaseData("*|*", CssSelector.Universal);
				yield return new TestCaseData("|*", CssSelector.WithElement("*", ""));
				yield return new TestCaseData("*", CssSelector.WithElement("*", null));
				yield return new TestCaseData("x", CssSelector.WithElement("x"));
				yield return new TestCaseData("abc", CssSelector.WithElement("abc"));
				yield return new TestCaseData("*|x", CssSelector.WithElement("x", "*"));
				yield return new TestCaseData("abc|*", CssSelector.WithElement("*", "abc"));
				yield return new TestCaseData("|abc", CssSelector.WithElement("abc", ""));
				yield return new TestCaseData("abc", CssSelector.WithElement("abc", null));
				yield return new TestCaseData("a|x", CssSelector.WithElement("x", "a"));
				yield return new TestCaseData("abc|href", CssSelector.WithElement("href", "abc"));
			}
		}

		#endregion

		#region Attribute selectors

		[Test]
		[TestCaseSource(nameof(AttributeSelectorTestCases))]
		public void AttributeSelector(string css, CssAttributeSelector expected)
		{
			VerifyRule(CssSelectorGrammar.AttributeSelector, css, expected);
		}

		public static IEnumerable<TestCaseData> AttributeSelectorTestCases
		{
			get
			{
				yield return new TestCaseData("[name]", CssSelector.WithAttribute("name"));
				yield return new TestCaseData("[*|name]", CssSelector.WithAttribute("name", "*"));
				yield return new TestCaseData("[|-name]", CssSelector.WithAttribute("-name", ""));
				yield return new TestCaseData("[ns|-n]", CssSelector.WithAttribute("-n", "ns"));
				yield return new TestCaseData("[name=val]", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Exact, "val"));
				yield return new TestCaseData("[name='val']", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Exact, "val"));
				yield return new TestCaseData("[name=\"val\"]", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Exact, "val"));
				yield return new TestCaseData("[ns|name=val]", CssSelector.WithAttribute("name", "ns", CssAttributeMatchOperator.Exact, "val"));
				yield return new TestCaseData("[name^='val']", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Prefix, "val"));
				yield return new TestCaseData("[name$=\"val\"]", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Suffix, "val"));
				yield return new TestCaseData("[name*=val]", CssSelector.WithAttribute("name", CssAttributeMatchOperator.Contains, "val"));
				yield return new TestCaseData("[name~='val']", CssSelector.WithAttribute("name", CssAttributeMatchOperator.ContainsWord, "val"));
				yield return new TestCaseData("[name|=\"val\"]", CssSelector.WithAttribute("name", CssAttributeMatchOperator.LanguageCode, "val"));
			}
		}

		#endregion

		#region Class and id selectors

		[Test]
		public void ClassSelector()
		{
			VerifyRule(CssSelectorGrammar.ClassSelector, ".some-class", CssSelector.WithClass("some-class"));
		}

		[Test]
		public void IdSelector()
		{
			VerifyRule(CssSelectorGrammar.IdSelector, "#id1", CssSelector.WithId("id1"));
		}

		#endregion

		#region Pseudo classes

		[Test]
		[TestCaseSource(nameof(PseudoClassSelectorTestCases))]
		public void PseudoClass(string css, CssPseudoClassSelector expected)
		{
			VerifyRule(CssSelectorGrammar.PseudoSelector, css, expected);
		}

		public static IEnumerable<TestCaseData> PseudoClassSelectorTestCases
		{
			get
			{
				yield return new TestCaseData(":link", CssSelector.WithPseudoClass("link"));
				yield return new TestCaseData(":visited", CssSelector.WithPseudoClass("visited"));
				yield return new TestCaseData(":hover", CssSelector.WithPseudoClass("hover"));
				yield return new TestCaseData(":active", CssSelector.WithPseudoClass("active"));
				yield return new TestCaseData(":focus", CssSelector.WithPseudoClass("focus"));
				yield return new TestCaseData(":target", CssSelector.WithPseudoClass("target"));
				yield return new TestCaseData(":enabled", CssSelector.WithPseudoClass("enabled"));
				yield return new TestCaseData(":disabled", CssSelector.WithPseudoClass("disabled"));
				yield return new TestCaseData(":checked", CssSelector.WithPseudoClass("checked"));
				//yield return new TestCaseData(":indeterminate", CssSelector.WithPseudoClass("indeterminate"));
				yield return new TestCaseData(":root", CssSelector.WithPseudoClass("root"));

				yield return new TestCaseData(":lang(fr)", CssSelector.WithLanguage("fr"));
				yield return new TestCaseData(":lang(fr-BE)", CssSelector.WithLanguage("fr-BE"));
				yield return new TestCaseData(":lang(fr-BE)", CssSelector.WithLanguage("fr-BE"));

				foreach (var testCase in StructuralPseudoClassTestCases("nth-child", CssSelector.NthChild)) yield return testCase;
				foreach (var testCase in StructuralPseudoClassTestCases("nth-last-child", CssSelector.NthLastChild)) yield return testCase;
				foreach (var testCase in StructuralPseudoClassTestCases("nth-of-type", CssSelector.NthOfType)) yield return testCase;
				foreach (var testCase in StructuralPseudoClassTestCases("nth-last-of-type", CssSelector.NthLastOfType)) yield return testCase;
			}
		}

		private static IEnumerable<TestCaseData> StructuralPseudoClassTestCases(string className, Func<CssCycleOffset, CssStructuralPseudoClassSelector> factory)
		{
			yield return new TestCaseData(":" + className + "(2n+1)", factory(new CssCycleOffset(2, 1)));
			yield return new TestCaseData(":" + className + "(odd)", factory(new CssCycleOffset(2, 1)));
			yield return new TestCaseData(":" + className + "(2n)", factory(new CssCycleOffset(2, 0)));
			yield return new TestCaseData(":" + className + "(even)", factory(new CssCycleOffset(2, 0)));
			yield return new TestCaseData(":" + className + "(10n-1)", factory(new CssCycleOffset(10, -1)));
			yield return new TestCaseData(":" + className + "(0n+5)", factory(new CssCycleOffset(0, 5)));
			yield return new TestCaseData(":" + className + "(5)", factory(new CssCycleOffset(0, 5)));
			yield return new TestCaseData(":" + className + "(n)", factory(new CssCycleOffset(1, 0)));
			yield return new TestCaseData(":" + className + "( 3n + 1 )", factory(new CssCycleOffset(3, 1)));
			yield return new TestCaseData(":" + className + "( +3n - 2 )", factory(new CssCycleOffset(3, -2)));
			yield return new TestCaseData(":" + className + "( -n+ 6 )", factory(new CssCycleOffset(-1, 6)));
		}

		#endregion

		#region Private methods

		private static void VerifyRule<T>(Parser<CssToken, T> parser, string css, T expected)
		{
			var tokenizer = new CssTokenizer(new CssReader(css));
			Assert.That(parser.ParseOrThrow(tokenizer.Tokenize()), Is.EqualTo(expected));
		}

		#endregion
	}
}
