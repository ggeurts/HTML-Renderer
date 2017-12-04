namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssPseudoClassSelector : CssSimpleSelector, ICssElementMatcher
	{
		#region Static fields

		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(0, 1, 0);

		#endregion

		#region Instance fields

		private readonly string _name;

		#endregion

		#region Constructor(s)

		protected CssPseudoClassSelector(string name)
			: this(name, DefaultSpecificity)
		{}

		protected CssPseudoClassSelector(string name, CssSpecificity specificity)
			: base(specificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			_name = name.ToLowerInvariant();
		}

		#endregion

		#region Properties

		public string ClassName
		{
			get { return _name; }
		}

		#endregion

		#region Public methods

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitPseudoClassSelector(this);
		}

		public abstract bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssPseudoClassSelector;
			return other != null
			       && GetType() == other.GetType()
			       && _name == other._name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		#endregion
	}
}