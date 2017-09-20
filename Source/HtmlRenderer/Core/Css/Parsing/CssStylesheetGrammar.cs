namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;

	public class CssStylesheetGrammar : CssGrammar
	{
		public override CssGrammar GetAtRuleGrammar(string name)
		{
			if (name == "page") return this;
			return base.GetAtRuleGrammar(name);
		}

		public override IEnumerable<CssComponent> ParseQualifiedRuleBlock(CssParser.Scope parser)
		{
			return parser.ParseDeclarationList();
		}

		public override IEnumerable<CssComponent> ParseAtRuleBlock(CssParser.Scope parser)
		{
			return parser.ParseDeclarationList();
		}
	}
}