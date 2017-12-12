namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	/// <summary>
	/// Represents a CSS type selector.
	/// </summary>
	public class CssTypeSelector : CssSimpleSelector, ICssSelectorSubject
	{
		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(0, 0, 1);
		private static readonly CssSpecificity UniversalSpecificity = default(CssSpecificity);

		private readonly string _localName;
		private readonly string _namespacePrefix;

		internal CssTypeSelector(string localName, string namespacePrefix)
			: base(localName == AnyLocalName ? UniversalSpecificity : DefaultSpecificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(localName, nameof(localName));
			_localName = localName;
			_namespacePrefix = namespacePrefix;
		}

		internal CssTypeSelector(string localName, string namespacePrefix, CssSpecificity specificity)
			: base(specificity)
		{
			ArgChecker.AssertArgNotNullOrEmpty(localName, nameof(localName));
			_localName = localName;
			_namespacePrefix = namespacePrefix;
		}

		/// <summary>
		/// Gets local name of matching elements.
		/// </summary>
		public string LocalName
		{
			get { return _localName; }
		}

		/// <summary>
		/// Gets namespace prefix for <see cref="LocalName"/>.
		/// </summary>
		public string NamespacePrefix
		{
			get { return _namespacePrefix; }
		}

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

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitTypeSelector(this);
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssTypeSelector;
			return other != null
			       && _localName == other._localName
			       && _namespacePrefix == other._namespacePrefix;
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(_localName.GetHashCode(), _namespacePrefix?.GetHashCode() ?? 0);
		}

	}
}