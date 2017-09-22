namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	internal class CssDynamicPseudoClassSelector : CssPseudoClassSelector
	{
		private readonly CssDynamicElementState _dynamicState;

		public CssDynamicPseudoClassSelector(string name, CssDynamicElementState dynamicState)
			: base(name)
		{
			_dynamicState = dynamicState;
		}

		public override bool Matches<TElement>(TElement element)
		{
			return element.HasDynamicState(_dynamicState);
		}
	}
}