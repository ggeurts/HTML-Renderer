namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssTypeNameSelector : CssTypeSelector
	{
		private readonly XName _name;

		public CssTypeNameSelector(XName name)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
		}

		public CssTypeNameSelector(string localName)
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

		public override bool Matches<TElement>(TElement element)
		{
			if (_name == null) return true;
			return _name.Namespace == AnyNamespace
				? element.HasName(_name.LocalName)
				: element.HasName(_name);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssTypeNameSelector;
			return other != null
				&& _name == other._name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			ToString(sb, _name, namespaceResolver);
		}
	}
}