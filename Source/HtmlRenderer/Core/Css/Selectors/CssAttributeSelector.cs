namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Globalization;
	using System.Text;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssAttributeSelector : CssSimpleSelector
	{
		private static readonly Func<string, StringComparison, bool> Always = (s, c) => true;
		private static readonly Func<string, StringComparison, bool> Never = (s, c) => false;

		private readonly XName _name;
		private readonly CssAttributeMatchOperator _matchOperator;
		private readonly string _matchOperand;
		private Func<string, StringComparison, bool> _predicate;

		public CssAttributeSelector(XName name)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
		}

		public CssAttributeSelector(XName name, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
			_matchOperator = matchOperator;
			_matchOperand = matchOperand;
		}

		public CssAttributeSelector(string localName)
		{
			_name = AnyNamespace + localName;
		}

		public CssAttributeSelector(string localName, CssAttributeMatchOperator matchOperator, string matchOperand)
		{
			_name = AnyNamespace + localName;
			_matchOperator = matchOperator;
			_matchOperand = matchOperand;
		}

		public override bool Matches<TElement>(TElement element)
		{
			var predicate = _predicate ?? (_predicate = CreatePredicate(_matchOperator, _matchOperand));
			return _name.Namespace == AnyNamespace
				? element.HasAttribute(_name.LocalName, predicate)
				: element.HasAttribute(_name, predicate);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAttributeSelector;
			return other != null
			    && _name == other._name
			    && _matchOperator == other._matchOperator
			    && _matchOperand == other._matchOperand;
		}

		public override int GetHashCode()
		{
			var hash = _name.GetHashCode();
			if (_matchOperator != CssAttributeMatchOperator.Any)
			{
				hash = HashUtility.Hash(hash, (int)_matchOperator);
				hash = HashUtility.Hash(hash, _matchOperand.GetHashCode());
			}
			return hash;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append('[');
			if (_name.Namespace != XNamespace.None)
			{
				sb.Append(_name.Namespace).Append('|');
			}
			sb.Append(_name.LocalName);
			if(_matchOperator != CssAttributeMatchOperator.Any)
			{
				if (_matchOperator != CssAttributeMatchOperator.Exact)
				{
					sb.Append((char) _matchOperator);
				}
				sb.Append('=')
					.Append('"')
					.Append(_matchOperand.Replace("\"", "\\\""))
					.Append('"');
			}
			sb.Append(']');
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