namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthLastChildPseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthLastChildPseudoClassSelector(CssCycleOffset cycleOffset)
			: base("nth-last-child", cycleOffset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return !element.IsRoot && Matches(element.Parent.ChildCount - element.ChildIndex);
		}
	}
}