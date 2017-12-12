namespace HtmlRenderer.UnitTests.Core.Css.Selectors
{
	using NUnit.Framework;
	using TheArtOfDev.HtmlRenderer.Core.Css.Selectors;

	[TestFixture]
	public class CssCycleOffsetTests
	{
		[Test]
		[TestCase(1, 1)]
		[TestCase(1, 0)]
		[TestCase(0, 1)]
		[TestCase(3, -4)]
		[TestCase(-3, 5)]
		public void CreateCycleOffset(int cycleSize, int offset)
		{
			var instance = new CssCycleOffset(cycleSize, offset);
			Assert.Multiple(() =>
			{
				Assert.That(instance.CycleSize, Is.EqualTo(cycleSize), nameof(instance.CycleSize));
				Assert.That(instance.Offset, Is.EqualTo(offset), nameof(instance.Offset));
			});
		}

		[Test]
		[TestCase("n+1", 1, 1)]
		[TestCase("1n+1", 1, 1)]
		[TestCase("+n+1", 1, 1)]
		[TestCase("+1n+1", 1, 1)]
		[TestCase("n", 1, 0)]
		[TestCase("-n", -1, 0)]
		[TestCase("2n", 2, 0)]
		[TestCase("-2n", -2, 0)]
		[TestCase("n+0", 1, 0)]
		[TestCase("1n+0", 1, 0)]
		[TestCase("+n", 1, 0)]
		[TestCase("+n+0", 1, 0)]
		[TestCase("+1n+0", 1, 0)]
		[TestCase("1", 0, 1)]
		[TestCase("+1", 0, 1)]
		[TestCase("0n+1", 0, 1)]
		[TestCase("3n-4", 3, -4)]
		[TestCase("-3n+5", -3, 5)]
		[TestCase(" \t -3n \t + \t 5", -3, 5)]
		[TestCase("odd", 2, 1)]
		[TestCase("Odd", 2, 1)]
		[TestCase("ODD", 2, 1)]
		[TestCase("even", 2, 0)]
		[TestCase("eVen", 2, 0)]
		[TestCase("EVEN", 2, 0)]
		public void CanParse(string input, int cycleSize, int offset)
		{
			var instance = CssCycleOffset.Parse(input);
			Assert.Multiple(() =>
			{
				Assert.That(instance.CycleSize, Is.EqualTo(cycleSize), nameof(instance.CycleSize));
				Assert.That(instance.Offset, Is.EqualTo(offset), nameof(instance.Offset));
			});
		}
	}
}
