namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssTargetPseudoClassSelector : CssPseudoClassSelector
	{
		public CssTargetPseudoClassSelector()
			: base("target")
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return element.IsTarget;
		}
	}
}