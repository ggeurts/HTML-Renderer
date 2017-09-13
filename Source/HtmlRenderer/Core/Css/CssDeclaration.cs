namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;

	public class CssDeclaration : ICssDeclaration
	{
		public string Name { get; }
		public ImmutableArray<CssSimpleComponent> Values { get; }
		public bool isImportant { get; }

		public CssDeclaration(string name, ImmutableArray<CssSimpleComponent> values, bool isImportant)
		{
			this.Name = name;
			this.Values = values;
			this.isImportant = isImportant;
		}
	}
}