namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
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

		private readonly RootScope _rootScope;

		#endregion

		#region Constructor(s)

		public CssParser(IEnumerable<CssToken> tokens)
			: this(null, tokens)
		{}

		public CssParser(CssGrammar grammar, IEnumerable<CssToken> tokens)
		{
			ArgChecker.AssertArgNotNull(tokens, nameof(tokens));
			_rootScope = new RootScope(tokens.GetEnumerator(), grammar ?? DefaultGrammar);
		}

		#endregion

		#region Parse operations

		public CssStyleSheet ParseStyleSheet()
		{
			return new CssStyleSheet(ImmutableList.CreateRange(_rootScope.ParseRuleList(true)));
		}

		public IEnumerable<CssRule> ParseRuleList()
		{
			return _rootScope.ParseRuleList();
		}

		public CssRule ParseRule()
		{
			return _rootScope.ParseRule();
		}

		/// <summary>
		/// Parses a list of <see cref="CssDeclaration"/> and <see cref="CssAtRule"/> instances.
		/// </summary>
		/// <returns>The declaration list.</returns>
		public IEnumerable<CssComponent> ParseDeclarationList()
		{
			return _rootScope.ParseDeclarationList();
		}

		public CssDeclaration ParseDeclaration()
		{
			return _rootScope.ParseDeclaration();
		}

		public IEnumerable<CssComponent> ParseComponentList()
		{
			return _rootScope.ParseComponentList();
		}

		public CssComponent ParseComponent()
		{
			return _rootScope.ParseComponent();
		}

		#endregion

		#region Inner classes

		public abstract class Scope
		{
			#region Instance fields

			private readonly CssGrammar _grammar;
			private CssToken _currentToken;

			#endregion

			#region Constructor(s)

			protected Scope(CssGrammar grammar)
			{
				_grammar = grammar;
			}

			#endregion

			#region Properties

			public CssToken CurrentToken
			{
				get { return _currentToken; }
				protected set { _currentToken = value; }
			}

			#endregion

			#region Public methods

			public CssRule ParseRule()
			{
				if (SkipWhiteSpace())
				{
					switch (CurrentToken.TokenType)
					{
						case CssTokenType.AtKeyword:
							return ConsumeAtRule();
						default:
							return ConsumeQualifiedRule();
					}
				}

				// syntax error
				return null;
			}

			public IEnumerable<CssRule> ParseRuleList(bool isTopLevel = false)
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
							var atRule = ConsumeAtRule();
							if (atRule != null) yield return atRule;
							break;
						default:
							var rule = ConsumeQualifiedRule();
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
			public IEnumerable<CssComponent> ParseDeclarationList()
			{
				while (SkipWhiteSpace())
				{
					switch (CurrentToken.TokenType)
					{
						case CssTokenType.Semicolon:
							continue;
						case CssTokenType.AtKeyword:
							var atRule = ConsumeAtRule();
							if (atRule != null) yield return atRule;
							continue;
						case CssTokenType.Identifier:
							var declaration = ConsumeDeclaration(CssTokenType.Semicolon);
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
					? ConsumeDeclaration()
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
					: null;     // Syntax error: EOF expected but other token found
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
			/// <returns></returns>
			private CssQualifiedRule ConsumeQualifiedRule()
			{
				var preludeFragmentParser = new QualifiedRulePreludeScope(this, _grammar);
				var prelude = _grammar.ParseQualifiedRulePrelude(preludeFragmentParser);

				if (CurrentToken.TokenType == CssTokenType.LeftCurlyBracket && MoveNext())
				{
					var blockFragmentParser = new RuleBlockScope(this, _grammar);
					var block = new CssBlock(CssBlockType.CurlyBrackets, _grammar.ParseQualifiedRuleBlock(blockFragmentParser));
					return _grammar.CreateQualifiedRule(prelude, block);
				}

				// Parse error
				return null;
			}

			private CssAtRule ConsumeAtRule()
			{
				var name = CurrentToken.StringValue;
				var grammar = _grammar.GetAtRuleGrammar(name);
				if (grammar == null)
				{
					// At rule not supported by grammar
					SkipAtRule();
					return null;
				}

				CssComponent prelude = null;
				CssBlock block = null;

				if (MoveNext())
				{
					var preludeScope = new AtRulePreludeScope(this, grammar);
					prelude = grammar.ParseAtRulePrelude(preludeScope);
					if (CurrentToken.TokenType == CssTokenType.LeftCurlyBracket && MoveNext())
					{
						var blockScope = new RuleBlockScope(this, grammar);
						block = new CssBlock(CssBlockType.CurlyBrackets, _grammar.ParseAtRuleBlock(blockScope));
					}
				}

				return grammar.CreateAtRule(name, prelude ?? CssComponent.Empty, block);
			}

			private void SkipAtRule()
			{
				var blockNestLevel = 0;
				while (MoveNext())
				{
					switch (this.CurrentToken.TokenType)
					{
						case CssTokenType.Semicolon:
							if (blockNestLevel <= 0)
							{
								MoveNext();
								return;
							}
							break;
						case CssTokenType.LeftCurlyBracket:
							blockNestLevel++;
							break;
						case CssTokenType.RightCurlyBracket:
							blockNestLevel--;
							if (blockNestLevel == 0) MoveNext();
							if (blockNestLevel <= 0) return;
							break;
					}
				}
			}

			private CssDeclaration ConsumeDeclaration(CssTokenType endTokenType = 0)
			{
				var name = CurrentToken.StringValue;
				if (!SkipWhiteSpace()) return null;								// Syntax error: unexpected EOF
				if (CurrentToken.TokenType != CssTokenType.Colon) return null;	// Parse error: expected colon

				var values = ImmutableArray.CreateBuilder<CssToken>(4);
				while (MoveNext() && CurrentToken.TokenType != endTokenType)
				{
					values.Add(CurrentToken);
				}

				var isImportant = TrimTrailingImportantPhrase(values);
				return _grammar.CreateDeclaration(name, values.ToImmutable(), isImportant);
			}

			/// <summary>
			/// Removes any trailing !important fragment.
			/// </summary>
			/// <param name="values"></param>
			/// <returns>Returns <c>true</c> if !important fragment was removed.</returns>
			private static bool TrimTrailingImportantPhrase(ImmutableArray<CssToken>.Builder values)
			{
				var m = values.Count - 1;
				int i;

				// Skip trailing whitespace
				for (i = m; i > 0 && values[i].IsWhitespace; i--) { }

				// Test for identifier "important" 
				if (i >= 1 && values[i].HasValue("important"))
				{
					// Skip whitespace before the "important" identifier 
					for (i = i - 1; i >= 0 && values[i].IsWhitespace; i--) { }
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

			private CssComponent ConsumeComponent()
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
						return CurrentToken;
				}
			}

			private CssBlock ConsumeSimpleBlock(CssBlockType blockType)
			{
				var components = ImmutableList.CreateBuilder<CssComponent>();
				while (MoveNext() && CurrentToken.TokenType != blockType.EndTokenType)
				{
					components.Add(ConsumeComponent());
				}
				return new CssBlock(blockType, components.ToImmutable());
			}

			private CssFunction ConsumeFunction()
			{
				var name = CurrentToken.StringValue;

				var components = ImmutableArray.CreateBuilder<CssComponent>();
				while (MoveNext() && CurrentToken.TokenType != CssTokenType.RightParenthesis)
				{
					components.Add(ConsumeComponent());
				}

				return _grammar.CreateFunction(name, components.ToImmutable());
			}

			#endregion

			#region Utility methods

			public abstract bool MoveNext();

			private bool SkipWhiteSpace()
			{
				while (MoveNext())
				{
					if (!CurrentToken.IsWhitespace) return true;
				}
				return false;
			}

			private bool SkipUntil(CssTokenType tokenType)
			{
				while (MoveNext())
				{
					if (CurrentToken.TokenType == tokenType) return true;
				}
				return false;
			}

			#endregion
		}

		private class RootScope : Scope
		{
			private readonly IEnumerator<CssToken> _tokenizer;

			public RootScope(IEnumerator<CssToken> tokenizer, CssGrammar grammar)
				: base(grammar)
			{
				ArgChecker.AssertArgNotNull(tokenizer, nameof(tokenizer));
				_tokenizer = tokenizer;
			}

			public override bool MoveNext()
			{
				if (_tokenizer.MoveNext())
				{
					this.CurrentToken = _tokenizer.Current;
					return true;
				}
				return false;
			}
		}

		private abstract class ChildScope : Scope
		{
			private readonly Scope _parent;
			private int _state = 0;

			protected ChildScope(Scope parent, CssGrammar grammar)
				: base(grammar)
			{
				ArgChecker.AssertArgNotNull(parent, nameof(parent));
				_parent = parent;
			}

			public override bool MoveNext()
			{
				CssToken nextToken;
				switch (_state)
				{
					case 0:
						nextToken = _parent.CurrentToken;
						break;
					case 1:
						nextToken = _parent.MoveNext()
							? _parent.CurrentToken
							: null;
						break;
					default:
						return false;
				}

				if (nextToken == null || IsEndToken(nextToken.TokenType))
				{
					_state = 2;
					this.CurrentToken = null;
					return false;
				}

				_state = 1;
				this.CurrentToken = nextToken;
				return true;
			}

			protected abstract bool IsEndToken(CssTokenType tokenType);
		}

		private class QualifiedRulePreludeScope : ChildScope
		{
			public QualifiedRulePreludeScope(Scope parent, CssGrammar grammar) 
				: base(parent, grammar)
			{}

			protected override bool IsEndToken(CssTokenType tokenType)
			{
				return tokenType == CssTokenType.LeftCurlyBracket;
			}
		}

		private class AtRulePreludeScope : ChildScope
		{
			public AtRulePreludeScope(Scope parent, CssGrammar grammar)
				: base(parent, grammar)
			{ }

			protected override bool IsEndToken(CssTokenType tokenType)
			{
				return tokenType == CssTokenType.LeftCurlyBracket
					|| tokenType == CssTokenType.Semicolon;
			}
		}

		private class RuleBlockScope : ChildScope
		{
			private int _nestLevel = 1;

			public RuleBlockScope(Scope parent, CssGrammar grammar)
				: base(parent, grammar)
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
