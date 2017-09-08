namespace HtmlRenderer.UnitTests.Core
{
	using System.Linq;
	using NSubstitute;
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Adapters;
	using TheArtOfDev.HtmlRenderer.Core;

	[TestFixture]
	public class CssDataTests
	{
		[Test]
		public void CanParseRuleWithSingleElementSelector()
		{
			const string CSS = @"
				body {
				}";

			var adapter = Substitute.For<RAdapter>();
			var cssData = CssData.Parse(adapter, CSS, false);

			Assert.That(cssData.ContainsCssBlock("body"), Is.True);
		}

		[Test]
		public void CanParseAtPageRuleWithoutMarginBoxes()
		{
			const string CSS = @"
				@page {
					margin: 20mm;
				}";

			var adapter = Substitute.For<RAdapter>();
			var cssData = CssData.Parse(adapter, CSS, false);

			Assert.That(cssData.ContainsCssBlock("@page"), Is.True, "ContainsCssBlock()");

			var cssAtPageBlock = cssData.GetCssBlock("@page").ToArray();
			Assert.That(cssAtPageBlock, Has.Length.EqualTo(1), "GetCssBlock()");
			Assert.That(cssAtPageBlock[0].Class, Is.EqualTo("@page"), "Class");
			Assert.That(cssAtPageBlock[0].Properties.Keys, 
				Is.EquivalentTo(new[] { "margin-left", "margin-top", "margin-right", "margin-bottom" }), 
				"Properties");
		}

		[Test]
		public void CanParseAtPageRuleWithMarginBoxes()
		{
			const string CSS = @"
				@page {
					@bottom-left {
						margin: 10pt 0 30pt 0;
						border-top: .25pt solid #666;
						content: 'My Book';
						font-size: 9pt;
						color: #333;
					}
				}";

			var adapter = Substitute.For<RAdapter>();
			var cssData = CssData.Parse(adapter, CSS, false);

			Assert.That(cssData.ContainsCssBlock("@page"), Is.True, "ContainsCssBlock()");

			var cssAtPageBlock = cssData.GetCssBlock("@page").ToArray();
			Assert.That(cssAtPageBlock, Has.Length.EqualTo(1), "GetCssBlock()");
			Assert.That(cssAtPageBlock[0].Class, Is.EqualTo("@page"), "Class");
			Assert.That(cssAtPageBlock[0].Properties.Keys, Is.Empty, "Properties");
		}
	}
}
