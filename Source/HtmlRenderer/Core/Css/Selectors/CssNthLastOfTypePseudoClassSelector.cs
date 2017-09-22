namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthLastOfTypePseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthLastOfTypePseudoClassSelector(int cycleSize, int offset)
			: base("nth-last-of-type", cycleSize, offset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return Matches(element.SiblingCount - element.SiblingIndex);
		}
	}
}