namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using TheArtOfDev.HtmlRenderer.Core.Css;

	public class CssTokenFactory
	{
		private readonly CssToken _whiteSpaceToken;

		public CssTokenFactory()
		{
			_whiteSpaceToken = new CssStringToken(CssTokenType.Whitespace, " ");
		}

		public CssToken CreateToken(char ch)
		{
			var tokenType = (CssTokenType)(ch & 0xFF);
			switch (tokenType)
			{
				case CssTokenType.Colon:
				case CssTokenType.Comma:
				case CssTokenType.Semicolon:
				case CssTokenType.LeftParenthesis:
				case CssTokenType.RightParenthesis:
				case CssTokenType.LeftSquareBracket:
				case CssTokenType.RightSquareBracket:
				case CssTokenType.LeftCurlyBracket:
				case CssTokenType.RightCurlyBracket:
					break;
				default:
					tokenType |= CssTokenType.Delimiter;
					break;
			}

			return new CssAsciiToken(tokenType);
		}

		public CssToken CreateIdentifierToken(string value)
		{
			return new CssStringToken(CssTokenType.Identifier, value);   
		}

		public CssToken CreateFunctionToken(string name)
		{
			return new CssStringToken(CssTokenType.Function, name);
		}

		public CssToken CreateUrlToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.Url;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssStringToken(tokenType, value);
		}

		public CssToken CreateHashToken(string value, bool isIdentifier)
		{
			var tokenType = CssTokenType.Hash;
			if (isIdentifier) tokenType |= CssTokenType.IdentifierType;
			return new CssStringToken(tokenType, value);
		}

		public CssToken CreateAtKeywordToken(string value)
		{
			return new CssStringToken(CssTokenType.AtKeyword, value);
		}

		public CssToken CreateStringToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.QuotedString;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssStringToken(tokenType, value);
		}

		public CssToken CreateNumericToken(bool isFloatingPoint, double value, string unit)
		{
			var tokenType = string.IsNullOrEmpty(unit)
				? CssTokenType.Number
				: unit == "%"
					? CssTokenType.Percentage
					: CssTokenType.Dimension;
			if (isFloatingPoint) tokenType |= CssTokenType.FloatingPointType;
			return new CssToken<CssNumeric>(tokenType, new CssNumeric(value, unit));
		}

		public CssToken CreateOperatorToken(char ch)
		{
			return new CssOperatorToken(CssTokenType.MatchOperator | (CssTokenType)ch);
		}

		public CssToken CreateColumnToken()
		{
			return new CssOperatorToken(CssTokenType.Column);
		}

		public CssToken CreateUnicodeRangeToken(int rangeStart, int rangeEnd)
		{
			return new CssToken<CssUnicodeRange>(CssTokenType.UnicodeRange, new CssUnicodeRange(rangeStart, rangeEnd));
		}

		//public CssToken CreateCommentToken(int startPos, int length)
		//{
		//	return new CssToken(CssTokenType.Comment, startPos, length, _rawStringData);
		//}

		public CssToken CreateCdoToken()
		{
			return new CssStringToken(CssTokenType.CDO, "<!--");
		}

		public CssToken CreateCdcToken()
		{
			return new CssStringToken(CssTokenType.CDC, "-->");
		}

		public CssToken CreateWhitespaceToken()
		{
			return _whiteSpaceToken;
		}
	}
}