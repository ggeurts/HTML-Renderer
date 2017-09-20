namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssStyleSheet : CssNode
	{
		private readonly IImmutableList<CssRule> _rules;

		public IImmutableList<CssRule> Rules
		{
			get { return _rules; }
		}

		public CssStyleSheet(IImmutableList<CssRule> rules)
		{
			ArgChecker.AssertArgNotNull(rules, nameof(rules));
			this._rules = rules;
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssStyleSheet;
			return other != null
				&& _rules.Count == other._rules.Count
				&& _rules.SequenceEqual(other._rules);
		}

		public override int GetHashCode()
		{
			var hash = 0;
			foreach (var rule in _rules)
			{
				hash = HashUtility.Hash(hash, rule.GetHashCode());
			}
			return hash;
		}

		public override void ToString(StringBuilder sb)
		{
			var isFirst = true;
			foreach (var rule in this.Rules)
			{
				if (isFirst)
				{
					isFirst = false;
				}
				else
				{
					sb.AppendLine();
				}
				rule.ToString(sb);
			}
		}
	}
}