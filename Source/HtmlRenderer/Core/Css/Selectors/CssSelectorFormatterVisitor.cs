namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssSelectorFormatterVisitor : CssSelectorVisitor
	{
		private readonly StringBuilder _sb;

		public CssSelectorFormatterVisitor(StringBuilder sb)
		{
			ArgChecker.AssertArgNotNull(sb, nameof(sb));
			_sb = sb;
		}

		public override void VisitAttributeSelector(CssAttributeSelector selector)
		{
			_sb.Append('[');

			if (selector.NamespacePrefix != null)
			{
				// We must write a namespace prefix for any explicit namespace. The default namespace
				// does not apply to attributes.
				_sb.Append(selector.NamespacePrefix).Append('|');
			}
			_sb.Append(selector.LocalName);

			if (selector.MatchOperator != CssAttributeMatchOperator.Any)
			{
				if (selector.MatchOperator != CssAttributeMatchOperator.Exact)
				{
					_sb.Append((char)selector.MatchOperator);
				}
				_sb.Append('=');
				selector.MatchOperandToken.ToString(_sb);
			}

			_sb.Append(']');
		}

		public override void VisitClassSelector(CssClassSelector selector)
		{
			_sb.Append('.').Append(selector.ClassName);
		}

		public override void VisitIdSelector(CssIdSelector selector)
		{
			_sb.Append('#').Append(selector.Id);
		}

		public override void VisitNegationSelector(CssNegationSelector selector)
		{
			_sb.Append("not(");
			selector.Operand.Apply(this);
			_sb.Append(')');
		}

		public override void VisitPseudoClassSelector(CssPseudoClassSelector selector)
		{
			_sb.Append(':').Append(selector.ClassName);
		}

		public override void VisitStructuralPseudoClassSelector(CssStructuralPseudoClassSelector selector)
		{
			VisitPseudoClassSelector(selector);
			_sb.Append('(');

			var useShortForm = false;
			switch (selector.CycleSize)
			{
				case -1:
					_sb.Append("-n");
					break;
				case 0:
					useShortForm = true;
					break;
				case 1:
					_sb.Append('n');
					break;
				default:
					_sb.Append(selector.CycleSize).Append('n');
					break;
			}

			if (useShortForm || selector.Offset < 0)
			{
				_sb.Append(selector.Offset);
			}
			else if (selector.Offset > 0)
			{
				_sb.Append('+').Append(selector.Offset);
			}

			_sb.Append(')');

		}

		public override void VisitPseudoElement(CssPseudoElement element)
		{
			element.ToString(_sb);
		}

		public override void VisitSelectorCombination(CssSelectorCombination combination)
		{
			combination.LeftOperand.Apply(this);
			_sb.Append(' ');
			if (combination.Combinator != CssCombinator.Descendant)
			{
				_sb.Append((char)combination.Combinator).Append(' ');
			}
			combination.RightOperand.Apply(this);
		}

		public override void VisitSelectorSequence(CssSimpleSelectorSequence sequence)
		{
			sequence.TypeSelector.Apply(this);
			foreach (var selector in sequence.OtherSelectors)
			{
				selector.Apply(this);
			}
			sequence.PseudoElement?.ToString(_sb);
		}

		public override void VisitTypeSelector(CssTypeSelector selector)
		{
			if (ReferenceEquals(selector, CssSelector.ImpliedUniversal)) return;

			if (selector.NamespacePrefix != null)
			{
				_sb.Append(selector.NamespacePrefix).Append('|');
			}
			_sb.Append(selector.LocalName);
		}
	}
}