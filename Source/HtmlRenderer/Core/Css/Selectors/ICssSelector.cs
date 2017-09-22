namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;

	public interface ICssSelector
	{
		CssTypeSelector TypeSelector { get; }
		bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;
		void ToString(StringBuilder sb);
	}

	public interface ICssSelectorSequence : ICssSelector
	{
	}
}