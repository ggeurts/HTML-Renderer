namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	internal static class CssCharExtensions
	{
		public static char ToLowerAscii(this char ch)
		{
			return ch >= '\u0041' && ch <= '\u005A'
				? (char)(ch + 0x20)
				: ch;
		}
	}
}