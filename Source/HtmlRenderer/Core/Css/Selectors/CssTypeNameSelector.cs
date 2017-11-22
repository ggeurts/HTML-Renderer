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
			if (_name.Namespace != XNamespace.None)
			{
				var defaultNamespace = namespaceResolver?.LookupNamespace("");
				if (!string.IsNullOrEmpty(defaultNamespace) && _name.NamespaceName != defaultNamespace)
				{
					var namespacePrefix = _name.Namespace == AnyNamespace
						? AnyNamespacePrefix
						: namespaceResolver.LookupPrefix(_name.NamespaceName);
					// We must write a namespace prefix when a default namespace has been defined 
					// and the default namespace differs from the namespace of the name.
					sb.Append(namespacePrefix).Append('|');
				}
			}
			sb.Append(_name.LocalName);
		}
	}
}