namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Globalization;
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
		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(0, 1, 0);

		private static readonly Func<string, StringComparison, bool> Always = (s, c) => true;
		private static readonly Func<string, StringComparison, bool> Never = (s, c) => false;

		private readonly string _localName;
		private readonly string _namespacePrefix;
		private readonly CssAttributeMatchOperator _matchOperator;
		private readonly CssStringToken _matchOperand;

		internal CssAttributeSelector(string localName, string namespacePrefix)
			: base(DefaultSpecificity)
		{
			_localName = localName;
			_namespacePrefix = namespacePrefix;
		}

		internal CssAttributeSelector(string localName, string namespacePrefix, CssAttributeMatchOperator matchOperator, CssStringToken matchOperand)
			: base(DefaultSpecificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(localName, nameof(localName));
			ArgChecker.AssertIsTrue<ArgumentNullException>(
				matchOperator == CssAttributeMatchOperator.Any || matchOperand != null,
				string.Format(CultureInfo.InvariantCulture, "The {0} is required for match operator '{1}'.", nameof(matchOperand), matchOperator));

			_localName = localName;
			_namespacePrefix = namespacePrefix;
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

		/// <summary>
		/// Gets value to which attribute values are matched.
		/// </summary>
		internal CssStringToken MatchOperandToken
		{
			get { return _matchOperand; }
		}

		public string LocalName
		{
			get { return _localName; }
		}

		public string NamespacePrefix
		{
			get { return _namespacePrefix; }
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitAttributeSelector(this);
		}

		/// <inheritdoc />
		public Func<string, StringComparison, bool> CreatePredicate()
		{
			var operand = this.MatchOperand;
			switch (_matchOperator)
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
							_matchOperator));
			}
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAttributeSelector;
			return other != null
			       && _localName == other._localName
			       && _namespacePrefix == other._namespacePrefix
			       && _matchOperator == other._matchOperator
			       && Equals(this.MatchOperand, other.MatchOperand);
		}

		public override int GetHashCode()
		{
			var hash = _localName.GetHashCode();
			hash = HashUtility.Hash(hash, _namespacePrefix?.GetHashCode() ?? 0);
			if (_matchOperator != CssAttributeMatchOperator.Any)
			{
				hash = HashUtility.Hash(hash, (int)_matchOperator);
				hash = HashUtility.Hash(hash, this.MatchOperand.GetHashCode());
			}
			return hash;
		}

		#region Private methods

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

		#endregion

		#region Inner classes


		#endregion
	}
}