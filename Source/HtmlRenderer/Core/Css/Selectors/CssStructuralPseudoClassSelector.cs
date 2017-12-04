namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssStructuralPseudoClassSelector : CssPseudoClassSelector
	{
		private readonly int _cycleSize;
		private readonly int _offset;

		protected CssStructuralPseudoClassSelector(string name, int cycleSize, int offset)
			: base(name)
		{
			_cycleSize = cycleSize;
			_offset = offset;
		}

		public int CycleSize
		{
			get { return _cycleSize; }
		}

		public int Offset
		{
			get { return _offset; }
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssStructuralPseudoClassSelector;
			return other != null
			       && this.GetType() == other.GetType()
			       && _cycleSize == other._cycleSize
			       && _offset == other._offset;
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(this.GetType().GetHashCode(), HashUtility.Hash(_cycleSize, _offset));
		}

		protected bool Matches(int elementIndex)
		{
			int remainder;
			return Math.DivRem(elementIndex - _offset, _cycleSize, out remainder) >= 0 && remainder == 0;
		}
	}
}