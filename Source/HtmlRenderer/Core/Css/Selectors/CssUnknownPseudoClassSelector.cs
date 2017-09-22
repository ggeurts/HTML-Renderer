namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssUnknownPseudoClassSelector : CssPseudoClassSelector
	{
		public CssUnknownPseudoClassSelector(string name)
			: base(name)
		{ }

		public override bool Matches<TElement>(TElement element)
		{
			return false;
		}
	}
}