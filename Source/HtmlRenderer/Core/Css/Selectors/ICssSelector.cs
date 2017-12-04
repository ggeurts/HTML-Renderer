namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	/// <summary>
	/// Represents a CSS selector
	/// </summary>
	public interface ICssSelector
	{
		/// <summary>
		/// Gets the selector's specificity.
		/// </summary>
		CssSpecificity Specificity { get; }

		void Apply(CssSelectorVisitor visitor);
	}

	public interface ICssElementMatcher
	{
		/// <summary>
		/// Indicates whether a given element matches this selector.
		/// </summary>
		/// <typeparam name="TElement">The element (wrapper) type.</typeparam>
		/// <param name="element">The element to be matched.</param>
		/// <returns>Returns <c>true</c> if <paramref name="element"/> matches this selector, or <c>false</c> otherwise.</returns>
		bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;
	}
}