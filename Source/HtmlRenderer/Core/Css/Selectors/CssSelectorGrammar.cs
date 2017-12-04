namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Pidgin;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using static Pidgin.Parser;
	using static Pidgin.Parser<Parsing.CssToken>;

	public class CssSyntaxGrammar
	{
		public static readonly Parser<CssToken, CssToken> IDENTIFIER = Token(t => t.IsIdentifier);
		public static readonly Parser<CssToken, CssToken> FUNCTION = Token(t => t.TokenType == CssTokenType.Function);
		public static readonly Parser<CssToken, CssToken> HASH = Token(t => t.TokenType == CssTokenType.Hash);
		public static readonly Parser<CssToken, CssToken> STRING = Token(t => t.IsQuotedString);
		public static readonly Parser<CssToken, CssToken> WHITESPACE = Token(t => t.IsWhitespace).Labelled("whitespace");

		public static readonly Parser<CssToken, Unit> COLON = Token(t => t.TokenType == CssTokenType.Colon).IgnoreResult();
		public static readonly Parser<CssToken, Unit> COMMA = Token(t => t.TokenType == CssTokenType.Comma).IgnoreResult();
		public static readonly Parser<CssToken, Unit> SEMICOLON = Token(t => t.TokenType == CssTokenType.Semicolon).IgnoreResult();
		public static readonly Parser<CssToken, Unit> LPAREN = Token(t => t.TokenType == CssTokenType.LeftParenthesis).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RPAREN = Token(t => t.TokenType == CssTokenType.RightParenthesis).IgnoreResult().Or(Try(End()));
		public static readonly Parser<CssToken, Unit> LSQUARE = Token(t => t.TokenType == CssTokenType.LeftSquareBracket).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RSQUARE = Token(t => t.TokenType == CssTokenType.RightSquareBracket).IgnoreResult().Or(Try(End()));
		public static readonly Parser<CssToken, Unit> LCURLY = Token(t => t.TokenType == CssTokenType.LeftCurlyBracket).IgnoreResult();
		public static readonly Parser<CssToken, Unit> RCURLY = Token(t => t.TokenType == CssTokenType.RightCurlyBracket).IgnoreResult().Or(Try(End()));

		public static Parser<CssToken, CssToken> Delimiter(char ch)
		{
			return Token(t => t.IsDelimiter(ch));
		}

		public static Parser<CssToken, CssToken> Identifier(string value = null)
		{
			return Token(t => t.IsIdentifier && (value == null || value.Equals(t.StringValue, StringComparison.OrdinalIgnoreCase)));
		}

		public static Parser<CssToken, CssToken> Function(string value = null)
		{
			return Token(t => t.IsFunction && (value == null || value.Equals(t.StringValue, StringComparison.OrdinalIgnoreCase)));
		}

		public static Parser<CssToken, CssToken> String(string value = null, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			return Token(t => t.IsQuotedString && (value == null || value.Equals(t.StringValue, comparison)));
		}

		public static readonly Parser<CssToken, Unit> SkipWhitespace = WHITESPACE.SkipMany().Labelled("whitespace");
	}

	public class CssSelectorGrammar : CssSyntaxGrammar
	{
		private static readonly Parser<CssToken, string> NamespacePrefix = Delimiter('*').Or(IDENTIFIER).Optional().Before(Delimiter('|'))
			.Select(t => t.HasValue ? t.Value.StringValue : "");

		#region Type selectors (includes universal selector)

		private static readonly Parser<CssToken, QualifiedName> ElementName = NamespacePrefix.Optional()
			.Then(Delimiter('*').Or(IDENTIFIER).Select(t => t.StringValue), 
				(prefix, name) => new QualifiedName(name, prefix.HasValue ? prefix.Value : null));

		public static readonly Parser<CssToken, CssTypeSelector> TypeSelector = ElementName
			.Select(n => n.NamespacePrefix != null 
				? CssSelector.WithElement(n.LocalName, n.NamespacePrefix) 
				: CssSelector.WithElement(n.LocalName));

		#endregion

		#region Attribute selectors

		private static readonly Parser<CssToken, QualifiedName> AttributeName = NamespacePrefix.Optional()
			.Then(IDENTIFIER.Select(t => t.StringValue),
				(prefix, name) => new QualifiedName(name, prefix.HasValue ? prefix.Value : null));
		private  static readonly Parser<CssToken, CssAttributeMatchOperator> AttributeMatchOperator = Delimiter('=').Select(t => CssAttributeMatchOperator.Any)
			.Or(Token(t => t.IsMatchOperator).Select(t => (CssAttributeMatchOperator) ((int) t.TokenType & 0xFF)));
		private static readonly Parser<CssToken, AttributeMatchExpression> AttributeMatch = AttributeMatchOperator.Before(SkipWhitespace)
			.Then(IDENTIFIER.Or(STRING), (matchOperator, matchOperand) => new AttributeMatchExpression(matchOperator, (CssStringToken)matchOperand));

		public static readonly Parser<CssToken, CssSimpleSelector> AttributeSelector = Delimiter('[')
			.Then(SkipWhitespace)
			.Then(AttributeName)
			.Then(AttributeMatch.Optional(), (name, match) => match.HasValue
				? (CssSimpleSelector)CssSelector.WithAttribute(name.LocalName, name.NamespacePrefix, match.Value.Operator, match.Value.Operand)
				: (CssSimpleSelector)CssSelector.WithAttribute(name.LocalName, name.NamespacePrefix))
			.Before(SkipWhitespace)
			.Before(Delimiter(']'));

		#endregion

		#region Class and id selectors

		public static readonly Parser<CssToken, CssSimpleSelector> ClassSelector = Delimiter('.').Then(IDENTIFIER)
			.Select(t => (CssSimpleSelector)CssSelector.WithClass(t.StringValue));

		public static readonly Parser<CssToken, CssSimpleSelector> IdSelector = HASH
			.Select(t => (CssSimpleSelector)CssSelector.WithId(t.StringValue));

		#endregion

		#region Pseudo-class selectors

		// an+b pattern parsing
		private static readonly Parser<char, CycleAndOffset> CycleSizeAndOffsetString = OneOf(
			Try(Int(10).Optional().Before(CIChar('n'))).Then(Int(10), (cycle, offset) => new CycleAndOffset(cycle.GetValueOrDefault(1), offset)),
			Int(10).Select(num => new CycleAndOffset(0, num)),
			CIString("odd").Select(id => new CycleAndOffset(2, 1)),
			CIString("event").Select(id => new CycleAndOffset(2, 0)));
		private static readonly Parser<CssToken, CycleAndOffset> CycleSizeAndOffset =
			Token(t => t.IsDimension || t.IsNumber || t.IsIdentifier || t.IsWhitespace).AtLeastOnceUntil(RPAREN)
				.Select(tokens => tokens.Aggregate(new StringBuilder(4), (sb, t) => { if (!t.IsWhitespace) t.ToString(sb); return sb; }, sb => sb.ToString()))
				.Select(text => CycleSizeAndOffsetString.Parse(text).GetValueOrDefault());

		private struct CycleAndOffset
		{
			public CycleAndOffset(int cycleSize, int offset)
			{
				this.CycleSize = cycleSize;
				this.Offset = offset;
			}

			public int CycleSize { get; }
			public int Offset { get; }
		}

		private static readonly Parser<CssToken, CssSimpleSelector> PseudoClass = IDENTIFIER
			.Where(t => !CssPseudoElement.AllowSingleColonPrefix(t.StringValue))
			.Select(t => (CssSimpleSelector)CssSelector.WithPseudoClass(t.StringValue));

		private static readonly Parser<CssToken, CssSimpleSelector> PseudoClassFunction = FUNCTION
			.Before(SkipWhitespace)
			.Then(OneOf(
				Function("not").Bind(t => Rec(() => SimpleSelector), (tok, sel) => sel.Negate()),
				Function("nth-child").Bind(t => CycleSizeAndOffset, (tok, cao) => CssSelector.NthChild(cao.CycleSize, cao.Offset)),
				Function("nth-last-child").Bind(t => CycleSizeAndOffset, (tok, cao) => CssSelector.NthLastChild(cao.CycleSize, cao.Offset)),
				Function("nth-of-type").Bind(t => CycleSizeAndOffset, (tok, cao) => CssSelector.NthOfType(cao.CycleSize, cao.Offset)),
				Function("nth-last-of-type").Bind(t => CycleSizeAndOffset, (tok, cao) => CssSelector.NthLastOfType(cao.CycleSize, cao.Offset))
			))
			.Before(SkipWhitespace)
			.Before(RPAREN);

		public static readonly Parser<CssToken, CssSimpleSelector> PseudoSelector =
			COLON.Then(PseudoClass.Or(PseudoClassFunction));

		#endregion

		#region Pseudo-element

		public static readonly Parser<CssToken, CssPseudoElement> PseudoElement = COLON
			.Then(OneOf(
				COLON.Then(IDENTIFIER),
				IDENTIFIER.Where(id => CssPseudoElement.AllowSingleColonPrefix(id.StringValue))
			))
			.Select(id => new CssPseudoElement(id.StringValue));

		#endregion

		#region Simple selectors and simple selector sequences 

		private static readonly Parser<CssToken, CssSimpleSelector> OtherSimpleSelector = 
			OneOf(IdSelector, ClassSelector, AttributeSelector, PseudoSelector);

		public static readonly Parser<CssToken, CssSimpleSelector> SimpleSelector = 
			TypeSelector.Cast<CssSimpleSelector>().Or(OtherSimpleSelector);

		public static readonly Parser<CssToken, ICssSelectorSequence> SimpleSelectorSequence = 
			OneOf(
				AggregateMany(TypeSelector.Cast<ICssSelectorSequence>(), OtherSimpleSelector, (seq, sel) => seq.Add(sel)),
				AggregateAtLeastOnce(Return((ICssSelectorSequence)CssSelector.ImpliedUniversal), OtherSimpleSelector, (seq, sel) => seq.Add(sel))
			)
			.Then(PseudoElement.Optional(), (seq, e) => e.HasValue ? seq.Add(e.Value) : seq);

		#endregion

		#region Selectors and selector groups

		private static readonly Parser<CssToken, CssCombinator> CombinatorDelimiter =
			Token(t => t.IsDelimiter('+') || t.IsDelimiter('>') || t.IsDelimiter('~')).Select(t => (CssCombinator) ((int) t.TokenType & 0xFF));
		private static readonly Parser<CssToken, CssCombinator> Combinator = CombinatorDelimiter
			.Or(WHITESPACE.AtLeastOnce().Then(CombinatorDelimiter).Or(Return(CssCombinator.Descendant)));
		
		public static readonly Parser<CssToken, ICssSelectorChain> Selector = AggregateMany(
			SimpleSelectorSequence.Cast<ICssSelectorChain>(),
			Combinator.Before(SkipWhitespace).Then(SimpleSelectorSequence, (combinator, seq) => new CombinationTerm(combinator, seq)),
			(chain, term) => chain.Combine(term.Combinator, term.RightOperand));

		public static readonly Parser<CssToken, IEnumerable<ICssSelectorChain>> SelectorGroup =
			Selector.Separated(SkipWhitespace.Then(COMMA).Before(SkipWhitespace));

		private struct CombinationTerm
		{
			public CombinationTerm(CssCombinator combinator, ICssSelectorSequence rightOperand)
			{
				this.Combinator = combinator;
				this.RightOperand = rightOperand;
			}

			public CssCombinator Combinator { get; }
			public ICssSelectorSequence RightOperand { get; }

		}

		#endregion

		#region Utility methods

		/// <summary>
		/// Returns a parser that aggregates one or more matching tokens into a single result.
		/// </summary>
		/// <typeparam name="TToken">The token type.</typeparam>
		/// <typeparam name="T">The aggregation result type.</typeparam>
		/// <typeparam name="U">The matching item type.</typeparam>
		/// <param name="seed">Initial result value.</param>
		/// <param name="match">Parser that matches items to be aggregated.</param>
		/// <param name="func">The aggregation function.</param>
		/// <returns></returns>
		private static Parser<TToken, T> AggregateAtLeastOnce<TToken, T, U>(Parser<TToken, T> seed, Parser<TToken,U> match, Func<T,U,T> func)
		{
			return seed.Then(match.AtLeastOnce(), (s, items) => items.Aggregate(s, func));
		}

		/// <summary>
		/// Returns a parser that aggregates zero or more matching tokens into a single result.
		/// </summary>
		/// <typeparam name="TToken">The token type.</typeparam>
		/// <typeparam name="T">The aggregation result type.</typeparam>
		/// <typeparam name="U">The matching item type.</typeparam>
		/// <param name="seed">Initial result value.</param>
		/// <param name="match">Parser that matches items to be aggregated.</param>
		/// <param name="func">The aggregation function.</param>
		/// <returns></returns>
		private static Parser<TToken, T> AggregateMany<TToken, T, U>(Parser<TToken, T> seed, Parser<TToken, U> match, Func<T,U,T> func)
		{
			return seed.Then(match.Many(), (s, items) => items.Aggregate(s, func));
		}

		#endregion

		private struct QualifiedName
		{
			public QualifiedName(string localName, string namespacePrefix)
			{
				this.LocalName = localName;
				this.NamespacePrefix = namespacePrefix;
			}

			public string LocalName { get; }
			public string NamespacePrefix { get; }
		}

		private struct AttributeMatchExpression
		{
			public AttributeMatchExpression(CssAttributeMatchOperator @operator, CssStringToken operand)
			{
				this.Operator = @operator;
				this.Operand = operand;
			}

			public CssAttributeMatchOperator Operator { get; }
			public CssStringToken Operand { get; }
		}
	}
}
