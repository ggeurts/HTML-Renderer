namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssFunction : CssCompositeComponent
	{
		public string Name { get; }

		public CssFunction(string name, ImmutableArray<CssComponent> components)
			: base(components)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			this.Name = name;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(this.Name).Append('(');
			base.ToString(sb);
			sb.Append(')');
		}

	}
}