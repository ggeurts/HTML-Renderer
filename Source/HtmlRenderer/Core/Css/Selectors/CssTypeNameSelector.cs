namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
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
			_name = XName.Get(localName, AnyNamespace.NamespaceName);
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
			if (_name.Namespace == AnyNamespace)
			{
				sb.Append('*');
			}
			else
			{
				sb.Append(_name.NamespaceName);
			}
			sb.Append('|').Append(_name.LocalName);
		}
	}
}