namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using TheArtOfDev.HtmlRenderer.Core.Css;

	public class CssTokenFactory
	{
		private readonly CssStringTokenData _whiteSpaceData;

		public CssTokenFactory()
		{
			_whiteSpaceData = new CssStringTokenData(" ");
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

			return new CssToken(tokenType, CssAsciiTokenData.Instance);
		}

		public CssToken CreateIdentifierToken(string value)
		{
			return new CssToken(CssTokenType.Identifier, new CssStringTokenData(value));   
		}

		public CssToken CreateFunctionToken(string name)
		{
			return new CssToken(CssTokenType.Function, new CssStringTokenData(name));
		}

		public CssToken CreateUrlToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.Url;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssToken(tokenType, new CssStringTokenData(value));
		}

		public CssToken CreateHashToken(string value, bool isIdentifier)
		{
			var tokenType = CssTokenType.Hash;
			if (isIdentifier) tokenType |= CssTokenType.IdentifierType;
			return new CssToken(tokenType, new CssStringTokenData(value));
		}

		public CssToken CreateAtKeywordToken(string value)
		{
			return new CssToken(CssTokenType.AtKeyword, new CssStringTokenData(value));
		}

		public CssToken CreateStringToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.QuotedString;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssToken(tokenType, new CssStringTokenData(value));
		}

		public CssToken CreateNumericToken(bool isFloatingPoint, double value, string unit)
		{
			var tokenType = string.IsNullOrEmpty(unit)
				? CssTokenType.Number
				: unit == "%"
					? CssTokenType.Percentage
					: CssTokenType.Dimension;
			if (isFloatingPoint) tokenType |= CssTokenType.FloatingPointType;
			return new CssToken(tokenType, new CssTokenData<CssNumeric>(new CssNumeric(value, unit)));
		}

		public CssToken CreateOperatorToken(char ch)
		{
			return new CssToken(CssTokenType.MatchOperator | (CssTokenType)ch, CssOperatorTokenData.Instance);
		}

		public CssToken CreateColumnToken()
		{
			return new CssToken(CssTokenType.Column, CssOperatorTokenData.Instance);
		}

		public CssToken CreateUnicodeRangeToken(int rangeStart, int rangeEnd)
		{
			return new CssToken(CssTokenType.UnicodeRange, new CssTokenData<CssUnicodeRange>(new CssUnicodeRange(rangeStart, rangeEnd)));
		}

		//public CssToken CreateCommentToken(int startPos, int length)
		//{
		//	return new CssToken(CssTokenType.Comment, startPos, length, _rawStringData);
		//}

		public CssToken CreateCdoToken()
		{
			return new CssToken(CssTokenType.CDO, new CssStringTokenData("<!--"));
		}

		public CssToken CreateCdcToken()
		{
			return new CssToken(CssTokenType.CDC, new CssStringTokenData("-->"));
		}

		public CssToken CreateWhitespaceToken()
		{
			return new CssToken(CssTokenType.Whitespace, _whiteSpaceData);
		}
	}
}