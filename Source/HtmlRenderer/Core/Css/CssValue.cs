namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssValue : CssComponent
	{
		public static readonly CssValue Whitespace = new CssValue<char>(CssTokenType.Whitespace, ' ');

		private readonly CssTokenType _tokenType;

		protected CssValue(CssTokenType tokenType)
		{
			_tokenType = tokenType;
		}

		public CssTokenType TokenType
		{
			get { return _tokenType; }
		}

		public bool HasNumericValue
		{
			get { return (_tokenType & (CssTokenType.Number | CssTokenType.Dimension | CssTokenType.Percentage)) != 0; }
		}

		public bool IsIdentifier
		{
			get { return _tokenType == CssTokenType.Identifier; }
		}

		public bool IsString
		{
			get { return _tokenType == CssTokenType.QuotedString; }
		}

		public bool IsDimension
		{
			get { return (_tokenType & CssTokenType.Dimension) == CssTokenType.Dimension; }
		}

		public bool IsNumber
		{
			get { return (_tokenType & CssTokenType.Number) == CssTokenType.Number; }
		}

		public bool IsPercentage
		{
			get { return (_tokenType & CssTokenType.Percentage) == CssTokenType.Percentage; }
		}

		public bool IsWhitespace
		{
			get { return _tokenType == CssTokenType.Whitespace; }
		}

		public string Value
		{
			get { return (this as CssValue<string>)?.Value ?? ToString(); }
		}

		public CssNumeric? NumericValue
		{
			get { return (this as CssValue<CssNumeric>)?.Value; }
		}

		public CssUnicodeRange? UnicodeRangeValue
		{
			get { return (this as CssValue<CssUnicodeRange>)?.Value; }
		}

		public bool HasValue(string value, StringComparison stringComparison)
		{
			var typedComponent = this as CssValue<string>;
			return typedComponent != null 
				&& string.Equals(typedComponent.Value, value, stringComparison);
		}

		public bool HasValue(char value)
		{
			var typedComponent = this as CssValue<char>;
			return typedComponent != null 
				&& typedComponent.Value == value;
		}
	}

	public class CssValue<T> : CssValue where T : IEquatable<T>
	{
		private readonly T _value;

		public CssValue(CssTokenType tokenType, T value)
			: base(tokenType)
		{
			_value = value;
		}

		public new T Value
		{
			get { return _value; }
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		public override bool Equals(object obj)
		{
			var otherValue = obj as CssValue<T>;
			return otherValue != null 
				&& CssEqualityComparer<T>.Default.Equals(_value, otherValue._value);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(
				(int)this.TokenType,
				CssEqualityComparer<T>.Default.GetHashCode(_value));
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_value);
		}
	}
}
