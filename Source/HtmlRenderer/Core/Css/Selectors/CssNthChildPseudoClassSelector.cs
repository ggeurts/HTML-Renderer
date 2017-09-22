namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssNthChildPseudoClassSelector : CssStructuralPseudoClassSelector
	{
		public CssNthChildPseudoClassSelector(int cycleSize, int offset)
			: base("nth-child", cycleSize, offset)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return !element.IsRoot && Matches(element.ChildIndex);
		}
	}
}