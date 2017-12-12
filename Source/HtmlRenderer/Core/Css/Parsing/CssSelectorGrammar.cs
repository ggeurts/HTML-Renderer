namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Pidgin;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;
	using static Pidgin.Parser;

	public class CssSelectorGrammar : CssSyntaxGrammar
	{
		public static readonly Parser<CssToken, string> NamespacePrefix = 
			OneOf(
				Delimiter('*'), 
				IDENTIFIER
			)
			.Optional()
			.Before(Delimiter('|'))
			.Select(t => t.HasValue ? t.Value.ToString() : "");

		#region Type selectors (includes universal selector)

		public static readonly Parser<CssToken, QualifiedName> ElementName = Try(NamespacePrefix).Optional()
			.Then(Delimiter('*').Or(IDENTIFIER).Select(t => t.ToString()), 
				(prefix, name) => new QualifiedName(name, prefix.HasValue ? prefix.Value : null));

		public static readonly Parser<CssToken, CssTypeSelector> TypeSelector = ElementName
			.Select(n => n.NamespacePrefix != null 
				? CssSelector.WithElement(n.LocalName, n.NamespacePrefix) 
				: CssSelector.WithElement(n.LocalName));

		#endregion

		#region Attribute selectors

		private static readonly Parser<CssToken, QualifiedName> AttributeName = Try(NamespacePrefix).Optional()
			.Then(IDENTIFIER.Select(t => t.ToString()),
				(prefix, name) => new QualifiedName(name, prefix.HasValue ? prefix.Value : null));
		private  static readonly Parser<CssToken, CssAttributeMatchOperator> AttributeMatchOperator = Delimiter('=').ThenReturn(CssAttributeMatchOperator.Exact)
			.Or(Parser<CssToken>.Token(t => t.IsMatchOperator).Select(t => (CssAttributeMatchOperator) ((int) t.TokenType & 0xFF)));
		private static readonly Parser<CssToken, AttributeMatchExpression> AttributeMatch = AttributeMatchOperator.Before(SkipWhitespace)
			.Then(IDENTIFIER.Or(STRING), (matchOperator, matchOperand) => new AttributeMatchExpression(matchOperator, (CssStringToken)matchOperand));

		public static readonly Parser<CssToken, CssSimpleSelector> AttributeSelector = LSQUARE
			.Then(SkipWhitespace)
			.Then(AttributeName)
			.Then(AttributeMatch.Optional(), (name, match) => match.HasValue
				? (CssSimpleSelector)CssSelector.WithAttribute(name.LocalName, name.NamespacePrefix, match.Value.Operator, match.Value.Operand)
				: (CssSimpleSelector)CssSelector.WithAttribute(name.LocalName, name.NamespacePrefix))
			.Before(SkipWhitespace)
			.Before(RSQUARE);

		#endregion

		#region Class and id selectors

		public static readonly Parser<CssToken, CssSimpleSelector> ClassSelector = Delimiter('.').Then(IDENTIFIER)
			.Select(t => (CssSimpleSelector)CssSelector.WithClass(t.StringValue));

		public static readonly Parser<CssToken, CssSimpleSelector> IdSelector = HASH
			.Select(t => (CssSimpleSelector)CssSelector.WithId(t.StringValue));

		#endregion

		#region Pseudo-class selectors

		private static readonly Parser<CssToken, CssSimpleSelector> PseudoClass = IDENTIFIER
			.Where(t => !CssPseudoElement.AllowSingleColonPrefix(t.StringValue))
			.Select(t => (CssSimpleSelector)CssSelector.WithPseudoClass(t.StringValue));

		private static readonly Parser<CssToken, string> IetfLanguageTag =
			Parser<CssToken>.Token(t => t.IsQuotedString || t.IsIdentifier).Select(t => t.StringValue);

		private static readonly Parser<CssToken, CssCycleOffset> CycleOffset =
			Parser<CssToken>.Token(t => t.IsDimension || t.IsNumber || t.IsIdentifier || t.IsWhitespace || t.IsDelimiter('+') || t.IsDelimiter('-')).AtLeastOnceUntil(RPAREN)
				.Select(tokens => tokens.Aggregate(
					new StringBuilder(4), 
					(sb, t) => { if (!t.IsWhitespace) t.ToString(sb); return sb; }, 
					sb => CssCycleOffset.Parse(sb.ToString())));

		public static readonly Parser<CssToken, CssSimpleSelector> PseudoSelector = COLON.Then(OneOf(
			PseudoClass,
			Function("not", Rec(() => SimpleSelector), sel => sel.Negate()),
			Function<string, CssSimpleSelector>("lang", IetfLanguageTag, CssSelector.WithLanguage),
			Function<CssCycleOffset, CssSimpleSelector>("nth-child", CycleOffset, CssSelector.NthChild),
			Function<CssCycleOffset, CssSimpleSelector>("nth-last-child", CycleOffset, CssSelector.NthLastChild),
			Function<CssCycleOffset, CssSimpleSelector>("nth-of-type", CycleOffset, CssSelector.NthOfType),
			Function<CssCycleOffset, CssSimpleSelector>("nth-last-of-type", CycleOffset, CssSelector.NthLastOfType)
			));

		#endregion

		#region Pseudo-element

		public static readonly Parser<CssToken, CssPseudoElement> PseudoElement = COLON
			.Then(Parser.OneOf(
				COLON.Then(IDENTIFIER),
				IDENTIFIER.Where(id => CssPseudoElement.AllowSingleColonPrefix(id.StringValue))
			))
			.Select(id => new CssPseudoElement(id.StringValue));

		#endregion

		#region Simple selectors and simple selector sequences 

		private static readonly Parser<CssToken, CssSimpleSelector> OtherSimpleSelector = 
			Parser.OneOf(IdSelector, ClassSelector, AttributeSelector, PseudoSelector);

		public static readonly Parser<CssToken, CssSimpleSelector> SimpleSelector = 
			TypeSelector.Cast<CssSimpleSelector>().Or(OtherSimpleSelector);

		public static readonly Parser<CssToken, ICssSelectorSequence> SimpleSelectorSequence = 
			Parser.OneOf(
				AggregateMany(TypeSelector.Cast<ICssSelectorSequence>(), OtherSimpleSelector, (seq, sel) => seq.Add(sel)),
				AggregateAtLeastOnce(Parser<CssToken>.Return((ICssSelectorSequence)CssSelector.ImpliedUniversal), OtherSimpleSelector, (seq, sel) => seq.Add(sel))
			)
			.Then(PseudoElement.Optional(), (seq, e) => e.HasValue ? seq.Add(e.Value) : seq);

		#endregion

		#region Selectors and selector groups

		private static readonly Parser<CssToken, CssCombinator> CombinatorDelimiter =
			Parser<CssToken>.Token(t => t.IsDelimiter('+') || t.IsDelimiter('>') || t.IsDelimiter('~')).Select(t => (CssCombinator) ((int) t.TokenType & 0xFF));
		private static readonly Parser<CssToken, CssCombinator> Combinator = CombinatorDelimiter
			.Or(WHITESPACE.AtLeastOnce().Then(CombinatorDelimiter).Or(Parser<CssToken>.Return(CssCombinator.Descendant)));
		
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

		public struct QualifiedName
		{
			public QualifiedName(string localName, string namespacePrefix)
			{
				this.LocalName = localName;
				this.NamespacePrefix = namespacePrefix;
			}

			public string LocalName { get; }
			public string NamespacePrefix { get; }

			public override string ToString()
			{
				return this.NamespacePrefix == null
					? this.LocalName
					: this.NamespacePrefix + "|" + this.LocalName;
			}
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
