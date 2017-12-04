namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Globalization;
	using System.Xml;
	using System.Xml.Linq;
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

		private readonly CssTokenizer _tokenizer;
		private readonly RootScope _rootScope;
		private readonly ParseErrorNotifier _parseErrorNotifier;
		private readonly XmlNamespaceManager _namespaceManager;

		#endregion

		#region Events

		public event EventHandler<CssErrorEventArgs> ParseError
		{
			add { _parseErrorNotifier.ParseError += value; }
			remove { _parseErrorNotifier.ParseError -= value; }
		}

		#endregion

		#region Constructor(s)

		public CssParser(CssTokenizer tokenizer)
			: this(tokenizer, null)
		{
		}

		public CssParser(CssTokenizer tokenizer, CssGrammar grammar)
		{
			ArgChecker.AssertArgNotNull(tokenizer, nameof(tokenizer));
			_tokenizer = tokenizer;
			_rootScope = new RootScope(this, tokenizer.Tokenize().GetEnumerator(), grammar ?? DefaultGrammar);
			_namespaceManager = new XmlNamespaceManager(new NameTable());

			var parseErrorNotifier = new ParseErrorNotifier(this);
			_parseErrorNotifier = parseErrorNotifier;
			_tokenizer.ParseError += (sender, e) => parseErrorNotifier.NotifyParseError(e);
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

		#region Namespace operations

		private bool HasDefaultNamespace
		{
			get { return !string.IsNullOrEmpty(_namespaceManager.DefaultNamespace); }
		}

		private XNamespace LookupNamespace(string prefix)
		{
			var ns = _namespaceManager.LookupNamespace(prefix);
			return ns != null 
				? XNamespace.Get(ns) 
				: null;
		}

		#endregion

		#region Error handling

		private void NotifyParseError(string message)
		{
			_parseErrorNotifier.NotifyParseError(new CssErrorEventArgs(_tokenizer.Line, _tokenizer.Column, message));
		}

		#endregion

		#region Inner classes

		public abstract class Scope
		{
			#region Instance fields

			private readonly CssGrammar _grammar;

			#endregion

			#region Constructor(s)

			protected Scope(CssGrammar grammar)
			{
				_grammar = grammar;
			}

			#endregion

			#region Properties

			public abstract CssParser Parser { get; }

			public bool HasDefaultNamespace
			{
				get { return this.Parser.HasDefaultNamespace; }
			}

			#endregion

			#region Public methods

			public XNamespace LookupNamespace(string prefix)
			{
				return this.Parser.LookupNamespace(prefix);
			}

			public void NotifyParseError(string message)
			{
				this.Parser.NotifyParseError(message);
			}

			public CssRule ParseRule()
			{
				CssToken token;
				if (SkipWhiteSpace(out token))
				{
					switch (token.TokenType)
					{
						case CssTokenType.AtKeyword:
							return ConsumeAtRule(token);
						default:
							return ConsumeQualifiedRule(token);
					}
				}

				// syntax error
				return null;
			}

			public IEnumerable<CssRule> ParseRuleList(bool isTopLevel = false)
			{
				CssToken token;
				while (SkipWhiteSpace(out token))
				{
					switch (token.TokenType)
					{
						case CssTokenType.CDO:
						case CssTokenType.CDC:
							if (isTopLevel) continue;
							goto default;
						case CssTokenType.AtKeyword:
							var atRule = ConsumeAtRule(token);
							if (atRule != null) yield return atRule;
							break;
						default:
							var rule = ConsumeQualifiedRule(token);
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
				CssToken token;
				while (SkipWhiteSpace(out token))
				{
					switch (token.TokenType)
					{
						case CssTokenType.Semicolon:
							break;
						case CssTokenType.AtKeyword:
							var atRule = ConsumeAtRule(token);
							if (atRule != null) yield return atRule;
							break;
						case CssTokenType.Identifier:
							var declaration = ConsumeDeclaration(token, CssTokenType.Semicolon);
							if (declaration != null) yield return declaration;
							break;
						default:
							NotifyParseError(string.Format(
								CultureInfo.InvariantCulture,
								"Parse error: expected {0}, {1} or {2} token", 
								CssTokenType.Semicolon, CssTokenType.AtKeyword, CssTokenType.Identifier));
							if (!SkipUntil(CssTokenType.Semicolon, out token)) yield break;
							break;
					}

				}
			}

			public CssDeclaration ParseDeclaration()
			{
				CssToken token;
				return SkipWhiteSpace(out token) && token.IsIdentifier
					? ConsumeDeclaration(token)
					: null;
			}

			public IEnumerable<CssComponent> ParseComponentList()
			{
				CssToken token;
				while (TryRead(out token))
				{
					yield return ConsumeComponent(token);
				}
			}

			public CssComponent ParseComponent()
			{
				CssToken token;
				if (!SkipWhiteSpace(out token))
				{
					NotifyParseError("Syntax error: premature EOF.");
					return null; // Syntax error: premature EOF
				}

				var component = ConsumeComponent(token);
				if (component != null && !SkipWhiteSpace(out token)) return component;

				NotifyParseError(string.Format(CultureInfo.InvariantCulture, "Syntax error: expected EOF but '{0}' token found", token.TokenType));
				return null;
			}

			#endregion

			#region Token operations

			public abstract CssToken Peek(int lookaheadIndex = 0);

			public abstract bool TryRead(out CssToken result);

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
			private CssQualifiedRule ConsumeQualifiedRule(CssToken token)
			{
				var preludeFragmentParser = new QualifiedRulePreludeScope(this, _grammar);
				var prelude = _grammar.ParseQualifiedRulePrelude(preludeFragmentParser);

				if (token.TokenType == CssTokenType.LeftCurlyBracket && TryRead(out token))
				{
					var blockFragmentParser = new RuleBlockScope(this, _grammar);
					var block = new CssBlock(CssBlockType.CurlyBrackets, _grammar.ParseQualifiedRuleBlock(blockFragmentParser));
					return _grammar.CreateQualifiedRule(prelude, block);
				}

				// Parse error
				return null;
			}

			private CssAtRule ConsumeAtRule(CssToken token)
			{
				var name = token.StringValue;
				var grammar = _grammar.GetAtRuleGrammar(name);
				if (grammar == null)
				{
					// At rule not supported by grammar
					SkipAtRule();
					return null;
				}

				CssComponent prelude = null;
				CssBlock block = null;

				if (TryRead(out token))
				{
					var preludeScope = new AtRulePreludeScope(this, grammar);
					prelude = grammar.ParseAtRulePrelude(preludeScope);
					if (token.TokenType == CssTokenType.LeftCurlyBracket)
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

				CssToken token;
				while (TryRead(out token))
				{
					switch (token.TokenType)
					{
						case CssTokenType.Semicolon:
							if (blockNestLevel <= 0) return;
							break;
						case CssTokenType.LeftCurlyBracket:
							blockNestLevel++;
							break;
						case CssTokenType.RightCurlyBracket:
							blockNestLevel--;
							if (blockNestLevel < 0) return;
							break;
					}
				}
			}

			private CssDeclaration ConsumeDeclaration(CssToken token, CssTokenType endTokenType = 0)
			{
				var name = token.StringValue;

				if (!SkipWhiteSpace(out token))
				{
					NotifyParseError("Syntax error: unexpected EOF");
					return null;
				}
				if (token.TokenType != CssTokenType.Colon)
				{
					NotifyParseError("Parse error: expected ':' token");
					return null;
				}

				var values = ImmutableArray.CreateBuilder<CssToken>(4);
				while (TryRead(out token) && token.TokenType != endTokenType)
				{
					values.Add(token);
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

			private CssComponent ConsumeComponent(CssToken token)
			{
				switch (token.TokenType)
				{
					case CssTokenType.LeftParenthesis:
						return ConsumeSimpleBlock(CssBlockType.Parentheses);
					case CssTokenType.LeftCurlyBracket:
						return ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
					case CssTokenType.LeftSquareBracket:
						return ConsumeSimpleBlock(CssBlockType.SquareBrackets);
					case CssTokenType.Function:
						return ConsumeFunction(token);
					default:
						return token;
				}
			}

			private CssBlock ConsumeSimpleBlock(CssBlockType blockType)
			{
				var components = ImmutableList.CreateBuilder<CssComponent>();

				CssToken token;
				while (TryRead(out token) && token.TokenType != blockType.EndTokenType)
				{
					components.Add(ConsumeComponent(token));
				}
				return new CssBlock(blockType, components.ToImmutable());
			}

			private CssFunction ConsumeFunction(CssToken token)
			{
				var name = token.StringValue;

				var components = ImmutableArray.CreateBuilder<CssComponent>();
				while (TryRead(out token) && token.TokenType != CssTokenType.RightParenthesis)
				{
					components.Add(ConsumeComponent(token));
				}

				return _grammar.CreateFunction(name, components.ToImmutable());
			}

			#endregion

			#region Utility methods

			public bool SkipWhiteSpace(out CssToken token)
			{
				while (TryRead(out token))
				{
					if (!token.IsWhitespace) return true;
				}

				token = null;
				return false;
			}

			public bool SkipUntil(CssTokenType tokenType, out CssToken token)
			{
				while (TryRead(out token))
				{
					if (token.TokenType == tokenType) return true;
				}
				return false;
			}

			#endregion
		}

		private class RootScope : Scope
		{
			private readonly CssParser _parser;
			private readonly IEnumerator<CssToken> _tokenizer;
			private CssToken[] _cyclicBuffer = new CssToken[4];
			private int _cyclicBufferPos;
			private int _cyclicBufferLen;

			public RootScope(CssParser parser, IEnumerator<CssToken> tokenizer, CssGrammar grammar)
				: base(grammar)
			{
				ArgChecker.AssertArgNotNull(parser, nameof(parser));
				ArgChecker.AssertArgNotNull(tokenizer, nameof(tokenizer));
				_parser = parser;
				_tokenizer = tokenizer;
			}

			public override CssParser Parser
			{
				get { return _parser; }
			}

			public override CssToken Peek(int lookAheadIndex)
			{
				return EnsureBufferLength(lookAheadIndex + 1)
					? _cyclicBuffer[(_cyclicBufferPos + lookAheadIndex) % _cyclicBuffer.Length]
					: CssToken.Empty;
			}

			public override bool TryRead(out CssToken result)
			{
				if (!EnsureBufferLength(1))
				{
					result = null;
					return false;
				}

				result = _cyclicBuffer[_cyclicBufferPos];
				_cyclicBufferPos = (_cyclicBufferPos + 1) % _cyclicBuffer.Length;
				_cyclicBufferLen--;
				return true;
			}

			private bool EnsureBufferLength(int minLength)
			{
				if (_cyclicBuffer == null) return false;
				if (minLength > _cyclicBuffer.Length)
				{
					throw new ArgumentOutOfRangeException(nameof(minLength), minLength, string.Format(CultureInfo.InvariantCulture,
						"Length cannot be greater than read-ahead buffer capacity of {0} characters.", _cyclicBuffer.Length));
				}

				if (_cyclicBufferLen >= minLength) return true;

				var writePos = _cyclicBufferPos + _cyclicBufferLen % _cyclicBuffer.Length;
				var writeCount = minLength - _cyclicBufferLen;
				for (var i = 0; i < writeCount; i++)
				{
					if (!_tokenizer.MoveNext())
					{
						if (_cyclicBufferLen == 0) _cyclicBuffer = null;
						return false;
					}

					_cyclicBuffer[writePos] = _tokenizer.Current;
					_cyclicBufferLen++;
					writePos = (writePos + 1) % _cyclicBuffer.Length;
				}

				return true;
			}
		}

		private abstract class ChildScope : Scope
		{
			private readonly Scope _parent;
			private bool _isEof;
			private int _safeLookaheadIndex = -1;

			protected ChildScope(Scope parent, CssGrammar grammar)
				: base(grammar)
			{
				ArgChecker.AssertArgNotNull(parent, nameof(parent));
				_parent = parent;
			}

			public override CssParser Parser
			{
				get { return _parent.Parser; }
			}

			public override bool TryRead(out CssToken result)
			{
				if (_isEof)
				{
					result = null;
					return false;
				}
				if (!_parent.TryRead(out result) || IsEndToken(result.TokenType))
				{
					_isEof = true;
					return false;
				}

				if (_safeLookaheadIndex >= 0) _safeLookaheadIndex--;
				return true;
			}

			public override CssToken Peek(int lookaheadIndex = 0)
			{
				if (_isEof) return CssToken.Empty;
				if (lookaheadIndex <= _safeLookaheadIndex) return _parent.Peek(lookaheadIndex);

				CssToken token;
				do
				{
					token = _parent.Peek(_safeLookaheadIndex + 1);
					if (IsEndToken(token.TokenType)) return CssToken.Empty;
					_safeLookaheadIndex++;

				} while (_safeLookaheadIndex < lookaheadIndex);
				return token;
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

		private class ParseErrorNotifier
		{
			private readonly WeakReference _parser;

			public event EventHandler<CssErrorEventArgs> ParseError = delegate {};

			public ParseErrorNotifier(CssParser parser)
			{
				_parser = new WeakReference(parser);
			}

			public void NotifyParseError(CssErrorEventArgs e)
			{
				ParseError(_parser.Target, e);
			}
		}

		#endregion
	}
}
