namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Css;

	internal abstract class CssTokenData
	{
		public abstract string GetRawValue(ref CssToken token);
		public abstract CssValue CreateComponent(ref CssToken token);
		public abstract bool Equals(ref CssToken token, ref CssToken otherToken, CssTokenData otherData);
		public abstract int GetHashCode(ref CssToken token);
	}

	internal sealed class CssTokenData<T> : CssTokenData where T : struct, IEquatable<T>
	{
		private readonly string _input;
		private readonly T _value;

		public CssTokenData(string input, T value)
		{
			_input = input;
			_value = value;
		}

		public T Value
		{
			get { return _value; }
		}

		public override string GetRawValue(ref CssToken token)
		{
			return _input.Substring(token.Position, token.Length);
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

		public override string GetRawValue(ref CssToken token)
		{
			return GetValue(ref token).ToString();
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
			var otherTypedData = otherData as CssAsciiTokenData;
			return otherTypedData != null
			    && otherTypedData.GetValue(ref otherToken).Equals(GetValue(ref token));
		}

		public override int GetHashCode(ref CssToken token)
		{
			return (int)token.TokenType & 0xFF;
		}
	}

	internal sealed class CssStringTokenData : CssTokenData
	{
		private readonly string _input;
		private string _value;

		public CssStringTokenData(string input, string value)
		{
			_input = input;
			_value = value;
		}

		public string GetValue(ref CssToken token)
		{
			return _value ?? (_value = GetRawValue(ref token) ?? "");
		}

		public override string GetRawValue(ref CssToken token)
		{
			return _input.Substring(token.Position, token.Length);
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
