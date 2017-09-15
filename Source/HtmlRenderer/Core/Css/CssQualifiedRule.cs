namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssQualifiedRule : CssRule
	{
		public CssQualifiedRule(CssComponent prelude, CssBlock block)
			: base(prelude, block)
		{
			ArgChecker.AssertArgNotNull(block, nameof(block));
		}
	}
}