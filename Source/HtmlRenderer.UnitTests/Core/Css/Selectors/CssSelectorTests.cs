﻿namespace HtmlRenderer.UnitTests.Core.Css.Selectors
{
	using System;
	using System.Linq;
	using System.Xml.Linq;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	[TestFixture]
	public class CssSelectorTests
	{
		#region Constants

		private const string XHTML_NAMESPACE = "https://www.w3.org/1999/xhtml/";

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
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.ToString(), Is.EqualTo("*|*"), nameof(selector.ToString));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		[Test]
		public void UniversalSelector_MatchesAllNodes()
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
				Assert.That(selector.NamespacePrefix, Is.EqualTo("*"), nameof(selector.NamespacePrefix));
				Assert.That(selector.ToString(), Is.EqualTo("*|h1"), nameof(selector.ToString));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
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
			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE), "");

			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.NamespacePrefix, Is.EqualTo(""), nameof(selector.NamespacePrefix));
				Assert.That(selector.ToString(), Is.EqualTo("|h1"), nameof(selector.ToString));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInDefaultNamespace_MatchesByQualifiedName()
		{
			const string SOME_NAMESPACE = "http://test.org/schema";

			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE), "");
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
			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE), "xhtml");
			Assert.Multiple(() =>
			{
				Assert.That(selector.LocalName, Is.EqualTo("h1"), nameof(selector.LocalName));
				Assert.That(selector.Namespace, Is.EqualTo(XNamespace.Get(XHTML_NAMESPACE)), nameof(selector.Namespace));
				Assert.That(selector.NamespacePrefix, Is.EqualTo("xhtml"), nameof(selector.NamespacePrefix));
				Assert.That(selector.ToString(), Is.EqualTo("xhtml|h1"), nameof(selector.ToString));
				Assert.That(selector.TypeSelector, Is.SameAs(selector), nameof(selector.TypeSelector));
			});
		}

		[Test]
		public void TypeSelector_ForQualifiedNameInNonDefaultNamespace_MatchesByQualifiedName()
		{
			const string SOME_NAMESPACE = "http://test.org/schema";

			var selector = CssSelector.WithName(XName.Get("h1", XHTML_NAMESPACE), "");
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
