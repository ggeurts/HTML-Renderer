namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	public enum CssCombinator
	{
		None = 0,
		DescendantOf = ' ',
		ChildOf = '>',
		AdjacentSiblingOf = '+',
		GeneralSiblingOf = '~'
	}
}