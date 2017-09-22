namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssEmptyPseudoClassSelector : CssPseudoClassSelector
	{
		public CssEmptyPseudoClassSelector()
			: base("empty")
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return element.ChildCount == 0;
		}
	}
}