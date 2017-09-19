namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Globalization;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssToken
	{
		private readonly CssTokenType _tokenType;

		internal CssToken(CssTokenType tokenType)
		{
			_tokenType = tokenType;
		}

		public CssTokenType TokenType
		{
			get { return _tokenType; }
		}

		public CssNumeric? NumericValue
		{
			get { return (this as CssToken<CssNumeric>)?.Value; }
		}

		public string StringValue
		{
			get { return (this as CssStringToken)?.Value; }
		}

		public CssUnicodeRange? UnicodeRangeValue
		{
			get { return (this as CssToken<CssUnicodeRange>)?.Value; }
		}

		public bool IsInvalid
		{
			get { return (_tokenType & CssTokenType.Invalid) != 0; }
		}

		public bool IsDimension
		{
			get { return (_tokenType & CssTokenType.Dimension) == CssTokenType.Dimension; }
		}

		public bool IsIdentifier
		{
			get { return (_tokenType & CssTokenType.Identifier) == CssTokenType.Identifier; }
		}

		public bool IsMatchOperator
		{
			get { return (_tokenType & CssTokenType.MatchOperator) == CssTokenType.MatchOperator; }
		}

		public bool IsNumber
		{
			get { return (_tokenType & CssTokenType.Number) == CssTokenType.Number; }
		}

		public bool IsPercentage
		{
			get { return (_tokenType & CssTokenType.Percentage) == CssTokenType.Percentage; }
		}

		public bool IsQuotedString
		{
			get { return (_tokenType & CssTokenType.QuotedString) == CssTokenType.QuotedString; }
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

		public abstract override string ToString();

		public override bool Equals(object obj)
		{
			var other = obj as CssToken;
			return other != null
			    && _tokenType == other._tokenType;
		}

		public override int GetHashCode()
		{
			return (int)_tokenType;
		}

		internal abstract CssValue CreateComponent();
	}

	public sealed class CssToken<T> : CssToken 
		where T: struct, IEquatable<T>, IFormattable
	{
		private readonly T _value;

		public CssToken(CssTokenType tokenType, T value)
			: base(tokenType)
		{
			_value = value;
		}
		public T Value
		{
			get { return _value; }
		}
		public override string ToString()
		{
			return _value.ToString(null, CultureInfo.InvariantCulture);
		}

		public override bool Equals(object obj)
		{
			var otherToken = obj as CssToken<T>;
			return otherToken != null
				&& this.TokenType == otherToken.TokenType
				&& _value.Equals(otherToken._value);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int) this.TokenType, _value.GetHashCode());
		}

		internal override CssValue CreateComponent()
		{
			return new CssValue<T>(this.TokenType, _value);
		}
	}

	public sealed class CssAsciiToken : CssToken
	{
		public CssAsciiToken(CssTokenType tokenType)
			: base(tokenType)
		{ }

		public char Value
		{
			get { return (char) ((int) this.TokenType & 0xFF); }
		}

		public override string ToString()
		{
			return new string(this.Value, 1);
		}

		internal override CssValue CreateComponent()
		{
			return this.IsWhitespace
				? CssValue.Whitespace
				: new CssValue<char>(this.TokenType, this.Value);
		}
	}

	internal sealed class CssOperatorToken : CssToken
	{
		public CssOperatorToken(CssTokenType tokenType)
			: base(tokenType)
		{ }

		public override string ToString()
		{
			switch (this.TokenType)
			{
				case CssTokenType.Column:
					return "||";
				default:
					return new StringBuilder(2).Append((char)((int)this.TokenType & 0xFF)).Append('=').ToString();
			}
		}

		internal override CssValue CreateComponent()
		{
			return new CssValue<string>(this.TokenType, ToString());
		}
	}

	internal sealed class CssStringToken : CssToken
	{
		private readonly string _value;

		public CssStringToken(CssTokenType tokenType, string value)
			: base(tokenType)
		{
			ArgChecker.AssertArgNotNull(value, nameof(value));
			_value = value;
		}

		public string Value
		{
			get { return _value; }
		}

		public override string ToString()
		{
			return _value;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssStringToken;
			return other != null
				&& this.TokenType == other.TokenType
				&& CssEqualityComparer<string>.Default.Equals(_value, other._value);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(base.GetHashCode(), CssEqualityComparer<string>.Default.GetHashCode(_value));
		}

		internal override CssValue CreateComponent()
		{
			return this.IsWhitespace
				? CssValue.Whitespace
				: new CssValue<string>(this.TokenType, _value);
		}
	}
}