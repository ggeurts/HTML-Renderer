namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Xml.Linq;

	public abstract class CssTypeSelector : CssSimpleSelector, ICssSelectorSequence
	{
		public CssTypeSelector TypeSelector
		{
			get { return this; }
		}
	}
}