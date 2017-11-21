namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Xml.Linq;

	/// <summary>
	/// Represents a CSS type selector.
	/// </summary>
	public abstract class CssTypeSelector : CssSimpleSelector, ICssSelectorSequence
	{
		public CssTypeSelector TypeSelector
		{
			get { return this; }
		}

		internal CssTypeSelector()
		{}

		/// <summary>
		/// Gets local name of matching elements.
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// Gets namespace of matching elements.
		/// </summary>
		public abstract XNamespace Namespace { get; }
	}
}