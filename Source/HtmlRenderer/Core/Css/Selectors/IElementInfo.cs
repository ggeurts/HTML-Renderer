namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Xml.Linq;

	/// <summary>
	/// Interface to be implemented by elements or element adapters
	/// </summary>
	public interface IElementInfo<TElement> where TElement : IElementInfo<TElement>
	{
		TElement Parent { get; }
		bool IsRoot { get; }
		bool IsTarget { get; }

		/// <summary>
		/// Gets the number of child elements of this element.
		/// </summary>
		int ChildCount { get; }

		/// <summary>
		/// Gets zero-based index of this element in the collection of sibling elements
		/// </summary>
		/// <remarks>NOTE: The CSS specification works with one-based collections (first collection item has index 1), 
		/// whereas .NET uses zero-based collections. This library follows the .NET conventions for calculations 
		/// involving indices.</remarks>
		int ChildIndex { get; }

		/// <summary>
		/// Gets zero-based index of this element in the collection of sibling elements 
		/// with the same name as this element.
		/// </summary>
		/// <remarks>NOTE: The CSS specification works with one-based collections (first collection item has index 1), 
		/// whereas .NET uses zero-based collections. This library follows the .NET conventions for calculations 
		/// involving indices.</remarks>
		int SiblingIndex { get; }

		/// <summary>
		/// Gets the number of sibling elements with the same name as this element.
		/// </summary>
		int SiblingCount { get; }

		bool HasName(XName name);
		bool HasName(string localName);
		bool HasNamespace(XNamespace ns);
		bool HasChildren { get; }
		bool HasAttribute(XName name, Func<string, StringComparison, bool> predicate);
		bool HasAttribute(string localName, Func<string, StringComparison, bool> predicate);
		bool HasAttributeInNamespace(XNamespace ns, Func<string, StringComparison, bool> predicate);
		bool HasClass(string name);
		bool HasId(string id);
		bool HasLanguage(string ietfLanguageTag);
		bool HasDynamicState(CssDynamicElementState state);
		bool TryGetPredecessor(ICssElementMatcher selector, bool immediateOnly, out TElement result);
	}
}