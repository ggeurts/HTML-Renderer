namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssNegationSelector : CssSimpleSelector
	{
		private readonly CssSimpleSelector _operand;

		internal CssNegationSelector(CssSimpleSelector operand)
			: base(CalculateSpecificity(operand))
		{
			_operand = operand;
		}

		private static CssSpecificity CalculateSpecificity(CssSimpleSelector selector)
		{
			ArgChecker.AssertArgNotNull(selector, nameof(selector));
			return selector.Specificity;
		}

		public CssSimpleSelector Operand
		{
			get { return _operand; }
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitNegationSelector(this);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssNegationSelector;
			return other != null
				&& _operand.Equals(other._operand);
		}

		public override int GetHashCode()
		{
			return ~_operand.GetHashCode();
		}
	}
}