namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssTypeNameSelector : CssTypeSelector
	{
		private readonly XName _name;
		private readonly string _namespacePrefix;

		public CssTypeNameSelector(XName name, string namespacePrefix)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			ArgChecker.AssertArgNotNull(namespacePrefix, nameof(namespacePrefix));
			_name = name;
			_namespacePrefix = namespacePrefix;
		}

		public CssTypeNameSelector(string localName)
		{
			_name = XName.Get(localName, AnyNamespace.NamespaceName);
			_namespacePrefix = AnyNamespacePrefix;
		}

		public override string LocalName
		{
			get { return _name.LocalName; }
		}

		public override XNamespace Namespace
		{
			get { return _name.Namespace; }
		}

		public override string NamespacePrefix
		{
			get { return _namespacePrefix; }
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

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_namespacePrefix).Append('|').Append(_name.LocalName);
		}
	}
}