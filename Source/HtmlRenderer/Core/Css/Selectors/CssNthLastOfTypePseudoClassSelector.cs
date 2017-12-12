namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthLastOfTypePseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthLastOfTypePseudoClassSelector(CssCycleOffset cycleOffset)
			: base("nth-last-of-type", cycleOffset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return Matches(element.SiblingCount - element.SiblingIndex);
		}
	}
}