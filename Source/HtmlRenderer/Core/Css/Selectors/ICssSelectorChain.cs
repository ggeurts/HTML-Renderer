namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Generic;

	/// <summary>
	/// Represents a chain of one or more selector sequences separated by combinators
	/// </summary>
	public interface ICssSelectorChain : ICssSelector
	{
		ICssSelectorSubject Subject { get; }
	}
}