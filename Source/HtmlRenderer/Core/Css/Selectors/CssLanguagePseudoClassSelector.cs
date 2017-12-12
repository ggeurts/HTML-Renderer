namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssLanguagePseudoClassSelector : CssPseudoClassSelector
	{
		private readonly string _ietfLanguageTag;

		internal CssLanguagePseudoClassSelector(string ietfLanguageTag)
			: base("lang")
		{
			ArgChecker.AssertArgNotNullOrEmpty(ietfLanguageTag, nameof(ietfLanguageTag));
			_ietfLanguageTag = ietfLanguageTag;
		}

		public string IetfLanfguageTag
		{
			get { return _ietfLanguageTag; }
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitLanguagePseudoClassSelector(this);
		}

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasLanguage(_ietfLanguageTag);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssLanguagePseudoClassSelector;
			return other != null
		       && StringComparer.OrdinalIgnoreCase.Equals(_ietfLanguageTag, other._ietfLanguageTag);
		}

		public override int GetHashCode()
		{
			return StringComparer.OrdinalIgnoreCase.GetHashCode(_ietfLanguageTag);
		}
	}
}
