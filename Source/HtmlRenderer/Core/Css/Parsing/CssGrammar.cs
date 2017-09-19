namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Css;

	/// <summary>
	/// Base class for creation of grammar specific rules, components and declarations. 
	/// </summary>
	public class CssGrammar
	{
		public virtual CssComponent ConsumeQualifiedRulePrelude(CssParser.FragmentParser parser)
		{
			var firstComponent = parser.CurrentToken;
			if (!parser.MoveNext()) return firstComponent;

			var components = ImmutableArray.CreateBuilder<CssComponent>();
			components.Add(firstComponent);
			do
			{
				components.Add(parser.CurrentToken);
			} while (parser.MoveNext());

			return new CssCompositeComponent(components.ToImmutable());
		}

		public virtual CssBlock ConsumeQualifiedRuleBlock(CssParser.FragmentParser parser)
		{
			return parser.ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
		}

		public virtual CssComponent ConsumeAtRulePrelude(string name, CssParser.FragmentParser parser)
		{
			var firstComponent = parser.CurrentToken;
			if (!parser.MoveNext()) return firstComponent;

			var components = ImmutableArray.CreateBuilder<CssComponent>();
			components.Add(firstComponent);
			do
			{
				components.Add(parser.CurrentToken);
			} while (parser.MoveNext());

			return new CssCompositeComponent(components.ToImmutable());
		}

		public virtual CssBlock ConsumeAtRuleBlock(string name, CssParser.FragmentParser parser)
		{
			return parser.ConsumeSimpleBlock(CssBlockType.CurlyBrackets);
		}

		public virtual CssQualifiedRule CreateQualifiedRule(CssComponent prelude, CssBlock block)
		{
			return new CssQualifiedRule(prelude, block);
		}

		public virtual CssAtRule CreateAtRule(string name, CssComponent prelude, CssBlock block)
		{
			return new CssAtRule(name, prelude, block);
		}

		public virtual CssDeclaration CreateDeclaration(string name, ImmutableArray<CssToken> values, bool isImportant)
		{
			return new CssDeclaration(name, values, isImportant);
		}

		public virtual CssFunction CreateFunction(string name, ImmutableArray<CssComponent> components)
		{
			return new CssFunction(name, components);
		}
 	}
}
