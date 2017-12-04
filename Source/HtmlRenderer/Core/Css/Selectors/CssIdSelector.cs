namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssIdSelector : CssSimpleSelector, ICssElementMatcher
	{
		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(1, 0, 0);

		private readonly string _id;

		internal CssIdSelector(string id)
			: base(DefaultSpecificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(id, nameof(id));
			_id= id;
		}

		public string Id
		{
			get { return _id; }
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitIdSelector(this);
		}

		public bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>
		{
			return element.HasId(_id);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssIdSelector;
			return other != null && _id == other._id;
		}

		public override int GetHashCode()
		{
			return '#' ^ _id.GetHashCode();
		}
	}
}