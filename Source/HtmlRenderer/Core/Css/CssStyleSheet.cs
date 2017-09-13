namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssStyleSheet
	{
		public IImmutableList<CssRule> Rules { get; }

		public CssStyleSheet(IImmutableList<CssRule> rules)
		{
			ArgChecker.AssertArgNotNull(rules, nameof(rules));
			this.Rules = rules;
		}
	}
}