namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Xml.Linq;

	public abstract class CssSelectorVisitor
	{
		public virtual void VisitAttributeSelector(CssAttributeSelector selector)
		{ }

		public virtual void VisitClassSelector(CssClassSelector selector)
		{}

		public virtual void VisitIdSelector(CssIdSelector selector)
		{ }

		public virtual void VisitNegationSelector(CssNegationSelector selector)
		{ }

		public virtual void VisitPseudoClassSelector(CssPseudoClassSelector selector)
		{}

		public virtual void VisitStructuralPseudoClassSelector(CssStructuralPseudoClassSelector selector)
		{}

		public virtual void VisitSelectorSequence(CssSimpleSelectorSequence sequence)
		{
			sequence.TypeSelector.Apply(this);
			foreach (var otherSelector in sequence.OtherSelectors)
			{
				otherSelector.Apply(this);
			}
			if (sequence.PseudoElement != null)
			{
				VisitPseudoElement(sequence.PseudoElement);
			}
		}

		public virtual void VisitSelectorCombination(CssSelectorCombination combination)
		{
			combination.LeftOperand.Apply(this);
			combination.RightOperand.Apply(this);
		}

		public virtual void VisitPseudoElement(CssPseudoElement element)
		{ }

		public virtual void VisitTypeSelector(CssTypeSelector selector)
		{ }
	}
}