namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthChildPseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthChildPseudoClassSelector(CssCycleOffset cycleOffset)
			: base("nth-child", cycleOffset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return !element.IsRoot && Matches(element.ChildIndex);
		}
	}
}