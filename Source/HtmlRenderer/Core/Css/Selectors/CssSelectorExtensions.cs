namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;
	using System.Xml;

	public static class CssSelectorExtensions
	{
		/// <summary>
		/// Creates a new <see cref="CssSimpleSelectorSequence"/> consisting of a given sequence with an additional selector appended.
		/// </summary>
		/// <param name="sequence">A <see cref="CssTypeSelector"/> or <see cref="CssSimpleSelectorSequence"/> instance.</param>
		/// <param name="otherSelector">A simple, non-type selector to append to the original sequence.</param>
		/// <returns>a new <see cref="CssSimpleSelectorSequence"/> consisting of the original <paramref name="sequence"/> followed
		/// by <paramref name="otherSelector"/>.</returns>
		public static ICssSelectorSequence Add(this ICssSelectorSequence sequence, CssSimpleSelector otherSelector)
		{
			return new CssSimpleSelectorSequence(sequence, otherSelector);
		}

		/// <summary>
		/// Creates a new <see cref="CssSimpleSelectorSequence"/> consisting of a given sequence with additional selectors appended.
		/// </summary>
		/// <param name="sequence">A <see cref="CssTypeSelector"/> or <see cref="CssSimpleSelectorSequence"/> instance.</param>
		/// <param name="otherSelectors">Zero or more simple, non-type selectors to append to the original sequence.</param>
		/// <returns>a new <see cref="CssSimpleSelectorSequence"/> consisting of the original <paramref name="sequence"/> followed
		/// by <paramref name="otherSelectors"/>.</returns>
		public static ICssSelectorSequence AddRange(this ICssSelectorSequence sequence, IEnumerable<CssSimpleSelector> otherSelectors)
		{
			var result = sequence;
			foreach (var otherSelector in otherSelectors)
			{
				result = result.Add(otherSelector);
			}
			return result;
		}

		/// <summary>
		/// Creates a new <see cref="CssSimpleSelectorSequence"/> consisting of a given sequence with an additional pseudo-element appended.
		/// </summary>
		/// <param name="sequence">A <see cref="CssTypeSelector"/> or <see cref="CssSimpleSelectorSequence"/> instance.</param>
		/// <param name="pseudoElement">The pseudo-element to append.</param>
		/// <returns>a new <see cref="CssSimpleSelectorSequence"/> consisting of the original <paramref name="sequence"/> followed
		/// by the <paramref name="pseudoElement"/>.</returns>
		public static ICssSelectorSequence Add(this ICssSelectorSequence sequence, CssPseudoElement pseudoElement)
		{
			return new CssSimpleSelectorSequence(sequence, pseudoElement);
		}

		/// <summary>
		/// Creates a selector consisting of two selector sequences and a <see cref="CssCombinator"/>.
		/// </summary>
		/// <param name="leftSelector">The left operand of the combination operator.</param>
		/// <param name="combinator">The combination operator.</param>
		/// <param name="rightSelector">The right operand of the combination operator.</param>
		/// <returns>The newly created combined selector.</returns>
		public static ICssSelectorChain Combine(this ICssSelectorChain leftSelector, CssCombinator combinator, ICssSelectorSequence rightSelector)
		{
			return new CssSelectorCombination(combinator, leftSelector, rightSelector);
		}

		public static ICssElementMatcher BuildMatcher(this ICssSelector selector, IXmlNamespaceResolver namespaceResolver = null)
		{
			var builder = new CssElementMatcherBuilderVisitor(namespaceResolver);
			selector.Apply(builder);
			return builder.Matcher;
		}
	}
}