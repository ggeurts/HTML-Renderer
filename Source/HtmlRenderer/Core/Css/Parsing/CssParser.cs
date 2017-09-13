namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssParser
	{
		#region Static fields

		private static readonly CssGrammar StylesheetGrammar = new CssGrammar();

		#endregion

		#region Instance fields

		private readonly IEnumerator<CssToken> _tokenizer;
		private readonly CssGrammar _grammar;

		/// <summary>
		/// A map of tokens to components to reduce CssSimpleComponent memory allocations
		/// </summary>
		private readonly Dictionary<CssToken, CssSimpleComponent> _componentTable = 
			new Dictionary<CssToken, CssSimpleComponent>(CssToken.TokenValueComparer);

		#endregion

		#region Constructor(s)

		public CssParser(IEnumerable<CssToken> tokens)
			: this(null, tokens)
		{}

		public CssParser(CssGrammar grammar, IEnumerable<CssToken> tokens)
		{
			ArgChecker.AssertArgNotNull(tokens, nameof(tokens));
			_grammar = grammar ?? StylesheetGrammar;
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
				switch (_tokenizer.Current.TokenType)
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

		private IEnumerable<CssRule> ParseRuleList(bool isTopLevel)
		{
			while (SkipWhiteSpace())
			{
				switch (_tokenizer.Current.TokenType)
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

		public IEnumerable<ICssDeclaration> ParseDeclarationList()
		{
			while (SkipWhiteSpace())
			{
				switch (_tokenizer.Current.TokenType)
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
			return ConsumeDeclaration();
		}

		public IEnumerable<CssComponent> ParseComponentList()
		{
			while (SkipWhiteSpace())
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

		public CssQualifiedRule ConsumeQualifiedRule()
		{
			var prelude = ImmutableArray.CreateBuilder<CssComponent>();

			do
			{
				if (_tokenizer.Current.TokenType == CssTokenType.LeftCurlyBracket)
				{
					var block = ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
					return _grammar.CreateQualifiedRule(prelude.ToImmutable(), block);
				}

				prelude.Add(ConsumeSimpleComponent());
			} while (_tokenizer.MoveNext());

			// Parse error
			return null;
		}

		public CssAtRule ConsumeAtRule()
		{
			var prelude = ImmutableArray.CreateBuilder<CssComponent>();

			do
			{
				switch (_tokenizer.Current.TokenType)
				{
					case CssTokenType.Semicolon:
						return _grammar.CreateAtRule(prelude.ToImmutable(), null);
					case CssTokenType.LeftCurlyBracket:
						var block = ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
						return _grammar.CreateAtRule(prelude.ToImmutable(), block);
				}

				prelude.Add(ConsumeSimpleComponent());
			} while (_tokenizer.MoveNext());

			return _grammar.CreateAtRule(prelude.ToImmutable(), null);
		}

		public CssDeclaration ConsumeDeclaration()
		{
			return ConsumeDeclaration(0);
		}

		public CssDeclaration ConsumeDeclaration(CssTokenType delimiter)
		{
			var name = _tokenizer.Current.StringValue;
			if (!SkipWhiteSpace()) return null;										// Syntax error: unexpected EOF
			if (_tokenizer.Current.TokenType != CssTokenType.Colon) return null;     // Parse error: expected colon

			var values = ImmutableArray.CreateBuilder<CssSimpleComponent>(4);
			while (_tokenizer.MoveNext() && _tokenizer.Current.TokenType != delimiter)
			{
				values.Add(ConsumeSimpleComponent());
			}

			var isImportant = TrimImportantKeyword(values);
			return _grammar.CreateDeclaration(name, values.ToImmutable(), isImportant);
		}

		private bool TrimImportantKeyword(ImmutableArray<CssSimpleComponent>.Builder values)
		{
			var i = values.Count - 1;
			while (i > 0 && values[i].IsWhitespace) i--;

			if (i >= 1
			    && values[i].HasValue("important", StringComparison.OrdinalIgnoreCase)
			    && values[i - 1].HasValue('!'))
			{
				values.RemoveAt(i);
				values.RemoveAt(i - 1);
				return true;
			}
			return false;
		}

		public CssComponent ConsumeComponent()
		{
			switch (_tokenizer.Current.TokenType)
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
					return ConsumeSimpleComponent();
			}
		}

		protected CssBlock ConsumeSimpleBlock(CssBlockType blockType)
		{
			var components = ImmutableList.CreateBuilder<CssComponent>();
			while (_tokenizer.MoveNext() && _tokenizer.Current.TokenType != blockType.EndTokenType)
			{
				components.Add(ConsumeComponent());
			}
			return new CssBlock(blockType, components.ToImmutable());
		}

		protected CssFunction ConsumeFunction()
		{
			var name = _tokenizer.Current.StringValue;

			var components = ImmutableArray.CreateBuilder<CssComponent>();
			while (_tokenizer.MoveNext() && _tokenizer.Current.TokenType != CssTokenType.RightParenthesis)
			{
				components.Add(ConsumeComponent());
			}

			return _grammar.CreateFunction(name, components.ToImmutable());
		}

		protected CssSimpleComponent ConsumeSimpleComponent()
		{
			CssSimpleComponent result;
			if (!_componentTable.TryGetValue(_tokenizer.Current, out result))
			{
				result = _tokenizer.Current.CreateComponent();
				_componentTable.Add(_tokenizer.Current, result);
			}
			return result;
		}

		#endregion

		#region Utility methods

		protected bool SkipWhiteSpace()
		{
			while (_tokenizer.MoveNext())
			{
				if (!_tokenizer.Current.IsWhitespace) return true;
			}
			return false;
		}

		protected bool SkipUntil(CssTokenType tokenType)
		{
			while (_tokenizer.MoveNext())
			{
				if (_tokenizer.Current.TokenType == tokenType) return true;
			}
			return false;
		}

		#endregion
	}
}
