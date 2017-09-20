namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;

	public interface CssRule
	{
		CssComponent Prelude { get; }
		CssBlock Block { get; }
		void ToString(StringBuilder sb);
	}
}