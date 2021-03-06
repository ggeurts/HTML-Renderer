namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;

	public sealed class CssBlockType
	{
		public static readonly CssBlockType Parentheses = new CssBlockType('(', ')');
		public static readonly CssBlockType SquareBrackets = new CssBlockType('[', ']');
		public static readonly CssBlockType CurlyBrackets = new CssBlockType('{', '}');

		public readonly char BeginChar;
		public readonly char EndChar;

		private CssBlockType(char beginChar, char endChar)
		{
			this.BeginChar = beginChar;
			this.EndChar = endChar;
		}

		public CssTokenType BeginTokenType
		{
			get { return (CssTokenType)this.BeginChar; }
		}

		public CssTokenType EndTokenType
		{
			get { return (CssTokenType)this.EndChar; }
		}

		public override int GetHashCode()
		{
			return this.BeginChar.GetHashCode();
		}

		public override string ToString()
		{
			return new StringBuilder(2)
				.Append(this.BeginChar)
				.Append(this.EndChar)
				.ToString();
		}
	}
}