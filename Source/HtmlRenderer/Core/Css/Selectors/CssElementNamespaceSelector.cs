namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssElementNamespaceSelector : CssTypeSelector
	{
		private readonly XNamespace _namespace;

		public CssElementNamespaceSelector(XNamespace ns)
			: base(new CssSpecificity(0, 0, 1))
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

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasNamespace(_namespace);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssElementNamespaceSelector;
			return other != null
			       && _namespace == other._namespace;
		}

		public override int GetHashCode()
		{
			return _namespace.GetHashCode();
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			if (_namespace == AnyNamespace)
			{
				sb.Append(AnyNamespace).Append("|");
			}
			else if (_namespace != XNamespace.None)
			{
				sb.Append(namespaceResolver.LookupPrefix(_namespace.NamespaceName)).Append("|");
			}
			sb.Append(AnyLocalName);
		}
	}
}