namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	public interface ICssSelectorChain : ICssSelector
	{
		ICssSelectorSubject Subject { get; }
	}
}