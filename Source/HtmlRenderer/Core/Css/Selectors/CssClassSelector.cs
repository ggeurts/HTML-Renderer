namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssClassSelector : CssSimpleSelector
	{
		private readonly string _name;

		public CssClassSelector(string name)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			_name = name;
		}

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasClass(_name);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssClassSelector;
			return other != null && _name == other._name;
		}

		public override int GetHashCode()
		{
			return '.' ^ _name.GetHashCode();
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			sb.Append('.').Append(_name);
		}
	}
}