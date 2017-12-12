namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssStructuralPseudoClassSelector : CssPseudoClassSelector
	{
		private readonly CssCycleOffset _cycleOffset;

		protected CssStructuralPseudoClassSelector(string name, CssCycleOffset cycleOffset)
			: base(name)
		{
			_cycleOffset = cycleOffset;
		}

		public int CycleSize
		{
			get { return _cycleOffset.CycleSize; }
		}

		public int Offset
		{
			get { return _cycleOffset.Offset; }
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssStructuralPseudoClassSelector;
			return other != null
			       && this.ClassName == other.ClassName
			       && _cycleOffset == other._cycleOffset;
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(this.ClassName.GetHashCode(), _cycleOffset.GetHashCode());
		}

		public override void Apply(CssSelectorVisitor visitor)
		{
			visitor.VisitStructuralPseudoClassSelector(this);
		}

		protected bool Matches(int elementIndex)
		{
			return _cycleOffset.Matches(elementIndex);
		}
	}
}