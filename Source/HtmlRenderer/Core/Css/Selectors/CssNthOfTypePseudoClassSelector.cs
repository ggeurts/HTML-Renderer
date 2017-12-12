namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthOfTypePseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthOfTypePseudoClassSelector(CssCycleOffset cycleOffset)
			: base("nth-of-type", cycleOffset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return Matches(element.SiblingIndex);
		}
	}
}