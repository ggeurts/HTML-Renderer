namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssRule : CssNode
	{
		public CssComponent Prelude { get; }
		public CssBlock Block { get; }

		protected CssRule(CssComponent prelude, CssBlock block)
		{
			ArgChecker.AssertArgNotNull(prelude, nameof(prelude));
			this.Prelude = prelude;
			this.Block = block;
		}

		public override void ToString(StringBuilder sb)
		{
			this.Prelude.ToString(sb);
			this.Block?.ToString(sb);
		}
	}
}