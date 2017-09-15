namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;

	public abstract class CssNode
	{
		public override string ToString()
		{
			var sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}

		public abstract void ToString(StringBuilder sb);
	}
}