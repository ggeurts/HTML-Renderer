namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthLastChildPseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthLastChildPseudoClassSelector(int cycleSize, int offset)
			: base("nth-last-child", cycleSize, offset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return !element.IsRoot && Matches(element.Parent.ChildCount - element.ChildIndex);
		}
	}
}