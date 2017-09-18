namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Generic;

	internal class CssCharEqualityComparer : EqualityComparer<char>
	{
		public override bool Equals(char x, char y)
		{
			return x.ToLowerAscii() == y.ToLowerAscii();
		}

		public override int GetHashCode(char obj)
		{
			return obj.ToLowerAscii().GetHashCode();
		}

	}
}