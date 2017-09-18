namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssUnicodeRange : IEquatable<CssUnicodeRange>, IFormattable
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

		public override string ToString()
		{
			return ToString(null, null);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return this.RangeStart == this.RangeEnd
				? this.RangeStart.ToString("X6", formatProvider)
				: string.Format(formatProvider, "{0:X6}-{1:X6}", this.RangeStart, this.RangeEnd);
		}

	}
}