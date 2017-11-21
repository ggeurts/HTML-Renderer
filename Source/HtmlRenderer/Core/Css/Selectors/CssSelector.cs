namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssSelector : CssComponent
	{
		#region Static fields

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
		/// <param name="namespacePrefix">The namespace prefix. An empty string represents the default namespace.</param>
		public static CssTypeSelector WithNamespace(XNamespace ns, string namespacePrefix)
		{
			return new CssTypeNamespaceSelector(ns, namespacePrefix);
		}

		/// <summary>
		/// Creates selector that matches elements with given qualified name. Use namespace <see cref="XNamespace.None"/>
		/// to match elements without a namespace.
		/// </summary>
		/// <param name="name">The qualified element name.</param>
		/// <param name="namespacePrefix">The namespace prefix. An empty string represents the default namespace.</param>
		public static CssTypeSelector WithName(XName name, string namespacePrefix)
		{
			return new CssTypeNameSelector(name, namespacePrefix);
		}

		/// <summary>
		/// Creates selector that matches elements with given name in any namespace, including those without a namespace.
		/// </summary>
		/// <param name="name">The element name.</param>
		public static CssTypeSelector WithLocalName(string name)
		{
			return new CssTypeNameSelector(name);
		}

		public static CssSimpleSelector WithAttribute(XName name)
		{
			return new CssAttributeSelector(name);
		}

		public static CssSimpleSelector WithAttribute(XName name, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return new CssAttributeSelector(name, matchOperator, matchOperand);
		}

		public static CssSimpleSelector WithAttribute(string localName)
		{
			return new CssAttributeSelector(localName);
		}

		public CssSimpleSelector WithAttribute(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return new CssAttributeSelector(localName, matchOperator, matchOperand);
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

		public abstract bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;

		#endregion
	}
}



