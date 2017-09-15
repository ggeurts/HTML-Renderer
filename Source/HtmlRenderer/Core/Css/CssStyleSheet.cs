namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssStyleSheet : CssNode
	{
		public IImmutableList<CssRule> Rules { get; }

		public CssStyleSheet(IImmutableList<CssRule> rules)
		{
			ArgChecker.AssertArgNotNull(rules, nameof(rules));
			this.Rules = rules;
		}

		public override void ToString(StringBuilder sb)
		{
			var isFirst = true;
			foreach (var rule in this.Rules)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					sb.AppendLine();
				}
				rule.ToString(sb);
			}
		}
	}
}