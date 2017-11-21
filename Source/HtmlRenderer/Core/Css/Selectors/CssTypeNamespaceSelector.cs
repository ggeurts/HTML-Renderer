namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssTypeNamespaceSelector : CssTypeSelector
	{
		private readonly XNamespace _namespace;
		private readonly string _namespacePrefix;

		public CssTypeNamespaceSelector(XNamespace ns, string namespacePrefix)
		{
			ArgChecker.AssertArgNotNull(ns, nameof(ns));
			ArgChecker.AssertArgNotNull(namespacePrefix, nameof(namespacePrefix));
			_namespace = ns;
			_namespacePrefix = namespacePrefix;
		}

		public override string LocalName
		{
			get { return AnyLocalName; }
		}

		public override XNamespace Namespace
		{
			get { return _namespace; }
		}

		public override string NamespacePrefix
		{
			get { return _namespacePrefix; }
		}

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasNamespace(_namespace);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssTypeNamespaceSelector;
			return other != null
			       && _namespace == other._namespace;
		}

		public override int GetHashCode()
		{
			return _namespace.GetHashCode();
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_namespacePrefix).Append("|").Append(AnyLocalName);
		}
	}
}