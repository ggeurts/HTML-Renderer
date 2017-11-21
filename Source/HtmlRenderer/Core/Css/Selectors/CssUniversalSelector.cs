namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	internal class CssUniversalSelector : CssTypeSelector
	{
		public override bool Matches<TElement>(TElement element)
		{
			return true;
		}

		public override string LocalName
		{
			get { return AnyLocalName; }
		}

		public override XNamespace Namespace
		{
			get { return AnyNamespace; }
		}

		public override bool Equals(object obj)
		{
			return obj is CssUniversalSelector;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			if (!string.IsNullOrEmpty(namespaceResolver?.LookupNamespace("")))
			{
				// We must write namespace prefix when default namespace has been defined 
				sb.Append(AnyNamespacePrefix).Append('|');
			}
			sb.Append(AnyLocalName);
		}
	}
}