namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Globalization;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssToken : CssComponent
	{
		public static readonly CssToken Whitespace = new CssStringToken(CssTokenType.Whitespace, " ");
		public new static readonly CssToken Empty = new CssStringToken(CssTokenType.Undefined, "");

		private readonly CssTokenType _tokenType;

		internal CssToken(CssTokenType tokenType)
		{
			_tokenType = tokenType;
		}

		public CssTokenType TokenType
		{
			get { return _tokenType; }
		}

		public virtual CssNumeric? NumericValue
		{
			get { return (this as CssNumericToken)?.Value; }
		}

		public string StringValue
		{
			get { return (this as CssStringToken)?.Value; }
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

		public bool IsFunction
		{
			get { return (_tokenType & CssTokenType.Function) == CssTokenType.Function; }
		}

		public bool IsHash
		{
			get { return (_tokenType & CssTokenType.Hash) == CssTokenType.Hash; }
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

		/// <summary>
		/// Case insensitive equality test of token text and a given string value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool HasValue(string value)
		{
			if (value == null) return false;
			if (value.Length == 1) return HasValue(value[0]);

			var token = this as CssStringToken;
			return token != null
			    && CssEqualityComparer<string>.Default.Equals(token.Value, value);
		}

		/// <summary>
		/// Case insensitive equality test of token text and a given character value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool HasValue(char value)
		{
			var asciiToken = this as CssAsciiToken;
			if (asciiToken != null && asciiToken.Value == value) return true;

			var stringToken = this as CssStringToken;
			if (stringToken != null 
				&& stringToken.Value.Length == 1 
				&& CssCharEqualityComparer.Default.Equals(stringToken.Value[0], value)) return true;

			return false;
		}

		/// <summary>
		/// Case insensitive test of whether token text starts with a given string value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool StartsWith(string value)
		{
			if (value == null) return false;
			if (value.Length == 1) return HasValue(value[0]);

			var token = this as CssStringToken;
			return token != null
			       && CssEqualityComparer<string>.Default.Equals(token.Value, value);
		}

		/// <summary>
		/// Case insensitive test of whether token text starts with a given character value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool StartsWith(char value)
		{
			var asciiToken = this as CssAsciiToken;
			if (asciiToken != null && asciiToken.Value == value) return true;

			var stringToken = this as CssStringToken;
			if (stringToken != null
			    && stringToken.Value.Length >= 1
			    && CssCharEqualityComparer.Default.Equals(stringToken.Value[0], value)) return true;

			return false;
		}

		public abstract override string ToString();

		public override void ToString(StringBuilder sb)
		{
			sb.Append(ToString());
		}

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
			var other = obj as CssToken<T>;
			return other != null
				&& this.TokenType == other.TokenType
				&& _value.Equals(other._value);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int) this.TokenType, _value.GetHashCode());
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
	}

	/// <summary>
	/// Represents an identifier, keyword or quoted string token.
	/// </summary>
	internal class CssStringToken : CssToken
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
			return this.IsQuotedString 
				? "\"" + _value.Replace("\"", "\\\"") + "\"" 
				: _value;
		}

		public override void ToString(StringBuilder sb)
		{
			if (this.IsQuotedString)
			{
				var quoteChar = EnsureQuoteChar(this.TokenType);
				sb.Append(quoteChar);

				if (_value != null)
				{
					// Append _value to string builder, while ensuring that embedded quote characters are escaped
					var fragmentStart = 0;
					var quoteIndex = _value.IndexOf(quoteChar);
					while (quoteIndex >= 0)
					{
						sb.Append(_value, fragmentStart, quoteIndex - fragmentStart).Append("\\");
						fragmentStart = quoteIndex;
						quoteIndex = _value.IndexOf(quoteChar, fragmentStart + 1);
					}
					sb.Append(_value, fragmentStart, _value.Length - fragmentStart);
				}

				sb.Append(quoteChar);
			}
			else
			{
				sb.Append(_value);
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssStringToken;
			return other != null
			    && this.TokenType == other.TokenType
			    && _value == other._value;
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int)this.TokenType, _value.GetHashCode());
		}

		private static char EnsureQuoteChar(CssTokenType tokenType)
		{
			var result = (int)tokenType & 0xFF;
			return result != 0
				? (char)result
				: '"';
		}
	}

	/// <summary>
	/// Represents a number, percentage or dimension token
	/// </summary>
	internal class CssNumericToken : CssToken
	{
		private readonly CssNumeric _value;

		// We must hold onto the original number representation, to support the "urange" production rule
		// that specifies the grammar of unicode ranges.
		private readonly string _unparsedValue;

		public CssNumericToken(CssTokenType tokenType, string value, string unit = null) 
			: base(tokenType)
		{
			ArgChecker.AssertArgNotNullOrEmpty(value, nameof(value));
			_unparsedValue = value;
			_value = new CssNumeric(double.Parse(value, CultureInfo.InvariantCulture), unit);
		}

		public CssNumeric Value
		{
			get { return _value; }
		}

		public override string ToString()
		{
			return _unparsedValue + _value.Unit;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_unparsedValue).Append(_value.Unit);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssNumericToken;
			return other != null
				&& this.TokenType == other.TokenType
				&& _value.Equals(other._value);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int)this.TokenType, _value.GetHashCode());
		}
	}
}