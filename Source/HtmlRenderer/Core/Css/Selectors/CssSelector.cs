namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssSelector : CssComponent, ICssSelector
	{
		#region Static fields

		public const string AnyLocalName = "*";
		public const string AnyNamespacePrefix = "*";

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
				Add(selector.ClassName, selector);
			}
		}

		#endregion

		#region Static properties

		public static readonly CssTypeSelector Universal = new CssTypeSelector(AnyLocalName, AnyNamespacePrefix);
		internal static readonly CssTypeSelector ImpliedUniversal = new CssTypeSelector(AnyLocalName, AnyNamespacePrefix);

		#endregion

		#region Factory methods

		/// <summary>
		/// Creates selector that matches elements with the namespace that is associated with the given namespace prefix.
		/// </summary>
		/// <param name="namespacePrefix">A namespace prefix that is resolvable to the namespace of matching elements.
		/// An empty value represents no namespace.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElementNamespace(string namespacePrefix)
		{
			return new CssTypeSelector(AnyLocalName, namespacePrefix);
		}

		/// <summary>
		/// Creates selector that matches elements with a given local name and the namespace that is associated with 
		/// the given namespace prefix..
		/// </summary>
		/// <param name="localName">The local name of matching elements.</param>
		/// <param name="namespacePrefix">An optional namespace prefix for <paramref name="localName"/>. 
		/// A <c>null</c> value represents the default namespace if a default namespace has been defined, 
		/// or any namespace otherwise. A <see cref="string.Empty"/> value represents no namespace.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElement(string localName, string namespacePrefix)
		{
			return new CssTypeSelector(localName, namespacePrefix);
		}

		/// <summary>
		/// Creates selector that matches elements with given local name in any namespace, if no default namespace 
		/// has been specified in the CSS document. Otherwise matches element with the given local name in the default 
		/// namespace.
		/// </summary>
		/// <param name="localName">The local name of matching elements.</param>
		/// <returns>The newly created selector.</returns>
		public static CssTypeSelector WithElement(string localName)
		{
			return new CssTypeSelector(localName, null);
		}

		/// <summary>
		/// Creates selector for elements that have a certain attribute.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="namespacePrefix">An optional namespace prefix for <paramref name="localName"/>. 
		/// A <c>null</c> value represents any namespace. A <see cref="string.Empty"/> value represents
		/// no namespace.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(string localName, string namespacePrefix)
		{
			return new CssAttributeSelector(localName, namespacePrefix ?? AnyNamespacePrefix);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="namespacePrefix">An optional namespace prefix for <paramref name="localName"/>. 
		/// A <c>null</c> value represents any namespace. A <see cref="string.Empty"/> value represents
		/// no namespace.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(string localName, string namespacePrefix, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return WithAttribute(localName, namespacePrefix, matchOperator, new CssStringToken(CssTokenType.QuotedString | CssTokenType.Quote, matchOperand));
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="namespacePrefix">An optional namespace prefix for <paramref name="localName"/>. 
		/// A <c>null</c> value represents any namespace. A <see cref="string.Empty"/> value represents
		/// no namespace.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		internal static CssAttributeSelector WithAttribute(string localName, string namespacePrefix, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
		{
			return new CssAttributeSelector(localName, namespacePrefix, matchOperator, matchOperand);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute with a given name in any namespace.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(string localName)
		{
			return new CssAttributeSelector(localName, null);
		}

		/// <summary>
		/// Creates selector for elements that have an attribute whose value satisfies a given predicate.
		/// </summary>
		/// <param name="localName">The local name of matching attributes.</param>
		/// <param name="matchOperator">An attribute string value matching operator.</param>
		/// <param name="matchOperand">The value to which attribute values are matched.</param>
		/// <returns>The newly created selector.</returns>
		public static CssAttributeSelector WithAttribute(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			return WithAttribute(localName, null, matchOperator, new CssStringToken(CssTokenType.QuotedString | CssTokenType.Quote, matchOperand));
		}

		/// <summary>
		/// Creates selector for element that has a given identifier.
		/// </summary>
		/// <param name="id">Identifier of matching element.</param>
		/// <returns>The newly created selector.</returns>
		public static CssIdSelector WithId(string id)
		{
			return new CssIdSelector(id);
		}

		/// <summary>
		/// Creates selector for elements that have a given CSS class.
		/// </summary>
		/// <param name="name">The CSS class name.</param>
		/// <returns>The newly created selector.</returns>
		public static CssClassSelector WithClass(string name)
		{
			return new CssClassSelector(name);
		}

		/// <summary>
		/// Creates selector for elements that match a given non-parameterized CSS pseudo-class.
		/// </summary>
		/// <param name="name">The CSS pseudo-class name.</param>
		/// <returns>The selector for the given CSS pseudo-class name.</returns>
		public static CssPseudoClassSelector WithPseudoClass(string name)
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

		#region Instance fields

		protected CssSelector(CssSpecificity specificity)
		{
			this.Specificity = specificity;
		}

		#endregion

		#region Properties

		public CssSpecificity Specificity { get; }

		#endregion

		#region Instance methods

		/// <summary>
		/// Applies visitor instance to this selector.
		/// </summary>
		/// <param name="visitor"></param>
		public abstract void Apply(CssSelectorVisitor visitor);

		public sealed override void ToString(StringBuilder sb)
		{
			Apply(new CssSelectorFormatterVisitor(sb));
		}

		#endregion
	}
}



