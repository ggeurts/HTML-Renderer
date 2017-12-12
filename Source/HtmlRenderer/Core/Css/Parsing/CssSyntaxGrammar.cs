namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using Pidgin;

	public class CssSyntaxGrammar
	{
		public static readonly Parser<CssToken, CssToken> IDENTIFIER = Parser<CssToken>.Token(t => t.IsIdentifier);
		public static readonly Parser<CssToken, CssToken> FUNCTION = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.Function);
		public static readonly Parser<CssToken, CssToken> HASH = Parser<CssToken>.Token(t => t.IsHash);
		public static readonly Parser<CssToken, CssToken> STRING = Parser<CssToken>.Token(t => t.IsQuotedString);
		public static readonly Parser<CssToken, CssToken> WHITESPACE = Parser<CssToken>.Token(t => t.IsWhitespace).Labelled("whitespace");

		public static readonly Parser<CssToken, Unit> COLON = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.Colon).IgnoreResult();
		public static readonly Parser<CssToken, Unit> COMMA = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.Comma).IgnoreResult();
		public static readonly Parser<CssToken, Unit> SEMICOLON = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.Semicolon).IgnoreResult();
		public static readonly Parser<CssToken, Unit> LPAREN = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.LeftParenthesis).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RPAREN = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.RightParenthesis).IgnoreResult().Or(Parser.Try(Parser<CssToken>.End()));
		public static readonly Parser<CssToken, Unit> LSQUARE = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.LeftSquareBracket).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RSQUARE = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.RightSquareBracket).IgnoreResult().Or(Parser.Try(Parser<CssToken>.End()));
		public static readonly Parser<CssToken, Unit> LCURLY = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.LeftCurlyBracket).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RCURLY = Parser<CssToken>.Token(t => t.TokenType == CssTokenType.RightCurlyBracket).IgnoreResult().Or(Parser.Try(Parser<CssToken>.End()));

		public static Parser<CssToken, CssToken> Delimiter(char ch)
		{
			return Parser<CssToken>.Token(t => t.IsDelimiter(ch));
		}

		public static Parser<CssToken, CssToken> Identifier(string value = null)
		{
			return Parser<CssToken>.Token(t => t.IsIdentifier && (value == null || value.Equals(t.StringValue, StringComparison.OrdinalIgnoreCase)));
		}

		public static Parser<CssToken, CssToken> Function(string value = null)
		{
			return Parser<CssToken>.Token(t => t.IsFunction && (value == null || value.Equals(t.StringValue, StringComparison.OrdinalIgnoreCase)));
		}

		public static Parser<CssToken, TResult> Function<TArg, TResult>(string name, Parser<CssToken, TArg> argParser, Func<TArg, TResult> factory)
		{
			return Function(name)
				.Before(SkipWhitespace)
				.Bind(t => argParser, (tok, arg) => factory(arg))
				.Before(SkipWhitespace)
				.Before(RPAREN);
		}

		public static Parser<CssToken, CssToken> String(string value = null, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			return Parser<CssToken>.Token(t => t.IsQuotedString && (value == null || value.Equals(t.StringValue, comparison)));
		}

		public static readonly Parser<CssToken, Unit> SkipWhitespace = WHITESPACE.SkipMany().Labelled("whitespace");
	}
}