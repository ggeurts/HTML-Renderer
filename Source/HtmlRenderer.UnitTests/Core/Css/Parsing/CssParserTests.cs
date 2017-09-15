﻿namespace HtmlRenderer.UnitTests.Core.Css.Parsing
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
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var component = parser.ParseComponent();
			Assert.That(component, Is.EqualTo(expectedBlock));
		}

		[Test]
		[TestCaseSource(nameof(SingleBlockTestCases))]
		public void ParseComponent_Block(string css, CssBlock expectedBlock)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var component = parser.ParseComponent();
			Assert.That(component, Is.EqualTo(expectedBlock));
		}

		#endregion

		#region ParseComponentList() Tests

		[Test]
		[TestCaseSource(nameof(SingleValueTestCases))]
		public void ParseComponentList_SingleValue(string css, CssValue expectedComponent)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(new[] { expectedComponent }));
		}

		[Test]
		[TestCaseSource(nameof(SingleBlockTestCases))]
		public void ParseComponentList_SingleBlock(string css, CssBlock expectedBlock)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(new[] { expectedBlock }));
		}

		[Test]
		[TestCaseSource(nameof(TwoValuesTestCases))]
		public void ParseComponentList_MultipleValues(string css, CssValue[] expectedComponents)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(expectedComponents));
		}

		[Test]
		[TestCaseSource(nameof(MultipleComponentTestCases))]
		public void ParseComponentList_MultipleComponents(string css, CssComponent[] expectedComponents)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var componentList = parser.ParseComponentList().ToList();
			Assert.That(componentList, Is.EquivalentTo(expectedComponents));
		}

		#endregion

		#region ParseDeclarationTest

		[Test]
		[TestCaseSource(nameof(SingleDeclarationTestCases))]
		public void ParseDeclaration(string css, CssComponent expectedDeclaration)
		{
			var parser = new CssParser(CssTokenizer.Tokenize(css));
			var component = parser.ParseDeclaration();
			Assert.That(component, Is.EqualTo(expectedDeclaration));
		}

		#endregion

		#region TestCase Sources

		public static IEnumerable<TestCaseData> SingleValueTestCases
		{
			get
			{
				yield return new TestCaseData("id", new CssValue<string>(CssTokenType.Identifier, "id"));
				yield return new TestCaseData("\"some value\"", new CssValue<string>(CssTokenType.QuotedString, "some value"));
				yield return new TestCaseData("'other value'", new CssValue<string>(CssTokenType.QuotedString, "other value"));
				yield return new TestCaseData("12", new CssValue<CssNumeric>(CssTokenType.Number, new CssNumeric(12)));
				yield return new TestCaseData("-14", new CssValue<CssNumeric>(CssTokenType.Number, new CssNumeric(-14)));
				yield return new TestCaseData("12.34e-5", new CssValue<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5)));
				yield return new TestCaseData("12.34E-5", new CssValue<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5)));
				yield return new TestCaseData("-.34e-5", new CssValue<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5)));
				yield return new TestCaseData("-.34E-5", new CssValue<CssNumeric>(CssTokenType.Number | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5)));
				yield return new TestCaseData("10pt", new CssValue<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")));
				yield return new TestCaseData("+10pt", new CssValue<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")));
				yield return new TestCaseData("-10pt", new CssValue<CssNumeric>(CssTokenType.Dimension, new CssNumeric(-10, "pt")));
				yield return new TestCaseData("10.5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(10.5, "pt")));
				yield return new TestCaseData("+10.5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(10.5, "pt")));
				yield return new TestCaseData("-10.5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-10.5, "pt")));
				yield return new TestCaseData("12.34e-5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "pt")));
				yield return new TestCaseData("12.34E-5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "pt")));
				yield return new TestCaseData("-.34e-5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "pt")));
				yield return new TestCaseData("-.34E-5pt", new CssValue<CssNumeric>(CssTokenType.Dimension | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "pt")));
				yield return new TestCaseData("10%", new CssValue<CssNumeric>(CssTokenType.Percentage, new CssNumeric(10, "%")));
				yield return new TestCaseData("+10%", new CssValue<CssNumeric>(CssTokenType.Percentage, new CssNumeric(10, "%")));
				yield return new TestCaseData("-10%", new CssValue<CssNumeric>(CssTokenType.Percentage, new CssNumeric(-10, "%")));
				yield return new TestCaseData("10.5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(10.5, "%")));
				yield return new TestCaseData("+10.5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(10.5, "%")));
				yield return new TestCaseData("-10.5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-10.5, "%")));
				yield return new TestCaseData("12.34e-5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "%")));
				yield return new TestCaseData("12.34E-5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(12.34e-5, "%")));
				yield return new TestCaseData("-.34e-5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "%")));
				yield return new TestCaseData("-.34E-5%", new CssValue<CssNumeric>(CssTokenType.Percentage | CssTokenType.FloatingPointType, new CssNumeric(-0.34e-5, "%")));
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
					var components = new List<CssComponent> { (CssValue)t1.Arguments[1] };

					var size = 2 + i % 3;
					for (var j = 1; j < size; j++)
					{
						var tn = singleComponentTestCases[(i + j + offset) % count];
						css = css + " " + (string)tn.Arguments[0];
						components.Add(CssValue.Whitespace);
						components.Add((CssValue)tn.Arguments[1]);
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
						case 2:
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
							(CssValue)t1.Arguments[1],
							CssValue.Whitespace,
							(CssValue)t2.Arguments[1]
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
							CssValue.Whitespace,
							(CssComponent)t2.Arguments[1]
						});
				}
			}
		}

		public static IEnumerable<TestCaseData> SingleDeclarationTestCases
		{
			get
			{
				yield return new TestCaseData("text-align:", new CssDeclaration("text-align", new CssValue[0], false));
				yield return new TestCaseData(" text-align:", new CssDeclaration("text-align", new CssValue[0], false));
				yield return new TestCaseData("text-align: ", new CssDeclaration("text-align", CssValue.Whitespace, false));
				yield return new TestCaseData("text-align:!important", new CssDeclaration("text-align", new CssValue[0], true));
				yield return new TestCaseData("text-align:! important", new CssDeclaration("text-align", new CssValue[0], true));
				yield return new TestCaseData("text-align:! important ", new CssDeclaration("text-align", new CssValue[0], true));
				yield return new TestCaseData("text-align: ! important ", new CssDeclaration("text-align", CssValue.Whitespace, true));
				yield return new TestCaseData("text-align:center", new CssDeclaration("text-align", 
					new CssValue<string>(CssTokenType.Identifier, "center"), 
					false));
				yield return new TestCaseData("text-align: center\t", new CssDeclaration("text-align",
					new[]
					{
						CssValue.Whitespace,
						new CssValue<string>(CssTokenType.Identifier, "center"),
						CssValue.Whitespace,
					}, 
					false));
				yield return new TestCaseData("text-align:center!important", new CssDeclaration("text-align", 
					new CssValue<string>(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("text-align:center !important", new CssDeclaration("text-align",
					new[]
					{
						new CssValue<string>(CssTokenType.Identifier, "center"),
						CssValue.Whitespace,
					},
					true));
				yield return new TestCaseData("text-align:center ! important", new CssDeclaration("text-align",
					new[]
					{
						new CssValue<string>(CssTokenType.Identifier, "center"),
						CssValue.Whitespace,
					},
					true));
				yield return new TestCaseData("text-align:center! important", new CssDeclaration("text-align",
					new CssValue<string>(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("text-align:center! important \t ", new CssDeclaration("text-align",
					new CssValue<string>(CssTokenType.Identifier, "center"),
					true));
				yield return new TestCaseData("font: sans-serif black 10pt", new CssDeclaration("font", 
					new[]
					{
						CssValue.Whitespace,
						new CssValue<string>(CssTokenType.Identifier, "sans-serif"),
						CssValue.Whitespace,
						new CssValue<string>(CssTokenType.Identifier, "black"),
						CssValue.Whitespace,
						new CssValue<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt"))
					}, 
					false));
				yield return new TestCaseData("font: sans-serif black 10pt !important", new CssDeclaration("font",
					new[]
					{
						CssValue.Whitespace,
						new CssValue<string>(CssTokenType.Identifier, "sans-serif"),
						CssValue.Whitespace,
						new CssValue<string>(CssTokenType.Identifier, "black"),
						CssValue.Whitespace,
						new CssValue<CssNumeric>(CssTokenType.Dimension, new CssNumeric(10, "pt")),
						CssValue.Whitespace,
					},
					true));
			}
		}

		#endregion
	}
}
