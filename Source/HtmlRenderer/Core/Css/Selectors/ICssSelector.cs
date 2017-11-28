namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;

	/// <summary>
	/// Represents a CSS selector
	/// </summary>
	public interface ICssSelector
	{
		/// <summary>
		/// Indicates whether a given element matches this selector.
		/// </summary>
		/// <typeparam name="TElement">The element (wrapper) type.</typeparam>
		/// <param name="element">The element to be matched.</param>
		/// <returns>Returns <c>true</c> if <paramref name="element"/> matches this selector, or <c>false</c> otherwise.</returns>
		bool Matches<TElement>(TElement element) where TElement : IElementInfo<TElement>;

		/// <summary>
		/// Appends string representation of this selector.
		/// </summary>
		/// <param name="sb">The <see cref="StringBuilder"/> to which string representation of this selector is to be added.</param>
		/// <param name="namespaceResolver">The XML namespace resolver to use for resolutaion of namespace prefixes.</param>
		void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver);
	}
}