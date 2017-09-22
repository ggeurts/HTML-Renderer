namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthOfTypePseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthOfTypePseudoClassSelector(int cycleSize, int offset)
			: base("nth-of-type", cycleSize, offset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return Matches(element.SiblingIndex);
		}
	}
}