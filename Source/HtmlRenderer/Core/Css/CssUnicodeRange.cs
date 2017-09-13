namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssUnicodeRange : IEquatable<CssUnicodeRange>
	{
		public int RangeStart { get; }
		public int RangeEnd { get; }

		public CssUnicodeRange(int rangeStart, int rangeEnd)
		{
			this.RangeStart = rangeStart;
			this.RangeEnd = rangeEnd;
		}

		public bool Equals(CssUnicodeRange other)
		{
			return this.RangeStart == other.RangeStart
			       && this.RangeEnd == other.RangeEnd;
		}

		public override bool Equals(object obj)
		{
			return obj is CssUnicodeRange 
			       && Equals((CssUnicodeRange) obj);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(this.RangeStart, this.RangeEnd);
		}
	}
}