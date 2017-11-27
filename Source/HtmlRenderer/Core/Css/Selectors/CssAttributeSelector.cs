namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Globalization;
	using System.Text;
	using System.Xml;
	using System.Xml.Linq;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	/// <summary>
	/// Represents a selector for elements that have an attribute with a given local name and/or namespace and/or whose value
	/// meets a given string matching criterion.
	/// </summary>
	public class CssAttributeSelector : CssSimpleSelector
	{
		private static readonly Func<string, StringComparison, bool> Always = (s, c) => true;
		private static readonly Func<string, StringComparison, bool> Never = (s, c) => false;

		private readonly XName _name;
		private readonly CssAttributeMatchOperator _matchOperator;
		private readonly CssStringToken _matchOperand;
		private Func<string, StringComparison, bool> _predicate;

		internal CssAttributeSelector(XName name)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
		}

		internal CssAttributeSelector(XName name, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			_name = name;
			_matchOperator = matchOperator;
			_matchOperand = matchOperand;
		}

		internal CssAttributeSelector(string localName)
		{
			_name = AnyNamespace + localName;
		}

		internal CssAttributeSelector(string localName, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
		{
			_name = AnyNamespace + localName;
			_matchOperator = matchOperator;
			_matchOperand = matchOperand;
		}

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
			get { return _matchOperand?.StringValue; }
		}

		public Func<string, StringComparison, bool> Predicate
		{
			get { return _predicate ?? (_predicate = CreatePredicate(_matchOperator, _matchOperand?.StringValue)); }
		}

		public virtual string LocalName
		{
			get { return _name.LocalName; }
		}

		public virtual XNamespace Namespace
		{
			get { return _name.Namespace; }
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			sb.Append('[');

			if (_name.Namespace != XNamespace.None)
			{
				var namespacePrefix = _name.Namespace == AnyNamespace
					? AnyNamespacePrefix
					: namespaceResolver.LookupPrefix(_name.NamespaceName);
				// We must write a namespace prefix for any explicit namespace. The default namespace
				// does not apply to attributes.
				sb.Append(namespacePrefix).Append('|');
			}
			sb.Append(_name.LocalName);

			if (_matchOperator != CssAttributeMatchOperator.Any)
			{
				if (_matchOperator != CssAttributeMatchOperator.Exact)
				{
					sb.Append((char)_matchOperator);
				}
				sb.Append('=');
				_matchOperand?.ToString(sb);
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
			ArgChecker.AssertArgNotNull(word, nameof(word));
			if (text == null) return false;

			var index = text.IndexOf(word, stringComparison);
			return index >= 0
				&& (index == 0 || IsWhitespace(text[index - 1]))
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

		/// <inheritdoc />
		public override bool Matches<TElement>(TElement element)
		{
			return _name.Namespace == AnyNamespace
				? element.HasAttribute(_name.LocalName, this.Predicate)
				: element.HasAttribute(_name, this.Predicate);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAttributeSelector;
			return other != null
			       && _name == other._name
			       && _matchOperator == other._matchOperator
			       && Equals(_matchOperand, other._matchOperand);
		}

		public override int GetHashCode()
		{
			var hash = _name.GetHashCode();
			if (this.MatchOperator != CssAttributeMatchOperator.Any)
			{
				hash = HashUtility.Hash(hash, (int)this.MatchOperator);
				hash = HashUtility.Hash(hash, this.MatchOperand.GetHashCode());
			}
			return hash;
		}
	}
}