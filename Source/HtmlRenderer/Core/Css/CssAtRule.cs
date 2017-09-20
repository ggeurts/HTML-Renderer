namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssAtRule : CssComponent, CssRule
	{
		private readonly string _name;
		private readonly CssComponent _prelude;
		private readonly CssBlock _block;

		public CssAtRule(string name, CssComponent prelude, CssBlock block)
		{
			ArgChecker.AssertArgNotNull(name, nameof(name));
			ArgChecker.AssertArgNotNull(prelude, nameof(prelude));
			_name = name;
			_prelude = prelude;
			_block = block;
		}

		public string Name
		{
			get { return _name; }
		}

		public CssComponent Prelude
		{
			get { return _prelude; }
		}

		public CssBlock Block
		{
			get { return _block; }
		}

		public override bool Equals(object obj)
		{
			var other = obj as CssAtRule;
			return other != null
				&& CssEqualityComparer<string>.Default.Equals(_name, other._name)
				&& Equals(_prelude, other._prelude)
				&& Equals(_block, other._block);
		}

		public override int GetHashCode()
		{
			var hash = CssEqualityComparer<string>.Default.GetHashCode(_name);
			hash = HashUtility.Hash(hash, _prelude.GetHashCode());
			if (_block != null) hash = HashUtility.Hash(hash, _block.GetHashCode());
			return hash;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append('@').Append(_name);
			this.Prelude.ToString(sb);
			this.Block?.ToString(sb);
		}
	}
}