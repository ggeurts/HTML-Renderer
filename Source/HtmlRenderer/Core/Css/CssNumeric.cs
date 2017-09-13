namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssNumeric : IEquatable<CssNumeric>
	{
		public double Value { get; }
		public string Unit { get; }

		public CssNumeric(double value, string unit)
		{
			this.Value = value;
			this.Unit = string.IsNullOrEmpty(unit) ? null : unit;
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public bool Equals(CssNumeric other)
		{
			return this.Value == other.Value
				&& string.Equals(this.Unit, other.Unit, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return obj is CssNumeric && Equals((CssNumeric)obj);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(this.Value.GetHashCode(), this.Unit.GetHashCode());
		}
	}
}