namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Generic;

	internal class CssCharEqualityComparer : EqualityComparer<char>
	{
		public override bool Equals(char x, char y)
		{
			return ToLowerAscii(x) == ToLowerAscii(y);
		}

		public override int GetHashCode(char obj)
		{
			return ToLowerAscii(obj).GetHashCode();
		}

		private static char ToLowerAscii(char ch)
		{
			return ch >= '\u0041' && ch <= '\u005A'
				? (char)(ch + 0x20)
				: ch;
		}
	}
}