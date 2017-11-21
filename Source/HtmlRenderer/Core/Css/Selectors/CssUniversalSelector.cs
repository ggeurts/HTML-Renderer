namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
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

		public override string NamespacePrefix
		{
			get { return AnyLocalName; }
		}

		public override bool Equals(object obj)
		{
			return obj is CssUniversalSelector;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(AnyNamespacePrefix).Append('|').Append(AnyLocalName);
		}
	}
}