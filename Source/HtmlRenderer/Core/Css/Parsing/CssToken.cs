namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Generic;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssToken
	{
		public static readonly EqualityComparer<CssToken> TokenValueComparer = new CssTokenValueEqualityComparer();

		private readonly CssTokenType _tokenType;
		private readonly int _position;
		private readonly int _length;
		private readonly CssTokenData _data;

		internal CssToken(CssTokenType tokenType, int startPos, int length, CssTokenData data)
		{
			ArgChecker.AssertArgNotNull(data, nameof(data));

			_tokenType = tokenType;
			_position = startPos;
			_length = length;
			_data = data;
		}

		public CssTokenType TokenType
		{
			get { return _tokenType; }
		}

		public int Position
		{
			get { return _position; }
		}

		public int Length
		{
			get { return _length; }
		}

		public string RawValue
		{
			get { return _data.GetRawValue(ref this); }
		}

		public CssNumeric? NumericValue
		{
			get { return (_data as CssTokenData<CssNumeric>)?.Value; }
		}

		public string StringValue
		{
			get { return (_data as CssStringTokenData)?.GetValue(ref this); }
		}

		public CssUnicodeRange? UnicodeRangeValue
		{
			get { return (_data as CssTokenData<CssUnicodeRange>)?.Value; }
		}

		public bool IsInvalid
		{
			get { return (_tokenType & CssTokenType.Invalid) != 0; }
		}

		public bool IsBadStringToken
		{
			get
			{
				const CssTokenType BAD_STRING = CssTokenType.String | CssTokenType.Invalid;
				return (_tokenType & BAD_STRING) == BAD_STRING;
			}
		}

		public bool IsMatchOperator
		{
			get { return (_tokenType & CssTokenType.MatchOperator) == CssTokenType.MatchOperator; }
		}

		public bool IsNumber
		{
			get { return (_tokenType & CssTokenType.Number) == CssTokenType.Number; }
		}

		public bool IsString
		{
			get { return (_tokenType & CssTokenType.String) == CssTokenType.String; }
		}

		public bool IsWhitespace
		{
			get { return (_tokenType & CssTokenType.Whitespace) == CssTokenType.Whitespace; }
		}

		public bool IsDelimiter()
		{
			return (_tokenType & CssTokenType.Delimiter) == CssTokenType.Delimiter;
		}

		public bool IsDelimiter(char value)
		{
			return value <= 0xFF 
				&& _tokenType == (CssTokenType.Delimiter | (CssTokenType)value);
		}

		internal CssSimpleComponent CreateComponent()
		{
			return _data?.CreateComponent(ref this);
		}

		private class CssTokenValueEqualityComparer : EqualityComparer<CssToken>
		{
			public override bool Equals(CssToken x, CssToken y)
			{
				return x._tokenType == y._tokenType
				    && x._data.Equals(ref x, ref y, y._data);
			}

			public override int GetHashCode(CssToken obj)
			{
				return obj._data != null
					? HashUtility.Hash((int)obj._tokenType, obj._data.GetHashCode())
					: (int)obj._tokenType;
			}
		}

	}
}