namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
	public class CssTokenFactory
	{
		private readonly string _input;
		private readonly CssStringTokenData _rawStringData;

		public CssTokenFactory(string input)
		{
			_input = input;
			_rawStringData = new CssStringTokenData(input, null);
		}

		public CssToken CreateToken(int pos)
		{
			var tokenType = (CssTokenType)(_input[pos] & 0xFF);
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

			return new CssToken(tokenType, pos, 1, CssAsciiTokenData.Instance);
		}

		public CssToken CreateIdentifierToken(int startPos, int length, string value)
		{
			return new CssToken(CssTokenType.Identifier, startPos, length, new CssStringTokenData(_input, value));   
		}

		public CssToken CreateFunctionToken(int startPos, int length, string name)
		{
			return new CssToken(CssTokenType.Function, startPos, length, new CssStringTokenData(_input, name));
		}

		public CssToken CreateUrlToken(int startPos, int length, string value, bool isInvalid)
		{
			var tokenType = CssTokenType.Url;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssToken(tokenType, startPos, length, new CssStringTokenData(_input, value));
		}

		public CssToken CreateHashToken(int startPos, int length, string value, bool isIdentifier)
		{
			var tokenType = CssTokenType.Hash;
			if (isIdentifier) tokenType |= CssTokenType.IdentifierType;
			return new CssToken(tokenType, startPos, length, new CssStringTokenData(_input, value));
		}

		public CssToken CreateAtKeywordToken(int startPos, int length, string value)
		{
			return new CssToken(CssTokenType.AtKeyword, startPos, length, new CssStringTokenData(_input, value));
		}

		public CssToken CreateStringToken(int startPos, int length, string value, bool isInvalid)
		{
			var tokenType = CssTokenType.String;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return new CssToken(tokenType, startPos, length, new CssStringTokenData(_input, value));
		}

		public CssToken CreateNumericToken(int startPos, int length, bool isFloatingPoint, double value, string unit)
		{
			var tokenType = string.IsNullOrEmpty(unit)
				? CssTokenType.Number
				: unit == "%"
					? CssTokenType.Percentage
					: CssTokenType.Dimension;
			if (isFloatingPoint) tokenType |= CssTokenType.NumberType;
			return new CssToken(tokenType, startPos, length, new CssNumericTokenData(_input, value, unit));
		}

		public CssToken CreateOperatorToken(int pos)
		{
			return new CssToken(CssTokenType.Operator | (CssTokenType)_input[pos], pos, 2, _rawStringData);
		}

		public CssToken CreateColumnToken(int pos)
		{
			return new CssToken(CssTokenType.Column, pos, 2, _rawStringData);
		}

		public CssToken CreateUnicodeRangeToken(int startPos, int length, char rangeStart, char rangeEnd)
		{
			return new CssToken(CssTokenType.UnicodeRange, startPos, length, new CssUnicodeRangeTokenData(_input, rangeStart, rangeEnd));
		}

		public CssToken CreateCommentToken(int startPos, int length)
		{
			return new CssToken(CssTokenType.Comment, startPos, length, _rawStringData);
		}

		public CssToken CreateCdoToken(int startPos)
		{
			return new CssToken(CssTokenType.CDO, startPos, 3, _rawStringData);
		}

		public CssToken CreateCdcToken(int startPos)
		{
			return new CssToken(CssTokenType.CDC, startPos, 3, _rawStringData);
		}

		public CssToken CreateWhitespaceToken(int startPos, int length)
		{
			return new CssToken(CssTokenType.Whitespace, startPos, length, _rawStringData);
		}
	}
}