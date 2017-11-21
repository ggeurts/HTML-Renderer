namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssAttributeNamespaceSelector : CssAttributeSelector
	{
		private readonly XNamespace _namespace;

		internal CssAttributeNamespaceSelector(XNamespace ns)
		{
			ArgChecker.AssertArgNotNull(ns, nameof(ns));
			_namespace = ns;
		}

		internal CssAttributeNamespaceSelector(XNamespace ns, CssAttributeMatchOperator matchOperator, string matchOperand)
			: base(matchOperator, matchOperand)
		{
			ArgChecker.AssertArgNotNull(ns, nameof(ns));
			_namespace = ns;
		}

		public override string LocalName
		{
			get { return AnyLocalName; }
		}

		public override XNamespace Namespace
		{
			get { return _namespace; }
		}

		/// <inheritdoc />
		public override bool Matches<TElement>(TElement element)
		{
			return element.HasAttributeInNamespace(_namespace, this.Predicate);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAttributeNamespaceSelector;
			return other != null
			       && _namespace == other._namespace
			       && this.MatchOperator == other.MatchOperator
			       && this.MatchOperand == other.MatchOperand;
		}

		public override int GetHashCode()
		{
			var hash = _namespace.GetHashCode();
			if (this.MatchOperator != CssAttributeMatchOperator.Any)
			{
				hash = HashUtility.Hash(hash, (int)this.MatchOperator);
				hash = HashUtility.Hash(hash, this.MatchOperand.GetHashCode());
			}
			return hash;
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			sb.Append('[');
			ToString(sb, _namespace, namespaceResolver);
			base.ToString(sb, namespaceResolver);
			sb.Append(']');
		}
	}
}