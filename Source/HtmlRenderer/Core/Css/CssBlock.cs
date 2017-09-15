namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssBlock : CssCompositeComponent
	{
		private readonly CssBlockType _blockType;

		public CssBlock(CssBlockType blockType, IEnumerable<CssComponent> components)
			: this(blockType, ImmutableArray.CreateRange(components))
		{}

		public CssBlock(CssBlockType blockType, IImmutableList<CssComponent> components)
			: base(components)
		{
			_blockType = blockType;
		}

		public CssBlockType BlockType
		{
			get { return _blockType; }
		}


		public override bool Equals(object obj)
		{
			var otherBlock = obj as CssBlock;
			return otherBlock != null
				&& _blockType == otherBlock._blockType
				&& base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(base.GetHashCode(), _blockType.BeginChar.GetHashCode());
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_blockType.BeginChar);
			if (_blockType == CssBlockType.CurlyBrackets) sb.AppendLine();
			base.ToString(sb);
			if (_blockType == CssBlockType.CurlyBrackets) sb.AppendLine();
			sb.Append(_blockType.EndChar);
		}
	}
}