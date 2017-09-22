namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;

	internal class CssUniversalSelector : CssTypeSelector
	{
		public override bool Matches<TElement>(TElement element)
		{
			return true;
		}

		public override bool Equals(object obj)
		{
			return obj is CssUniversalSelector;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append("*|*");
		}
	}
}