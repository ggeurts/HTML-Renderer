namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssSelectorSequence : CssSelector, ICssSelectorSequence
	{
		private readonly CssTypeSelector _typeSelector;
		private readonly ImmutableArray<CssSelector> _otherSelectors;

		public CssSelectorSequence(CssTypeSelector typeSelector, ImmutableArray<CssSimpleSelector> otherSelectors)
		{
			ArgChecker.AssertArgNotNull(typeSelector, nameof(typeSelector));
			ArgChecker.AssertArgNotNull(otherSelectors, nameof(otherSelectors));
			if (otherSelectors.Length < 1)
			{
				throw new ArgumentException("One or more items expected", nameof(otherSelectors));
			}

			_typeSelector = typeSelector;
			_otherSelectors = otherSelectors.As<CssSelector>();
		}

		public CssTypeSelector TypeSelector
		{
			get { return _typeSelector; }
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
			var other = obj as CssSelectorSequence;
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