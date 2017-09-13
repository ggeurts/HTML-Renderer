namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;

	public class CssBlock : CssCompositeComponent
	{
		public CssBlockType BlockType { get; }

		public CssBlock(CssBlockType blockType, IImmutableList<CssComponent> components)
			: base(components)
		{
			this.BlockType = blockType;
		}
	}
}