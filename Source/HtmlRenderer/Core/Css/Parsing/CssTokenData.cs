namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Globalization;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal abstract class CssTokenData
	{
		public abstract string ToString(ref CssToken token);
		public abstract CssValue CreateComponent(ref CssToken token);
		public abstract bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData);
		public abstract int GetHashCode(ref CssToken token);
	}

	internal sealed class CssTokenData<T> : CssTokenData where T : struct, IEquatable<T>, IFormattable
	{
		private readonly T _value;

		public CssTokenData(T value)
		{
			_value = value;
		}

		public T Value
		{
			get { return _value; }
		}

		public override string ToString(ref CssToken token)
		{
			return _value.ToString(null, CultureInfo.InvariantCulture);
		}

		public override CssValue CreateComponent(ref CssToken token)
		{
			return new CssValue<T>(token.TokenType, _value);
		}

		public override bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData)
		{
			var otherTypedData = otherData as CssTokenData<T>;
			return otherTypedData != null
			       && otherTypedData._value.Equals(_value);
		}

		public override int GetHashCode(ref CssToken token)
		{
			return this.Value.GetHashCode();
		}
	}

	internal sealed class CssAsciiTokenData : CssTokenData
	{
		public static readonly CssAsciiTokenData Instance = new CssAsciiTokenData();

		private CssAsciiTokenData()
		{ }

		public override string ToString(ref CssToken token)
		{
			return new string(GetValue(ref token), 1);
		}

		public char GetValue(ref CssToken token)
		{
			return (char)((int)token.TokenType & 0xFF);
		}

		public override CssValue CreateComponent(ref CssToken token)
		{
			return token.IsWhitespace
				? CssValue.Whitespace
				: new CssValue<char>(token.TokenType, GetValue(ref token));
		}

		public override bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData)
		{
			return token.TokenType == otherToken.TokenType
				&& ReferenceEquals(this, otherData);
		}

		public override int GetHashCode(ref CssToken token)
		{
			return (int)token.TokenType & 0xFF;
		}
	}

	internal sealed class CssOperatorTokenData : CssTokenData
	{
		public static readonly CssOperatorTokenData Instance = new CssOperatorTokenData();

		private CssOperatorTokenData()
		{ }

		public override string ToString(ref CssToken token)
		{
			switch (token.TokenType)
			{
				case CssTokenType.Column:
					return "||";
				default:
					return new StringBuilder(2).Append((char) ((int) token.TokenType & 0xFF)).Append('=').ToString();
			}
		}

		public override CssValue CreateComponent(ref CssToken token)
		{
			return token.IsWhitespace
				? CssValue.Whitespace
				: new CssValue<string>(token.TokenType, ToString(ref token));
		}

		public override bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData)
		{
			return token.TokenType == otherToken.TokenType
				&& ReferenceEquals(this, otherData);
		}

		public override int GetHashCode(ref CssToken token)
		{
			return (int)token.TokenType & 0xFF;
		}
	}

	internal sealed class CssStringTokenData : CssTokenData
	{
		private readonly string _value;

		public CssStringTokenData(string value)
		{
			ArgChecker.AssertArgNotNull(value, nameof(value));
			_value = value;
		}

		public string GetValue(ref CssToken token)
		{
			return _value;
		}

		public override string ToString(ref CssToken token)
		{
			return _value;
		}

		public override CssValue CreateComponent(ref CssToken token)
		{
			return token.IsWhitespace
				? CssValue.Whitespace
				: new CssValue<string>(token.TokenType, GetValue(ref token));
		}

		public override bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData)
		{
			var otherTypedData = otherData as CssStringTokenData;
			return otherTypedData != null
			    && otherTypedData.GetValue(ref otherToken).Equals(GetValue(ref token));
		}

		public override int GetHashCode(ref CssToken token)
		{
			return GetValue(ref token).GetHashCode();
		}
	}
}
