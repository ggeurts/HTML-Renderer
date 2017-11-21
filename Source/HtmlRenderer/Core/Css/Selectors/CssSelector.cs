namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssSelector : CssComponent
	{
		#region Static fields

		protected const string AnyLocalName = "*";
		protected const string AnyNamespacePrefix = "*";

		protected static readonly XNamespace AnyNamespace = XNamespace.Get("*");

		public static readonly CssTypeSelector Universal = new CssUniversalSelector();

		private static readonly PredefinedPseudoClassSelectorDictionary _predefinedSelectors =
			new PredefinedPseudoClassSelectorDictionary
			{
				new CssDynamicPseudoClassSelector("link", CssDynamicElementState.IsUnvisitedLink),
				new CssDynamicPseudoClassSelector("visited", CssDynamicElementState.IsVisitedLink),
				new CssDynamicPseudoClassSelector("hover", CssDynamicElementState.IsPointerOver),
				new CssDynamicPseudoClassSelector("active", CssDynamicElementState.IsPointerOver),
				new CssDynamicPseudoClassSelector("focus", CssDynamicElementState.HasFocus),
				new CssDynamicPseudoClassSelector("enabled", CssDynamicElementState.IsEnabled),
				new CssDynamicPseudoClassSelector("disabled", CssDynamicElementState.IsDisabled),
				new CssTargetPseudoClassSelector(),
				new CssRootPseudoClassSelector(),
				new CssOnlyChildPseudoClassSelector(),
				new CssOnlyOfTypePseudoClassSelector(),
				new CssEmptyPseudoClassSelector(),
				{ "first-child", new CssNthChildPseudoClassSelector(0, 1) },
				{ "last-child", new CssNthLastChildPseudoClassSelector(0, 1) },
				{ "first-of-type", new CssNthOfTypePseudoClassSelector(0, 1) },
				{ "last-of-type", new CssNthLastOfTypePseudoClassSelector(0, 1) }
			};

		private class PredefinedPseudoClassSelectorDictionary : Dictionary<string, CssPseudoClassSelector>
		{
			public PredefinedPseudoClassSelectorDictionary()
				: base(CssEqualityComparer<string>.Default)
			{ }

			public void Add(CssPseudoClassSelector selector)
			{
				Add(selector.Name, selector);
			}
		}

		#endregion

		#region Factory methods

		/// <summary>
		/// Creates selector that matches elements with given namespace. Use namespace <see cref="XNamespace.None"/>
		/// to match elements without a namespace.
		/// </summary>
		/// <param name="ns">The namespace name.</param>
		public static CssTypeSelector WithNamespace(XNamespace ns)
		{
			return new CssTypeNamespaceSelector(ns);
		}

		/// <summary>
		/// Creates selector that matches elements with given qualified name. Use namespace <see cref="XNamespace.None"/>
		/// to match elements without a namespace.
		/// </summary>
		/// <param name="name">The qualified name of matching elements.</param>
		public static CssTypeSelector WithName(XName name)
		{
			return new CssTypeNameSelector(name);
		}

		/// <summary>
		/// Creates selector that matches elements with given local name in any namespace, including those without a namespace.
		/// </summary>
		/// <param name="localName">The local name of matching elements.</param>
		public static CssTypeSelector WithLocalName(string localName)
		{
			return new CssTypeNameSelector(localName);
		}

		/// <summary>
		/// Creates selector for elements that have a certain attribute.
		/// </summary>
		/// <param name="name">Qualified name of matching attributes.</param>
		public static CssAttributeSelector WithAttribute(XName name)
		{
			return new CssAttributeNameSelector(name);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="name">Qualified name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		public static CssAttributeSelector WithAttribute(XName name, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return new CssAttributeNameSelector(name, matchOperator, matchOperand);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute with a given name in any namespace, including those without a namespace.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		public static CssAttributeSelector WithAttribute(string localName)
		{
			return new CssAttributeNameSelector(localName);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		public CssAttributeSelector WithAttribute(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return new CssAttributeNameSelector(localName, matchOperator, matchOperand);
		}

		/// <summary>
		/// Creates selector for elements that have any attributes in a given namespace. Use namespace <see cref="XNamespace.None"/>
		/// to match attributes without a namespace.
		/// </summary>
		/// <param name="ns">The namespace name.</param>
		public static CssAttributeSelector WithAttributeInNamespace(XNamespace ns)
		{
			return new CssAttributeNamespaceSelector(ns);
		}

		public static CssSimpleSelector WithId(string id)
		{
			return new CssIdSelector(id);
		}

		public static CssSimpleSelector WithClass(string name)
		{
			return new CssClassSelector(name);
		}

		public static CssSimpleSelector WithPseudoClass(string name)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));

			CssPseudoClassSelector result;
			return _predefinedSelectors.TryGetValue(name, out result)
				? result
				: new CssUnknownPseudoClassSelector(name);
		}

		public static CssSimpleSelector NthChild(int cycleSize, int offset)
		{
			return new CssNthChildPseudoClassSelector(cycleSize, offset);
		}

		public static CssSimpleSelector NthLastChild(int cycleSize, int offset)
		{
			return new CssNthLastChildPseudoClassSelector(cycleSize, offset);
		}

		public static CssSimpleSelector NthOfType(int cycleSize, int offset)
		{
			return new CssNthOfTypePseudoClassSelector(cycleSize, offset);
		}

		public static CssSimpleSelector NthLastOfType(int cycleSize, int offset)
		{
			return new CssNthLastOfTypePseudoClassSelector(cycleSize, offset);
		}

		public static CssSimpleSelector Not(CssSimpleSelector selector)
		{
			var notSelector = selector as CssNotPseudoClassSelector;
			return notSelector == null
				? new CssNotPseudoClassSelector(selector)
				: notSelector.Selector;
		}

		public static CssCombinatorSelector Combinator(CssCombinator combinator, ICssSelectorSequence firstSequence)
		{
			return new CssCombinatorSelector(combinator, firstSequence, null);
		}

		public static CssCombinatorSelector Chain(CssCombinatorSelector chain, CssCombinator combinator, ICssSelectorSequence nextSequence)
		{
			return new CssCombinatorSelector(combinator, nextSequence, chain);
		}

		public static CssSelectorChain Chain(CssCombinatorSelector previous, ICssSelectorSequence finalSequence)
		{
			return new CssSelectorChain(finalSequence, previous);
		}

		#endregion

		#region Instance methods

		/// <summary>
		/// Indicates whether a given element matches this selector.
		/// </summary>
		/// <typeparam name="TElement">The element (wrapper) type.</typeparam>
		/// <param name="element">The element to be matched.</param>
		/// <returns>Returns <c>true</c> if <paramref name="element"/> matches this selector, or <c>false</c> otherwise.</returns>
		public abstract bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;

		public override string ToString()
		{
			return ToString(DefaultNamespaceManager);
		}

		public string ToString(IXmlNamespaceResolver namespaceResolver)
		{
			var sb = new StringBuilder();
			ToString(sb, namespaceResolver);
			return sb.ToString();
		}

		public sealed override void ToString(StringBuilder sb)
		{
			ToString(sb, DefaultNamespaceManager);
		}

		public abstract void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver);

		private readonly XmlNamespaceManager DefaultNamespaceManager = new CssNamespaceManager();

		private sealed class CssNamespaceManager : XmlNamespaceManager
		{
			public CssNamespaceManager()
				: base(new NameTable())
			{
				AddNamespace(AnyNamespacePrefix, AnyNamespace.NamespaceName);
			}
		}

		#endregion

		#region Protected methods

		protected static void ToString(StringBuilder sb, XName name, IXmlNamespaceResolver namespaceResolver)
		{
			if (name.Namespace != XNamespace.None)
			{
				var defaultNamespace = namespaceResolver?.LookupNamespace("");
				if (!string.IsNullOrEmpty(defaultNamespace) && name.NamespaceName != defaultNamespace)
				{
					var namespacePrefix = name.Namespace == AnyNamespace
						? AnyNamespacePrefix
						: namespaceResolver.LookupPrefix(name.NamespaceName);
					// We must write a namespace prefix when a default namespace has been defined 
					// and the default namespace differs from the namespace of the name.
					sb.Append(namespacePrefix).Append('|');
				}
			}
			sb.Append(name.LocalName);
		}

		protected static void ToString(StringBuilder sb, XNamespace ns, IXmlNamespaceResolver namespaceResolver)
		{
			if (ns == AnyNamespace)
			{
				sb.Append(AnyNamespace).Append("|");
			}
			else if (ns != XNamespace.None)
			{
				sb.Append(namespaceResolver.LookupPrefix(ns.NamespaceName)).Append("|");
			}
			sb.Append(AnyLocalName);
		}

		#endregion
	}
}



