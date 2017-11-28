namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssSelector : CssComponent
	{
		#region Static fields

		protected const string AnyLocalName = "*";
		protected const string AnyNamespacePrefix = "*";

		protected static readonly XNamespace AnyNamespace = XNamespace.Get("*");

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

		#region Properties

		public static CssTypeSelector Universal
		{
			get { return CssUniversalSelector.Explicit; }	
		}

		#endregion

		#region Factory methods

		/// <summary>
		/// Creates selector that matches elements with given namespace. Use namespace <see cref="XNamespace.None"/>
		/// to match elements without a namespace.
		/// </summary>
		/// <param name="ns">The namespace name.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElementNamespace(XNamespace ns)
		{
			return new CssElementNamespaceSelector(ns);
		}

		/// <summary>
		/// Creates selector that matches elements with given qualified name. Use namespace <see cref="XNamespace.None"/>
		/// to match elements without a namespace.
		/// </summary>
		/// <param name="name">The qualified name of matching elements.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElement(XName name)
		{
			return new CssElementNameSelector(name);
		}

		/// <summary>
		/// Creates selector that matches elements with given local name in any namespace, including those without a namespace.
		/// </summary>
		/// <param name="localName">The local name of matching elements.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElement(string localName)
		{
			return new CssElementNameSelector(localName);
		}

		/// <summary>
		/// Creates selector for elements that have a certain attribute.
		/// </summary>
		/// <param name="name">Qualified name of matching attributes.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(XName name)
		{
			return new CssAttributeSelector(name);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="name">Qualified name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(XName name, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return WithAttribute(name, matchOperator, new CssStringToken(CssTokenType.QuotedString | CssTokenType.Quote, matchOperand));
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="name">Qualified name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		internal static CssAttributeSelector WithAttribute(XName name, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
		{
			return new CssAttributeSelector(name, matchOperator, matchOperand);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute with a given name in any namespace, including those without a namespace.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(string localName)
		{
			return new CssAttributeSelector(localName);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		internal CssAttributeSelector WithAttribute(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return WithAttribute(localName, matchOperator, new CssStringToken(CssTokenType.QuotedString | CssTokenType.Quote, matchOperand));
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		internal CssAttributeSelector WithAttribute(string localName, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
		{
			return new CssAttributeSelector(localName, matchOperator, matchOperand);
		}

		/// <summary>
		/// Creates selector for element that has a given identifier.
		/// </summary>
		/// <param name="id">Identifier of matching element.</param>
		/// <returns>The newly created selector.</returns>
		public static CssSimpleSelector WithId(string id)
		{
			return new CssIdSelector(id);
		}

		/// <summary>
		/// Creates selector for elements that have a given CSS class.
		/// </summary>
		/// <param name="name">The CSS class name.</param>
		/// <returns>The newly created selector.</returns>
		public static CssSimpleSelector WithClass(string name)
		{
			return new CssClassSelector(name);
		}

		/// <summary>
		/// Creates selector for elements that match a given non-parameterized CSS pseudo-class.
		/// </summary>
		/// <param name="name">The CSS pseudo-class name.</param>
		/// <returns>The selector for the given CSS pseudo-class name.</returns>
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
	}
}



