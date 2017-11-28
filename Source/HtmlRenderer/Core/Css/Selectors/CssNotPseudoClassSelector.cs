namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssNotPseudoClassSelector : CssPseudoClassSelector
	{
		private readonly CssSimpleSelector _selector;

		public CssNotPseudoClassSelector(CssSimpleSelector selector)
			: base("not", CalculateSpecificity(selector))
		{
			_selector = selector;
		}

		private static CssSpecificity CalculateSpecificity(CssSimpleSelector selector)
		{
			ArgChecker.AssertArgNotNull(selector, nameof(selector));
			return selector.Specificity;
		}

		public CssSimpleSelector Selector
		{
			get { return _selector; }
		}

		public override bool Matches<TElement>(TElement element)
		{
			return !this.Selector.Matches(element);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssNotPseudoClassSelector;
			return other != null
			       && this.Selector.Equals(other.Selector);
		}

		public override int GetHashCode()
		{
			return ~this.Selector.GetHashCode();
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			base.ToString(sb, namespaceResolver);
			sb.Append('(');
			this.Selector.ToString(sb, namespaceResolver);
			sb.Append(')');
		}
	}
}