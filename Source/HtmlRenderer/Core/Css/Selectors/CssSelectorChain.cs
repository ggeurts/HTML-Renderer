namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssSelectorChain : CssSelector, ICssSelector
	{
		private readonly ICssSelector _finalSequence;
		private readonly CssCombinatorSelector _combinator;

		internal CssSelectorChain(ICssSelector finalSequence, CssCombinatorSelector combinator)
		{
			ArgChecker.AssertArgNotNull(finalSequence, nameof(finalSequence));
			ArgChecker.AssertArgNotNull(combinator, nameof(combinator));
			_finalSequence = finalSequence;
			_combinator = combinator;
		}

		public CssTypeSelector TypeSelector
		{
			get { return _finalSequence.TypeSelector; }
		}

		public override bool Matches<TElement>(TElement element)
		{
			return _finalSequence.Matches(element) && _combinator.Matches(element);
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			_combinator.ToString(sb, namespaceResolver);
			_finalSequence.ToString(sb, namespaceResolver);
		}
	}
}