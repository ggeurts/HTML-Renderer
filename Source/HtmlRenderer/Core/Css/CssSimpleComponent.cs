namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;

	public abstract class CssSimpleComponent : CssComponent
	{
		internal CssTokenType TokenType { get; }

		protected CssSimpleComponent(CssTokenType tokenType)
		{
			this.TokenType = tokenType;
		}

		public bool IsWhitespace
		{
			get { return this.TokenType == CssTokenType.Whitespace; }
		}

		public bool HasValue(string value, StringComparison stringComparison)
		{
			var typedComponent = this as CssSimpleComponent<string>;
			return typedComponent != null 
				&& string.Equals(typedComponent.Value, value, stringComparison);
		}

		public bool HasValue(char value)
		{
			var typedComponent = this as CssSimpleComponent<char>;
			return typedComponent != null 
				&& typedComponent.Value == value;
		}
	}

	public class CssSimpleComponent<T> : CssSimpleComponent
	{
		public T Value { get; }

		public CssSimpleComponent(CssTokenType tokenType, T value)
			: base(tokenType)
		{
			this.Value = value;
		}
	}
}
