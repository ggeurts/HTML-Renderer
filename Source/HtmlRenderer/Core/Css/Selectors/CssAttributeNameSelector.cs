namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssAttributeNameSelector : CssAttributeSelector
	{
		private readonly XName _name;

		internal CssAttributeNameSelector(XName name)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
		}

		internal CssAttributeNameSelector(XName name, CssAttributeMatchOperator matchOperator, string matchOperand)
			: base(matchOperator, matchOperand)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
		}

		internal CssAttributeNameSelector(string localName)
		{
			_name = AnyNamespace + localName;
		}

		internal CssAttributeNameSelector(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
			: base(matchOperator, matchOperand)
		{
			_name = AnyNamespace + localName;
		}

		public override string LocalName
		{
			get { return _name.LocalName; }
		}

		public override XNamespace Namespace
		{
			get { return _name.Namespace; }
		}

		/// <inheritdoc />
		public override bool Matches<TElement>(TElement element)
		{
			return _name.Namespace == AnyNamespace
				? element.HasAttribute(_name.LocalName, this.Predicate)
				: element.HasAttribute(_name, this.Predicate);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAttributeNameSelector;
			return other != null
			       && _name == other._name
			       && this.MatchOperator == other.MatchOperator
			       && this.MatchOperand == other.MatchOperand;
		}

		public override int GetHashCode()
		{
			var hash = _name.GetHashCode();
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
			ToString(sb, _name, namespaceResolver);
			base.ToString(sb, namespaceResolver);
			sb.Append(']');
		}
	}
}