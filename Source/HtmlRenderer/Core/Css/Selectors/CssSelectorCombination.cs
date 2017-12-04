namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssSelectorCombination : CssSelector, ICssSelectorChain
	{
		#region Instance fields

		private readonly CssCombinator _combinator;
		private readonly ICssSelectorChain _leftOperand;
		private readonly ICssSelectorSequence _rightOperand;

		#endregion

		#region Constructor(s)

		internal CssSelectorCombination(CssCombinator combinator, ICssSelectorChain leftOperand, ICssSelectorSequence rightOperand)
			: base(CalculateSpecificity(leftOperand, rightOperand))
		{
			ArgChecker.AssertIsTrue<ArgumentException>(leftOperand.Subject.PseudoElement == null, 
				"Pseudo-elements are only allowed in the right operand of a combinator");
			_combinator = combinator;
			_leftOperand = leftOperand;
			_rightOperand = rightOperand;
		}

		private static CssSpecificity CalculateSpecificity(ICssSelectorChain leftOperand, ICssSelectorSequence rightOperand)
		{
			ArgChecker.AssertArgNotNull(leftOperand, nameof(leftOperand));
			ArgChecker.AssertArgNotNull(rightOperand, nameof(rightOperand));
			return leftOperand.Specificity + rightOperand.Specificity;
		}

		#endregion

		#region Properties

		public ICssSelectorSubject Subject
		{
			get { return this.RightOperand.Subject; }
		}

		public CssCombinator Combinator
		{
			get { return _combinator; }
		}

		public ICssSelectorChain LeftOperand
		{
			get { return _leftOperand; }
		}

		public ICssSelectorSequence RightOperand
		{
			get { return _rightOperand; }
		}

		#endregion

		#region Public methods

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitSelectorCombination(this);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssSelectorCombination;
			return other != null
				&& this.Combinator == other.Combinator
				&& Equals(this.LeftOperand, other.LeftOperand)
				&& Equals(this.RightOperand, other.RightOperand);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int)this.Combinator, this.RightOperand.GetHashCode());
		}

		#endregion
	}
}