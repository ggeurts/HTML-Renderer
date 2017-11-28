namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssIdSelector : CssSimpleSelector
	{
		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(1, 0, 0);

		private readonly string _id;

		public CssIdSelector(string id)
			: base(DefaultSpecificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(id, nameof(id));
			_id= id;
		}

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasId(_id);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssIdSelector;
			return other != null && _id == other._id;
		}

		public override int GetHashCode()
		{
			return '#' ^ _id.GetHashCode();
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			sb.Append('#').Append(_id);
		}
	}
}