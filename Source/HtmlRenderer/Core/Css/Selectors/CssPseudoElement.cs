namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Collections.Immutable;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssPseudoElement : CssComponent
	{
		private static readonly ImmutableHashSet<string> SingleColonNames 
			= ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, "first-line", "first-letter", "before", "after");

		private readonly string _name;

		internal CssPseudoElement(string name)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			_name = name;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssPseudoElement;
			return other != null
				&& CssEqualityComparer<string>.Default.Equals(_name, other._name);
		}

		public override int GetHashCode()
		{
			return CssEqualityComparer<string>.Default.GetHashCode(_name);
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(':', AllowSingleColonPrefix(_name) ? 1 : 2).Append(_name);
		}

		public static bool AllowSingleColonPrefix(string name)
		{
			return SingleColonNames.Contains(name);
		}
	}
}