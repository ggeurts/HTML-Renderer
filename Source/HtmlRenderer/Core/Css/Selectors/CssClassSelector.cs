namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssClassSelector : CssSimpleSelector, ICssElementMatcher
	{
		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(0, 1, 0);

		private readonly string _className;

		internal CssClassSelector(string className)
			: base(DefaultSpecificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(className, nameof(className));
			_className = className;
		}

		public string ClassName
		{
			get { return _className; }
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitClassSelector(this);
		}

		public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
		{
			return element.HasClass(_className);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssClassSelector;
			return other != null && _className == other._className;
		}

		public override int GetHashCode()
		{
			return '.' ^ _className.GetHashCode();
		}
	}
}