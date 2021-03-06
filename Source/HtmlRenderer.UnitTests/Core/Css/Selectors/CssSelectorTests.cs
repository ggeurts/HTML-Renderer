﻿namespace HtmlRenderer.UnitTests.Core.Css.Selectors
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;
	using NSubstitute;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	[TestFixture]
	[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
	public class CssSelectorTests
	{
		#region Constants

		private const string XHTML_NAMESPACE = "https://www.w3.org/1999/xhtml/";
		private const string SOME_NAMESPACE = "http://test.org/schema";

		#endregion

		#region Instance fields

		private readonly XmlNameTable _nameTable = new NameTable();
		private XmlNamespaceManager _namespaceManager;

		#endregion

		#region SetUp / TearDown

		[SetUp]
		public void SetUp()
		{
			_namespaceManager = new XmlNamespaceManager(_nameTable);
		}

		#endregion

		#region Universal selector

		[Test]
		public void UniversalSelector()
		{
			var selector = CssSelector.Universal;
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("*"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void UniversalSelector_ToString()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			Assert.That(CssSelector.Universal.ToString(), Is.EqualTo("*|*"));
		}

		[Test]
		public void UniversalSelector_MatchesAllElements()
		{
			var xdoc = XDocument.Parse("<html><head /><body /></html>");
			var allElements = xdoc.Root.DescendantsAndSelf().ToList();

			var matchingElements = Match(xdoc, CssSelector.Universal);
			Assert.That(matchingElements, Is.EquivalentTo(allElements));
		}

		#endregion

		#region Element selectors

		[Test]
		public void ElementSelector_ForNameInUnspecifiedNamespace()
		{
			var selector = CssSelector.WithElement("h1");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.Null, nameof(selector.NamespacePrefix));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 1)), nameof(selector.Specificity));
			});
		}

		public void ElementSelector_ForNameInUnspecifiedNamespace_ToString()
		{
			Assert.That(CssSelector.WithElement("h1").ToString(), Is.EqualTo("h1"));
		}

		[Test]
		public void ElementSelector_ForNameInUnspecifiedNamespace_MatchesNamesInAnyNamespace_WhenNoDefaultNamespaceDefined()
		{
			var selector = CssSelector.WithElement("h1");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:x='{0}'>
					<head />
					<body>
						<h1 id='h1_1'>Title 1</h1>
						<x:h1 id='h1_2'>Title 2</x:h1>
						<h2 id='h2'>Title 2.1</h2>
					</body>
				</html>", XHTML_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1_1", "h1_2" }));
		}

		[Test]
		public void ElementSelector_ForNameInUnspecifiedNamespace_MatchesNamesInDefaultNamespace_WhenDefaultNamespaceDefined()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);

			var selector = CssSelector.WithElement("h1");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns='{0}' xmlns:s='{1}'>
					<head />
					<body>
						<h1 id='h1_1'>Title 1</h1>
						<s:h1 id='h1_2'>Title 2</s:h1>
						<h2 id='h2'>Title 2.1</h2>
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1_1" }));
		}

		[Test]
		public void ElementSelector_ForNameInAnyNamespace()
		{
			var selector = CssSelector.WithElement("h1", "*");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 1)), nameof(selector.Specificity));
			});
		}

		public void ElementSelector_ForNameInAnyNamespace_ToString()
		{
			Assert.That(CssSelector.WithElement("h1", "*").ToString(), Is.EqualTo("*|h1"));
		}

		[Test]
		public void ElementSelector_ForNameInAnyNamespace_Matches()
		{
			var selector = CssSelector.WithElement("h1", "*");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:x='{0}'>
					<head />
					<body>
						<h1 id='h1_1'>Title 1</h1>
						<x:h1 id='h1_2'>Title 2</x:h1>
						<h2 id='h2'>Title 2.1</h2>
					</body>
				</html>", XHTML_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1_1", "h1_2" }));
		}


		[Test]
		public void ElementSelector_ForNameWithoutNamespace()
		{
			var selector = CssSelector.WithElement("h1", "");

			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.Empty, nameof(selector.NamespacePrefix));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 1)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void ElementSelector_ForNameWithoutNamespace_ToString()
		{
			Assert.That(CssSelector.WithElement("h1", "").ToString(), Is.EqualTo("|h1"));
		}

		[Test]
		public void ElementSelector_ForNameInNonDefaultNamespace()
		{
			var selector = CssSelector.WithElement("h1", "x");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("x"), nameof(selector.NamespacePrefix));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 1)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void ElementSelector_ForNameInNonDefaultNamespace_ToString()
		{
			Assert.That(CssSelector.WithElement("h1", "x").ToString(), Is.EqualTo("x|h1"));
		}

		[Test]
		public void ElementSelector_ForNameInNonDefaultNamespace_Matches()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);
			_namespaceManager.AddNamespace("s", SOME_NAMESPACE);

			var selector = CssSelector.WithElement("h1", "s");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:x='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<x:h1 id='h1_1'>Title 1</x:h1>
						<t:h1 id='h1_2'>Title 1</t:h1>
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1_2" }));
		}

		#endregion

		#region Attribute selectors

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithAnyValue()
		{
			var selector = CssSelector.WithAttribute("href", "");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo(""), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithAnyValue_ToString()
		{
			var selector = CssSelector.WithAttribute("href", "");
			Assert.That(selector.ToString(), Is.EqualTo("[|href]"));
		}

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithAnyValue_Matches()
		{
			var selector = CssSelector.WithAttribute("href", "");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<a id='anchor1' href='http://some.where.else' />
						<a id='anchor2' />
					</body>
				</html>");

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor1" }));
		}

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithExactValue()
		{
			var selector = CssSelector.WithAttribute("href", "", CssAttributeMatchOperator.Exact, "#some-target");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo(""), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Exact), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("#some-target"), nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithExactValue_ToString()
		{
			var selector = CssSelector.WithAttribute("href", "", CssAttributeMatchOperator.Exact, "#some-target");
			Assert.That(selector.ToString(), Is.EqualTo("[|href=\"#some-target\"]"));
		}

		[Test]
		public void AttributeSelector_ForNameWithoutNamespace_WithExactValue_Matches()
		{
			var selector = CssSelector.WithAttribute("href", "", CssAttributeMatchOperator.Exact, "#another-target");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' href='#another-target' />
					</body>
				</html>");

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor2" }));
		}

		[Test]
		public void AttributeSelector_ForNameInUnspecifiedNamespace()
		{
			var selector = CssSelector.WithAttribute("href");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.Null, nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForNameInUnspecifiedNamespace_WithAnyValue_ToString()
		{
			var selector = CssSelector.WithAttribute("href");
			Assert.That(selector.ToString(), Is.EqualTo("[href]"));
		}

		[Test]
		public void AttributeSelector_ForNameInUnspecifiedNamespace_WithAnyValue_MatchesNamesWithoutNamespace_WhenDefaultNamespaceDefined()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);

			var selector = CssSelector.WithAttribute("href");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' s:href='#another-target' />
						<a id='anchor3' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor1" }));
		}

		[Test]
		public void AttributeSelector_ForNameInUnspecifiedNamespace_WithAnyValue_MatchesNamesInAnyNamespace_WhenNoDefaultNamespaceDefined()
		{
			var selector = CssSelector.WithAttribute("href");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' s:href='#another-target' />
						<a id='anchor3' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor1", "anchor2" }));
		}


		[Test]
		public void AttributeSelector_ForNameInAnyNamespace_WithAnyValue()
		{
			var selector = CssSelector.WithAttribute("href", "*");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForNameInAnyNamespace_WithAnyValue_ToString()
		{
			var selector = CssSelector.WithAttribute("href", "*");
			Assert.That(selector.ToString(), Is.EqualTo("[*|href]"));
		}

		[Test]
		public void AttributeSelector_ForNameInAnyNamespace_WithAnyValue_Matches()
		{
			var selector = CssSelector.WithAttribute("href", "*");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' s:href='#another-target' />
						<a id='anchor3' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor1", "anchor2" }));
		}

		[Test]
		public void AttributeSelector_ForNameInAnyNamespace_WithValueContainingWord()
		{
			var selector = CssSelector.WithAttribute("href", "*", CssAttributeMatchOperator.ContainsWord, "test");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.ContainsWord), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("test"), nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForNameInAnyNamespace_WithValueContainingWord_ToString()
		{
			var selector = CssSelector.WithAttribute("href", "*", CssAttributeMatchOperator.ContainsWord, "test");
			Assert.That(selector.ToString(), Is.EqualTo("[*|href~=\"test\"]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithValueContainingWord_Matches()
		{
			var selector = CssSelector.WithAttribute("href", "*", CssAttributeMatchOperator.ContainsWord, "test");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' href='blurb' />
						<a id='anchor2' s:href='blurb with test' />
						<a id='anchor3' t:href='test' />
						<a id='anchor4' t:href='a test too' />
						<a id='anchor5' t:href='more tests' />
						<a id='anchor6' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor2", "anchor3", "anchor4" }));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithAnyValue()
		{
			var selector = CssSelector.WithAttribute("href", "x");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("x"), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithAnyValue_ToString()
		{
			Assert.That(CssSelector.WithAttribute("href", "x").ToString(), Is.EqualTo("[x|href]"));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode()
		{
			var selector = CssSelector.WithAttribute("href", "x", CssAttributeMatchOperator.LanguageCode, "fr");
			Assert.Multiple(() =>
			{
				Assert.That(selector, Is.InstanceOf<CssSimpleSelector>());
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("x"), nameof(selector.NamespacePrefix));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.LanguageCode), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("fr"), nameof(selector.MatchOperand));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode_ToString()
		{
			var selector = CssSelector.WithAttribute("lang", "x", CssAttributeMatchOperator.LanguageCode, "fr");
			Assert.That(selector.ToString(), Is.EqualTo("[x|lang|=\"fr\"]"));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode_Matches()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);
			_namespaceManager.AddNamespace("s", SOME_NAMESPACE);

			var selector = CssSelector.WithAttribute("lang", "x", CssAttributeMatchOperator.LanguageCode, "fr");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:x='{0}' xmlns:s='{1}'>
					<head />
					<body>
						<a id='anchor1' x:lang='nl' />
						<a id='anchor2' x:lang='fr' />
						<a id='anchor3' x:lang='french' />
						<a id='anchor4' x:lang='fr-FR' />
						<a id='anchor5' x:lang='-fr' />
						<a id='anchor6' s:lang='fr' />
						<a id='anchor7' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor2", "anchor4" }));
		}

		#endregion

		#region Class selectors

		[Test]
		public void ClassSelector()
		{
			var selector = CssSelector.WithClass("sect");
			Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
		}

		[Test]
		public void ClassSelector_ToString()
		{
			Assert.That(CssSelector.WithClass("sect").ToString(), Is.EqualTo(".sect"));
		}

		[Test]
		public void ClassSelector_Matches()
		{
			var selector = CssSelector.WithClass("sect");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<p id='par1' class='sect' />
						<p id='par2' class='section' />
						<p id='par3' class='sect note' />
						<p id='par4' class='right sect note' />
						<p id='par5' class='right sect' />
						<p id='par6' class='' />
						<p id='par7' />
					</body>
				</html>");

			var matchingElements = xdoc.Descendants().Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "par1", "par3", "par4", "par5" }));
		}

		#endregion

		#region ID selectors

		[Test]
		public void IdSelector()
		{
			var selector = CssSelector.WithId("par1");
			Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(1, 0, 0)), nameof(selector.Specificity));
		}

		[Test]
		public void IdSelector_ToString()
		{
			Assert.That(CssSelector.WithId("par1").ToString(), Is.EqualTo("#par1"));
		}

		[Test]
		public void IdSelector_Matches()
		{
			var selector = CssSelector.WithId("par1");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<p id='par1' />
						<p id='par2' />
						<p id='par1b' />
						<p id='PAR1' />
						<p id='' />
						<p />
					</body>
				</html>");

			var matchingElements = xdoc.Descendants().Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "par1" }));
		}

		#endregion

		#region Pseudo class selectors

		[Test]
		public void PseudoClassSelector_ForStandardPseudoclass()
		{
			var selector = CssSelector.WithPseudoClass("target");
			Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
		}

		[Test]
		public void PseudoClassSelector_ForNonStandardPseudoclass()
		{
			var selector = CssSelector.WithPseudoClass("unknown");
			Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 1, 0)), nameof(selector.Specificity));
		}

		[Test]
		public void PseudoClassSelector_TargetPseudoClass_ToString()
		{
			Assert.That(CssSelector.WithPseudoClass("target").ToString(), Is.EqualTo(":target"));
		}

		[Test]
		public void PseudoClassSelector_TargetPseudoClass_Matches()
		{
			var selector = CssSelector.WithPseudoClass("target");

			var pseudoClassInfo = Substitute.For<XElementPseudoClassInfo>();
			pseudoClassInfo.IsTarget(Arg.Any<XElement>())
				.Returns(call => "true".Equals(call.Arg<XElement>().Attribute("istarget")?.Value, StringComparison.OrdinalIgnoreCase));

			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<h1 id='ch1' />
						<h1 id='ch2' istarget='true' />
						<h1 id='ch3' />
					</body>
				</html>");

			var matchingElements = xdoc.Descendants().Where(e => selector.Matches(new XElementInfo(e, pseudoClassInfo))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "ch2" }));
		}

		// TODO: Don't allow arbitrary parameterized pseudo classes such as :lang() or :nth-child()
		// TODO: Add tests for :lang() and :nth-child() type selectors
		// TODO: Add not() tests

		#endregion

		#region Sequence tests
		
		[Test]
		public void SelectorSequence_WithTwoSelectors()
		{
			var typeSelector = CssSelector.WithElement("h1");
			var extraSelector = CssSelector.WithId("chapter1");
			var selector = typeSelector.Add(extraSelector);

			Assert.Multiple(() =>
			{
				Assert.That(selector.TypeSelector, Is.SameAs(typeSelector), nameof(selector.TypeSelector));
				Assert.That(selector.OtherSelectors, Is.EquivalentTo(new[] { extraSelector }), nameof(selector.OtherSelectors));
				Assert.That(selector.Subject, Is.SameAs(selector), nameof(selector.Subject));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(1, 0, 1)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void SelectorSequence_WithTwoSelectors_ToString()
		{
			var selector = CssSelector.WithElement("h1", null)
				.Add(CssSelector.WithId("chapter1"));
			Assert.That(selector.ToString(), Is.EqualTo("h1#chapter1"));
		}

		[Test]
		public void SelectorSequence_WithTwoSelectors_Matches()
		{
			var selector = CssSelector.WithElement("h1", null)
				.Add(CssSelector.WithId("chapter1"));

			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<p id='chapter1' />
						<h1 id='chapter1' />
						<h1 id='ch2' />
						<h1 id='ch3' />
					</body>
				</html>");

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Name.LocalName + "." + e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1.chapter1" }));
		}

		[Test]
		public void SelectorSequence_WithThreeSelectors()
		{
			var typeSelector = CssSelector.WithElement("h1", null);
			var extraSelectors = new CssSimpleSelector[]
			{
				CssSelector.WithId("chapter1"),
				CssSelector.WithPseudoClass("target")
			};

			var selector = typeSelector.Add(extraSelectors[0]);
			for (var i = 1; i < extraSelectors.Length; i++)
			{
				selector = selector.Add(extraSelectors[i]);
			}

			Assert.Multiple(() =>
			{
				Assert.That(selector.TypeSelector, Is.SameAs(typeSelector), nameof(selector.TypeSelector));
				Assert.That(selector.OtherSelectors, Is.EquivalentTo(extraSelectors), nameof(selector.OtherSelectors));
				Assert.That(selector.Subject, Is.SameAs(selector), nameof(selector.Subject));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(1, 1, 1)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void SelectorSequence_WithThreeSelectors_ToString()
		{
			var selector = CssSelector.WithElement("h1", null)
				.Add(CssSelector.WithId("chapter1"))
				.Add(CssSelector.WithPseudoClass("target"));
			Assert.That(selector.ToString(), Is.EqualTo("h1#chapter1:target"));

		}

		[Test]
		public void SelectorSequence_WithThreeSelectors_Matches()
		{
			var selector = CssSelector.WithElement("h1", null)
				.Add(CssSelector.WithId("chapter1"));

			var pseudoClassInfo = Substitute.For<XElementPseudoClassInfo>();
			pseudoClassInfo.IsTarget(Arg.Any<XElement>())
				.Returns(call => "true".Equals(call.Arg<XElement>().Attribute("istarget")?.Value, StringComparison.OrdinalIgnoreCase));

			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<p id='chapter1' istarget='true' />
						<h1 id='chapter1' istarget='true' />
						<h1 id='ch2' istarget='true' />
						<h1 id='ch3' />
					</body>
				</html>");

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Name.LocalName + "." + e.Attribute("id").Value), Is.EquivalentTo(new[] { "h1.chapter1" }));
		}

		#endregion

		#region Combinator tests

		[Test]
		public void DescendantCombinator_WithoutNesting()
		{
			var leftSelector = CssSelector.WithElement("h1");
			var rightSelector = CssSelector.WithElement("em");
			var selector = leftSelector.Combine(CssCombinator.Descendant, rightSelector);

			Assert.Multiple(() =>
			{
				Assert.That(selector.Subject, Is.SameAs(rightSelector), nameof(selector.Subject));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 2)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void DescendantCombinator_WithoutNesting_ToString()
		{
			var leftSelector = CssSelector.WithElement("h1", null);
			var rightSelector = CssSelector.WithElement("em", null);
			var selector = leftSelector.Combine(CssCombinator.Descendant, rightSelector);

			Assert.That(selector.ToString(), Is.EqualTo("h1 em"));
		}

		[Test]
		public void DescendantCombinator_WithNesting()
		{
			var selector1 = CssSelector.WithElement("div");
			var selector2 = CssSelector.WithElement("ol");
			var selector3 = CssSelector.WithElement("li");
			var selector4 = CssSelector.WithElement("p");
			var selector = selector1
				.Combine(CssCombinator.Descendant, selector2)
				.Combine(CssCombinator.Descendant, selector3)
				.Combine(CssCombinator.Descendant, selector4);

			Assert.Multiple(() =>
			{
				Assert.That(selector.Subject, Is.SameAs(selector4), nameof(selector.Subject));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 4)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void DescendantCombinator_WithNesting_ToString()
		{
			var selector = CssSelector.WithElement("div", null)
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("ol", null))
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("li", null))
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("p", null));

			Assert.That(selector.ToString(), Is.EqualTo("div ol li p"));
		}

		[Test]
		public void DescendantCombinator_WithNesting_Matches()
		{
			var selector = CssSelector.WithElement("div", null)
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("ol", null))
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("li", null))
				.Combine(CssCombinator.Descendant, CssSelector.WithElement("p", null));

			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<div id='div00'>
							<ol id='ol01'>
								<li id='li01'><p id='p01' /></li>
								<li id='li02'><p id='p02_1' /><p id='p02_2' /></li>
								<li id='li03' />
								<li id='li04'><div id='div04'><p id='p04' /></div></li>
								<div id='div05'>
									<li id='li05'><p id='p05' /></li>
								</div>
							</ol>
						</div>
						<ol id='ol02'>
							<li id='li06'><p id='p06' /></li>
						</ol>
					</body>
				</html>");

			var matchingElements = Match(xdoc, selector);
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "p01", "p02_1", "p02_2", "p04", "p05" }));
		}

		[Test]
		public void ChildCombinator_WithoutNesting()
		{
			var leftSelector = CssSelector.WithElement("h1", null);
			var rightSelector = CssSelector.WithElement("em", null);
			var selector = leftSelector.Combine(CssCombinator.Child, rightSelector);

			Assert.Multiple(() =>
			{
				Assert.That(selector.Subject, Is.SameAs(rightSelector), nameof(selector.Subject));
				Assert.That(selector.Specificity, Is.EqualTo(new CssSpecificity(0, 0, 2)), nameof(selector.Specificity));
			});
		}

		[Test]
		public void ChildCombinator_WithoutNesting_ToString()
		{
			var leftSelector = CssSelector.WithElement("h1", null);
			var rightSelector = CssSelector.WithElement("em", null);
			var selector = leftSelector.Combine(CssCombinator.Child, rightSelector);

			Assert.That(selector.ToString(), Is.EqualTo("h1 > em"));
		}

		#endregion

		#region Factory methods

		private IEnumerable<XElement> Match(XDocument xdoc, ICssSelector selector)
		{
			var matcher = selector.BuildMatcher(_namespaceManager);
			return xdoc.Descendants().Where(e => matcher.Matches(new XElementInfo(e)));
		}

		#endregion

		#region Inner classes

		public class XElementPseudoClassInfo
		{
			public static readonly XElementPseudoClassInfo Default = new XElementPseudoClassInfo();

			public virtual bool IsTarget(XElement element)
			{
				return false;
			}

			public virtual bool HasDynamicState(XElement element, CssDynamicElementState state)
			{
				return false;
			}
		}

		private struct XElementInfo : IElementInfo<XElementInfo>
		{
			private static readonly XName ClassAttributeName = XNamespace.None + "class";
			private static readonly XName IdAttributeName = XNamespace.None + "id";

			private readonly XElement _element;
			private readonly XElementPseudoClassInfo _pseudoClassInfo;

			public XElementInfo(XElement element, XElementPseudoClassInfo pseudoClassInfo = null)
			{
				_element = element;
				_pseudoClassInfo = pseudoClassInfo;
			}

			public XElementInfo Parent
			{
				get
				{
					return _element != null
						? new XElementInfo(_element.Parent)
						: default(XElementInfo);
				}
			}

			public bool IsRoot
			{
				get { return _element?.Parent == null; }
			}

			public bool IsTarget
			{
				get { return (_pseudoClassInfo ?? XElementPseudoClassInfo.Default).IsTarget(_element); }
			}

			public int ChildCount
			{
				get { return _element?.Elements().Count() ?? 0; }
			}

			public int ChildIndex
			{
				get { return _element?.ElementsBeforeSelf().Count() ?? 0; }
			}

			public int SiblingIndex
			{
				get { return _element?.ElementsBeforeSelf(_element.Name).Count() ?? 0; }
			}

			public int SiblingCount
			{
				get
				{
					if (_element == null) return 0;
					return _element.ElementsBeforeSelf(_element.Name).Count() 
						+ _element.ElementsAfterSelf(_element.Name).Count() 
						+ 1;
				}
			}

			public bool HasName(XName name)
			{
				return _element != null && _element.Name == name;
			}

			public bool HasName(string localName)
			{
				return _element != null && _element.Name.LocalName == localName;
			}

			public bool HasNamespace(XNamespace ns)
			{
				return _element != null && _element.Name.Namespace == ns;
			}

			public bool HasChildren
			{
				get { return _element != null && _element.HasElements; }
			}

			public bool HasAttribute(XName name, Func<string, StringComparison, bool> predicate)
			{
				if (_element == null) return false;

				var att = _element.Attribute(name);
				return att != null && predicate(att.Value, StringComparison.Ordinal);
			}

			public bool HasAttribute(string localName, Func<string, StringComparison, bool> predicate)
			{
				if (_element == null) return false;
				foreach (var att in _element.Attributes())
				{
					if (att.Name.LocalName == localName && predicate(att.Value, StringComparison.Ordinal)) return true;
				}
				return false;
			}

			public bool HasAttributeInNamespace(XNamespace ns, Func<string, StringComparison, bool> predicate)
			{
				if (_element == null) return false;
				foreach (var att in _element.Attributes())
				{
					if (att.Name.Namespace == ns && predicate(att.Value, StringComparison.Ordinal)) return true;
				}
				return false;
			}

			public bool HasClass(string name)
			{
				var value = _element?.Attribute(ClassAttributeName)?.Value;
				if (value == null) return false;

				var index = value.IndexOf(name, StringComparison.Ordinal);
				return index >= 0 
					&& (index == 0 || char.IsWhiteSpace(value[index - 1]))
					&& (index + name.Length >= value.Length || char.IsWhiteSpace(value[index + name.Length]));
			}


			public bool HasId(string id)
			{
				return _element?.Attribute(IdAttributeName)?.Value == id;

			}

			public bool HasLanguage(string ietfLanguageTag)
			{
				var actualLanguageTag = _element.Attribute(XNamespace.Xml + "lang")?.Value;
				return ietfLanguageTag != null
				       && actualLanguageTag != null
				       && actualLanguageTag.StartsWith(ietfLanguageTag, StringComparison.OrdinalIgnoreCase)
				       && (actualLanguageTag.Length == ietfLanguageTag.Length || actualLanguageTag[ietfLanguageTag.Length] == '-');
			}

			public bool HasDynamicState(CssDynamicElementState state)
			{
				return (_pseudoClassInfo ?? XElementPseudoClassInfo.Default).HasDynamicState(_element, state);
			}

			public bool TryGetPredecessor(ICssElementMatcher selector, bool immediateOnly, out XElementInfo result)
			{
				ArgChecker.AssertArgNotNull(selector, nameof(selector));

				var prevElement = PrevElement(_element);
				if (prevElement != null)
				{
					result = new XElementInfo(prevElement);
					if (selector.Matches(result)) return true;

					if (!immediateOnly)
					{
						prevElement = PrevElement(prevElement);
						while (prevElement != null)
						{
							result = new XElementInfo(prevElement);
							if (selector.Matches(result)) return true;

							prevElement = PrevElement(prevElement);
						}
					}
				}

				result = default(XElementInfo);
				return false;
			}

			private static XElement PrevElement(XElement element)
			{
				var prevNode = element?.PreviousNode;
				while (prevNode != null)
				{
					var prevElement = prevNode as XElement;
					if (prevElement != null) return prevElement;

					prevNode = prevNode.PreviousNode;
				}
				return null;
			}
		}

		#endregion
	}
}
