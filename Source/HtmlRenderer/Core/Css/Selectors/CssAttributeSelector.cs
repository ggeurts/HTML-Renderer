namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Globalization;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;

	/// <summary>
	/// Represents a selector for elements that have an attribute with a given local name and/or namespace and/or whose value
	/// meets a given string matching criterion.
	/// </summary>
	public abstract class CssAttributeSelector : CssSimpleSelector
	{
		private static readonly Func<string, StringComparison, bool> Always = (s, c) => true;
		private static readonly Func<string, StringComparison, bool> Never = (s, c) => false;

		private readonly CssAttributeMatchOperator _matchOperator;
		private readonly string _matchOperand;
		private Func<string, StringComparison, bool> _predicate;

		protected CssAttributeSelector()
		{}

		protected CssAttributeSelector(CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			_matchOperator = matchOperator;
			_matchOperand = matchOperand;
		}

		/// <summary>
		/// Gets local name of matching attributes.
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// Gets namespace of matching attributes.
		/// </summary>
		public abstract XNamespace Namespace { get; }

		/// <summary>
		/// Gets operator for matching of attribute values to <see cref="MatchOperand"/>.
		/// </summary>
		public CssAttributeMatchOperator MatchOperator
		{
			get { return _matchOperator; }
		}

		/// <summary>
		/// Gets value to which attribute values are matched.
		/// </summary>
		public string MatchOperand
		{
			get { return _matchOperand; }
		}

		public Func<string, StringComparison, bool> Predicate
		{
			get { return _predicate ?? (_predicate = CreatePredicate(_matchOperator, _matchOperand)); }
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			if (_matchOperator != CssAttributeMatchOperator.Any)
			{
				if (_matchOperator != CssAttributeMatchOperator.Exact)
				{
					sb.Append((char)_matchOperator);
				}
				sb.Append('=')
					.Append('"')
					.Append(_matchOperand.Replace("\"", "\\\""))
					.Append('"');
			}
		}

		private static Func<string, StringComparison, bool> CreatePredicate(CssAttributeMatchOperator matchOperator, string operand)
		{
			switch (matchOperator)
			{
				case CssAttributeMatchOperator.Any:
					return Always;
				case CssAttributeMatchOperator.Exact:
					return (s, c) => (s ?? "").Equals(operand, c);
				case CssAttributeMatchOperator.Prefix:
					return (s, c) => s != null && s.StartsWith(operand, c);
				case CssAttributeMatchOperator.Suffix:
					return (s, c) => s != null && s.EndsWith(operand, c);
				case CssAttributeMatchOperator.Contains:
					return (s, c) => s != null && s.IndexOf(operand, c) >= 0;
				case CssAttributeMatchOperator.ContainsWord:
					return ContainsWhitespace(operand) 
						? Never
						: (s, c) => ContainsWord(s, operand, c);
				case CssAttributeMatchOperator.LanguageCode:
					return (s, c) => s != null 
						&& s.StartsWith(operand, c)
					    && (s.Length == operand.Length || s[operand.Length] == '-');
				default:
					throw new NotImplementedException(
						string.Format(
							CultureInfo.InvariantCulture, 
							"Attribute value matching with '{0}' operator has not yet been implemented", 
							matchOperator));
			}
		}

		private static bool ContainsWord(string text, string word, StringComparison stringComparison)
		{
			if (text == null) return false;

			var index = text.IndexOf(word, stringComparison);
			return index >= 0
			       && (index == 0 || IsWhitespace(text[index]))
			       && (index + word.Length >= text.Length || IsWhitespace(text[index + word.Length]));
		}

		private static bool ContainsWhitespace(string text)
		{
			if (text == null) return false;
			for (var i = 0; i < text.Length; i++)
			{
				if (IsWhitespace(text[i])) return true;
			}
			return false;
		}

		private static bool IsWhitespace(char ch)
		{
			switch (ch)
			{
				case ' ':
				case '\t':
				case '\f':
				case '\r':
				case '\n':
					return true;
				default:
					return false;
			}
		}
	}
}