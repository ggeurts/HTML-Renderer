namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssAtRule : CssRule
	{
		public string Name { get; }

		public CssAtRule(string name, CssComponent prelude, CssBlock block)
			: base(prelude, block)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(this.Name);
			base.ToString(sb);
		}
	}
}