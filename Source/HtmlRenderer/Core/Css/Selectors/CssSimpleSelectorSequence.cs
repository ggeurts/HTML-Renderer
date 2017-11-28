namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	/// <summary>
	/// Represents a type selector followed by one or more simple selectors that are not type selectors.
	/// </summary>
	internal class CssSimpleSelectorSequence : CssSelector, ICssSelectorSubject
	{
		private readonly CssTypeSelector _typeSelector;
		private readonly ImmutableArray<CssSelector> _otherSelectors;
		private readonly CssPseudoElement _pseudoElement;

		internal CssSimpleSelectorSequence(ICssSelectorSequence sequence, CssSimpleSelector otherSelector)
		{
			ArgChecker.AssertArgNotNull(sequence, nameof(sequence));
			ArgChecker.AssertArgNotNull(otherSelector, nameof(otherSelector));
			ArgChecker.AssertIsTrue<ArgumentException>(!(otherSelector is CssTypeSelector), "Selector sequence cannot contain more than one type selector.");
			_typeSelector = sequence.TypeSelector;
			_otherSelectors = sequence.OtherSelectors.Add(otherSelector);
		}

		internal CssSimpleSelectorSequence(ICssSelectorSequence sequence, CssPseudoElement pseudoElement)
		{
			ArgChecker.AssertArgNotNull(sequence, nameof(sequence));
			ArgChecker.AssertArgNotNull(pseudoElement, nameof(pseudoElement));
			ArgChecker.AssertIsTrue<ArgumentException>(sequence.Subject.PseudoElement == null, "Selector sequence cannot contain more than one pseudo-element.");
			_typeSelector = sequence.TypeSelector;
			_otherSelectors = sequence.OtherSelectors;
			_pseudoElement = pseudoElement;
		}

		public ICssSelectorSubject Subject
		{
			get { return this; }
		}

		public CssTypeSelector TypeSelector
		{
			get { return _typeSelector; }
		}

		public ImmutableArray<CssSelector> OtherSelectors
		{
			get { return _otherSelectors; }
		}

		CssPseudoElement ICssSelectorSubject.PseudoElement
		{
			get { return _pseudoElement; }
		}

		public override bool Matches<TElement>(TElement element)
		{
			if (!_typeSelector.Matches(element)) return false;
			foreach (var selector in _otherSelectors)
			{
				if (!selector.Matches(element)) return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssSimpleSelectorSequence;
			return other != null
			       && _typeSelector.Equals(other._typeSelector)
			       && _otherSelectors.SequenceEqual(other._otherSelectors);
		}

		public override int GetHashCode()
		{
			var hash = _typeSelector.GetHashCode();
			foreach (var selector in _otherSelectors)
			{
				hash = HashUtility.Hash(hash, selector.GetHashCode());
			}
			return hash;
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			_typeSelector.ToString(sb, namespaceResolver);
			foreach (var selector in _otherSelectors)
			{
				selector.ToString(sb, namespaceResolver);
			}
		}
	}
}