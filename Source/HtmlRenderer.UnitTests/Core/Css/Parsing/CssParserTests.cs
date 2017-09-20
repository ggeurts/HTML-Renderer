namespace HtmlRenderer.UnitTests.Core.Css.Parsing
{
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;

	[TestFixture]
	public class CssParserTests
	{
		#region ParseComponent() Tests;

		[Test]
		[TestCaseSource(nameof(SingleValueTestCases))]
		public void ParseComponent_Value(string css, CssComponent expectedBlock)
		{
			var parser = CreateParser(css);
			var component = parser.ParseComponent();
			Assert.That(component, Is.EqualTo(expectedBlock));
		}

		[Test]
		[TestCaseSource(nameof(SingleBlockTestCases))]
		public void ParseComponent_Block(string css, CssBlock expectedBlock)
		{
			var parser = CreateParser(css);
			var component = parser.ParseComponent();
			Assert.That(component, Is.EqualTo(expectedBlock));
		}

		#endregion

		#region ParseComponentList() Tests

		[Test]
		[TestCaseSource(nameof(SingleValueTestCases))]
		public void ParseComponentList_SingleValue(string css, CssToken expectedComponent)
		{
			var parser = CreateParser(css);
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(new[] { expectedComponent }));
		}

		[Test]
		[TestCaseSource(nameof(SingleBlockTestCases))]
		public void ParseComponentList_SingleBlock(string css, CssBlock expectedBlock)
		{
			var parser = CreateParser(css);
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(new[] { expectedBlock }));
		}

		[Test]
		[TestCaseSource(nameof(TwoValuesTestCases))]
		public void ParseComponentList_MultipleValues(string css, CssToken[] expectedComponents)
		{
			var parser = CreateParser(css);
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(expectedComponents));
		}

		[Test]
		[TestCaseSource(nameof(MultipleComponentTestCases))]
		public void ParseComponentList_MultipleComponents(string css, CssComponent[] expectedComponents)
		{
			var parser = CreateParser(css);
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(expectedComponents));
		}

		#endregion

		#region ParseDeclaration Tests

		[Test]
		[TestCaseSource(nameof(SingleDeclarationTestCases))]
		public void ParseDeclaration(string css, CssComponent expectedDeclaration)
		{
			var parser = CreateParser(css);
			var declaration = parser.ParseDeclaration();
			Assert.That(declaration, Is.EqualTo(expectedDeclaration));
		}

		#endregion

		#region ParseDeclarationList tests

		[Test]
		[TestCaseSource(nameof(SingleDeclarationTestCases))]
		public void ParseDeclarationList_SingleDeclaration(string css, CssComponent expectedDeclaration)
		{
			var parser = CreateParser(css);
			var declarations = parser.ParseDeclarationList().ToList();
			Assert.That(declarations, Is.EquivalentTo(new[] { expectedDeclaration }));
		}

		[Test]
		public void ParseDeclarationList_MultipleDeclarations()
		{
			var css = @"
				font: sans-serif black 10pt !important;
				border: 1px silver solid";
			var parser = CreateParser(css);
			var declarations = parser.ParseDeclarationList().ToList();
			Assert.That(declarations, Is.EquivalentTo(new[]
			{
				new CssDeclaration("font", new[]
					{
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "sans-serif"),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "black"),
						CssToken.Whitespace,
						new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")),
						CssToken.Whitespace,
					},
					true),
				new CssDeclaration("border", new[]
					{
						CssToken.Whitespace,
						new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(1, "px")),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "silver"),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "solid"),
					},
					true),
			}));
		}

		[Test]
		[TestCaseSource(nameof(SingleAtRuleTestCases))]
		public void ParseDeclarationList_SingleAtRule(string css, CssNode expectedDeclaration)
		{
			var parser = CreateParser(css);
			var declarations = parser.ParseDeclarationList().ToList();
			Assert.That(declarations, Is.EquivalentTo(new[] { expectedDeclaration }));
		}

		[Test]
		public void ParseDeclarationList_DeclarationAndAtRule()
		{
			var css = @"
				font: sans-serif black 10pt !important;
				@page { margin: 2.5cm }";
			var parser = CreateParser(css);
			var declarations = parser.ParseDeclarationList().ToList();
			Assert.That(declarations, Is.EquivalentTo(new CssComponent[]
			{
				new CssDeclaration("font", new[]
					{
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "sans-serif"),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "black"),
						CssToken.Whitespace,
						new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")),
						CssToken.Whitespace,
					},
					true),
				new CssAtRule("page", CssToken.Whitespace, new CssBlock(CssBlockType.CurlyBrackets, new[]
					{
						new CssDeclaration("margin", new[]
						{
							CssToken.Whitespace,
							new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(2.5, "cm")),
							CssToken.Whitespace,
						}, false)
					}))
			}));
		}

		#endregion

		#region TestCase Sources

		public static IEnumerable<TestCaseData> SingleValueTestCases
		{
			get
			{
				yield return new TestCaseData("id", new CssStringToken(CssTokenType.Identifier, "id"));
				yield return new TestCaseData("\"some value\"", new CssStringToken(CssTokenType.QuotedString, "some value"));
				yield return new TestCaseData("'other value'", new CssStringToken(CssTokenType.QuotedString, "other value"));
				yield return new TestCaseData("12", new CssToken<CssNumeric>(CssTokenType.Number, new CssNumeric(12)));
				yield return new TestCaseData("-14", new CssToken<CssNumeric>(CssTokenType.Number, new CssNumeric(-14)));
				yield return new TestCaseData("12.34e-5", new CssToken<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5)));
				yield return new TestCaseData("12.34E-5", new CssToken<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5)));
				yield return new TestCaseData("-.34e-5", new CssToken<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5)));
				yield return new TestCaseData("-.34E-5", new CssToken<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5)));
				yield return new TestCaseData("10pt", new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")));
				yield return new TestCaseData("+10pt", new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")));
				yield return new TestCaseData("-10pt", new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(-10, "pt")));
				yield return new TestCaseData("10.5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(10.5, "pt")));
				yield return new TestCaseData("+10.5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(10.5, "pt")));
				yield return new TestCaseData("-10.5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-10.5, "pt")));
				yield return new TestCaseData("12.34e-5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "pt")));
				yield return new TestCaseData("12.34E-5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "pt")));
				yield return new TestCaseData("-.34e-5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "pt")));
				yield return new TestCaseData("-.34E-5pt", new CssToken<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "pt")));
				yield return new TestCaseData("10%", new CssToken<CssNumeric>(CssTokenType.Percentage, new CssNumeric(10, "%")));
				yield return new TestCaseData("+10%", new CssToken<CssNumeric>(CssTokenType.Percentage, new CssNumeric(10, "%")));
				yield return new TestCaseData("-10%", new CssToken<CssNumeric>(CssTokenType.Percentage, new CssNumeric(-10, "%")));
				yield return new TestCaseData("10.5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(10.5, "%")));
				yield return new TestCaseData("+10.5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(10.5, "%")));
				yield return new TestCaseData("-10.5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-10.5, "%")));
				yield return new TestCaseData("12.34e-5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "%")));
				yield return new TestCaseData("12.34E-5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "%")));
				yield return new TestCaseData("-.34e-5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "%")));
				yield return new TestCaseData("-.34E-5%", new CssToken<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "%")));
			}
		}

		public static IEnumerable<TestCaseData> MultipleValueTestCases
		{
			get
			{
				var singleComponentTestCases = SingleValueTestCases.ToList();
				var count = singleComponentTestCases.Count;
				var offset = count / 2;

				for (var i = 0; i < count; i++)
				{
					var t1 = singleComponentTestCases[i];

					var css = (string)t1.Arguments[0];
					var components = new List<CssComponent> { (CssToken)t1.Arguments[1] };

					var size = 2 + i % 3;
					for (var j = 1; j < size; j++)
					{
						var tn = singleComponentTestCases[(i + j + offset) % count];
						css = css + " " + (string)tn.Arguments[0];
						components.Add(CssToken.Whitespace);
						components.Add((CssToken)tn.Arguments[1]);
					}

					yield return new TestCaseData(css, components.ToArray());
				}
			}
		}

		public static IEnumerable<TestCaseData> SingleBlockTestCases
		{
			get
			{
				var i = 0;
				return MultipleValueTestCases.Select(tc =>
				{
					i++;
					CssBlockType blockType = null;
					switch (i % 3)
					{
						case 0:
							blockType = CssBlockType.CurlyBrackets;
							break;
						case 1:
							blockType = CssBlockType.SquareBrackets;
							break;
						default:
							blockType = CssBlockType.Parentheses;
							break;
					}

					var css = blockType.BeginChar + (string)tc.Arguments[0] + blockType.EndChar;
					var block = new CssBlock(blockType, (CssComponent[])tc.Arguments[1]);
					return new TestCaseData(css, block);
				});
			}
		}

		public static IEnumerable<TestCaseData> TwoValuesTestCases
		{
			get
			{
				var singleComponentTestCases = SingleValueTestCases.ToList();
				var count = singleComponentTestCases.Count;
				var offset = count / 2;

				for (var i = 0; i < count; i++)
				{
					var t1 = singleComponentTestCases[i];
					var t2 = singleComponentTestCases[(i + offset) % count];
					yield return new TestCaseData(
						(string)t1.Arguments[0] + " " + (string)t2.Arguments[0],
						new[]
						{
							(CssToken)t1.Arguments[1],
							CssToken.Whitespace,
							(CssToken)t2.Arguments[1]
						});
				}
			}
		}

		public static IEnumerable<TestCaseData> MultipleComponentTestCases
		{
			get
			{
				var singleComponentTestCases = SingleValueTestCases.ToList();
				var singleBlockTestCases = SingleBlockTestCases.ToList();
				var count = singleComponentTestCases.Count;

				for (var i = 0; i < count; i++)
				{
					var t1 = singleComponentTestCases[i];
					var t2 = singleBlockTestCases[i % singleBlockTestCases.Count];

					if (i % 2 == 0)
					{
						var temp = t1; t1 = t2; t2 = temp;
					}

					yield return new TestCaseData(
						(string)t1.Arguments[0] + " " + (string)t2.Arguments[0],
						new[]
						{
							(CssComponent)t1.Arguments[1],
							CssToken.Whitespace,
							(CssComponent)t2.Arguments[1]
						});
				}
			}
		}

		public static IEnumerable<TestCaseData> SingleDeclarationTestCases
		{
			get
			{
				yield return new TestCaseData("text-align:", new CssDeclaration("text-align", new CssToken[0], false));
				yield return new TestCaseData(" text-align:", new CssDeclaration("text-align", new CssToken[0], false));
				yield return new TestCaseData("text-align: ", new CssDeclaration("text-align", CssToken.Whitespace, false));
				yield return new TestCaseData("text-align:!important", new CssDeclaration("text-align", new CssToken[0], true));
				yield return new TestCaseData("text-align:! important", new CssDeclaration("text-align", new CssToken[0], true));
				yield return new TestCaseData("text-align:! important ", new CssDeclaration("text-align", new CssToken[0], true));
				yield return new TestCaseData("text-align: ! important ", new CssDeclaration("text-align", CssToken.Whitespace, true));
				yield return new TestCaseData("text-align:center", new CssDeclaration("text-align", 
					new CssStringToken(CssTokenType.Identifier, "center"), 
					false));
				yield return new TestCaseData("text-align: center\t", new CssDeclaration("text-align",
					new[]
					{
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "center"),
						CssToken.Whitespace,
					}, 
					false));
				yield return new TestCaseData("text-align:center!important", new CssDeclaration("text-align", 
					new CssStringToken(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("text-align:center !important", new CssDeclaration("text-align",
					new[]
					{
						new CssStringToken(CssTokenType.Identifier, "center"),
						CssToken.Whitespace,
					},
					true));
				yield return new TestCaseData("text-align:center ! important", new CssDeclaration("text-align",
					new[]
					{
						new CssStringToken(CssTokenType.Identifier, "center"),
						CssToken.Whitespace,
					},
					true));
				yield return new TestCaseData("text-align:center! important", new CssDeclaration("text-align",
					new CssStringToken(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("text-align:center! important \t ", new CssDeclaration("text-align",
					new CssStringToken(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("font: sans-serif black 10pt", new CssDeclaration("font", 
					new[]
					{
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "sans-serif"),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "black"),
						CssToken.Whitespace,
						new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt"))
					}, 
					false));
				yield return new TestCaseData("font: sans-serif black 10pt !important", new CssDeclaration("font",
					new[]
					{
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "sans-serif"),
						CssToken.Whitespace,
						new CssStringToken(CssTokenType.Identifier, "black"),
						CssToken.Whitespace,
						new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")),
						CssToken.Whitespace,
					},
					true));
			}
		}

		public static IEnumerable<TestCaseData> SingleAtRuleTestCases
		{
			get
			{
				yield return new TestCaseData("@page { margin: 2cm }", 
					new CssAtRule("page", CssToken.Whitespace, new CssBlock(CssBlockType.CurlyBrackets, new[]
						{
							new CssDeclaration("margin", new[]
								{
									CssToken.Whitespace,
									new CssToken<CssNumeric>(CssTokenType.Dimension, new CssNumeric(2, "cm")),
									CssToken.Whitespace,
								}, false)
						})));
			}
		}

		#endregion

		#region Factory methods

		private static CssParser CreateParser(string css)
		{
			return new CssParser(new CssTokenizer(new CssReader(css)));
		}

		#endregion
	}
}
