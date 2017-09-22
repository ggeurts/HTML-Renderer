namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssOnlyOfTypePseudoClassSelector : CssPseudoClassSelector
	{
		public CssOnlyOfTypePseudoClassSelector()
			: base("only-of-type")
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return element.SiblingCount == 1;
		}
	}
}