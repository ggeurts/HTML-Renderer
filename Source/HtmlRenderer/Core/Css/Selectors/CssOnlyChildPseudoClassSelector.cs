namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssOnlyChildPseudoClassSelector : CssPseudoClassSelector
	{
		public CssOnlyChildPseudoClassSelector()
			: base("only-child")
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return !element.IsRoot && element.Parent.ChildCount == 1;
		}
	}
}