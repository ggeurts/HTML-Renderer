namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;

	public abstract class CssComponent : CssNode
	{
		public static readonly CssComponent Empty = new CssEmptyComponent();

		private class CssEmptyComponent : CssComponent
		{
			public override void ToString(StringBuilder sb)
			{}
		}
	}
}