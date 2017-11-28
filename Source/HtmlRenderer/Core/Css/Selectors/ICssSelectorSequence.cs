namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Immutable;

	/// <summary>
	/// Represents a sequence of a type selector followed by zero or more simple, non-type selectors.
	/// </summary>
	public interface ICssSelectorSequence : ICssSelectorChain
	{
		CssTypeSelector TypeSelector { get; }

		ImmutableArray<CssSelector> OtherSelectors { get; }
	}
}