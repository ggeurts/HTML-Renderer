namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Xml;
	using System.Xml.Linq;

	public class CssElementMatcherBuilderVisitor : CssSelectorVisitor
	{
		private static readonly ICssElementMatcher Always = new DefaultMatcher(true);
		private static readonly ICssElementMatcher Never = new DefaultMatcher(false);

		private readonly IXmlNamespaceResolver _namespaceResolver;
		private ICssElementMatcher _matcher;

		public CssElementMatcherBuilderVisitor(IXmlNamespaceResolver namespaceResolver)
		{
			_namespaceResolver = namespaceResolver;
		}

		public ICssElementMatcher Matcher
		{
			get { return _matcher; }
		}

		public override void VisitAttributeSelector(CssAttributeSelector selector)
		{
			switch (selector.NamespacePrefix)
			{
				case CssSelector.AnyNamespacePrefix:
					_matcher = new AttributeLocalNameMatcher(selector.LocalName, selector.CreatePredicate());
					break;
				case "":
					_matcher = new AttributeNameMatcher(XNamespace.None + selector.LocalName, selector.CreatePredicate());
					break;
				case null:
					var defaultNamespaceName = _namespaceResolver.LookupNamespace("");
					_matcher = string.IsNullOrEmpty(defaultNamespaceName)
						? (ICssElementMatcher)new AttributeLocalNameMatcher(selector.LocalName, selector.CreatePredicate())
						: new AttributeNameMatcher(XNamespace.None + selector.LocalName, selector.CreatePredicate());
					break;
				default:
					var namespaceName = _namespaceResolver.LookupNamespace(selector.NamespacePrefix);
					_matcher = string.IsNullOrEmpty(namespaceName)
						? Never
						: new AttributeNameMatcher(XName.Get(selector.LocalName, namespaceName), selector.CreatePredicate());
					break;
			}
		}

		public override void VisitClassSelector(CssClassSelector selector)
		{
			_matcher = selector;
		}

		public override void VisitIdSelector(CssIdSelector selector)
		{
			_matcher = selector;
		}

		public override void VisitNegationSelector(CssNegationSelector selector)
		{
			if (ReferenceEquals(_matcher, Never))
			{
				_matcher = Always;
			}
			else if (ReferenceEquals(_matcher, Always))
			{
				_matcher = Never;
			}
			else
			{
				_matcher = new NegationMatcher(_matcher);
			}
		}

		public override void VisitSelectorCombination(CssSelectorCombination combination)
		{
			combination.LeftOperand.Apply(this);
			if (ReferenceEquals(_matcher, Never)) return;
			var leftOperand = _matcher;

			combination.RightOperand.Apply(this);
			if (ReferenceEquals(_matcher, Never)) return;
			var rightOperand = _matcher;

			_matcher = new CombinationMatcher(combination.Combinator, leftOperand, rightOperand);
		}

		public override void VisitPseudoClassSelector(CssPseudoClassSelector selector)
		{
			_matcher = selector;
		}

		public override void VisitLanguagePseudoClassSelector(CssLanguagePseudoClassSelector selector)
		{
			_matcher = selector;
		}

		public override void VisitStructuralPseudoClassSelector(CssStructuralPseudoClassSelector selector)
		{
			_matcher = selector;
		}

		public override void VisitSelectorSequence(CssSimpleSelectorSequence sequence)
		{
			var count = sequence.OtherSelectors.Length;
			var items = new ICssElementMatcher[1 + count];

			sequence.TypeSelector.Apply(this);
			if (ReferenceEquals(_matcher, Never)) return;
			items[0] = _matcher;

			for (var i = 0; i < count; i++)
			{
				sequence.OtherSelectors[i].Apply(this);
				if (ReferenceEquals(_matcher, Never)) return;
				items[i + 1] = _matcher;
			}

			_matcher = new SequenceMatcher(items);
		}

		public override void VisitTypeSelector(CssTypeSelector selector)
		{
			switch (selector.NamespacePrefix)
			{
				case CssSelector.AnyNamespacePrefix:
					_matcher = selector.LocalName == CssSelector.AnyLocalName
						? Always
						: new ElementLocalNameMatcher(selector.LocalName);
					break;

				case "":
					_matcher = selector.LocalName == CssSelector.AnyLocalName
						? (ICssElementMatcher)new ElementNamespaceMatcher(XNamespace.None)
						: new ElementNameMatcher(XNamespace.None + selector.LocalName);
					break;

				case null:
					var defaultNamespaceName = _namespaceResolver.LookupNamespace("");
					if (string.IsNullOrEmpty(defaultNamespaceName))
					{
						_matcher = selector.LocalName == CssSelector.AnyLocalName
							? Always
							: new ElementLocalNameMatcher(selector.LocalName);
					}
					else
					{
						var defaultNamespace = XNamespace.Get(defaultNamespaceName);
						_matcher = selector.LocalName == CssSelector.AnyLocalName
							? (ICssElementMatcher)new ElementNamespaceMatcher(defaultNamespace)
							: new ElementNameMatcher(defaultNamespace + selector.LocalName);
					}
					break;

				default:
					var namespaceName = _namespaceResolver.LookupNamespace(selector.NamespacePrefix);
					if (string.IsNullOrEmpty(namespaceName))
					{
						_matcher = Never;
					}
					else
					{
						var ns = XNamespace.Get(namespaceName);
						_matcher = selector.LocalName == CssSelector.AnyLocalName
							? (ICssElementMatcher)new ElementNamespaceMatcher(ns)
							: new ElementNameMatcher(ns + selector.LocalName);
					}
					break;
			}
		}

		private class AttributeLocalNameMatcher : ICssElementMatcher
		{
			private readonly string _localName;
			private readonly Func<string, StringComparison, bool> _predicate;

			public AttributeLocalNameMatcher(string localName, Func<string, StringComparison, bool> predicate)
			{
				_localName = localName;
				_predicate = predicate;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return element.HasAttribute(_localName, _predicate);
			}
		}

		private class AttributeNameMatcher : ICssElementMatcher
		{
			private readonly XName _name;
			private readonly Func<string, StringComparison, bool> _predicate;

			public AttributeNameMatcher(XName name, Func<string, StringComparison, bool> predicate)
			{
				_name = name;
				_predicate = predicate;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return element.HasAttribute(_name, _predicate);
			}
		}

		private class ElementLocalNameMatcher : ICssElementMatcher
		{
			private readonly string _localName;

			public ElementLocalNameMatcher(string localName)
			{
				_localName = localName;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return element.HasName(_localName);
			}
		}

		private class ElementNameMatcher : ICssElementMatcher
		{
			private readonly XName _name;

			public ElementNameMatcher(XName name)
			{
				_name = name;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return element.HasName(_name);
			}
		}

		private class ElementNamespaceMatcher : ICssElementMatcher
		{
			private readonly XNamespace _namespace;

			public ElementNamespaceMatcher(XNamespace ns)
			{
				_namespace = ns;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return element.HasNamespace(_namespace);
			}
		}

		private class NegationMatcher : ICssElementMatcher
		{
			private readonly ICssElementMatcher _operand;

			public NegationMatcher(ICssElementMatcher operand)
			{
				_operand = operand;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return !_operand.Matches(element);
			}
		}

		private class CombinationMatcher : ICssElementMatcher
		{
			private readonly CssCombinator _combinator;
			private readonly ICssElementMatcher _leftOperand;
			private readonly ICssElementMatcher _rightOperand;

			public CombinationMatcher(CssCombinator combinator, ICssElementMatcher leftOperand, ICssElementMatcher rightOperand)
			{
				_combinator = combinator;
				_leftOperand = leftOperand;
				_rightOperand = rightOperand;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				if (!_rightOperand.Matches(element)) return false;

				TElement relatedElement;
				switch (_combinator)
				{
					case CssCombinator.Descendant:
						if (!TryGetParent(element, false, out relatedElement)) return false;
						break;
					case CssCombinator.Child:
						if (!TryGetParent(element, true, out relatedElement)) return false;
						break;
					case CssCombinator.AdjacentSibling:
						if (!element.TryGetPredecessor(_rightOperand, true, out relatedElement)) return false;
						break;
					case CssCombinator.GeneralSibling:
						if (!element.TryGetPredecessor(_rightOperand, false, out relatedElement)) return false;
						break;
					default:
						return false;
				}

				return _leftOperand.Matches(relatedElement);
			}

			private bool TryGetParent<TElement>(TElement element, bool immediateOnly, out TElement result) where TElement : IElementInfo<TElement>
			{
				while (!element.IsRoot)
				{
					element = element.Parent;
					if (_leftOperand.Matches(element))
					{
						result = element;
						return true;
					}
					if (immediateOnly)
					{
						break;
					}
				}

				result = default(TElement);
				return false;
			}
		}

		private class SequenceMatcher : ICssElementMatcher
		{
			private readonly ICssElementMatcher[] _items;

			public SequenceMatcher(ICssElementMatcher[] items)
			{
				_items = items;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				for (var i = 0; i < _items.Length; i++)
				{
					if (!_items[i].Matches(element)) return false;
				}
				return true;
			}
		}

		private class DefaultMatcher : ICssElementMatcher
		{
			private readonly bool _result;

			public DefaultMatcher(bool result)
			{
				_result = result;
			}

			public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
			{
				return _result;
			}
		}
	}
}