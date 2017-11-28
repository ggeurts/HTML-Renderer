namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	public interface ICssSelectorSubject : ICssSelectorSequence
	{
		CssPseudoElement PseudoElement { get; }
	}
}