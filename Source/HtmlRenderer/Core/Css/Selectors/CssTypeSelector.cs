namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Immutable;
	using System.Xml.Linq;

	/// <summary>
	/// Represents a CSS type selector.
	/// </summary>
	public abstract class CssTypeSelector : CssSimpleSelector, ICssSelectorSubject
	{
		internal CssTypeSelector(CssSpecificity specificity)
			: base(specificity)
		{}

		/// <summary>
		/// Gets local name of matching elements.
		/// </summary>
		public abstract string LocalName { get; }

		/// <summary>
		/// Gets namespace of matching elements.
		/// </summary>
		public abstract XNamespace Namespace { get; }

		public ICssSelectorSubject Subject
		{
			get { return this; }
		}

		public CssTypeSelector TypeSelector
		{
			get { return this; }
		}

		ImmutableArray<CssSelector> ICssSelectorSequence.OtherSelectors
		{
			get { return ImmutableArray<CssSelector>.Empty; }
		}

		CssPseudoElement ICssSelectorSubject.PseudoElement
		{
			get { return null; }
		}
	}
}