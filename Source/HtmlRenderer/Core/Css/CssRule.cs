namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssRule
	{
		public ImmutableArray<CssComponent> Prelude { get; }
		public CssBlock Block { get; }

		protected CssRule(ImmutableArray<CssComponent> prelude, CssBlock block)
		{
			ArgChecker.AssertArgNotNull(prelude, nameof(prelude));
			this.Prelude = prelude;
			this.Block = block;
		}
	}
}