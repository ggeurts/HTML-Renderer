namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;

	public class CssAtRule : CssRule, ICssDeclaration
	{
		public CssAtRule(ImmutableArray<CssComponent> prelude, CssBlock block)
			: base(prelude, block)
		{ }
	}
}