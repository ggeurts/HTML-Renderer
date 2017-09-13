namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssCompositeComponent : CssComponent
	{
		public IImmutableList<CssComponent> Components { get; }

		public CssCompositeComponent(IImmutableList<CssComponent> components)
		{
			ArgChecker.AssertArgNotNull(components, nameof(components));
			this.Components = components;
		}
	}
}