namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Generic;
	using TheArtOfDev.HtmlRenderer.Core.Css;

	public class CssTokenFactory
	{
		private readonly Dictionary<CssTokenType, CssToken> _simpleTokenCache 
			= new Dictionary<CssTokenType, CssToken>();
		private readonly Dictionary<KeyValuePair<CssTokenType, string>, CssStringToken> _stringTokenCache 
			= new Dictionary<KeyValuePair<CssTokenType, string>, CssStringToken>();
		private readonly Dictionary<CssNumeric, CssToken<CssNumeric>> _numericTokenCache
			= new Dictionary<CssNumeric, CssToken<CssNumeric>>();

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

			CssToken token;
			if (!_simpleTokenCache.TryGetValue(tokenType, out token))
			{
				token = new CssAsciiToken(tokenType);
				_simpleTokenCache.Add(tokenType, token);
			}
			return token;
		}

		public CssToken CreateIdentifierToken(string value)
		{
			return GetOrCreateStringToken(CssTokenType.Identifier, value);   
		}

		public CssToken CreateFunctionToken(string name)
		{
			return GetOrCreateStringToken(CssTokenType.Function, name);
		}

		public CssToken CreateUrlToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.Url;
			if (isInvalid) tokenType |= CssTokenType.Invalid;
			return GetOrCreateStringToken(tokenType, value);
		}

		public CssToken CreateHashToken(string value, bool isIdentifier)
		{
			var tokenType = CssTokenType.Hash;
			if (isIdentifier) tokenType |= CssTokenType.IdentifierType;
			return GetOrCreateStringToken(tokenType, value);
		}

		public CssToken CreateAtKeywordToken(string value)
		{
			return GetOrCreateStringToken(CssTokenType.AtKeyword, value);
		}

		public CssToken CreateStringToken(string value, bool isInvalid)
		{
			var tokenType = CssTokenType.QuotedString;
			if (isInvalid) tokenType |= CssTokenType.Invalid;

			return GetOrCreateStringToken(tokenType, value);
		}

		public CssToken CreateNumericToken(bool isFloatingPoint, double value, string unit)
		{
			var tokenType = string.IsNullOrEmpty(unit)
				? CssTokenType.Number
				: unit == "%"
					? CssTokenType.Percentage
					: CssTokenType.Dimension;
			if (isFloatingPoint) tokenType |= CssTokenType.FloatingPointType;

			var numeric = new CssNumeric(value, unit);

			CssToken<CssNumeric> token;
			if (!_numericTokenCache.TryGetValue(numeric, out token))
			{
				token = new CssToken<CssNumeric>(tokenType, numeric);
				_numericTokenCache.Add(numeric, token);
			}
			return token;
		}

		public CssToken CreateOperatorToken(char ch)
		{
			var tokenType = CssTokenType.MatchOperator | (CssTokenType) ch;

			CssToken token;
			if (!_simpleTokenCache.TryGetValue(tokenType, out token))
			{
				token = new CssOperatorToken(tokenType);
				_simpleTokenCache.Add(tokenType, token);
			}
			return token;
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
			return GetOrCreateStringToken(CssTokenType.CDO, "<!--");
		}

		public CssToken CreateCdcToken()
		{
			return GetOrCreateStringToken(CssTokenType.CDC, "-->");
		}

		private CssStringToken GetOrCreateStringToken(CssTokenType tokenType, string value)
		{
			var key = new KeyValuePair<CssTokenType, string>(tokenType, value);

			CssStringToken token;
			if (!_stringTokenCache.TryGetValue(key, out token))
			{
				token = new CssStringToken(tokenType, value);
				_stringTokenCache.Add(key, token);
			}
			return token;
		}
	}
}