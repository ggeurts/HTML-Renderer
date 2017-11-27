namespace HtmlRenderer.UnitTests.Core.Css.Selectors
{
	using System;
	using System.Linq;
	using System.Xml;
	using System.Xml.Linq;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	[TestFixture]
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
				Assert.That(selector.LocalName, Is.EqualTo("*"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get("*")), nameof(selector.Namespace));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		public void UniversalSelector_ToString_NoDefaultNamespaceExists()
		{
			Assert.That(CssSelector.Universal.ToString(_namespaceManager), Is.EqualTo("*"));
		}

		[Test]
		public void UniversalSelector_ToString_WhenDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			Assert.That(CssSelector.Universal.ToString(_namespaceManager), Is.EqualTo("*|*"));
		}

		[Test]
		public void UniversalSelector_ToString_WhenNoDefaultNamespaceExists()
		{
			Assert.That(CssSelector.Universal.ToString(_namespaceManager), Is.EqualTo("*"));
		}

		[Test]
		public void UniversalSelector_MatchesAllElements()
		{
			var xdoc = XDocument.Parse("<html><head /><body /></html>");
			var allElements = xdoc.Root.DescendantsAndSelf().ToList();
			var matchingElements = allElements.Where(e => CssSelector.Universal.Matches(new XElementInfo(e)));
			Assert.That(matchingElements, Is.EquivalentTo(allElements));
		}

		#endregion

		#region Type selectors

		[Test]
		public void TypeSelector_ForLocalNameInAnyNamespace()
		{
			var selector = CssSelector.WithLocalName("h1");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get("*")), nameof(selector.Namespace));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		public void TypeSelector_ForLocalNameInAnyNamespace_ToString_WhenDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			Assert.That(CssSelector.WithLocalName("h1").ToString(_namespaceManager), Is.EqualTo("*|h1"));
		}

		public void TypeSelector_ForLocalNameInAnyNamespace_ToString_WhenNoDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			Assert.That(CssSelector.WithLocalName("h1").ToString(_namespaceManager), Is.EqualTo("h1"));
		}

		[Test]
		public void TypeSelector_ForLocalNameInAnyNamespace_MatchesNodesByLocalName()
		{
			var selector = CssSelector.WithLocalName("h1");
			Assert.That(selector.Matches(CreateElement("h1")), Is.True, "h1");
			Assert.That(selector.Matches(CreateElement("h1", XHTML_NAMESPACE)), Is.True, "xhtml:h1");
			Assert.That(selector.Matches(CreateElement("h2")), Is.False, "h2");
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInDefaultNamespace()
		{
			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE));

			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInDefaultNamespace_ToString()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			Assert.That(CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE)).ToString(_namespaceManager), Is.EqualTo("h1"));
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInDefaultNamespace_MatchesByQualifiedName()
		{
			const string SOME_NAMESPACE = "http://test.org/schema";

			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE));
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<h1>Title 1</h1>
						<t:h1>Title 1</t:h1>
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var element1 = new XElementInfo(xdoc.Descendants(XName.Get("h1", XHTML_NAMESPACE)).First());
			Assert.That(selector.Matches(element1), Is.True, "h1");

			var element2 = new XElementInfo(xdoc.Descendants(XName.Get("h1", SOME_NAMESPACE)).First());
			Assert.That(selector.Matches(element2), Is.False, "t:h1");
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInNonDefaultNamespace()
		{
			_namespaceManager.AddNamespace("xhtml", XHTML_NAMESPACE);

			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE));
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInNonDefaultNamespace_ToString()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			_namespaceManager.AddNamespace("t", SOME_NAMESPACE);
			Assert.That(CssSelector.WithName(XName.Get("h1", SOME_NAMESPACE)).ToString(_namespaceManager), Is.EqualTo("t|h1"));
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInNonDefaultNamespace_MatchesByQualifiedName()
		{
			const string SOME_NAMESPACE = "http://test.org/schema";

			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE));
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<s:h1>Title 1</s:h1>
						<t:h1>Title 1</t:h1>
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var element1 = new XElementInfo(xdoc.Descendants(XName.Get("h1", XHTML_NAMESPACE)).First());
			Assert.That(selector.Matches(element1), Is.True, "s:h1");

			var element2 = new XElementInfo(xdoc.Descendants(XName.Get("h1", SOME_NAMESPACE)).First());
			Assert.That(selector.Matches(element2), Is.False, "t:h1");
		}

		#endregion

		#region Attribute selectors

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithAnyValue()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.None), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithAnyValue_ToString()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href");
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[href]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithAnyValue_MatchesByLocalName()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<a id='anchor1' href='http://some.where.else' />
						<a id='anchor2' />
					</body>
				</html>");

			var matchingElements = xdoc.Descendants(XName.Get("a")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements, Has.Count.EqualTo(1));
			Assert.That(matchingElements[0].Attribute("id").Value, Is.EqualTo("anchor1"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithExactValue()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href", CssAttributeMatchOperator.Exact, "#some-target");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.None), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Exact), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("#some-target"), nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithExactValue_ToString()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href", CssAttributeMatchOperator.Exact, "#some-target");
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[href=\"#some-target\"]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameWithoutNamespace_WithExactValue_MatchesByLocalName()
		{
			var selector = CssSelector.WithAttribute(XNamespace.None + "href", CssAttributeMatchOperator.Exact, "#another-target");
			var xdoc = XDocument.Parse(@"
				<html>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' href='#another-target' />
					</body>
				</html>");

			var matchingElements = xdoc.Descendants(XName.Get("a")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements, Has.Count.EqualTo(1));
			Assert.That(matchingElements[0].Attribute("id").Value, Is.EqualTo("anchor2"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithAnyValue()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"));
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get("*")), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithAnyValue_ToString_WhenDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("", XHTML_NAMESPACE);
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"));
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[*|href]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithAnyValue_ToString_WhenNoDefaultNamespaceExists()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"));
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[*|href]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithAnyValue_Matches()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"));
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' href='#some-target' />
						<a id='anchor2' s:href='#another-target' />
						<a id='anchor3' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = xdoc.Descendants(XName.Get("a")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor1", "anchor2" }));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithValueContainingWord()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"), CssAttributeMatchOperator.ContainsWord, "test");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get("*")), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.ContainsWord), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("test"), nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithValueContainingWord_ToString()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"), CssAttributeMatchOperator.ContainsWord, "test");
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[*|href~=\"test\"]"));
		}

		[Test]
		public void AttributeSelector_ForLocalNameInAnyNamespace_WithValueContainingWord_Matches()
		{
			var selector = CssSelector.WithAttribute(XName.Get("href", "*"), CssAttributeMatchOperator.ContainsWord, "test");
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

			var matchingElements = xdoc.Descendants(XName.Get("a")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor2", "anchor3", "anchor4" }));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithAnyValue()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);

			var selector = CssSelector.WithAttribute(XName.Get("href", XHTML_NAMESPACE));
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.Any), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.Null, nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithAnyValue_ToString_WhenDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("", SOME_NAMESPACE);
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);
			Assert.That(CssSelector.WithAttribute(XName.Get("href", XHTML_NAMESPACE)).ToString(_namespaceManager), Is.EqualTo("[x|href]"));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithAnyValue_ToString_WhenNoDefaultNamespaceExists()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);
			Assert.That(CssSelector.WithAttribute(XName.Get("href", XHTML_NAMESPACE)).ToString(_namespaceManager), Is.EqualTo("[x|href]"));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);

			var selector = CssSelector.WithAttribute(XName.Get("href", XHTML_NAMESPACE), CssAttributeMatchOperator.LanguageCode, "fr");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("href"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.MatchOperator, Is.EqualTo(CssAttributeMatchOperator.LanguageCode), nameof(selector.MatchOperator));
				Assert.That(selector.MatchOperand, Is.EqualTo("fr"), nameof(selector.MatchOperand));
			});
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode_ToString()
		{
			_namespaceManager.AddNamespace("x", XHTML_NAMESPACE);

			var selector = CssSelector.WithAttribute(XName.Get("lang", XHTML_NAMESPACE), CssAttributeMatchOperator.LanguageCode, "fr");
			Assert.That(selector.ToString(_namespaceManager), Is.EqualTo("[x|lang|=\"fr\"]"));
		}

		[Test]
		public void AttributeSelector_ForQualifiedName_WithValueIsLanguageCode_Matches()
		{
			var selector = CssSelector.WithAttribute(XName.Get("lang", XHTML_NAMESPACE), CssAttributeMatchOperator.LanguageCode, "fr");
			var xdoc = XDocument.Parse(string.Format(@"
				<html xmlns:s='{0}' xmlns:t='{1}'>
					<head />
					<body>
						<a id='anchor1' s:lang='nl' />
						<a id='anchor2' s:lang='fr' />
						<a id='anchor3' s:lang='french' />
						<a id='anchor4' s:lang='fr-FR' />
						<a id='anchor5' s:lang='-fr' />
						<a id='anchor6' t:lang='fr' />
						<a id='anchor7' />
					</body>
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = xdoc.Descendants(XName.Get("a")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "anchor2", "anchor4" }));
		}

		#endregion

		#region Class selectors

		[Test]
		public void ClassSelector_ToString()
		{
			Assert.That(CssSelector.WithClass("sect").ToString(), Is.EqualTo(".sect"));
		}

		[Test]
		public void ClassSelector_Matches()
		{
			var selector = CssSelector.WithClass("sect");
			var xdoc = XDocument.Parse(string.Format(@"
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
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = xdoc.Descendants(XName.Get("p")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "par1", "par3", "par4", "par5" }));
		}

		#endregion

		#region ID selectors

		[Test]
		public void IdSelector_ToString()
		{
			Assert.That(CssSelector.WithId("par1").ToString(), Is.EqualTo("#par1"));
		}

		[Test]
		public void IdSelector_Matches()
		{
			var selector = CssSelector.WithId("par1");
			var xdoc = XDocument.Parse(string.Format(@"
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
				</html>", XHTML_NAMESPACE, SOME_NAMESPACE));

			var matchingElements = xdoc.Descendants(XName.Get("p")).Where(e => selector.Matches(new XElementInfo(e))).ToList();
			Assert.That(matchingElements.Select(e => e.Attribute("id").Value), Is.EquivalentTo(new[] { "par1" }));
		}

		#endregion

		#region Factory methods

		private XElementInfo CreateElement(string localName, string ns = null)
		{
			return new XElementInfo(new XElement(XName.Get(localName, ns ?? XNamespace.None.NamespaceName)));
		}

		#endregion

		#region Inner classes

		private struct XElementInfo : IElementInfo<XElementInfo>
		{
			private static readonly XName ClassAttributeName = XNamespace.None + "class";
			private static readonly XName IdAttributeName = XNamespace.None + "id";

			private readonly XElement _element;

			public XElementInfo(XElement element)
			{
				_element = element;
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
				get { return false; }
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

			public bool HasDynamicState(CssDynamicElementState state)
			{
				return false;
			}

			public bool TryGetPredecessor(ICssSelector selector, bool immediateOnly, out XElementInfo result)
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
