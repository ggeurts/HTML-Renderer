namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Immutable;

	public class CssStylesheetGrammar : CssGrammar
	{
		public override CssBlock ConsumeQualifiedRuleBlock(CssParser.FragmentParser parser)
		{
			var declarations = ImmutableArray.CreateBuilder<CssDeclaration>();
			do
			{
				declarations.Add(parser.ConsumeDeclaration(this));
			} while (parser.MoveNext());

			return new CssBlock(CssBlockType.CurlyBrackets, declarations.ToImmutable().CastArray<CssComponent>());
		}
	}
}