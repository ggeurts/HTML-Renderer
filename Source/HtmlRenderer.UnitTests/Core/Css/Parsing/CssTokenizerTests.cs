﻿namespace HtmlRenderer.UnitTests.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;

	[TestFixture]
	//[Timeout(100)]
	public class CssTokenizerTests
	{
		#region Static fields

		private static readonly TokenData Colon = new TokenData(CssTokenType.Colon);
		private static readonly TokenData WhiteSpace = new TokenData(CssTokenType.Whitespace);
		private static readonly TokenData LeftCurlyBracket = new TokenData(CssTokenType.LeftCurlyBracket);
		private static readonly TokenData RightCurlyBracket = new TokenData(CssTokenType.RightCurlyBracket);
		private static readonly TokenData Semicolon = new TokenData(CssTokenType.Semicolon);

		#endregion

		#region Input tests

		[Test]
		public void CanTokenizeString()
		{
			VerifyTokens(new CssTokenizer(new CssReader( "body { margin:0 }")).Tokenize(),
				Identifier("body"), WhiteSpace,
				LeftCurlyBracket,
				WhiteSpace, Identifier("margin"), Colon, NumberLiteral(0), WhiteSpace,
				RightCurlyBracket);
		}

		[Test]
		public void CanTokenizeSubstring()
		{
			var xml = "<style>body { margin:0 }</style>";
			var index = xml.IndexOf("body", StringComparison.Ordinal);
			var count = xml.IndexOf("</style>", StringComparison.Ordinal) - index;

			VerifyTokens(new CssTokenizer(new CssReader("<style>body { margin:0 }</style>", index, count)).Tokenize(),
				Identifier("body"), WhiteSpace,
				LeftCurlyBracket,
				WhiteSpace, Identifier("margin"), Colon, NumberLiteral(0), WhiteSpace,
				RightCurlyBracket);
		}

		[Test]
		public void CanTokenizeTextReader()
		{
			VerifyTokens(new CssTokenizer(new CssReader(new StringReader("body { margin:0 }"))).Tokenize(),
				Identifier("body"), WhiteSpace,
				LeftCurlyBracket,
				WhiteSpace, Identifier("margin"), Colon, NumberLiteral(0), WhiteSpace,
				RightCurlyBracket);
		}

		#endregion

		#region Stylesheet and rules tests

		[Test]
		public void EmptyStylesheet()
		{
			VerifyTokenizer("");
		}

		[Test]
		public void EmptyRule()
		{
			VerifyTokenizer("body {}", Identifier("body"), WhiteSpace, LeftCurlyBracket, RightCurlyBracket);
		}

		[Test]
		public void RuleWithDeclaration()
		{
			VerifyTokenizer("body { margin:0 }", 
				Identifier("body"), WhiteSpace, 
					LeftCurlyBracket, 
						WhiteSpace, Identifier("margin"), Colon, NumberLiteral(0), WhiteSpace,
					RightCurlyBracket);
		}

		[Test]
		public void RuleWithDeclarationList()
		{
			VerifyTokenizer("body { margin:0; text-align: center }",
				Identifier("body"), WhiteSpace,
				LeftCurlyBracket,
					WhiteSpace, Identifier("margin"), Colon, NumberLiteral(0), Semicolon,
					WhiteSpace, Identifier("text-align"), Colon, WhiteSpace, Identifier("center"), WhiteSpace,
				RightCurlyBracket);
		}

		#endregion

		#region Comment tokens

		[Test]
		[TestCase("/* some comment */")]
		[TestCase("/* some comment")]
		[TestCase("/* some\ncomment */")]
		[TestCase("/* some\ncomment")]
		public void IgnoreComments(string css)
		{
			VerifyTokenizer(css);
		}

		#endregion

		#region Whitespace tokens

		[Test]
		[TestCase(" ")]
		[TestCase("\t")]
		[TestCase("\n")]
		[TestCase("\r\n")]
		[TestCase("\r")]
		[TestCase("\f")]
		[TestCase("\f\r\n\t ")]
		public void TokenizeWhitespace(string css)
		{
			VerifyTokenizer(css, WhiteSpace);
		}

		#endregion

		#region String tokens

		[TestCase(@"""Some value""")]
		[TestCase(@"'Some value'")]
		public void TokenizeStringToken(string css)
		{
			VerifyTokenizer(css, StringLiteral("Some value", css[0]));
		}

		[Test]
		[TestCase("\"Some\nValue\"")]
		[TestCase("\"Some\rValue\"")]
		[TestCase("\"Some\fValue\"")]
		public void TokenizeStringWithUnescapedNewLine(string css)
		{
			VerifyTokenizer(css, StringLiteral("Some", css[0], false), WhiteSpace, Identifier("Value"));
		}

		[Test]
		[TestCase("\"Some\\\nValue\"")]
		[TestCase("\"Some\\\rValue\"")]
		[TestCase("\"Some\\\fValue\"")]
		public void TokenizeStringWithEscapedNewLine(string css)
		{
			VerifyTokenizer(css, StringLiteral("SomeValue"));
		}

		[TestCase("\"Some 'value'\"")]
		[TestCase("'Some \\'value\\'")]
		public void TokenizeStringTokenWithEmbeddedSingleQuotes(string css)
		{
			VerifyTokenizer(css, StringLiteral("Some 'value'", css[0]));
		}

		#endregion

		#region Numeric tokens

		[Test]
		[TestCase("0", 0)]
		[TestCase("987654321", 987654321)]
		[TestCase("-1", -1)]
		[TestCase("+1", 1)]
		public void TokenizeIntegerNumberToken(string css, double expectedValue)
		{
			VerifyTokenizer(css, NumberLiteral(expectedValue));
		}

		[Test]
		[TestCase("0.1", 0.1)]
		[TestCase("0.123456789", 0.123456789)]
		[TestCase(".22", 0.22)]
		[TestCase("-0.2", -0.2)]
		[TestCase("-.3", -0.3)]
		[TestCase("+0.2", 0.2)]
		[TestCase("+.3", 0.3)]
		[TestCase("1e4", 1e4)]
		[TestCase("1.23e45", 1.23e45)]
		[TestCase("-2e3", -2e3)]
		[TestCase("3e-4", 3e-4)]
		[TestCase(".29e5", 0.29e5)]
		[TestCase("-.31e5", -0.31e5)]
		[TestCase("1E4", 1E4)]
		[TestCase("1.23E45", 1.23E45)]
		[TestCase("-2E3", -2E3)]
		[TestCase("3E-4", 3E-4)]
		[TestCase(".29E5", 0.29E5)]
		[TestCase("-.31E5", -0.31E5)]
		public void TokenizeFloatingPointNumberToken(string css, double expectedValue)
		{
			VerifyTokenizer(css, NumberLiteral(expectedValue, true));
		}

		[Test]
		[TestCase("+0.2pt", 0.2, "pt")]
		[TestCase("-987654.321mm", -987654.321, "mm")]
		[TestCase("1E45cm", 1E45, "cm")]
		public void TokenizeFloatingPointDimensionToken(string css, double expectedValue, string expectedUnit)
		{
			VerifyTokenizer(css, DimensionLiteral(expectedValue, expectedUnit, true));
		}

		[Test]
		[TestCase("+2pt", 2, "pt")]
		[TestCase("-987654mm", -987654, "mm")]
		[TestCase("1q", 1, "q")]
		[TestCase("10e", 10, "e")]
		[TestCase("-9E", -9, "E")]
		public void TokenizeIntegerDimensionToken(string css, double expectedValue, string expectedUnit)
		{
			VerifyTokenizer(css, DimensionLiteral(expectedValue, expectedUnit));
		}

		[Test]
		[TestCase("+0.2%", 0.2)]
		[TestCase("-987654.321%", -987654.321)]
		[TestCase("1E45%", 1E45)]
		public void TokenizeFloatingPointPercentageToken(string css, double expectedValue)
		{
			VerifyTokenizer(css, PercentageLiteral(expectedValue, true));
		}

		[Test]
		[TestCase("+2%", 2)]
		[TestCase("-987654%", -987654)]
		[TestCase("1%", 1)]
		public void TokenizeIntegerPercentageToken(string css, double expectedValue)
		{
			VerifyTokenizer(css, PercentageLiteral(expectedValue));
		}

		#endregion

		#region Unicode range tokens

		[Test]
		public void TokenizeUnicodeRangeWithoutQuestionMarks()
		{
			VerifyTokenizer("u+a", Identifier("u"), Delimiter('+'), Identifier("a"));
			VerifyTokenizer("u+0B", Identifier("u"), DimensionLiteral(0, "B"));
			VerifyTokenizer("u+00C", Identifier("u"), DimensionLiteral(0, "C"));
			VerifyTokenizer("u+d-d", Identifier("u"), Delimiter('+'), Identifier("d-d"));
			VerifyTokenizer("u+0E-0E", Identifier("u"), DimensionLiteral(0, "E", true));
			VerifyTokenizer("u+0F-0F", Identifier("u"), DimensionLiteral(0, "F-0F"));
			VerifyTokenizer("u+1-1", Identifier("u"), NumberLiteral(1), NumberLiteral(-1));
			VerifyTokenizer("u+20-3A", Identifier("u"), NumberLiteral(20), DimensionLiteral(-3, "A"));
			VerifyTokenizer("u+3A-40", Identifier("u"), DimensionLiteral(3, "A-40"));
		}

		[Test]
		public void TokenizeUnicodeRangeWithQuestionMarks()
		{
			VerifyTokenizer("u+a?", Identifier("u"), Delimiter('+'), Identifier("a"), Delimiter('?'));
			VerifyTokenizer("u+0B?", Identifier("u"), DimensionLiteral(0, "B"), Delimiter('?'));
			VerifyTokenizer("u+00C??", Identifier("u"), DimensionLiteral(0, "C"), Delimiter('?'), Delimiter('?'));
			VerifyTokenizer("u+1?", Identifier("u"), NumberLiteral(1), Delimiter('?'));
		}

		#endregion

		#region Url tokens

		[Test]
		[TestCase("url(http://example.com/some.css)")]
		[TestCase("url( http://example.com/some.css )")]
		public void TokenizeUrlToken(string css)
		{
			VerifyTokenizer(css, Url("http://example.com/some.css"));
		}

		[Test]
		[TestCase("url(\"http://example.com/some.css\")")]
		[TestCase("url('http://example.com/some.css')")]
		public void TokenizeUrlFunctionTokenWithoutWhitespace(string css)
		{
			VerifyTokenizer(css, Function("url("), StringLiteral("http://example.com/some.css", css[4]), Delimiter(')'));
		}

		[Test]
		[TestCase("url( \"http://example.com/some.css\" )")]
		[TestCase("url( 'http://example.com/some.css' )")]
		public void TokenizeUrlFunctionTokenWithWhitespace(string css)
		{
			VerifyTokenizer(css, Function("url("), WhiteSpace, StringLiteral("http://example.com/some.css", css[5]), WhiteSpace, Delimiter(')'));
		}

		#endregion

		#region Identifier tokens

		/// <summary>
		/// Paragraph 4.3.7 (http://www.w3.org/TR/css-syntax-3/#consume-an-escaped-code-point0)
		/// </summary>
		[Test]
		[TestCase("\\57", "W")]
		[TestCase("\\057", "W")]
		[TestCase("\\0057", "W")]
		[TestCase("\\00057", "W")]
		[TestCase("\\000057", "W")]
		[TestCase("\\57 eb", "Web")]
		[TestCase("\\57eb", "\u57EB")]
		[TestCase("NO\\57", "NOW")]
		[TestCase("NO\\57 ", "NOW")]
		[TestCase("-\\6E", "-n")]
		[TestCase("-\\6E op", "-nop")]
		[TestCase("-\\6Eop", "-nop")]
		[TestCase("-o\\6E", "-on")]
		[TestCase("-o\\6E ", "-on")]
		[Description("http://www.w3.org/TR/css-syntax-3/#check-if-two-code-points-are-a-valid-escape")]
		public void TokenizeIdentifierWithEscapedChar(string css, string expectedValue)
		{
			VerifyTokenizer(css, Identifier(expectedValue));
		}

		/// <summary>
		/// Paragraph 4.3.7 (http://www.w3.org/TR/css-syntax-3/#consume-an-escaped-code-point0)
		/// </summary>
		[TestCase("\\10FFFF", "\U0010FFFF")]
		[TestCase("-\\10FFFF", "-\U0010FFFF")]
		[TestCase("ab\\10FFFFcd", "ab\U0010FFFFcd")]
		[TestCase("ab\\10FFFF cd", "ab\U0010FFFFcd")]
		[TestCase("-ab\\10FFFFcd", "-ab\U0010FFFFcd")]
		[TestCase("-ab\\10FFFF cd", "-ab\U0010FFFFcd")]
		[Description("http://www.w3.org/TR/css-syntax-3/#check-if-two-code-points-are-a-valid-escape")]
		public void TokenizeIdentifierWithEscapedSurrogatePair(string css, string expectedValue)
		{
			VerifyTokenizer(css, Identifier(expectedValue));
		}

		/// <summary>
		/// Paragraph 4.3.7 (http://www.w3.org/TR/css-syntax-3/#consume-an-escaped-code-point0)
		/// </summary>
		[Test]
		[TestCase("\\0", "\uFFFD")]			// Zero
		[TestCase("\\D800", "\uFFFD")]		// Surrogate code point - lower interval boundary
		[TestCase("\\DFFF", "\uFFFD")]		// Surrogate code point - upper interval boundary
		[TestCase("\\110000", "\uFFFD")]    // Greater than maximum allowed code point
		[TestCase("\\", "\uFFFD")]			// EOF
		public void TokenizeIdentifierWithInvalidEscapedChar(string css, string expectedValue)
		{
			VerifyTokenizer(css, Identifier(expectedValue));
		}

		#endregion

		#region Hash tokens

		[Test]
		[TestCase("#_", "#_")]			// Underscore
		[TestCase("#€", "#€")]			// Non-ascii
		[TestCase("#a", "#a")]
		[TestCase("#ab", "#ab")]
		[TestCase("#abc", "#abc")]
		[TestCase("#-a", "#-a")]
		[TestCase("#\\6E", "#n")]
		[TestCase("#-\\6E", "#-n")]
		[TestCase("#\\", "#\uFFFD")]    // EOF
		[TestCase("#-\\", "#-\uFFFD")]  // EOF
		public void TokenizeHashIdentifier(string css, string expectedValue)
		{
			VerifyTokenizer(css, Hash(expectedValue, true));
		}

		[Test]
		[TestCase("#0", "#0")]
		[TestCase("#1_", "#1_")]
		[TestCase("#2a", "#2a")]
		[TestCase("#3\\", "#3\uFFFD")]		// EOF
		[TestCase("#4-a", "#4-a")]
		[TestCase("#5\\6E", "#5n")]
		[TestCase("#6-\\6E", "#6-n")]
		[TestCase("#7-\\", "#7-\uFFFD")]	// EOF
		public void TokenizeHashName(string css, string expectedValue)
		{
			VerifyTokenizer(css, Hash(expectedValue));
		}

		#endregion

		#region At keyword tokens

		[Test]
		[TestCase("@_", "@_")]          // Underscore
		[TestCase("@€", "@€")]          // Non-ascii
		[TestCase("@a", "@a")]
		[TestCase("@ab", "@ab")]
		[TestCase("@abc", "@abc")]
		[TestCase("@-a", "@-a")]
		[TestCase("@\\6E", "@n")]
		[TestCase("@-\\6E", "@-n")]
		[TestCase("@\\", "@\uFFFD")]    // EOF
		[TestCase("@-\\", "@-\uFFFD")]  // EOF
		public void TokenizeAtKeywordToken(string css, string expectedValue)
		{
			VerifyTokenizer(css, AtKeyword(expectedValue));
		}

		[Test]
		public void TokenizeAtDelimiterFollowedByDigit()
		{
			VerifyTokenizer("@1", Delimiter('@'), NumberLiteral(1));
		}

		#endregion

		#region Match operators

		[Test]
		[TestCase("$=", CssTokenType.SuffixMatchOperator)]
		[TestCase("*=", CssTokenType.SubstringMatchOperator)]
		[TestCase("^=", CssTokenType.PrefixMatchOperator)]
		[TestCase("|=", CssTokenType.DashMatchOperator)]
		[TestCase("~=", CssTokenType.IncludeMatchOperator)]
		[TestCase("||", CssTokenType.Column)]
		public void TokenizeMatchOperator(string css, CssTokenType expectedType)
		{
			VerifyTokenizer(css, Token(expectedType));
		}

		#endregion

		#region Delimiters

		[Test]
		[TestCase('(')]
		[TestCase(')')]
		[TestCase('[')]
		[TestCase(']')]
		[TestCase('{')]
		[TestCase('}')]
		[TestCase(',')]
		[TestCase(':')]
		[TestCase(';')]
		public void TokenizeMainDelimiters(char delimiter)
		{
			VerifyTokenizer(delimiter.ToString(), new TokenData((CssTokenType)(delimiter & 0xFF)));
		}

		[Test]
		[TestCase('#')]
		[TestCase('$')]
		[TestCase('*')]
		[TestCase('^')]
		[TestCase('|')]
		[TestCase('~')]
		[TestCase('+')]
		[TestCase('-')]
		[TestCase('.')]
		[TestCase('/')]
		[TestCase('<')]
		[TestCase('@')]
		public void TokenizeSecondaryDelimiters(char delimiter)
		{
			VerifyTokenizer(delimiter.ToString(), Delimiter(delimiter));
		}

		[Test]
		public void TokenizeIncompleteEscapeFollowedByNewLine()
		{
			// This is a parse error condition
			VerifyTokenizer("\\\n", new TokenData(CssTokenType.Delimiter | (CssTokenType)'\\'), WhiteSpace);
		}

		#endregion

		#region Utility methods

		private static void VerifyTokenizer(string css, params TokenData[] expectedTokens)
		{
			VerifyTokens(new CssTokenizer(new CssReader(css)).Tokenize(), expectedTokens);
		}

		private static void VerifyTokens(IEnumerable<CssToken> actualTokens, params TokenData[] expectedTokens)
		{
			var tokens = actualTokens.ToArray();
			var tokenIndex = 0;
			foreach (var expectedToken in expectedTokens)
			{
				if (tokens.Length <= tokenIndex)
				{
					Assert.Fail("[Token {0}]Expected {0} token but reached EOF", expectedToken.TokenType);
				}
				var token = tokens[tokenIndex++];

				Assert.That(token.TokenType, Is.EqualTo(expectedToken.TokenType), "[Token {0}]TokenType", tokenIndex);
				switch (expectedToken.TokenType)
				{
					case CssTokenType.Identifier:
					case CssTokenType.QuotedString:
						Assert.That(token.StringValue, Is.EqualTo(expectedToken.Value), "[Token {0}]StringValue", tokenIndex);
						break;
					case CssTokenType.MatchOperator:
						Assert.That(token.IsMatchOperator, Is.True, "[Token {0}]IsMatchOperator", tokenIndex);
						Assert.That(token.StringValue, Is.EqualTo((char) expectedToken.Value + "="), "[Token {0}]RawValue", tokenIndex);
						break;
					default:
						if (token.IsNumber || token.IsDimension || token.IsPercentage)
						{
							Assert.That(token.NumericValue, Is.EqualTo((CssNumeric) expectedToken.Value), "[Token {0}]NumericValue",
								tokenIndex);
						}
						break;
				}
			}
		}

		#endregion

		#region TokenData struct and factory methods

		private static TokenData AtKeyword(string value)
		{
			return new TokenData(CssTokenType.AtKeyword, value);
		}

		private static TokenData Delimiter(char value)
		{
			switch (value)
			{
				case '(':
				case ')':
				case '[':
				case ']':
				case '{':
				case '}':
				case ',':
				case ':':
				case ';':
					return new TokenData((CssTokenType)(value & 0xFF));
				default:
					return new TokenData(CssTokenType.Delimiter | (CssTokenType)(value & 0xFF));
			}
		}

		private static TokenData DimensionLiteral(double value, string unit, bool isFloatingPoint = false)
		{
			return new TokenData(CssTokenType.Dimension | (isFloatingPoint ? CssTokenType.FloatingPointType : 0), new CssNumeric(value, unit));
		}

		private static TokenData Function(string value)
		{
			return new TokenData(CssTokenType.Function, value);
		}

		private static TokenData Hash(string value, bool isIdentifier = false)
		{
			return new TokenData(CssTokenType.Hash | (isIdentifier ? CssTokenType.IdentifierType : 0), value);
		}

		private static TokenData Identifier(string value)
		{
			return new TokenData(CssTokenType.Identifier, value);
		}

		private static TokenData NumberLiteral(double value, bool isFloatingPoint = false)
		{
			return new TokenData(CssTokenType.Number | (isFloatingPoint ? CssTokenType.FloatingPointType : 0), new CssNumeric(value, null));
		}

		private static TokenData PercentageLiteral(double value, bool isFloatingPoint = false)
		{
			return new TokenData(CssTokenType.Percentage | (isFloatingPoint ? CssTokenType.FloatingPointType : 0), new CssNumeric(value, "%"));
		}

		private static TokenData StringLiteral(string value, char quoteChar = '"', bool isValid = true)
		{
			return new TokenData(CssTokenType.QuotedString | (CssTokenType)quoteChar | (isValid ? 0 : CssTokenType.Invalid), value);
		}

		private static TokenData Token(CssTokenType tokenType)
		{
			return new TokenData(tokenType);
		}

		private static TokenData Url(string value, bool isValid = true)
		{
			return new TokenData(CssTokenType.Url | (isValid ? 0 : CssTokenType.Invalid), value);
		}

		private struct TokenData
		{
			public readonly CssTokenType TokenType;
			public readonly object Value;

			public TokenData(CssTokenType tokenType, object value = null)
			{
				this.TokenType = tokenType;
				this.Value = value;
			}
		}

		#endregion
	}
}
