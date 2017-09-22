namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssRootPseudoClassSelector : CssPseudoClassSelector
	{
		public CssRootPseudoClassSelector()
			: base("root")
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return element.IsRoot;
		}
	}
}