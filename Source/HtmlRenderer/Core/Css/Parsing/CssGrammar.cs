namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using TheArtOfDev.HtmlRenderer.Core.Css;

	/// <summary>
	/// Base class for creation of grammar specific rules, components and declarations. 
	/// </summary>
	public class CssGrammar
	{
		public virtual CssGrammar GetAtRuleGrammar(string name)
		{
			return null;
		}

		public virtual CssComponent ParseQualifiedRulePrelude(CssParser.Scope parser)
		{
			return ParsePrelude(parser);
		}

		public virtual IEnumerable<CssComponent> ParseQualifiedRuleBlock(CssParser.Scope parser)
		{
			return parser.ParseComponentList();
		}

		public virtual CssComponent ParseAtRulePrelude(CssParser.Scope parser)
		{
			return ParsePrelude(parser);
		}

		public virtual IEnumerable<CssComponent> ParseAtRuleBlock(CssParser.Scope parser)
		{
			return parser.ParseComponentList();
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

		private static CssComponent ParsePrelude(CssParser.Scope parser)
		{
			using (var componentsEnum = parser.ParseComponentList().GetEnumerator())
			{
				if (!componentsEnum.MoveNext()) return CssComponent.Empty;

				var firstComponent = componentsEnum.Current;
				if (!componentsEnum.MoveNext()) return firstComponent;

				var components = ImmutableArray.CreateBuilder<CssComponent>();
				components.Add(firstComponent);
				do
				{
					components.Add(componentsEnum.Current);
				} while (componentsEnum.MoveNext());

				return new CssCompositeComponent(components.ToImmutable());
			}
		}
	}
}
