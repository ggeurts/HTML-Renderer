namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Xml.Linq;

	/// <summary>
	/// Represents a CSS type selector.
	/// </summary>
	public abstract class CssTypeSelector : CssSimpleSelector, ICssSelectorSequence
	{
		protected const string AnyLocalName = "*";
		protected const string AnyNamespacePrefix = "*";

		public CssTypeSelector TypeSelector
		{
			get { return this; }
		}

		public abstract string LocalName { get; }
		public abstract XNamespace Namespace { get; }
		public abstract string NamespacePrefix { get; }
	}
}