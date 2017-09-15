namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	/// <summary>
	/// Parser CSS values into <see cref="CssNode"/> instances.
	/// </summary>
	public class CssParser
	{
		#region Static fields

		private static readonly CssGrammar DefaultGrammar = new CssStylesheetGrammar();

		#endregion

		#region Instance fields

		private readonly IEnumerator<CssToken> _tokenizer;
		private readonly CssGrammar _grammar;

		/// <summary>
		/// A map of tokens to components to reduce CssSimpleComponent memory allocations
		/// </summary>
		private readonly Dictionary<CssToken, CssValue> _componentTable = 
			new Dictionary<CssToken, CssValue>(CssToken.TokenValueComparer);

		#endregion

		#region Constructor(s)

		public CssParser(IEnumerable<CssToken> tokens)
			: this(null, tokens)
		{}

		public CssParser(CssGrammar grammar, IEnumerable<CssToken> tokens)
		{
			ArgChecker.AssertArgNotNull(tokens, nameof(tokens));
			_grammar = grammar ?? DefaultGrammar;
			_tokenizer = tokens.GetEnumerator();
		}

		#endregion

		#region Parse operations

		public CssStyleSheet ParseStyleSheet()
		{
			return new CssStyleSheet(ImmutableList.CreateRange(ParseRuleList(isTopLevel: true)));
		}

		public IEnumerable<CssRule> ParseRuleList()
		{
			return ParseRuleList(isTopLevel: false);
		}

		public CssRule ParseRule()
		{
			if (SkipWhiteSpace())
			{
				switch (CurrentToken.TokenType)
				{
					case CssTokenType.AtKeyword:
						return ConsumeAtRule(_grammar);
					default:
						return ConsumeQualifiedRule(_grammar);
				}
			}

			// syntax error
			return null;
		}

		private IEnumerable<CssRule> ParseRuleList(bool isTopLevel)
		{
			while (SkipWhiteSpace())
			{
				switch (CurrentToken.TokenType)
				{
					case CssTokenType.CDO:
					case CssTokenType.CDC:
						if (isTopLevel) continue;
						goto default;
					case CssTokenType.AtKeyword:
						var atRule = ConsumeAtRule(_grammar);
						if (atRule != null) yield return atRule;
						break;
					default:
						var rule = ConsumeQualifiedRule(_grammar);
						if (rule != null) yield return rule;
						// NOTE: null rule is parse error
						break;
				}
			}
		}

		/// <summary>
		/// Parses a list of <see cref="CssDeclaration"/> and <see cref="CssAtRule"/> instances.
		/// </summary>
		/// <returns>The declaration list.</returns>
		public IEnumerable<CssNode> ParseDeclarationList()
		{
			while (SkipWhiteSpace())
			{
				switch (CurrentToken.TokenType)
				{
					case CssTokenType.Semicolon:
						continue;
					case CssTokenType.AtKeyword:
						var atRule = ConsumeAtRule(_grammar);
						if (atRule != null) yield return atRule;
						continue;
					case CssTokenType.Identifier:
						var declaration = ConsumeDeclaration(DefaultGrammar, CssTokenType.Semicolon);
						if (declaration != null) yield return declaration;
						continue;
				}

				// Parse error
				if (!SkipUntil(CssTokenType.Semicolon)) yield break;
			}
		}

		public CssDeclaration ParseDeclaration()
		{
			return SkipWhiteSpace() && CurrentToken.IsIdentifier
				? ConsumeDeclaration(DefaultGrammar)
				: null;
		}

		public IEnumerable<CssComponent> ParseComponentList()
		{
			while (MoveNext())
			{
				yield return ConsumeComponent();
			}
		}

		public CssComponent ParseComponent()
		{
			if (!SkipWhiteSpace()) return null; // Syntax error: premature EOF
			var component = ConsumeComponent();
			return component != null && !SkipWhiteSpace()
				? component
				: null;		// Syntax error: EOF expected but other token found
		}

		#endregion

		#region Consume operations - use current token as starting point

		/* 
		 * NOTE: Each ConsumeXXX method consumes the token that is current as of when the 
		 * method is called and consumes all subsequent tokens that are read from the tokenizer 
		 * during method execution. ConsumeXXX operations must only call other ConsumeYYY 
		 * operations and not call any ParseXXX methods.
		 */ 

		/// <summary>
		/// Reads current token and all subsequent tokens that compose into a <see cref="CssQualifiedRule"/> instance.
		/// </summary>
		/// <param name="grammar"></param>
		/// <returns></returns>
		protected CssQualifiedRule ConsumeQualifiedRule(CssGrammar grammar)
		{
			var preludeFragmentParser = new QualifiedRulePreludeFragmentParser(this);
			var prelude = grammar.ConsumeQualifiedRulePrelude(preludeFragmentParser);

			if (CurrentToken.TokenType == CssTokenType.LeftCurlyBracket && MoveNext())
			{
				var blockFragmentParser = new BlockFragmentParser(this);
				var block = grammar.ConsumeQualifiedRuleBlock(blockFragmentParser);
				return grammar.CreateQualifiedRule(prelude, block);
			}

			// Parse error
			return null;
		}

		protected CssAtRule ConsumeAtRule(CssGrammar grammar)
		{ 
			var name = CurrentToken.StringValue;
			CssComponent prelude = null;
			CssBlock block = null;

			if (MoveNext())
			{
				var preludeFragmentParser = new AtRulePreludeFragmentParser(this);
				prelude = grammar.ConsumeAtRulePrelude(name, preludeFragmentParser);
				if (CurrentToken.TokenType == CssTokenType.LeftCurlyBracket && MoveNext())
				{
					var blockFragmentParser = new BlockFragmentParser(this);
					block = grammar.ConsumeAtRuleBlock(name, blockFragmentParser);
				}
			}
			return grammar.CreateAtRule(name, prelude ?? CssComponent.Empty, block);
		}

		protected CssDeclaration ConsumeDeclaration(CssGrammar grammar)
		{
			return ConsumeDeclaration(grammar, 0);
		}

		protected CssDeclaration ConsumeDeclaration(CssGrammar grammar, CssTokenType endTokenType)
		{
			var name = CurrentToken.StringValue;
			if (!SkipWhiteSpace()) return null;								// Syntax error: unexpected EOF
			if (CurrentToken.TokenType != CssTokenType.Colon) return null;	// Parse error: expected colon

			var values = ImmutableArray.CreateBuilder<CssValue>(4);
			while (MoveNext() && CurrentToken.TokenType != endTokenType)
			{
				values.Add(ConsumeValue());
			}

			var isImportant = TrimTrailingImportantPhrase(values);
			return grammar.CreateDeclaration(name, values.ToImmutable(), isImportant);
		}

		/// <summary>
		/// Removes any trailing !important fragment.
		/// </summary>
		/// <param name="values"></param>
		/// <returns>Returns <c>true</c> if !important fragment was removed.</returns>
		private static bool TrimTrailingImportantPhrase(ImmutableArray<CssValue>.Builder values)
		{
			var m = values.Count - 1;
			int i;

			// Skip trailing whitespace
			for (i = m; i > 0 && values[i].IsWhitespace; i--) ;

			// Test for identifier "important" 
			if (i >= 1 && values[i].HasValue("important", StringComparison.OrdinalIgnoreCase))
			{
				// Skip whitespace before the "important" identifier 
				for (i = i - 1; i >= 0 && values[i].IsWhitespace; i--) ;
				// Test for "!" delimiter
				if (i >= 0 && values[i].HasValue('!'))
				{
					// !important phrase has been found => trim the tokens that make up the phrase 
					// (including any whitespace in or after it) from the declaration values.
					for (var j = m; j >= i; j--)
					{
						values.RemoveAt(j);
					}
					return true;
				}
			}

			return false;
		}

		protected CssComponent ConsumeComponent()
		{
			switch (CurrentToken.TokenType)
			{
				case CssTokenType.LeftParenthesis:
					return ConsumeSimpleBlock(CssBlockType.Parentheses);
				case CssTokenType.LeftCurlyBracket:
					return ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
				case CssTokenType.LeftSquareBracket:
					return ConsumeSimpleBlock(CssBlockType.SquareBrackets);
				case CssTokenType.Function:
					return ConsumeFunction();
				default:
					return ConsumeValue();
			}
		}

		protected CssBlock ConsumeSimpleBlock(CssBlockType blockType)
		{
			var components = ImmutableList.CreateBuilder<CssComponent>();
			while (MoveNext() && CurrentToken.TokenType != blockType.EndTokenType)
			{
				components.Add(ConsumeComponent());
			}
			return new CssBlock(blockType, components.ToImmutable());
		}

		protected CssFunction ConsumeFunction()
		{
			var name = CurrentToken.StringValue;

			var components = ImmutableArray.CreateBuilder<CssComponent>();
			while (MoveNext() && CurrentToken.TokenType != CssTokenType.RightParenthesis)
			{
				components.Add(ConsumeComponent());
			}

			return _grammar.CreateFunction(name, components.ToImmutable());
		}

		protected CssValue ConsumeValue()
		{
			CssValue result;
			if (!_componentTable.TryGetValue(CurrentToken, out result))
			{
				result = CurrentToken.CreateComponent();
				_componentTable.Add(CurrentToken, result);
			}
			return result;
		}

		#endregion

		#region Utility methods

		protected CssToken CurrentToken
		{
			get { return _tokenizer.Current; }
		}

		protected bool MoveNext()
		{
			return _tokenizer.MoveNext();
		}

		protected bool SkipWhiteSpace()
		{
			while (MoveNext())
			{
				if (!CurrentToken.IsWhitespace) return true;
			}
			return false;
		}

		protected bool SkipUntil(CssTokenType tokenType)
		{
			while (MoveNext())
			{
				if (CurrentToken.TokenType == tokenType) return true;
			}
			return false;
		}

		#endregion

		#region Inner classes

		public abstract class FragmentParser
		{
			private readonly CssParser _parser;
			private int _state;

			internal FragmentParser(CssParser parser)
			{
				_parser = parser;
			}

			public CssToken CurrentToken
			{
				get { return _parser._tokenizer.Current; }
			}

			public bool IsEof
			{
				get { return _state >= 2; }
			}

			public bool MoveNext()
			{
				var tokenizer = _parser._tokenizer;
				if (_state == 0)
				{
					if (IsEndToken(tokenizer.Current.TokenType))
					{
						_state = 2;
						return false;
					}
					_state = 1;
				}
				else if (_state == 1)
				{
					if (!tokenizer.MoveNext() || IsEndToken(tokenizer.Current.TokenType))
					{
						_state = 2;
					}
				}
				return _state < 2;
			}

			public CssQualifiedRule ConsumeQualifiedRule(CssGrammar grammar)
			{
				return _parser.ConsumeQualifiedRule(grammar);
			}

			public CssAtRule ConsumeAtRule(CssGrammar grammar)
			{
				return _parser.ConsumeAtRule(grammar);
			}

			public CssDeclaration ConsumeDeclaration(CssGrammar grammar)
			{
				return _parser.ConsumeDeclaration(grammar, CssTokenType.Semicolon);
			}

			public CssComponent ConsumeComponent()
			{
				return _parser.ConsumeValue();
			}

			public CssBlock ConsumeSimpleBlock(CssBlockType blockType)
			{
				return _parser.ConsumeSimpleBlock(blockType);
			}

			public CssValue ConsumeValue()
			{
				return _parser.ConsumeValue();
			}

			protected abstract bool IsEndToken(CssTokenType tokenType);
		}

		private class QualifiedRulePreludeFragmentParser : FragmentParser
		{
			public QualifiedRulePreludeFragmentParser(CssParser parser) 
				: base(parser)
			{}

			protected override bool IsEndToken(CssTokenType tokenType)
			{
				return tokenType == 0 
					|| tokenType == CssTokenType.LeftCurlyBracket;
			}
		}

		private class AtRulePreludeFragmentParser : FragmentParser
		{
			public AtRulePreludeFragmentParser(CssParser parser)
				: base(parser)
			{ }

			protected override bool IsEndToken(CssTokenType tokenType)
			{
				return tokenType == 0
				    || tokenType == CssTokenType.LeftCurlyBracket
					|| tokenType == CssTokenType.Semicolon;
			}
		}

		private class BlockFragmentParser : FragmentParser
		{
			private int _nestLevel = 1;

			public BlockFragmentParser(CssParser parser) 
				: base(parser)
			{}

			protected override bool IsEndToken(CssTokenType tokenType)
			{
				switch (tokenType)
				{
					case CssTokenType.LeftCurlyBracket:
						_nestLevel++;
						break;
					case CssTokenType.RightCurlyBracket:
						_nestLevel--;
						return _nestLevel <= 0;
				}
				return false;
			}
		}

		#endregion
	}
}
