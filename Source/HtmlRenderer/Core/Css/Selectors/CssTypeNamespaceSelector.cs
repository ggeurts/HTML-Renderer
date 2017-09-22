namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssTypeNamespaceSelector : CssTypeSelector
	{
		private readonly XNamespace _namespace;

		public CssTypeNamespaceSelector(XNamespace ns)
		{
			ArgChecker.AssertArgNotNull(ns, nameof(ns));
			_namespace = ns;
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
			sb.Append(_namespace).Append("|*");
		}
	}
}