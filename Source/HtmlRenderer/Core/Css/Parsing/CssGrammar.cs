namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Css;

	/// <summary>
	/// Base class for creation of grammar specific rules, components and declarations. 
	/// </summary>
	public class CssGrammar
	{
		public virtual CssQualifiedRule CreateQualifiedRule(ImmutableArray<CssComponent> prelude, CssBlock block)
		{
			return new CssQualifiedRule(prelude, block);
		}

		public virtual CssAtRule CreateAtRule(ImmutableArray<CssComponent> prelude, CssBlock block)
		{
			return new CssAtRule(prelude, block);
		}

		public virtual CssFunction CreateFunction(string name, ImmutableArray<CssComponent> components)
		{
			return new CssFunction(name, components);
		}

		public virtual CssDeclaration CreateDeclaration(string name, ImmutableArray<CssSimpleComponent> values, bool isImportant)
		{
			return new CssDeclaration(name, values, isImportant);
		}
	}
}
