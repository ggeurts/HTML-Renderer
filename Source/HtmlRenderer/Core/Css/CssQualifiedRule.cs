namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssQualifiedRule : CssNode, CssRule
	{
		private readonly CssComponent _prelude;
		private readonly CssBlock _block;

		public CssQualifiedRule(CssComponent prelude, CssBlock block)
		{
			ArgChecker.AssertArgNotNull(prelude, nameof(prelude));
			ArgChecker.AssertArgNotNull(block, nameof(block));
			_prelude = prelude;
			_block = block;
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
			var other = obj as CssQualifiedRule;
			return other != null
			    && Equals(_prelude, other._prelude)
			    && Equals(_block, other._block);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(_prelude.GetHashCode(), _block.GetHashCode());
		}

		public override void ToString(StringBuilder sb)
		{
			_prelude.ToString(sb);
			_block.ToString(sb);
		}
	}
}